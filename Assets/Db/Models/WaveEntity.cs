using SQLite;

namespace Assets.Db.Models
{
    [Table("waves")]
    class WaveEntity
    {
        [Column("id"), PrimaryKey]
        public int Id { get; set; }
    }
}