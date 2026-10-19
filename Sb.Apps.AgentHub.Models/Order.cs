using FreeSql.DataAnnotations;

namespace Sb.Apps.AgentHub.Models
{
    /// <summary>
    /// 订单
    /// </summary>
    [Table(Name = "sb_agenthub_order")]
    [Index("ix_order_orderno", "OrderNo", true)]
    [Index("ix_order_memberid", "MemberId")]
    [Index("ix_order_status", "Status")]
    [Index("ix_order_createtime", "CreateTime")]
    public class Order : IEntity, ISoftDelete
    {
        /// <summary>
        /// 主键Id
        /// </summary>
        [Column(Name = "Id", IsPrimary = true, IsIdentity = true)]
        public int Id { get; set; }

        /// <summary>
        /// 订单号
        /// </summary>
        [Column(Name = "OrderNo", StringLength = 50)]
        public string OrderNo { get; set; } = "";

        /// <summary>
        /// 站长Id
        /// </summary>
        [Column(Name = "MemberId")]
        public int MemberId { get; set; }

        /// <summary>
        /// 套餐Id
        /// </summary>
        [Column(Name = "PackageId")]
        public int PackageId { get; set; }

        /// <summary>
        /// 套餐名称（冗余）
        /// </summary>
        [Column(Name = "PackageName", StringLength = 100)]
        public string PackageName { get; set; } = "";

        /// <summary>
        /// 订单类型
        /// </summary>
        [Column(Name = "OrderType", MapType = typeof(string))]
        public OrderType OrderType { get; set; }

        /// <summary>
        /// 订单金额
        /// </summary>
        [Column(Name = "Amount", DbType = "DECIMAL(10,2)")]
        public decimal Amount { get; set; }

        /// <summary>
        /// 货币
        /// </summary>
        [Column(Name = "Currency", StringLength = 10)]
        public string Currency { get; set; } = "CNY";

        /// <summary>
        /// 订单状态
        /// </summary>
        [Column(Name = "Status", MapType = typeof(string))]
        public OrderStatus Status { get; set; }

        /// <summary>
        /// 支付方式: alipay/wechat/manual
        /// </summary>
        [Column(Name = "PaymentMethod", StringLength = 20)]
        public string PaymentMethod { get; set; } = "";

        /// <summary>
        /// 支付时间
        /// </summary>
        [Column(Name = "PaymentTime")]
        public DateTime? PaymentTime { get; set; }

        /// <summary>
        /// 第三方支付交易号
        /// </summary>
        [Column(Name = "PaymentTransactionId", StringLength = 100)]
        public string PaymentTransactionId { get; set; } = "";

        /// <summary>
        /// 生效开始时间
        /// </summary>
        [Column(Name = "EffectiveStartTime")]
        public DateTime? EffectiveStartTime { get; set; }

        /// <summary>
        /// 生效结束时间
        /// </summary>
        [Column(Name = "EffectiveEndTime")]
        public DateTime? EffectiveEndTime { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [Column(Name = "Remark", StringLength = 500)]
        public string Remark { get; set; } = "";

        /// <summary>
        /// 创建时间
        /// </summary>
        [Column(Name = "CreateTime", DbType = "timestamp", InsertValueSql = "now()")]
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        [Column(Name = "UpdateTime")]
        public DateTime? UpdateTime { get; set; }

        /// <summary>
        /// 软删除标记
        /// </summary>
        [Column(Name = "IsDeleted", IsNullable = false, InsertValueSql = "false")]
        public bool IsDeleted { get; set; }

        /// <summary>
        /// 删除时间
        /// </summary>
        [Column(Name = "DeletedAt", IsNullable = true)]
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// 删除人Id
        /// </summary>
        [Column(Name = "DeletedBy", IsNullable = true)]
        public int? DeletedBy { get; set; }
    }

    /// <summary>
    /// 订单类型
    /// </summary>
    public enum OrderType
    {
        /// <summary>新购</summary>
        NewPurchase = 0,
        /// <summary>续费</summary>
        Renewal = 1,
        /// <summary>升级</summary>
        Upgrade = 2,
        /// <summary>降级</summary>
        Downgrade = 3
    }

    /// <summary>
    /// 订单状态
    /// </summary>
    public enum OrderStatus
    {
        /// <summary>待支付</summary>
        Pending = 0,
        /// <summary>已支付</summary>
        Paid = 1,
        /// <summary>已取消</summary>
        Cancelled = 2,
        /// <summary>已退款</summary>
        Refunded = 3
    }
}
