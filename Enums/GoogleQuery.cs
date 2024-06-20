using System.ComponentModel;

namespace schedulebackup.Enums
{
    /// <summary>
    /// 請參考: https://developers.google.com/drive/api/guides/ref-search-terms?hl=zh-tw#file-properties
    /// </summary>
    public enum GoogleQueryTerm
    {
        [Description("name")]
        Name,
        [Description("fullText")]
        FullText,
        [Description("mimeType")]
        MimeType,
        [Description("modifiedTime")]
        ModifiedTime,
        [Description("viewedByMeTime")]
        ViewedByMeTime,
        [Description("trashed")]
        Trashed,
        [Description("parents")]
        Parents,
        [Description("createdTime")]
        CreatedTime,
    }

    /// <summary>
    /// 請參考: https://developers.google.com/drive/api/guides/ref-search-terms?hl=zh-tw#file-properties
    /// </summary>
    public enum GoogleQueryOperator
    {
        [Description("")]
        None,
        [Description("contains")]
        Contains,
        [Description("=")]
        EQ,
        [Description("!=")]
        NEQ,
        [Description("<")]
        LT,
        [Description("<=")]
        LTEQ,
        [Description(">")]
        GT,
        [Description(">=")]
        GTEQ,
        [Description("in")]
        In,
        [Description("and")]
        And,
        [Description("or")]
        Or,
        [Description("not")]
        Not,
        [Description("has")]
        Has,
    }
}