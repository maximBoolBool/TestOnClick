using Assets.Db.Enums;
using SQLite;

namespace Assets.Db.Models
{
    [Table("progress_data")]
    class ProgressDataEntity
    {
        [Column("id"), PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Column("type")]
        public ProgressDataType Type { get; set; }

        [Column("value")]
        public string Value { get; set; }
    }
}
