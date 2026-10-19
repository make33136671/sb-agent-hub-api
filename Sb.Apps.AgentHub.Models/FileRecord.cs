using FreeSql.DataAnnotations;
using System;

namespace Sb.Apps.AgentHub.Models
{
    /// <summary>
    /// 文件上传记录（用于存储配额追踪）
    /// </summary>
    [Table(Name = "sb_agenthub_file_record")]
    [Index("ix_filerecord_ownerid_platformid", "OwnerId,PlatformId")]
    [Index("ix_filerecord_uploaderid", "UploaderId")]
    public class FileRecord : IEntity, ISoftDelete
    {
        /// <summary>主键Id</summary>
        [Column(IsPrimary = true, IsIdentity = true)]
        public int Id { get; set; }

        /// <summary>所属站长ID</summary>
        public int OwnerId { get; set; }

        /// <summary>所属平台ID</summary>
        public int PlatformId { get; set; }

        /// <summary>上传者ID</summary>
        [Column(StringLength = 64)]
        public string UploaderId { get; set; } = null!;

        /// <summary>上传者类型: visitor/agent</summary>
        [Column(StringLength = 10)]
        public string UploaderType { get; set; } = "visitor";

        /// <summary>原始文件名</summary>
        [Column(StringLength = 200)]
        public string FileName { get; set; } = null!;

        /// <summary>存储路径</summary>
        [Column(StringLength = 500)]
        public string StoredPath { get; set; } = null!;

        /// <summary>文件大小(字节)</summary>
        public long FileSize { get; set; }

        /// <summary>文件类型</summary>
        [Column(StringLength = 20)]
        public string FileType { get; set; } = null!;

        /// <summary>MIME类型</summary>
        [Column(StringLength = 100)]
        public string MimeType { get; set; } = null!;

        /// <summary>创建时间</summary>
        [Column(Name = "CreateTime", DbType = "timestamp", InsertValueSql = "now()")]
        public DateTime CreateTime { get; set; }

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
