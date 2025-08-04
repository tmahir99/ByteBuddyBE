using JwtAuthAspNet7WebAPI.Core.Dtos;

namespace JwtAuthAspNet7WebAPI.Core.Interfaces
{
    public interface ISearchService
    {
        Task<SearchResponseDto> SearchCodeSnippetsAsync(SearchRequestDto searchRequest);
        Task<SearchSuggestionsDto> GetSearchSuggestionsAsync(string query);
        Task<List<string>> GetPopularTagsAsync(int count = 10);
        Task<List<string>> GetProgrammingLanguagesAsync();
        Task<List<ApplicationUserDto>> GetActiveUsersAsync(int count = 10);
    }
}
