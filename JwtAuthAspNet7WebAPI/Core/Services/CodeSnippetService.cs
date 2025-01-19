using JwtAuthAspNet7WebAPI.Core.DbContext;
using JwtAuthAspNet7WebAPI.Core.Dtos;
using JwtAuthAspNet7WebAPI.Core.Entities;
using JwtAuthAspNet7WebAPI.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JwtAuthAspNet7WebAPI.Core.Services
{
    public class CodeSnippetService : ICodeSnippetService
    {
        private readonly ApplicationDbContext _context;

        public CodeSnippetService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CodeSnippetDto> CreateAsync(CreateCodeSnippetDto dto)
        {
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
                    throw new InvalidOperationException("One or more tag IDs are invalid");
                }

                codeSnippet.Tags = tags;
            }

            _context.CodeSnippets.Add(codeSnippet);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(codeSnippet.Id);
        }

        public async Task<CodeSnippetDto> GetByIdAsync(long id)
        {
            var codeSnippet = await _context.CodeSnippets
                .Include(cs => cs.Tags)
                .Include(cs => cs.CreatedBy)
                .Include(cs => cs.Likes)
                .Include(cs => cs.Comments)
                .FirstOrDefaultAsync(cs => cs.Id == id);

            if (codeSnippet == null)
            {
                throw new KeyNotFoundException($"Code snippet with ID {id} not found");
            }

            return MapToDto(codeSnippet);
        }

        public async Task<PaginatedResult<CodeSnippetDto>> GetAllAsync(CodeSnippetFilterDto filter)
        {
            ValidateFilterDto(filter);

            var query = BuildFilterQuery(filter);
            var total = await query.CountAsync();
            
            var items = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(cs => MapToDto(cs))
                .ToListAsync();

            return new PaginatedResult<CodeSnippetDto>
            {
                Items = items,
                TotalCount = total,
                PageSize = filter.PageSize,
                CurrentPage = filter.Page,
                TotalPages = (int)Math.Ceiling(total / (double)filter.PageSize)
            };
        }

        public async Task UpdateAsync(long id, UpdateCodeSnippetDto dto)
        {
            var codeSnippet = await _context.CodeSnippets
                .Include(cs => cs.Tags)
                .FirstOrDefaultAsync(cs => cs.Id == id);

            if (codeSnippet == null)
            {
                throw new KeyNotFoundException($"Code snippet with ID {id} not found");
            }

            if (dto.CreatedById != codeSnippet.CreatedById)
            {
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
                    throw new InvalidOperationException("One or more tag IDs are invalid");
                }

                codeSnippet.Tags.Clear();
                foreach (var tag in newTags)
                {
                    codeSnippet.Tags.Add(tag);
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(long id)
        {
            var codeSnippet = await _context.CodeSnippets
                .Include(cs => cs.Comments)
                .Include(cs => cs.Likes)
                .FirstOrDefaultAsync(cs => cs.Id == id);

            if (codeSnippet == null)
            {
                throw new KeyNotFoundException($"Code snippet with ID {id} not found");
            }

            // Remove related entities first
            _context.Comments.RemoveRange(codeSnippet.Comments);
            _context.Likes.RemoveRange(codeSnippet.Likes);
            
            _context.CodeSnippets.Remove(codeSnippet);
            await _context.SaveChangesAsync();
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
                    cs.Tags.Any(t => t.Name.ToLower().Contains(searchTerm))
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

        private CodeSnippetDto MapToDto(CodeSnippet codeSnippet)
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