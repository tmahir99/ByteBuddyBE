using JwtAuthAspNet7WebAPI.Core.Dtos;
using JwtAuthAspNet7WebAPI.Core.Interfaces;
using JwtAuthAspNet7WebAPI.Core.OtherObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JwtAuthAspNet7WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SearchController : ControllerBase
    {
        private readonly ISearchService _searchService;
        private readonly ILogger<SearchController> _logger;

        public SearchController(ISearchService searchService, ILogger<SearchController> logger)
        {
            _searchService = searchService ?? throw new ArgumentNullException(nameof(searchService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Search code snippets with advanced filtering and pagination
        /// </summary>
        /// <param name="searchRequest">Search parameters</param>
        /// <returns>Search results with pagination</returns>
        [HttpPost]
        [Route("codesnippets")]
        [Authorize(Roles = StaticUserRoles.USER)]
        [ProducesResponseType(typeof(SearchResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> SearchCodeSnippets([FromBody] SearchRequestDto searchRequest)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid search request model state");
                    return BadRequest(ModelState);
                }

                if (searchRequest == null)
                {
                    _logger.LogWarning("Search request is null");
                    return BadRequest(new { message = "Search request is required" });
                }

                _logger.LogInformation("Search request received: Query={Query}, Language={Language}, Tags={Tags}", 
                    searchRequest.Query, searchRequest.ProgrammingLanguage, string.Join(",", searchRequest.Tags ?? new List<string>()));

                var result = await _searchService.SearchCodeSnippetsAsync(searchRequest);

                _logger.LogInformation("Search completed: {TotalCount} results found", result.TotalCount);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during code snippet search");
                return StatusCode(500, new { message = "An error occurred during search", details = ex.Message });
            }
        }

        /// <summary>
        /// Get search suggestions based on query
        /// </summary>
        /// <param name="query">Search query for suggestions</param>
        /// <returns>Search suggestions</returns>
        [HttpGet]
        [Route("suggestions")]
        [Authorize(Roles = StaticUserRoles.USER)]
        [ProducesResponseType(typeof(SearchSuggestionsDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetSearchSuggestions([FromQuery] string query = "")
        {
            try
            {
                _logger.LogInformation("Getting search suggestions for query: {Query}", query);

                var suggestions = await _searchService.GetSearchSuggestionsAsync(query);

                _logger.LogInformation("Retrieved suggestions: {TagCount} tags, {LanguageCount} languages, {UserCount} users", 
                    suggestions.Tags.Count, suggestions.ProgrammingLanguages.Count, suggestions.Users.Count);

                return Ok(suggestions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting search suggestions for query: {Query}", query);
                return StatusCode(500, new { message = "An error occurred while getting search suggestions", details = ex.Message });
            }
        }

        /// <summary>
        /// Get popular tags
        /// </summary>
        /// <param name="count">Number of popular tags to return</param>
        /// <returns>List of popular tags</returns>
        [HttpGet]
        [Route("popular-tags")]
        [Authorize(Roles = StaticUserRoles.USER)]
        [ProducesResponseType(typeof(List<string>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetPopularTags([FromQuery] int count = 10)
        {
            try
            {
                if (count <= 0 || count > 50)
                {
                    _logger.LogWarning("Invalid count parameter: {Count}", count);
                    return BadRequest(new { message = "Count must be between 1 and 50" });
                }

                _logger.LogInformation("Getting {Count} popular tags", count);

                var popularTags = await _searchService.GetPopularTagsAsync(count);

                _logger.LogInformation("Retrieved {Count} popular tags", popularTags.Count);
                return Ok(popularTags);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting popular tags");
                return StatusCode(500, new { message = "An error occurred while getting popular tags", details = ex.Message });
            }
        }

        /// <summary>
        /// Get all programming languages used in code snippets
        /// </summary>
        /// <returns>List of programming languages</returns>
        [HttpGet]
        [Route("programming-languages")]
        [Authorize(Roles = StaticUserRoles.USER)]
        [ProducesResponseType(typeof(List<string>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetProgrammingLanguages()
        {
            try
            {
                _logger.LogInformation("Getting all programming languages");

                var languages = await _searchService.GetProgrammingLanguagesAsync();

                _logger.LogInformation("Retrieved {Count} programming languages", languages.Count);
                return Ok(languages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting programming languages");
                return StatusCode(500, new { message = "An error occurred while getting programming languages", details = ex.Message });
            }
        }

        /// <summary>
        /// Get active users with most code snippets
        /// </summary>
        /// <param name="count">Number of active users to return</param>
        /// <returns>List of active users</returns>
        [HttpGet]
        [Route("active-users")]
        [Authorize(Roles = StaticUserRoles.USER)]
        [ProducesResponseType(typeof(List<ApplicationUserDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetActiveUsers([FromQuery] int count = 10)
        {
            try
            {
                if (count <= 0 || count > 50)
                {
                    _logger.LogWarning("Invalid count parameter: {Count}", count);
                    return BadRequest(new { message = "Count must be between 1 and 50" });
                }

                _logger.LogInformation("Getting {Count} active users", count);

                var activeUsers = await _searchService.GetActiveUsersAsync(count);

                _logger.LogInformation("Retrieved {Count} active users", activeUsers.Count);
                return Ok(activeUsers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active users");
                return StatusCode(500, new { message = "An error occurred while getting active users", details = ex.Message });
            }
        }

        /// <summary>
        /// Quick search endpoint for simple text queries
        /// </summary>
        /// <param name="q">Search query</param>
        /// <param name="limit">Maximum number of results</param>
        /// <returns>Quick search results</returns>
        [HttpGet]
        [Route("quick")]
        [Authorize(Roles = StaticUserRoles.USER)]
        [ProducesResponseType(typeof(SearchResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> QuickSearch([FromQuery] string q, [FromQuery] int limit = 5)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(q))
                {
                    return BadRequest(new { message = "Search query is required" });
                }

                if (limit <= 0 || limit > 20)
                {
                    limit = 5;
                }

                _logger.LogInformation("Quick search for: {Query}, limit: {Limit}", q, limit);

                var searchRequest = new SearchRequestDto
                {
                    Query = q,
                    PageSize = limit,
                    PageNumber = 1,
                    SortBy = "CreatedAt",
                    SortOrder = "desc"
                };

                var result = await _searchService.SearchCodeSnippetsAsync(searchRequest);

                _logger.LogInformation("Quick search completed: {ResultCount} results", result.Results.Count);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during quick search for query: {Query}", q);
                return StatusCode(500, new { message = "An error occurred during quick search", details = ex.Message });
            }
        }
    }
}
