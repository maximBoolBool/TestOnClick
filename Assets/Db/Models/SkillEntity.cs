using Assets.Db.Enums;
using SQLite;

namespace Assets.Db.Models
{
    [Table("skills")]
    public class SkillEntity
    {
        [PrimaryKey]
        [Column("id")]
        public int Id { get; set; }

        [Column("point_cost")]
        public int PointCost { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Column("type")]
        public virtual ActionTargetType Type { get; set; }
    }
}
