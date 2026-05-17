using SQLite;
using Assets.UnitsCharacteristics;

namespace Assets.Db.Models
{
    [Table("units")]
    public class UnitEntity
    {
        [Column("id"), PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("health_points")]
        public int HealthPoints { get; set; }

        [Column("active_action_points")]
        public int ActiveActionPoints { get; set; }

        [Column("reaction_action_points")]
        public int ReactionActionPoints { get; set; }

        [Column("agility")]
        public int Agility { get; set; }

        [Column("melee_skill")]
        public int MeleeSkill { get; set; }

        [Column("defend_skill")]
        public int DefendSkill { get; set; }

        [Column("side")]
        public SideType Side { get; set; }
    }
}