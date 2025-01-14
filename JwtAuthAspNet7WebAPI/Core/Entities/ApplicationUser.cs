using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace JwtAuthAspNet7WebAPI.Core.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string BirthPlace { get; set; }
        public string Address { get; set; }
        public List<string> Roles { get; set; }

        public ICollection<CodeSnippet> CodeSnippets { get; set; }
        public ICollection<Like> Likes { get; set; }
        public ICollection<Page> Pages { get; set; }
        public virtual ICollection<Friendship> FriendshipsRequested { get; set; }
        public virtual ICollection<Friendship> FriendshipsReceived { get; set; }
        public virtual ICollection<Message> MessagesSent { get; set; }
        public virtual ICollection<Message> MessagesReceived { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
    }
}
