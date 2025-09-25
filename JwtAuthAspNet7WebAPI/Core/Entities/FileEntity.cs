using System;
using System.ComponentModel.DataAnnotations;

namespace JwtAuthAspNet7WebAPI.Core.Entities
{
    /// <summary>
    /// Represents a file uploaded to the system.
    /// </summary>
    public class FileEntity
    {
        [Key]
        public long FileId { get; set; }
        [Required]
        public string FileName { get; set; }
        [Required]
        public string FileUrl { get; set; }
        public long FileSize { get; set; }
        public string ContentType { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        public string UploadedById { get; set; }
    }
}
