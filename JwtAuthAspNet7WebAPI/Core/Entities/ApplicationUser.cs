using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JwtAuthAspNet7WebAPI.Core.Entities
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }
        
        [Required]
        [StringLength(50)]
        public string LastName { get; set; }
        
        [Required]
        public DateTime DateOfBirth { get; set; }
        
        [Required]
        [StringLength(10)]
        public string Gender { get; set; }
        
        [Required]
        [StringLength(100)]
        public string BirthPlace { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Address { get; set; }
        
        // Email Activation Fields
        public bool IsEmailConfirmed { get; set; } = false;
        
        [StringLength(500)]
        public string? EmailConfirmationToken { get; set; }
        
        public DateTime? EmailConfirmationTokenExpiry { get; set; }
        
        [StringLength(500)]
        public string? PasswordResetToken { get; set; }
        
        public DateTime? PasswordResetTokenExpiry { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? LastLoginAt { get; set; }
        
        [NotMapped]
        public List<string> Roles { get; set; } = new List<string>();
        
        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";

        // Navigation Properties
        public virtual ICollection<CodeSnippet> CodeSnippets { get; set; } = new List<CodeSnippet>();
        public virtual ICollection<Like> Likes { get; set; } = new List<Like>();
        public virtual ICollection<Page> Pages { get; set; } = new List<Page>();
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
        
        [InverseProperty("Requester")]
        public virtual ICollection<Friendship> FriendshipsRequested { get; set; } = new List<Friendship>();
        
        [InverseProperty("Addressee")]
        public virtual ICollection<Friendship> FriendshipsReceived { get; set; } = new List<Friendship>();
        
        [InverseProperty("Sender")]
        public virtual ICollection<Message> MessagesSent { get; set; } = new List<Message>();
        
        [InverseProperty("Receiver")]
        public virtual ICollection<Message> MessagesReceived { get; set; } = new List<Message>();
    }
}
