using AutoMapper;
using JwtAuthAspNet7WebAPI.Core.DbContext;
using JwtAuthAspNet7WebAPI.Core.Dtos;
using JwtAuthAspNet7WebAPI.Core.Entities;
using JwtAuthAspNet7WebAPI.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JwtAuthAspNet7WebAPI.Core.Services
{
    public class SearchService : ISearchService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<SearchService> _logger;

        public SearchService(ApplicationDbContext context, IMapper mapper, ILogger<SearchService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<SearchResponseDto> SearchCodeSnippetsAsync(SearchRequestDto searchRequest)
        {
            try
            {
                if (searchRequest == null)
                {
                    _logger.LogWarning("SearchCodeSnippetsAsync called with null searchRequest");
                    throw new ArgumentNullException(nameof(searchRequest));
                }

                _logger.LogInformation("Searching code snippets with query: {Query}", searchRequest.Query);

                var query = _context.CodeSnippets
                    .Include(cs => cs.CreatedBy)
                    .Include(cs => cs.Tags)
                    .Include(cs => cs.Likes)
                    .Include(cs => cs.Comments)
                    .AsQueryable();

                var appliedFilters = new List<string>();

                // Text search in title, description, and code content
                if (!string.IsNullOrWhiteSpace(searchRequest.Query))
                {
                    var searchTerm = searchRequest.Query.Trim().ToLower();
                    query = query.Where(cs => 
                        cs.Title.ToLower().Contains(searchTerm) ||
                        cs.Description.ToLower().Contains(searchTerm) ||
                        cs.CodeContent.ToLower().Contains(searchTerm));
                    appliedFilters.Add($"Query: {searchRequest.Query}");
                }

                // Filter by programming language
                if (!string.IsNullOrWhiteSpace(searchRequest.ProgrammingLanguage))
                {
                    query = query.Where(cs => cs.ProgrammingLanguage.ToLower() == searchRequest.ProgrammingLanguage.ToLower());
                    appliedFilters.Add($"Language: {searchRequest.ProgrammingLanguage}");
                }

                // Filter by tags
                if (searchRequest.Tags != null && searchRequest.Tags.Any())
                {
                    var tagNames = searchRequest.Tags.Select(t => t.ToLower()).ToList();
                    query = query.Where(cs => cs.Tags.Any(t => tagNames.Contains(t.Name.ToLower())));
                    appliedFilters.Add($"Tags: {string.Join(", ", searchRequest.Tags)}");
                }

                // Filter by user ID
                if (!string.IsNullOrWhiteSpace(searchRequest.UserId))
                {
                    query = query.Where(cs => cs.CreatedById == searchRequest.UserId);
                    appliedFilters.Add($"User ID: {searchRequest.UserId}");
                }

                // Filter by username
                if (!string.IsNullOrWhiteSpace(searchRequest.UserName))
                {
                    query = query.Where(cs => cs.CreatedBy.UserName.ToLower().Contains(searchRequest.UserName.ToLower()));
                    appliedFilters.Add($"Username: {searchRequest.UserName}");
                }

                // Filter by creation date
                if (searchRequest.CreatedAfter.HasValue)
                {
                    query = query.Where(cs => cs.CreatedAt >= searchRequest.CreatedAfter.Value);
                    appliedFilters.Add($"Created after: {searchRequest.CreatedAfter.Value:yyyy-MM-dd}");
                }

                if (searchRequest.CreatedBefore.HasValue)
                {
                    query = query.Where(cs => cs.CreatedAt <= searchRequest.CreatedBefore.Value);
                    appliedFilters.Add($"Created before: {searchRequest.CreatedBefore.Value:yyyy-MM-dd}");
                }

                // Filter by file attachment
                if (searchRequest.HasFile)
                {
                    query = query.Where(cs => !string.IsNullOrEmpty(cs.FileUrl));
                    appliedFilters.Add("Has file attachment");
                }

                // Get total count before pagination
                var totalCount = await query.CountAsync();

                // Apply sorting
                query = ApplySorting(query, searchRequest.SortBy, searchRequest.SortOrder);

                // Apply pagination
                var skip = (searchRequest.PageNumber - 1) * searchRequest.PageSize;
                var codeSnippets = await query
                    .Skip(skip)
                    .Take(searchRequest.PageSize)
                    .ToListAsync();

                // Map to DTOs
                var codeSnippetDtos = _mapper.Map<List<CodeSnippetDto>>(codeSnippets);

                var totalPages = (int)Math.Ceiling((double)totalCount / searchRequest.PageSize);

                _logger.LogInformation("Search completed. Found {TotalCount} results, returning page {PageNumber} of {TotalPages}", 
                    totalCount, searchRequest.PageNumber, totalPages);

                return new SearchResponseDto
                {
                    Results = codeSnippetDtos,
                    TotalCount = totalCount,
                    PageNumber = searchRequest.PageNumber,
                    PageSize = searchRequest.PageSize,
                    TotalPages = totalPages,
                    HasNextPage = searchRequest.PageNumber < totalPages,
                    HasPreviousPage = searchRequest.PageNumber > 1,
                    Query = searchRequest.Query,
                    AppliedFilters = appliedFilters
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching code snippets with query: {Query}", searchRequest?.Query);
                throw;
            }
        }

        public async Task<SearchSuggestionsDto> GetSearchSuggestionsAsync(string query)
        {
            try
            {
                _logger.LogInformation("Getting search suggestions for query: {Query}", query);

                var suggestions = new SearchSuggestionsDto();

                if (string.IsNullOrWhiteSpace(query))
                {
                    // Return popular items if no query
                    suggestions.Tags = await GetPopularTagsAsync(5);
                    suggestions.ProgrammingLanguages = await GetProgrammingLanguagesAsync();
                    var activeUsers = await GetActiveUsersAsync(5);
                    suggestions.Users = activeUsers.Select(u => u.UserName).ToList();
                }
                else
                {
                    var searchTerm = query.Trim().ToLower();

                    // Get matching tags
                    suggestions.Tags = await _context.Tags
                        .Where(t => t.Name.ToLower().Contains(searchTerm))
                        .Select(t => t.Name)
                        .Take(5)
                        .ToListAsync();

                    // Get matching programming languages
                    suggestions.ProgrammingLanguages = await _context.CodeSnippets
                        .Where(cs => cs.ProgrammingLanguage.ToLower().Contains(searchTerm))
                        .Select(cs => cs.ProgrammingLanguage)
                        .Distinct()
                        .Take(5)
                        .ToListAsync();

                    // Get matching users
                    suggestions.Users = await _context.Users
                        .Where(u => u.UserName.ToLower().Contains(searchTerm) || 
                                   u.FirstName.ToLower().Contains(searchTerm) || 
                                   u.LastName.ToLower().Contains(searchTerm))
                        .Select(u => u.UserName)
                        .Take(5)
                        .ToListAsync();
                }

                _logger.LogInformation("Retrieved {TagCount} tag suggestions, {LanguageCount} language suggestions, {UserCount} user suggestions", 
                    suggestions.Tags.Count, suggestions.ProgrammingLanguages.Count, suggestions.Users.Count);

                return suggestions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting search suggestions for query: {Query}", query);
                throw;
            }
        }

        public async Task<List<string>> GetPopularTagsAsync(int count = 10)
        {
            try
            {
                _logger.LogInformation("Getting {Count} popular tags", count);

                var popularTags = await _context.Tags
                    .Include(t => t.CodeSnippets)
                    .OrderByDescending(t => t.CodeSnippets.Count)
                    .Take(count)
                    .Select(t => t.Name)
                    .ToListAsync();

                _logger.LogInformation("Retrieved {Count} popular tags", popularTags.Count);
                return popularTags;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting popular tags");
                throw;
            }
        }

        public async Task<List<string>> GetProgrammingLanguagesAsync()
        {
            try
            {
                _logger.LogInformation("Getting all programming languages");

                var languages = await _context.CodeSnippets
                    .Select(cs => cs.ProgrammingLanguage)
                    .Distinct()
                    .OrderBy(lang => lang)
                    .ToListAsync();

                _logger.LogInformation("Retrieved {Count} programming languages", languages.Count);
                return languages;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting programming languages");
                throw;
            }
        }

        public async Task<List<ApplicationUserDto>> GetActiveUsersAsync(int count = 10)
        {
            try
            {
                _logger.LogInformation("Getting {Count} active users", count);

                var activeUsers = await _context.Users
                    .Include(u => u.CodeSnippets)
                    .OrderByDescending(u => u.CodeSnippets.Count)
                    .ThenByDescending(u => u.LastLoginAt)
                    .Take(count)
                    .ToListAsync();

                var userDtos = _mapper.Map<List<ApplicationUserDto>>(activeUsers);

                _logger.LogInformation("Retrieved {Count} active users", userDtos.Count);
                return userDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active users");
                throw;
            }
        }

        private IQueryable<CodeSnippet> ApplySorting(IQueryable<CodeSnippet> query, string sortBy, string sortOrder)
        {
            var isDescending = sortOrder?.ToLower() == "desc";

            return sortBy?.ToLower() switch
            {
                "title" => isDescending 
                    ? query.OrderByDescending(cs => cs.Title)
                    : query.OrderBy(cs => cs.Title),
                "likes" => isDescending
                    ? query.OrderByDescending(cs => cs.Likes.Count)
                    : query.OrderBy(cs => cs.Likes.Count),
                "comments" => isDescending
                    ? query.OrderByDescending(cs => cs.Comments.Count)
                    : query.OrderBy(cs => cs.Comments.Count),
                "language" => isDescending
                    ? query.OrderByDescending(cs => cs.ProgrammingLanguage)
                    : query.OrderBy(cs => cs.ProgrammingLanguage),
                "createdat" or _ => isDescending
                    ? query.OrderByDescending(cs => cs.CreatedAt)
                    : query.OrderBy(cs => cs.CreatedAt)
            };
        }
    }
}
