using SQLite;

namespace Assets.Db.Models
{
    [Table("wave_room")]
    public class WaveRoomEntity
    {
        [Column("id"), AutoIncrement, PrimaryKey]
        public int Id { get; set; }

        [Column("wave_id")]
        public int WaveId { get; set; }

        [Column("room_id")]
        public int RoomId { get; set; }

        [Column("order")]
        public int Order { get; set; }
    }
}
