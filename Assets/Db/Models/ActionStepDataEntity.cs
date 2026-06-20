using Assets.Db.Enums;
using SQLite;

namespace Assets.Db.Models
{
    [Table("action_step_data")]
    public class ActionStepDataEntity
    {
        [Column("id")]
        public int Id { get; set; }
        
        [Column("step_id")]
        public int StepId { get; set; }

        [Column("value")]
        public string Value { get; set; }

        [Column("type")]
        public ActionStepType Type { get; set; }
    }
}
