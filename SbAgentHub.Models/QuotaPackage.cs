using FreeSql.DataAnnotations;

namespace SbAgentHub.Models
{
    /// <summary>
    /// 套餐模板
    /// </summary>
    [Table(Name = "SB_QuotaPackage")]
    public class QuotaPackage : IEntity, ISoftDelete
    {
        [Column(Name = "Id", IsPrimary = true, IsIdentity = true)]
        public int Id { get; set; }

        [Column(Name = "PackageName", StringLength = 100)]
        public string PackageName { get; set; } = "";

        [Column(Name = "PackageKey", StringLength = 50)]
        public string PackageKey { get; set; } = "";

        [Column(Name = "Description", StringLength = 500)]
        public string Description { get; set; } = "";

        [Column(Name = "Price", DbType = "DECIMAL(10,2)")]
        public decimal Price { get; set; }

        /// <summary>
        /// 计费周期: month/year
        /// </summary>
        [Column(Name = "PricePeriod", StringLength = 20)]
        public string PricePeriod { get; set; } = "month";

        /// <summary>
        /// 配额设置(JSON)，格式: {"max_platforms":1,"max_agents":3,...}
        /// </summary>
        [Column(Name = "QuotaSettings", DbType = "TEXT")]
        public string QuotaSettings { get; set; } = "{}";

        [Column(Name = "SortOrder")]
        public int SortOrder { get; set; }

        [Column(Name = "IsEnabled")]
        public bool IsEnabled { get; set; } = true;

        [Column(Name = "CreateTime")]
        public DateTime CreateTime { get; set; } = DateTime.Now;

        [Column(Name = "UpdateTime")]
        public DateTime? UpdateTime { get; set; }

        [Column(Name = "IsDeleted", IsNullable = false, InsertValueSql = "0")]
        public bool IsDeleted { get; set; }
    }
}
