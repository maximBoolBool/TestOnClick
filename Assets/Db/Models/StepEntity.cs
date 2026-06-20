using Assets.Db.Enums;
using SQLite;

namespace Assets.Db.Models
{
    [Table("steps")]
    public class StepEntity
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Column("type")]
        public ActionStepType Type { get; set; }
    }
}
