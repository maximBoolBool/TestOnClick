using SQLite;

namespace Assets.Db.Models
{
    [Table("room_wave")]
    public class RoomWaveEntity
    {
        [Column("id"),PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Column("room_id")]
        public int RoomId { get; set; }

        [Column("wave_id")]
        public int WaveId { get; set; }

        [Column("order")]
        public int Order { get; set; }
    }
}
