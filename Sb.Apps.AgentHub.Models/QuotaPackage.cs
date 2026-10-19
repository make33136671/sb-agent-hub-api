using FreeSql.DataAnnotations;

namespace Sb.Apps.AgentHub.Models
{
    /// <summary>
    /// 套餐模板
    /// </summary>
    [Table(Name = "sb_agenthub_quota_package")]
    public class QuotaPackage : IEntity, ISoftDelete
    {
        /// <summary>主键Id</summary>
        [Column(Name = "Id", IsPrimary = true, IsIdentity = true)]
        public int Id { get; set; }

        /// <summary>套餐名称</summary>
        [Column(Name = "PackageName", StringLength = 100)]
        public string PackageName { get; set; } = "";

        /// <summary>套餐标识</summary>
        [Column(Name = "PackageKey", StringLength = 50)]
        public string PackageKey { get; set; } = "";

        /// <summary>套餐描述</summary>
        [Column(Name = "Description", StringLength = 500)]
        public string Description { get; set; } = "";

        /// <summary>价格</summary>
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

        /// <summary>排序</summary>
        [Column(Name = "SortOrder")]
        public int SortOrder { get; set; }

        /// <summary>是否启用</summary>
        [Column(Name = "IsEnabled")]
        public bool IsEnabled { get; set; } = true;

        /// <summary>创建时间</summary>
        [Column(Name = "CreateTime", DbType = "timestamp", InsertValueSql = "now()")]
        public DateTime CreateTime { get; set; }

        /// <summary>更新时间</summary>
        [Column(Name = "UpdateTime")]
        public DateTime? UpdateTime { get; set; }

        /// <summary>软删除标记</summary>
        [Column(Name = "IsDeleted", IsNullable = false, InsertValueSql = "false")]
        public bool IsDeleted { get; set; }

        /// <summary>删除时间</summary>
        [Column(Name = "DeletedAt", IsNullable = true)]
        public DateTime? DeletedAt { get; set; }

        /// <summary>删除人Id</summary>
        [Column(Name = "DeletedBy", IsNullable = true)]
        public int? DeletedBy { get; set; }
    }
}
