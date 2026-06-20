using SQLite;

namespace Assets.Db.Models
{
    [Table("skill_steps")]
    public class SkillStepEntity
    {
        [Column("skill_id")]
        public int SkillId { get; set; }

        [Column("step_id")]
        public int StepId { get; set; }

        [Column("order")]
        public int Order { get; set; }

        [Column("before_step_result")]
        public int? BeforeStepResult { get; set; }
    }
}
