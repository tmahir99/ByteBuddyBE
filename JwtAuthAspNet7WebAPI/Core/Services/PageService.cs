using JwtAuthAspNet7WebAPI.Core.DbContext;
using JwtAuthAspNet7WebAPI.Core.Dtos;
using JwtAuthAspNet7WebAPI.Core.Entities;
using JwtAuthAspNet7WebAPI.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JwtAuthAspNet7WebAPI.Core.Services
{
    public class PageService : IPageService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PageService> _logger;

        public PageService(ApplicationDbContext context, ILogger<PageService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<PageDto> CreateAsync(CreatePageDto dto)
        {
            try
            {
                if (dto == null)
                {
                    _logger.LogWarning("CreateAsync called with null dto");
                    throw new ArgumentNullException(nameof(dto));
                }

                if (string.IsNullOrWhiteSpace(dto.Title))
                {
                    _logger.LogWarning("CreateAsync called with empty title");
                    throw new ArgumentException("Title is required", nameof(dto));
                }

                if (string.IsNullOrWhiteSpace(dto.CreatedById))
                {
                    _logger.LogWarning("CreateAsync called with empty CreatedById");
                    throw new ArgumentException("CreatedById is required", nameof(dto));
                }

                _logger.LogInformation("Creating page with title: {Title} by user: {UserId}", dto.Title, dto.CreatedById);

                // Validate user exists
                var user = await _context.Users.FindAsync(dto.CreatedById);
                if (user == null)
                {
                    _logger.LogWarning("User not found with ID: {UserId}", dto.CreatedById);
                    throw new KeyNotFoundException($"User with ID {dto.CreatedById} not found");
                }

                var page = new Page
                {
                    Title = dto.Title.Trim(),
                    Description = dto.Description?.Trim(),
                    CreatedAt = DateTime.UtcNow,
                    CreatedById = dto.CreatedById
                };

                _context.Pages.Add(page);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Page created successfully with ID: {PageId}", page.Id);
                return await GetByIdAsync(page.Id, dto.CreatedById);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating page with title: {Title}", dto?.Title);
                throw;
            }
        }

        public async Task<PageDto> GetByIdAsync(long id, string currentUserId = null)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning("GetByIdAsync called with invalid ID: {Id}", id);
                    throw new ArgumentException("ID must be greater than 0", nameof(id));
                }

                _logger.LogInformation("Getting page by ID: {Id}", id);

                var page = await _context.Pages
                    .Include(p => p.CreatedBy)
                    .Include(p => p.Likes)
                    .Include(p => p.File)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (page == null)
                {
                    _logger.LogWarning("Page not found with ID: {Id}", id);
                    throw new KeyNotFoundException($"Page with ID {id} not found");
                }

                var dto = new PageDto
                {
                    Id = page.Id,
                    Title = page.Title,
                    Description = page.Description,
                    CreatedAt = page.CreatedAt,
                    CreatedById = page.CreatedById,
                    CreatedBy = new ApplicationUserDto
                    {
                        Id = page.CreatedBy.Id,
                        UserName = page.CreatedBy.UserName,
                        FirstName = page.CreatedBy.FirstName,
                        LastName = page.CreatedBy.LastName,
                        Email = page.CreatedBy.Email
                    },
                    LikesCount = page.Likes?.Count ?? 0,
                    IsLikedByCurrentUser = page.Likes?.Any(l => l.UserId == currentUserId) ?? false,
                    FileId = page.FileId,
                    FileUrl = page.File?.FileUrl
                };

                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting page by ID: {Id}", id);
                throw;
            }
        }

        public async Task<PaginatedResult<PageDto>> GetAllAsync(PageFilterDto filter, string currentUserId = null)
        {
            try
            {
                if (filter == null)
                {
                    _logger.LogWarning("GetAllAsync called with null filter");
                    throw new ArgumentNullException(nameof(filter));
                }

                _logger.LogInformation("Getting pages with filter - Page: {Page}, PageSize: {PageSize}", filter.Page, filter.PageSize);
                ValidateFilterDto(filter);

                var query = BuildFilterQuery(filter);
                var total = await query.CountAsync();

                var items = await query
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToListAsync();

                var mappedItems = items.Select(p => MapToDto(p, currentUserId)).ToList();

                _logger.LogInformation("Retrieved {Count} pages out of {Total} total", mappedItems.Count, total);

                return new PaginatedResult<PageDto>
                {
                    Items = mappedItems,
                    TotalCount = total,
                    PageSize = filter.PageSize,
                    CurrentPage = filter.Page,
                    TotalPages = (int)Math.Ceiling(total / (double)filter.PageSize)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pages with filter");
                throw;
            }
        }

        public async Task UpdateAsync(long id, UpdatePageDto dto)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning("UpdateAsync called with invalid ID: {Id}", id);
                    throw new ArgumentException("ID must be greater than 0", nameof(id));
                }

                if (dto == null)
                {
                    _logger.LogWarning("UpdateAsync called with null dto");
                    throw new ArgumentNullException(nameof(dto));
                }

                _logger.LogInformation("Updating page with ID: {Id}", id);

                var page = await _context.Pages.FindAsync(id);

                if (page == null)
                {
                    _logger.LogWarning("Page not found for update with ID: {Id}", id);
                    throw new KeyNotFoundException($"Page with ID {id} not found");
                }

                if (page.CreatedById != dto.CreatedById)
                {
                    _logger.LogWarning("Unauthorized update attempt on page {Id} by user {UserId}", id, dto.CreatedById);
                    throw new UnauthorizedAccessException("You can only update your own pages");
                }

                page.Title = dto.Title.Trim();
                page.Description = dto.Description?.Trim();

                await _context.SaveChangesAsync();
                _logger.LogInformation("Page updated successfully with ID: {Id}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating page with ID: {Id}", id);
                throw;
            }
        }

        public async Task DeleteAsync(long id, string userId)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning("DeleteAsync called with invalid ID: {Id}", id);
                    throw new ArgumentException("ID must be greater than 0", nameof(id));
                }

                if (string.IsNullOrWhiteSpace(userId))
                {
                    _logger.LogWarning("DeleteAsync called with empty userId");
                    throw new ArgumentException("User ID is required", nameof(userId));
                }

                _logger.LogInformation("Deleting page with ID: {Id} by user: {UserId}", id, userId);

                var page = await _context.Pages
                    .Include(p => p.Likes)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (page == null)
                {
                    _logger.LogWarning("Page not found for deletion with ID: {Id}", id);
                    throw new KeyNotFoundException($"Page with ID {id} not found");
                }

                if (page.CreatedById != userId)
                {
                    _logger.LogWarning("Unauthorized delete attempt on page {Id} by user {UserId}", id, userId);
                    throw new UnauthorizedAccessException("You can only delete your own pages");
                }

                // Remove related likes first
                _context.Likes.RemoveRange(page.Likes);
                _context.Pages.Remove(page);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Page deleted successfully with ID: {Id}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting page with ID: {Id}", id);
                throw;
            }
        }

        public async Task<List<PageDto>> GetUserPagesAsync(string userId, string currentUserId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    _logger.LogWarning("GetUserPagesAsync called with empty userId");
                    throw new ArgumentException("User ID is required", nameof(userId));
                }

                _logger.LogInformation("Getting pages for user: {UserId}", userId);

                var pages = await _context.Pages
                    .Include(p => p.CreatedBy)
                    .Include(p => p.Likes)
                    .Where(p => p.CreatedById == userId)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();

                var mappedPages = pages.Select(p => MapToDto(p, currentUserId)).ToList();

                _logger.LogInformation("Retrieved {Count} pages for user: {UserId}", mappedPages.Count, userId);
                return mappedPages;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pages for user: {UserId}", userId);
                throw;
            }
        }

        public async Task<PageDto> ToggleLikeAsync(long pageId, string userId)
        {
            try
            {
                if (pageId <= 0)
                {
                    _logger.LogWarning("ToggleLikeAsync called with invalid pageId: {PageId}", pageId);
                    throw new ArgumentException("Page ID must be greater than 0", nameof(pageId));
                }

                if (string.IsNullOrWhiteSpace(userId))
                {
                    _logger.LogWarning("ToggleLikeAsync called with empty userId");
                    throw new ArgumentException("User ID is required", nameof(userId));
                }

                _logger.LogInformation("Toggling like for page {PageId} by user {UserId}", pageId, userId);

                var page = await _context.Pages
                    .Include(p => p.Likes)
                    .FirstOrDefaultAsync(p => p.Id == pageId);

                if (page == null)
                {
                    _logger.LogWarning("Page not found for like toggle with ID: {PageId}", pageId);
                    throw new KeyNotFoundException($"Page with ID {pageId} not found");
                }

                var existingLike = await _context.Likes
                    .FirstOrDefaultAsync(l => l.PageId == pageId && l.UserId == userId);

                if (existingLike != null)
                {
                    // Unlike
                    _context.Likes.Remove(existingLike);
                    _logger.LogInformation("Like removed for page {PageId} by user {UserId}", pageId, userId);
                }
                else
                {
                    // Like
                    var like = new Like
                    {
                        PageId = pageId,
                        UserId = userId,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Likes.Add(like);
                    _logger.LogInformation("Like added for page {PageId} by user {UserId}", pageId, userId);
                }

                await _context.SaveChangesAsync();
                return await GetByIdAsync(pageId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling like for page {PageId} by user {UserId}", pageId, userId);
                throw;
            }
        }

        public async Task<List<ApplicationUserDto>> GetPageLikersAsync(long pageId)
        {
            try
            {
                if (pageId <= 0)
                {
                    _logger.LogWarning("GetPageLikersAsync called with invalid pageId: {PageId}", pageId);
                    throw new ArgumentException("Page ID must be greater than 0", nameof(pageId));
                }

                _logger.LogInformation("Getting likers for page: {PageId}", pageId);

                var likers = await _context.Likes
                    .Include(l => l.User)
                    .Where(l => l.PageId == pageId)
                    .Select(l => new ApplicationUserDto
                    {
                        Id = l.User.Id,
                        UserName = l.User.UserName,
                        FirstName = l.User.FirstName,
                        LastName = l.User.LastName,
                        Email = l.User.Email
                    })
                    .ToListAsync();

                _logger.LogInformation("Retrieved {Count} likers for page: {PageId}", likers.Count, pageId);
                return likers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting likers for page: {PageId}", pageId);
                throw;
            }
        }

        private IQueryable<Page> BuildFilterQuery(PageFilterDto filter)
        {
            var query = _context.Pages
                .Include(p => p.CreatedBy)
                .Include(p => p.Likes)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var searchTerm = filter.SearchTerm.ToLower();
                query = query.Where(p =>
                    p.Title.ToLower().Contains(searchTerm) ||
                    p.Description.ToLower().Contains(searchTerm)
                );
            }

            if (!string.IsNullOrWhiteSpace(filter.CreatedById))
            {
                query = query.Where(p => p.CreatedById == filter.CreatedById);
            }

            if (filter.CreatedAfter.HasValue)
            {
                query = query.Where(p => p.CreatedAt >= filter.CreatedAfter.Value);
            }

            if (filter.CreatedBefore.HasValue)
            {
                query = query.Where(p => p.CreatedAt <= filter.CreatedBefore.Value);
            }

            query = ApplySorting(query, filter.SortBy);

            return query;
        }

        private IQueryable<Page> ApplySorting(IQueryable<Page> query, string sortBy)
        {
            return sortBy?.ToLower() switch
            {
                "latest" => query.OrderByDescending(p => p.CreatedAt),
                "popular" => query.OrderByDescending(p => p.Likes.Count),
                "title" => query.OrderBy(p => p.Title),
                _ => query.OrderByDescending(p => p.CreatedAt)
            };
        }

        private void ValidateFilterDto(PageFilterDto filter)
        {
            if (filter.Page < 1)
            {
                throw new ArgumentException("Page must be greater than 0");
            }

            if (filter.PageSize < 1 || filter.PageSize > 100)
            {
                throw new ArgumentException("PageSize must be between 1 and 100");
            }
        }

        private PageDto MapToDto(Page page, string currentUserId = null)
        {
            var isLikedByCurrentUser = !string.IsNullOrWhiteSpace(currentUserId) &&
                                       page.Likes.Any(l => l.UserId == currentUserId);

            return new PageDto
            {
                Id = page.Id,
                Title = page.Title,
                Description = page.Description,
                CreatedAt = page.CreatedAt,
                CreatedById = page.CreatedById,
                CreatedBy = new ApplicationUserDto
                {
                    Id = page.CreatedBy.Id,
                    UserName = page.CreatedBy.UserName,
                    FirstName = page.CreatedBy.FirstName,
                    LastName = page.CreatedBy.LastName,
                    Email = page.CreatedBy.Email
                },
                LikesCount = page.Likes.Count,
                IsLikedByCurrentUser = isLikedByCurrentUser,
                FileId = page.FileId,
                FileUrl = page.File?.FileUrl
            };
        }
    }
}
