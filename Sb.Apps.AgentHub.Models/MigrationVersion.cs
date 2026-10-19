using FreeSql.DataAnnotations;
using System;

namespace Sb.Apps.AgentHub.Models
{
    /// <summary>
    /// 数据库迁移版本记录
    /// </summary>
    [Table(Name = "sb_agenthub_migration_version")]
    [Index("ix_migrationversion_version", "Version", true)]
    public class MigrationVersion : IEntity
    {
        /// <summary>主键Id</summary>
        [Column(Name = "Id", IsPrimary = true, IsIdentity = true)]
        public int Id { get; set; }

        /// <summary>
        /// 迁移版本号（如 20260820_01）
        /// </summary>
        [Column(Name = "Version", StringLength = 50, IsNullable = false)]
        public required string Version { get; set; }

        /// <summary>
        /// 迁移描述
        /// </summary>
        [Column(Name = "Description", StringLength = 200)]
        public string Description { get; set; } = null!;

        /// <summary>
        /// 执行时间
        /// </summary>
        [Column(Name = "AppliedAt", DbType = "timestamp", InsertValueSql = "now()")]
        public DateTime AppliedAt { get; set; }
    }
}
