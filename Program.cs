using Common.Extensions;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using schedulebackup.Clients;
using schedulebackup.Enums;
using schedulebackup.Models;
using schedulebackup.Settings;

var builder = new HostBuilder()
.ConfigureHostConfiguration(configHost =>
{
    configHost.SetBasePath(Directory.GetCurrentDirectory());
    configHost.AddJsonFile("appsettings.json", optional: true);
})
.ConfigureServices((hostContext, services) =>
{
    // appsettings.json
    services.Configure<AppSettings>(hostContext.Configuration.GetSection("AppSettings"));
    // 註冊服務
    services.AddSingleton<HttpClient>();
    services.AddSingleton<GoogleDriveClient>();
    services.AddSingleton<DirectoryFileClient>();
    services.AddSingleton<SmtpEmailClient>();

});;

var host = builder.Build();

using var serviceScope = host.Services.CreateScope();
var services = serviceScope.ServiceProvider;
var googleDriveClient = services.GetRequiredService<GoogleDriveClient>();
var directoryFileClient = services.GetRequiredService<DirectoryFileClient>();
var smtpEmailClient = services.GetRequiredService<SmtpEmailClient>();


#region 參數讀取
var action = Enum.Parse<ActionType>(args[0]);

var targetDirectory = "";
var date = new DateTime();
var account = "";

switch(action)
{
    case ActionType.Auth:
        account = args[1];
        break;
    case ActionType.Backup:
        targetDirectory = args[1];
        account = args[2];
        date = DateTime.Parse(args[3]);
        break;
    case ActionType.Delete:
        targetDirectory = args[1];
        date = DateTime.Parse(args[2]);
        break;
}
#endregion 參數讀取

#region 雲端帳號授權
if(action == ActionType.Auth)
{
    await googleDriveClient.ClientOAuth(account);
    return;   
}
#endregion 雲端帳號授權

#region 讀取目錄檔案
var targetDirectoryName = Path.GetFileName(targetDirectory);
if(!Directory.Exists(targetDirectory)) Directory.CreateDirectory(targetDirectory);

// 取得目錄下所有檔案結構
var root = new LocalFile
{
    Name = targetDirectoryName,
    Path = targetDirectory,
    Parent = "",
    CreateTime = File.GetCreationTime(targetDirectory),
    UpdateTime = File.GetLastWriteTime(targetDirectory),
    Type = FileType.Folder,
    Children = directoryFileClient.GetDirectoryFiles(targetDirectory)
};
#endregion 讀取目錄檔案

#region 刪除目錄檔案
if (action == ActionType.Delete)
{
    directoryFileClient.DeleteFileBeforeDate(root, date);
    return;
} 
#endregion 刪除目錄檔案

#region Google Drive處理
if (action == ActionType.Backup)
{
    var googleCredential = await googleDriveClient.ClientOAuth(account);
            
    //取得根目錄
    var googleRootFolder = await googleDriveClient.GetRoot(googleCredential);

    //檢查資料夾是否已存在於根目錄
    var res = await googleDriveClient.SearchFiles(new List<GoogleQueryCondition>
    {
        new GoogleQueryCondition{ QueryTerm = GoogleQueryTerm.MimeType, QueryOperator = GoogleQueryOperator.EQ, Value = MimeType.Folder.GetDescription()},
        new GoogleQueryCondition{ ConnectionOperator = GoogleQueryOperator.And, QueryTerm = GoogleQueryTerm.Name, QueryOperator = GoogleQueryOperator.EQ, Value = root.Name},
        new GoogleQueryCondition{ ConnectionOperator = GoogleQueryOperator.And, QueryTerm = GoogleQueryTerm.Parents, QueryOperator = GoogleQueryOperator.In, Value = googleRootFolder.Id},
        new GoogleQueryCondition{ ConnectionOperator = GoogleQueryOperator.And, QueryTerm = GoogleQueryTerm.Trashed, QueryOperator = GoogleQueryOperator.EQ, Value = false},

    }, googleCredential);

    //不存在則新增雲端主資料夾
    if(res.Count() == 0)
    {
        var folderId = await googleDriveClient.CreateItem(new CreateRequest
        {
            Name = targetDirectoryName,
            ParentIds = new List<string>{googleRootFolder.Id},
            MimeType = MimeType.Folder
        }, googleCredential);

        googleRootFolder = await googleDriveClient.GetFileById(folderId, googleCredential);
    }
    //已存在，則把根目錄替換成主資料夾
    else
    {
        googleRootFolder = res.FirstOrDefault();
    }

    // 上傳檔案
    var emailBody = await GoogleUploadFiles(root, googleRootFolder.Id, googleDriveClient, googleCredential, date);

    //傳送 email 通知
    smtpEmailClient.SendEmailNotify($"{DateTime.Today:yyyy/MM/dd} {Path.GetFileName(targetDirectory)} 備份結果通知", emailBody);
}

async Task<string> GoogleUploadFiles(LocalFile folder, string folderId, GoogleDriveClient googleDriveClient, ICredential credential, DateTime date, int count = 1)
{
    var googleFolder = await googleDriveClient.GetFileById(folderId, credential);
    var message = $"{folder.Path} 資料夾\n\n";
    // 檔案處理
    foreach(var file in folder.Children.Where(f => f.Type == FileType.File))
    {
        if (File.Exists(file.Path))
        {
            try
            {
                //日期小於帶入日期 => 不上傳
                Console.WriteLine($"File: {file.Name}, UpdateDate: {file.UpdateTime.Date}, TargetDate: {date.Date}");
                if(file.UpdateTime.AddHours(8).Date < date.Date)
                {
                    continue;
                }

                //上傳檔案
                using FileStream fileStream = new FileStream(file.Path, FileMode.Open, FileAccess.Read);
                await googleDriveClient.UploadToFolder(new UploadFileRequest
                {
                    FileStream = fileStream,
                    FileName = file.Name,
                    FolderId = googleFolder.Id
                }, credential);

                message += $"✅ [{count++}.] {file.Name} (檔案大小: {file.Size.GetFileSizeFormat()}).\n\n";
            }
            catch(Exception ex)
            {
                message += $"❌ [{count++}.] {file.Name} (檔案大小: {file.Size.GetFileSizeFormat()}) : {ex.Message}\n\n";
            }
        }
    }

    //資料夾遞回處理
    message += $"-------------------------\n\n";

    foreach(var sub in folder.Children.Where(f => f.Type == FileType.Folder))
    {
        //檢查資料夾是否已存在
        var res = await googleDriveClient.SearchFiles(new List<GoogleQueryCondition>
        {
            new GoogleQueryCondition{ QueryTerm = GoogleQueryTerm.MimeType, QueryOperator = GoogleQueryOperator.EQ, Value = MimeType.Folder.GetDescription()},
            new GoogleQueryCondition{ ConnectionOperator = GoogleQueryOperator.And, QueryTerm = GoogleQueryTerm.Name, QueryOperator = GoogleQueryOperator.EQ, Value = sub.Name},
            new GoogleQueryCondition{ ConnectionOperator = GoogleQueryOperator.And, QueryTerm = GoogleQueryTerm.Parents, QueryOperator = GoogleQueryOperator.In, Value = googleFolder.Id},
            new GoogleQueryCondition{ ConnectionOperator = GoogleQueryOperator.And, QueryTerm = GoogleQueryTerm.Trashed, QueryOperator = GoogleQueryOperator.EQ, Value = false},

        }, credential);

        var tempFolder = googleFolder;
        //建立新雲端資料夾
        if(res.Count() == 0)
        {
            folderId = await googleDriveClient.CreateItem(new CreateRequest
            {
                Name = sub.Name,
                ParentIds = new List<string>{googleFolder.Id},
                MimeType = MimeType.Folder
            }, credential);

            tempFolder = await googleDriveClient.GetFileById(folderId, credential);
        }
        //取得雲端資料夾
        else
        {
            tempFolder = res.FirstOrDefault();
        }

        //遞迴處理
        message += await GoogleUploadFiles(sub, tempFolder.Id, googleDriveClient, credential, date);
    }
    
    return message;
}
#endregion Google Drive處理
