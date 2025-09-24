using System.ComponentModel.DataAnnotations;

namespace JwtAuthAspNet7WebAPI.Core.Dtos
{
    /// <summary>
    /// Data transfer object for a page.
    /// </summary>
    public class PageDto
    {
        public long Id { get; set; }
        
        [Required]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; }
        
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string Description { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public string CreatedById { get; set; }
        
        public ApplicationUserDto CreatedBy { get; set; }
        
        public int LikesCount { get; set; }
        
        public bool IsLikedByCurrentUser { get; set; }
    }

    public class CreatePageDto
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; }
        
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string Description { get; set; }
        
        [Required]
        public string CreatedById { get; set; }
    }

    public class UpdatePageDto
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; }
        
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string Description { get; set; }
        
        [Required]
        public string CreatedById { get; set; }
    }

    public class PageFilterDto
    {
        public string SearchTerm { get; set; }
        public string CreatedById { get; set; }
        public DateTime? CreatedAfter { get; set; }
        public DateTime? CreatedBefore { get; set; }
        public string SortBy { get; set; } = "latest"; // latest, popular, title
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class PaginatedResult<T>
    {
        public List<T> Items { get; set; }
        public int TotalCount { get; set; }
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
    }
}
