using schedulebackup.Enums;

namespace schedulebackup.Models
{
    public class GoogleQueryCondition
    {
        public GoogleQueryOperator ConnectionOperator {get; set;} = default;
        public GoogleQueryTerm QueryTerm {get; set;}
        public GoogleQueryOperator QueryOperator {get; set;}
        public dynamic? Value {get; set;}
    }
}