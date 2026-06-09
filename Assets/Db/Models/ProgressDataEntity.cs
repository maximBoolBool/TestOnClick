using Assets.Db.Enums;
using SQLite;

namespace Assets.Db.Models
{
    [Table("progress_data")]
    public class ProgressDataEntity
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("type"), PrimaryKey]
        public ProgressDataType Type { get; set; }

        [Column("value")]
        public string Value { get; set; }
    }
}
