using schedulebackup.Enums;
using schedulebackup.Models;

namespace schedulebackup.Clients
{
    public class DirectoryFileClient
    {
        public DirectoryFileClient(){}

        /// <summary>
        /// 取得 targetDirectory 下的完整資料夾結構
        /// </summary>
        /// <param name="targetDirectory"></param>
        /// <returns></returns>
        public List<LocalFile> GetDirectoryFiles(string targetDirectory)
        {
            var files = new List<LocalFile>();
            // 取得目錄中的所有檔案
            string[] filePaths = Directory.GetFiles(targetDirectory);

            files.AddRange(filePaths.Select(f => new LocalFile
            {
                Name = Path.GetFileName(f),
                Size = new FileInfo(f).Length,
                Path = f,
                Parent = Path.GetFileName(targetDirectory),
                CreateTime = File.GetCreationTime(f),
                UpdateTime = File.GetLastWriteTime(f)
            }));

            // 遞迴處理子目錄
            string[] subdirectories = Directory.GetDirectories(targetDirectory);
            foreach (string sub in subdirectories)
            {
                var subFolder = new LocalFile
                {
                    Name = Path.GetFileName(sub),
                    Path = sub,
                    Parent = Path.GetFileName(targetDirectory),
                    Type = FileType.Folder,
                    CreateTime = File.GetCreationTime(sub),
                    UpdateTime = File.GetLastWriteTime(sub)
                };

                subFolder.Children.AddRange(GetDirectoryFiles(sub));
                files.Add(subFolder);
            }

            return files;
        }

        /// <summary>
        /// 刪除 root 下所有更新時間 <= dataTime 的檔案
        /// </summary>
        /// <param name="root"></param>
        /// <param name="dateTime"></param>
        public void DeleteFileBeforeDate(LocalFile root, DateTime dateTime)
        {
            foreach(var file in root.Children.Where(f => f.UpdateTime.Date < dateTime.Date && f.Type == FileType.File))
            {
                if(file.UpdateTime.AddHours(8).Date <= dateTime.Date)
                {
                    File.Delete(file.Path);
                }
            }

            foreach(var folder in root.Children.Where(f => f.UpdateTime.Date < dateTime.Date && f.Type == FileType.Folder))
            {
                DeleteFileBeforeDate(folder, dateTime);

                if(folder.Children.Count() == 0)
                {
                    Directory.Delete(folder.Path, true);
                }
            }

        }
    }
}