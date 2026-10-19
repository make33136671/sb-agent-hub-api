using System;

namespace Sb.Apps.AgentHub.Models
{
    /// <summary>
    /// 软删除接口 - 实现此接口的实体将自动应用全局软删除过滤
    /// </summary>
    public interface ISoftDelete
    {
        /// <summary>软删除标记</summary>
        bool IsDeleted { get; set; }
        /// <summary>删除时间</summary>
        DateTime? DeletedAt { get; set; }
        /// <summary>删除人Id</summary>
        int? DeletedBy { get; set; }
    }
}
