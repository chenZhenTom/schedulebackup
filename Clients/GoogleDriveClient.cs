using schedulebackup.Models;
using Common.Extensions;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace schedulebackup.Clients
{
    public class GoogleDriveClient
    {
        //google drive 檔案屬性欄位
        private readonly string fileFields = "id, name, parents, mimeType, size, trashed, capabilities, modifiedTime, webViewLink, webContentLink, modifiedTime";
        
        /// <summary>
        /// 取得使用者授權憑證
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public async Task<UserCredential> ClientOAuth(string account)
        {
            UserCredential credential;
            using (var stream = new FileStream("google_client_secrets.json", FileMode.Open, FileAccess.Read))
            {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    new[] {DriveService.Scope.Drive },
                    account,                    
                    CancellationToken.None,
                    new FileDataStore("./Credentials", true)
                );
            }
            return credential;
        }

        /// <summary>
        /// 使用者憑證轉換DriveService
        /// </summary>
        /// <param name="credential"></param>
        /// <returns></returns>
        private DriveService GetDriveService(ICredential credential)
        {
            return new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "Drive API Snippets"
            });
        }

        /// <summary>
        /// 取得 google drive root 資料夾
        /// </summary>
        /// <param name="credential"></param>
        /// <returns></returns>
        public async Task<Google.Apis.Drive.v3.Data.File> GetRoot(ICredential credential)
        {
            try
            {
                var service = GetDriveService(credential);

                var req = service.Files.Get("root");

                req.Fields = $"{fileFields}";

                var file = await req.ExecuteAsync();

                return file;
            }
            catch(Exception ex)
            {
                throw ex;
            }
            
        }
        
        /// <summary>
        /// 取得 google drive file by id
        /// </summary>
        /// <param name="fileId"></param>
        /// <param name="credential"></param>
        /// <returns></returns>
        public async Task<Google.Apis.Drive.v3.Data.File> GetFileById(string fileId, ICredential credential)
        {
            var service = GetDriveService(credential);

            var req = service.Files.Get(fileId);

            req.Fields = $"{fileFields}";

            var file = await req.ExecuteAsync();

            return file;
        }

        /// <summary>
        /// 搜尋 google drvive file by conditions
        /// </summary>
        /// <param name="conditions"></param>
        /// <param name="credential"></param>
        /// <returns></returns>    
        public async Task<List<Google.Apis.Drive.v3.Data.File>> SearchFiles(List<GoogleQueryCondition> conditions, ICredential credential)
        {
            List<Google.Apis.Drive.v3.Data.File> listOfFiles = new List<Google.Apis.Drive.v3.Data.File>();

            var service = GetDriveService(credential);

            FilesResource.ListRequest listRequest = service.Files.List();
            listRequest.PageSize = 1000;
            listRequest.Fields = $"nextPageToken, files({fileFields})";
            listRequest.Q = GetQueryConditionString(conditions);

            FileList fileFeedList = await listRequest.ExecuteAsync();

            while (fileFeedList != null)
            {
                foreach (Google.Apis.Drive.v3.Data.File file in fileFeedList.Files)
                {
                    listOfFiles.Add(file);
                }
                if (fileFeedList.NextPageToken == null)
                {
                    break;
                }
                else
                {
                    listRequest.PageToken = fileFeedList.NextPageToken;
                    fileFeedList = await listRequest.ExecuteAsync();
                }
            }
            return listOfFiles;
        }

        /// <summary>
        /// 上傳檔案到指定資料夾
        /// </summary>
        /// <param name="uploadRequest"></param>
        /// <param name="credential"></param>
        /// <returns></returns>
        public async Task<Google.Apis.Drive.v3.Data.File> UploadToFolder (UploadFileRequest uploadRequest, ICredential credential)
        {
            try
            {
                var service = GetDriveService(credential);

                var fileMetadata = new Google.Apis.Drive.v3.Data.File()
                {
                    Id = uploadRequest.FileId != null ? uploadRequest.FileId : null,
                    Name = uploadRequest.FileName,
                    Parents = new List<string>
                    {
                        uploadRequest.FolderId
                    },
                };

                FilesResource.CreateMediaUpload request;
                request = service.Files.Create(
                    fileMetadata, uploadRequest.FileStream, uploadRequest.FileFormat);
                request.Fields = "id";
                var res = await request.UploadAsync();
                var fileContent = request.ResponseBody;

                if(res.Status == Google.Apis.Upload.UploadStatus.Failed)
                {
                    if(res.Exception.Message.Contains("The user's Drive storage quota has been exceeded."))
                    {
                        throw new Exception("雲端空間不足");
                    }
                    else
                    {
                        throw new Exception(res.Exception.Message);
                    }
                }

                return fileContent;
            }
            catch (Exception e)
            {
                throw e;
            }            
        }

        /// <summary>
        /// 建立雲端資料夾檔案(含資料夾)
        /// </summary>
        /// <param name="createRequest"></param>
        /// <param name="credential"></param>
        /// <returns></returns>    
        public async Task<string> CreateItem(CreateRequest createRequest, ICredential credential)
        {
            try
            {

                var service = GetDriveService(credential);

                // File metadata
                var fileMetadata = new Google.Apis.Drive.v3.Data.File()
                {
                    Name = createRequest.Name,
                    MimeType = createRequest.MimeType.GetDescription(),
                    Parents = createRequest.ParentIds.Count() != 0 ? createRequest.ParentIds : null
                };

                // Create a new item on drive.
                var request = service.Files.Create(fileMetadata);
                request.Fields = "id";
                var file = await request.ExecuteAsync();
                
                return file.Id;
            }
            catch (Exception e)
            {
                // TODO(developer) - handle error appropriately
                if (e is AggregateException)
                {
                    Console.WriteLine("Credential Not found");
                }
                else
                {
                    throw;
                }
            }
            return null;
        }

        /// <summary>
        /// 組合搜尋條件
        /// </summary>
        /// <param name="conditions"></param>
        /// <returns></returns>
        private string GetQueryConditionString(List<GoogleQueryCondition> conditions)
        {
            var q = "";
            foreach(var con in conditions)
            {
                var valueString = "";

                if(con.Value == null) 
                {
                    valueString = "";
                }
                else if(con.Value.GetType() == typeof(string))
                {
                    valueString = $"'{con.Value}'";
                }
                else if(con.Value.GetType() == typeof(bool))
                {
                    valueString = $"{con.Value}";
                }

                if(con.QueryOperator == Enums.GoogleQueryOperator.In)
                    q += $"{con.ConnectionOperator.GetDescription()} {valueString} {con.QueryOperator.GetDescription()} {con.QueryTerm.GetDescription()} ";
                else
                    q += $"{con.ConnectionOperator.GetDescription()} {con.QueryTerm.GetDescription()} {con.QueryOperator.GetDescription()} {valueString} ";
            }
            return q;
        }
    }
}