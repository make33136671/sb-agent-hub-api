using FreeSql.DataAnnotations;

namespace Sb.Apps.AgentHub.Models
{
    /// <summary>
    /// 配额定义
    /// </summary>
    [Table(Name = "sb_agenthub_quota_config")]
    public class QuotaConfig : IEntity, ISoftDelete
    {
        /// <summary>主键Id</summary>
        [Column(Name = "Id", IsPrimary = true, IsIdentity = true)]
        public int Id { get; set; }

        /// <summary>
        /// 配额标识，如 max_platforms
        /// </summary>
        [Column(Name = "QuotaKey", StringLength = 50, IsNullable = false)]
        public required string QuotaKey { get; set; }

        /// <summary>
        /// 显示名称，如 最大平台数
        /// </summary>
        [Column(Name = "QuotaName", StringLength = 100, IsNullable = false)]
        public required string QuotaName { get; set; }

        /// <summary>
        /// 配额类型: 0=数量型 1=开关型 2=存储型
        /// </summary>
        [Column(Name = "QuotaType")]
        public int QuotaType { get; set; } = 0;

        /// <summary>
        /// 默认值（未单独配置时的值）
        /// </summary>
        [Column(Name = "DefaultValue")]
        public int DefaultValue { get; set; } = 0;

        /// <summary>
        /// 最小值
        /// </summary>
        [Column(Name = "MinValue")]
        public int MinValue { get; set; } = 0;

        /// <summary>
        /// 最大值
        /// </summary>
        [Column(Name = "MaxValue")]
        public int MaxValue { get; set; } = 999999;

        /// <summary>
        /// 单位描述（个、MB、天）
        /// </summary>
        [Column(Name = "Unit", StringLength = 20)]
        public string Unit { get; set; } = "";

        /// <summary>
        /// 配额说明
        /// </summary>
        [Column(Name = "Description", StringLength = 500)]
        public string Description { get; set; } = "";

        /// <summary>
        /// 排序
        /// </summary>
        [Column(Name = "SortOrder")]
        public int SortOrder { get; set; } = 0;

        /// <summary>
        /// 是否启用
        /// </summary>
        [Column(Name = "IsEnabled")]
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 软删除
        /// </summary>
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
