using SQLite;

namespace Assets.Db.Models
{
    [Table("waves")]
    public class WaveEntity
    {
        [Column("id"), PrimaryKey, AutoIncrement]
        public int Id { get; set; }
    }
}