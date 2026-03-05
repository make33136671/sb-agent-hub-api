using FreeSql.DataAnnotations;
using System;

namespace SbAgentHub.Models
{
    /// <summary>
    /// 文件上传记录（用于存储配额追踪）
    /// </summary>
    [Table(Name = "SB_FileRecord")]
    public class FileRecord : IEntity, ISoftDelete
    {
        [Column(IsPrimary = true, IsIdentity = true)]
        public int Id { get; set; }

        /// <summary>所属站长ID</summary>
        public int OwnerId { get; set; }

        /// <summary>所属平台ID</summary>
        public int PlatformId { get; set; }

        /// <summary>上传者ID</summary>
        [Column(StringLength = 64)]
        public string UploaderId { get; set; }

        /// <summary>上传者类型: visitor/agent</summary>
        [Column(StringLength = 10)]
        public string UploaderType { get; set; } = "visitor";

        /// <summary>原始文件名</summary>
        [Column(StringLength = 200)]
        public string FileName { get; set; }

        /// <summary>存储路径</summary>
        [Column(StringLength = 500)]
        public string StoredPath { get; set; }

        /// <summary>文件大小(字节)</summary>
        public long FileSize { get; set; }

        /// <summary>文件类型</summary>
        [Column(StringLength = 20)]
        public string FileType { get; set; }

        /// <summary>MIME类型</summary>
        [Column(StringLength = 100)]
        public string MimeType { get; set; }

        public DateTime CreateTime { get; set; }

        public bool IsDeleted { get; set; }
    }
}
