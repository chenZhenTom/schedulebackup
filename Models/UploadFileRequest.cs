namespace schedulebackup.Models
{
    public class UploadFileRequest
    {
        public Stream FileStream { get; set; } = null!;
        /// <summary>
        /// FileId: 更新指定檔案使用
        /// </summary>
        public string? FileId { get; set; }
        public string FileName { get; set; } = null!;        
        public string? FolderId { get; set; }
        public string? FileFormat { get; set; }
    }
}