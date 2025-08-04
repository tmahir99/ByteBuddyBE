using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JwtAuthAspNet7WebAPI.Core.Entities
{
    public class Message
    {
        [Key]
        public long Id { get; set; }
        
        [Required]
        [StringLength(450)]
        public string SenderId { get; set; }
        
        [ForeignKey("SenderId")]
        public virtual ApplicationUser Sender { get; set; }
        
        [Required]
        [StringLength(450)]
        public string ReceiverId { get; set; }
        
        [ForeignKey("ReceiverId")]
        public virtual ApplicationUser Receiver { get; set; }
        
        [Required]
        [StringLength(2000)]
        public string Content { get; set; }
        
        [Required]
        public DateTime SentAt { get; set; }
        
        public bool IsRead { get; set; } = false;
        
        public DateTime? ReadAt { get; set; }
    }
}
