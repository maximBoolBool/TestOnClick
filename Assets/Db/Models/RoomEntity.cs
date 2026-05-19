using SQLite;

namespace Assets.Db.Models
{
    [Table("rooms")]
    public class RoomEntity
    {
        [Column("id"), PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("room_type")]
        public Enums.RoomType RoomType { get; set; }

        [Column("location_type")]
        public Enums.LocationType LocationType { get; set; }
    }
}
