using schedulebackup.Enums;

namespace schedulebackup.Models
{
    public class CreateRequest
    {
        public string Name { get; set; }

        public MimeType MimeType { get; set; }        

        public List<string> ParentIds { get; set; }
    }
}