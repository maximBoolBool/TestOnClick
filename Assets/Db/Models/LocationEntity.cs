using Assets.Db.Enums;
using SQLite;

namespace Assets.Db.Models
{
    [Table("locations")]
    public class LocationEntity
    {
        [Column("type"), PrimaryKey, AutoIncrement]
        public LocationType Type { get; set; }

        [Column("min_room_count")]
        public int MinRoomCount { get; set; }

        [Column("max_room_count")]
        public int MaxRoomCount { get; set; }
    }
}
