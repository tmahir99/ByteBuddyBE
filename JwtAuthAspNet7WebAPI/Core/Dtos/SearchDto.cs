using System.ComponentModel.DataAnnotations;

namespace JwtAuthAspNet7WebAPI.Core.Dtos
{
    public class SearchRequestDto
    {
        [StringLength(200)]
        public string Query { get; set; }
        
        [StringLength(100)]
        public string ProgrammingLanguage { get; set; }
        
        public List<string> Tags { get; set; } = new List<string>();
        
        [StringLength(450)]
        public string UserId { get; set; }
        
        [StringLength(100)]
        public string UserName { get; set; }
        
        public DateTime? CreatedAfter { get; set; }
        
        public DateTime? CreatedBefore { get; set; }
        
        public bool HasFile { get; set; } = false;
        
        public string SortBy { get; set; } = "CreatedAt"; // CreatedAt, Title, Likes
        
        public string SortOrder { get; set; } = "desc"; // asc, desc
        
        [Range(1, 100)]
        public int PageSize { get; set; } = 10;
        
        [Range(1, int.MaxValue)]
        public int PageNumber { get; set; } = 1;
    }

    public class SearchResponseDto
    {
        public List<CodeSnippetDto> Results { get; set; } = new List<CodeSnippetDto>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
        public string Query { get; set; }
        public List<string> AppliedFilters { get; set; } = new List<string>();
    }

    public class SearchSuggestionsDto
    {
        public List<string> Tags { get; set; } = new List<string>();
        public List<string> ProgrammingLanguages { get; set; } = new List<string>();
        public List<string> Users { get; set; } = new List<string>();
    }
}
