using SQLite;

namespace Assets.Db.Models
{
    [Table("unit_skills")]
    public class UnitSkillEntity
    {
        [Column("unit_id")]
        public int UnitId { get; set; }

        [Column("skill_id")]
        public int SkillId { get; set; }

        [Column("order")]
        public int Order { get; set; }
    }
}
