using System.Reflection.Metadata.Ecma335;
using schedulebackup.Enums;

namespace schedulebackup.Models
{
    public class LocalFile
    {
        public string Name {get; set;} = null!; 
        public string Path {get; set;} = null!;
        public string Parent {get; set;} = null!;
        public long Size {get; set;}
        public FileType Type {get; set;}
        public DateTime CreateTime {get; set;}
        public DateTime UpdateTime {get; set;}
        public List<LocalFile> Children { get; set; } = new List<LocalFile>();
    }
}