using AutoMapper;
using JwtAuthAspNet7WebAPI.Core.DbContext;
using JwtAuthAspNet7WebAPI.Core.Dtos;
using JwtAuthAspNet7WebAPI.Core.Entities;
using JwtAuthAspNet7WebAPI.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JwtAuthAspNet7WebAPI.Core.Services
{
    public class CodeSnippetService : ICodeSnippetService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<CodeSnippetService> _logger;

        public CodeSnippetService(ApplicationDbContext context, IMapper mapper, ILogger<CodeSnippetService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<CodeSnippetDto> CreateAsync(CreateCodeSnippetDto dto)
        {
            try
            {
                if (dto == null)
                {
                    _logger.LogWarning("CreateAsync called with null dto");
                    throw new ArgumentNullException(nameof(dto));
                }

                _logger.LogInformation("Creating code snippet with title: {Title}", dto.Title);
                ValidateCreateDto(dto);

                var codeSnippet = new CodeSnippet
                {
                    Title = dto.Title.Trim(),
                    CodeContent = dto.CodeContent,
                    Description = dto.Description?.Trim(),
                    ProgrammingLanguage = dto.ProgrammingLanguage.Trim(),
                    FileUrl = dto.FileUrl?.Trim(),
                    CreatedAt = DateTime.UtcNow,
                    CreatedById = dto.CreatedById,
                    Tags = new List<Tag>()
                };

                if (dto.TagIds?.Any() == true)
                {
                    var tags = await _context.Tags
                        .Where(t => dto.TagIds.Contains(t.Id))
                        .ToListAsync();

                    if (tags.Count != dto.TagIds.Count)
                    {
                        _logger.LogWarning("Invalid tag IDs provided for code snippet creation");
                        throw new InvalidOperationException("One or more tag IDs are invalid");
                    }

                    codeSnippet.Tags = tags;
                }

                _context.CodeSnippets.Add(codeSnippet);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Code snippet created successfully with ID: {Id}", codeSnippet.Id);
                return await GetByIdAsync(codeSnippet.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating code snippet with title: {Title}", dto?.Title);
                throw;
            }
        }

        public async Task<CodeSnippetDto> GetByIdAsync(long id)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning("GetByIdAsync called with invalid ID: {Id}", id);
                    throw new ArgumentException("ID must be greater than 0", nameof(id));
                }

                _logger.LogInformation("Getting code snippet by ID: {Id}", id);

                var codeSnippet = await _context.CodeSnippets
                    .Include(cs => cs.Tags)
                    .Include(cs => cs.CreatedBy)
                    .Include(cs => cs.Likes)
                    .Include(cs => cs.Comments)
                    .FirstOrDefaultAsync(cs => cs.Id == id);

                if (codeSnippet == null)
                {
                    _logger.LogWarning("Code snippet not found with ID: {Id}", id);
                    throw new KeyNotFoundException($"Code snippet with ID {id} not found");
                }

                return MapToDto(codeSnippet);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting code snippet by ID: {Id}", id);
                throw;
            }
        }

        public async Task<PaginatedResult<CodeSnippetDto>> GetAllAsync(CodeSnippetFilterDto filter)
        {
            try
            {
                if (filter == null)
                {
                    _logger.LogWarning("GetAllAsync called with null filter");
                    throw new ArgumentNullException(nameof(filter));
                }

                _logger.LogInformation("Getting code snippets with filter - Page: {Page}, PageSize: {PageSize}", filter.Page, filter.PageSize);
                ValidateFilterDto(filter);

                var query = BuildFilterQuery(filter);
                var total = await query.CountAsync();

                var items = await query
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToListAsync();

                var mappedItems = items.Select(MapToDto).ToList();

                _logger.LogInformation("Retrieved {Count} code snippets out of {Total} total", mappedItems.Count, total);

                return new PaginatedResult<CodeSnippetDto>
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
                _logger.LogError(ex, "Error getting code snippets with filter");
                throw;
            }
        }

        public async Task UpdateAsync(long id, UpdateCodeSnippetDto dto)
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

                _logger.LogInformation("Updating code snippet with ID: {Id}", id);

                var codeSnippet = await _context.CodeSnippets
                    .Include(cs => cs.Tags)
                    .FirstOrDefaultAsync(cs => cs.Id == id);

                if (codeSnippet == null)
                {
                    _logger.LogWarning("Code snippet not found for update with ID: {Id}", id);
                    throw new KeyNotFoundException($"Code snippet with ID {id} not found");
                }

                if (dto.CreatedById != codeSnippet.CreatedById)
                {
                    _logger.LogWarning("Unauthorized update attempt on code snippet {Id} by user {UserId}", id, dto.CreatedById);
                    throw new UnauthorizedAccessException("You can only update your own code snippets");
                }

                ValidateUpdateDto(dto);

                // Update basic properties if provided
                codeSnippet.Title = dto.Title?.Trim() ?? codeSnippet.Title;
                codeSnippet.CodeContent = dto.CodeContent ?? codeSnippet.CodeContent;
                codeSnippet.Description = dto.Description?.Trim() ?? codeSnippet.Description;
                codeSnippet.ProgrammingLanguage = dto.ProgrammingLanguage?.Trim() ?? codeSnippet.ProgrammingLanguage;
                codeSnippet.FileUrl = dto.FileUrl?.Trim() ?? codeSnippet.FileUrl;

                // Update tags if provided
                if (dto.TagIds != null)
                {
                    var newTags = await _context.Tags
                        .Where(t => dto.TagIds.Contains(t.Id))
                        .ToListAsync();

                    if (newTags.Count != dto.TagIds.Count)
                    {
                        _logger.LogWarning("Invalid tag IDs provided for code snippet update");
                        throw new InvalidOperationException("One or more tag IDs are invalid");
                    }

                    codeSnippet.Tags.Clear();
                    foreach (var tag in newTags)
                    {
                        codeSnippet.Tags.Add(tag);
                    }
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Code snippet updated successfully with ID: {Id}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating code snippet with ID: {Id}", id);
                throw;
            }
        }

        public async Task DeleteAsync(long id)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning("DeleteAsync called with invalid ID: {Id}", id);
                    throw new ArgumentException("ID must be greater than 0", nameof(id));
                }

                _logger.LogInformation("Deleting code snippet with ID: {Id}", id);

                var codeSnippet = await _context.CodeSnippets
                    .Include(cs => cs.Comments)
                    .Include(cs => cs.Likes)
                    .FirstOrDefaultAsync(cs => cs.Id == id);

                if (codeSnippet == null)
                {
                    _logger.LogWarning("Code snippet not found for deletion with ID: {Id}", id);
                    throw new KeyNotFoundException($"Code snippet with ID {id} not found");
                }

                // Remove related entities first
                _context.Comments.RemoveRange(codeSnippet.Comments);
                _context.Likes.RemoveRange(codeSnippet.Likes);
                
                _context.CodeSnippets.Remove(codeSnippet);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Code snippet deleted successfully with ID: {Id}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting code snippet with ID: {Id}", id);
                throw;
            }
        }

        private IQueryable<CodeSnippet> BuildFilterQuery(CodeSnippetFilterDto filter)
        {
            var query = _context.CodeSnippets
                .Include(cs => cs.Tags)
                .Include(cs => cs.CreatedBy)
                .Include(cs => cs.Likes)
                .Include(cs => cs.Comments)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var searchTerm = filter.SearchTerm.ToLower();
                query = query.Where(cs =>
                    cs.Title.ToLower().Contains(searchTerm) ||
                    cs.Description.ToLower().Contains(searchTerm) ||
                    cs.ProgrammingLanguage.ToLower().Contains(searchTerm) ||
                    cs.Tags.Any(t => t.Name.ToLower().Contains(searchTerm)) ||
                    cs.Tags.Any(t => t.Area.ToLower().Contains(searchTerm))
                );
            }

            if (!string.IsNullOrWhiteSpace(filter.ProgrammingLanguage))
            {
                query = query.Where(cs => cs.ProgrammingLanguage == filter.ProgrammingLanguage);
            }

            if (!string.IsNullOrWhiteSpace(filter.CreatedById))
            {
                query = query.Where(cs => cs.CreatedById == filter.CreatedById);
            }

            query = ApplySorting(query, filter.SortBy);

            return query;
        }

        private IQueryable<CodeSnippet> ApplySorting(IQueryable<CodeSnippet> query, string sortBy)
        {
            return sortBy?.ToLower() switch
            {
                "latest" => query.OrderByDescending(cs => cs.CreatedAt),
                "popular" => query.OrderByDescending(cs => cs.Likes.Count),
                "title" => query.OrderBy(cs => cs.Title),
                _ => query.OrderByDescending(cs => cs.CreatedAt)
            };
        }

        private static CodeSnippetDto MapToDto(CodeSnippet codeSnippet)
        {
            return new CodeSnippetDto
            {
                Id = codeSnippet.Id,
                Title = codeSnippet.Title,
                CodeContent = codeSnippet.CodeContent,
                Description = codeSnippet.Description,
                ProgrammingLanguage = codeSnippet.ProgrammingLanguage,
                FileUrl = codeSnippet.FileUrl,
                CreatedAt = codeSnippet.CreatedAt,
                CreatedById = codeSnippet.CreatedById,
                CreatedBy = new ApplicationUserDto
                {
                    UserName = codeSnippet.CreatedBy.UserName,
                    FirstName = codeSnippet.CreatedBy.FirstName,
                    LastName = codeSnippet.CreatedBy.LastName,
                    Email = codeSnippet.CreatedBy.Email
                },
                Tags = codeSnippet.Tags.Select(tag => new Tag
                {
                    Id = tag.Id,
                    Name = tag.Name,
                    Area = tag.Area
                }).ToList(),
                LikesCount = codeSnippet.Likes?.Count ?? 0,
                LikedByUsers = codeSnippet.Likes?
    .Where(like => like.User != null)
    .Select(like => like.User.UserName!)
    .ToList() ?? new List<string>(),
                CommentsCount = codeSnippet.Comments?.Count ?? 0
            };
        }

        private void ValidateCreateDto(CreateCodeSnippetDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Title))
                throw new ArgumentException("Title is required");

            if (string.IsNullOrWhiteSpace(dto.CodeContent))
                throw new ArgumentException("Code content is required");

            if (string.IsNullOrWhiteSpace(dto.ProgrammingLanguage))
                throw new ArgumentException("Programming language is required");

            if (string.IsNullOrWhiteSpace(dto.CreatedById))
                throw new ArgumentException("Creator ID is required");
        }

        private void ValidateUpdateDto(UpdateCodeSnippetDto dto)
        {
            if (dto.Title != null && string.IsNullOrWhiteSpace(dto.Title))
                throw new ArgumentException("Title cannot be empty");

            if (dto.CodeContent != null && string.IsNullOrWhiteSpace(dto.CodeContent))
                throw new ArgumentException("Code content cannot be empty");

            if (dto.ProgrammingLanguage != null && string.IsNullOrWhiteSpace(dto.ProgrammingLanguage))
                throw new ArgumentException("Programming language cannot be empty");
        }

        private void ValidateFilterDto(CodeSnippetFilterDto filter)
        {
            if (filter.Page < 1)
                filter.Page = 1;

            if (filter.PageSize < 1)
                filter.PageSize = 10;
            else if (filter.PageSize > 100)
                filter.PageSize = 100;
        }
    }
}