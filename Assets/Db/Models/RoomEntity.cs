using Assets.Db.Enums;
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

        [Column("key")]
        public string Key { get; set; }

        [Column("room_type")]
        public RoomType RoomType { get; set; }

        [Column("location_type")]
        public LocationType LocationType { get; set; }

        //PRT-9 Сделать как добавим РАЗНОЕ количество "этажей в команты"
        /*public RoomLayerType MaxRoomLayre {  get; set; }

        public RoomLayerType MinRoomlayre { get; set; }*/
    }
}
