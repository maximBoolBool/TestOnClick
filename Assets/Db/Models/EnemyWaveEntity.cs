using SQLite;

namespace Assets.Db.Models
{
    [Table("enemy_wave")]
    public class EnemyWaveEntity
    {
        [Column("id"), PrimaryKey]
        public int Id { get; set; }

        [Column("wave_id")]
        public int WaveId { get; set; }

        [Column("room_id")]
        public int RoomId { get; set; }
    }
}