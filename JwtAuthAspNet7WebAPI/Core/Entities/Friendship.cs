using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JwtAuthAspNet7WebAPI.Core.Entities
{
    public class Friendship
    {
        [Key, Column(Order = 0)]
        [Required]
        [StringLength(450)]
        public string RequesterId { get; set; }
        
        [ForeignKey("RequesterId")]
        public virtual ApplicationUser Requester { get; set; }
        
        [Key, Column(Order = 1)]
        [Required]
        [StringLength(450)]
        public string AddresseeId { get; set; }
        
        [ForeignKey("AddresseeId")]
        public virtual ApplicationUser Addressee { get; set; }
        
        [Required]
        public FriendshipStatus Status { get; set; }
        
        [Required]
        public DateTime CreatedAt { get; set; }
        
        public DateTime? UpdatedAt { get; set; }
    }
}
