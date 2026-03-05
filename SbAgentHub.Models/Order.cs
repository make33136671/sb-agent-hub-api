using FreeSql.DataAnnotations;

namespace SbAgentHub.Models
{
    /// <summary>
    /// 订单
    /// </summary>
    [Table(Name = "SB_Order")]
    public class Order : IEntity, ISoftDelete
    {
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
        /// 订单类型: 0=新购 1=续费 2=升级 3=降级
        /// </summary>
        [Column(Name = "OrderType")]
        public int OrderType { get; set; }

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
        /// 订单状态: 0=待支付 1=已支付 2=已取消 3=已退款
        /// </summary>
        [Column(Name = "Status")]
        public int Status { get; set; }

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

        [Column(Name = "CreateTime")]
        public DateTime CreateTime { get; set; } = DateTime.Now;

        [Column(Name = "UpdateTime")]
        public DateTime? UpdateTime { get; set; }

        [Column(Name = "IsDeleted", IsNullable = false, InsertValueSql = "0")]
        public bool IsDeleted { get; set; }
    }

    /// <summary>
    /// 订单类型
    /// </summary>
    public static class OrderType
    {
        public const int NewPurchase = 0;
        public const int Renewal = 1;
        public const int Upgrade = 2;
        public const int Downgrade = 3;
    }

    /// <summary>
    /// 订单状态
    /// </summary>
    public static class OrderStatus
    {
        public const int Pending = 0;
        public const int Paid = 1;
        public const int Cancelled = 2;
        public const int Refunded = 3;
    }
}
