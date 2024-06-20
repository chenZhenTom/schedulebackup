
using System.ComponentModel;

namespace schedulebackup.Enums
{
    /// <summary>
    /// 請參考: https://developers.google.com/drive/api/guides/mime-types?hl=zh-tw
    /// </summary>
    public enum MimeType
    {
        [Description("")]
        Unknown,
        [Description("application/vnd.google-apps.audio")]
        Audio,
        [Description("application/vnd.google-apps.video")]
        Video,
        [Description("application/vnd.google-apps.file")]
        File,
        [Description("application/vnd.google-apps.folder")]
        Folder,
    }
}