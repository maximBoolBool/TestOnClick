using SQLite;

namespace Assets.Db.Models
{
    [Table("wave_enemies")]
    public class WaveEnemies
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Column("wave_id")]
        public int WaveId { get; set; }

        [Column("unit_id")]
        public int UnitId { get; set; }

        [Column("count")]
        public int Count { get; set; }
    }
}
