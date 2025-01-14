namespace JwtAuthAspNet7WebAPI.Core.Services
{
    using JwtAuthAspNet7WebAPI.Core.DbContext;
    using JwtAuthAspNet7WebAPI.Core.Dtos;
    using JwtAuthAspNet7WebAPI.Core.Entities;
    using JwtAuthAspNet7WebAPI.Core.Interfaces;
    using Microsoft.EntityFrameworkCore;

    public class CodeSnippetService : ICodeSnippetService
    {
        private readonly ApplicationDbContext _context;

        public CodeSnippetService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CodeSnippetDto> CreateAsync(CreateCodeSnippetDto dto)
        {
            var codeSnippet = new CodeSnippet
            {
                Title = dto.Title,
                CodeContent = dto.CodeContent,
                Description = dto.Description,
                ProgrammingLanguage = dto.ProgrammingLanguage,
                FileUrl = dto.FileUrl,
                CreatedAt = DateTime.UtcNow,
                CreatedById = dto.CreatedById
            };

            if (dto.TagIds != null && dto.TagIds.Any())
            {
                var tags = await _context.Tags
                    .Where(t => dto.TagIds.Contains(t.Id))
                    .ToListAsync();
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
                .Include(cs => cs.Likes)
                .Include(cs => cs.Comments)
                .Include(cs => cs.CreatedBy)
                .FirstOrDefaultAsync(cs => cs.Id == id);

            if (codeSnippet == null)
            {
                throw new KeyNotFoundException($"Code snippet with ID {id} not found");
            }

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
                    LastName = codeSnippet.CreatedBy.LastName
                },
                Tags = codeSnippet.Tags.Select(tag => new Tag
                {
                    Id = tag.Id,
                    Name = tag.Name,
                    Area = tag.Area
                }).ToList(),
                LikesCount = codeSnippet.Likes.Count,
                CommentsCount = codeSnippet.Comments.Count
            };
        }

        public async Task<PaginatedResult<CodeSnippetDto>> GetAllAsync(CodeSnippetFilterDto filter)
        {
            var query = _context.CodeSnippets
                .Include(cs => cs.Tags)
                .Include(cs => cs.CreatedBy)
                .Include(cs => cs.Likes)
                .Include(cs => cs.Comments)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                query = query.Where(cs =>
                    cs.Title.Contains(filter.SearchTerm) ||
                    cs.Description.Contains(filter.SearchTerm) ||
                    cs.ProgrammingLanguage.Contains(filter.SearchTerm) ||
                    cs.Tags.Any(t => t.Name.Contains(filter.SearchTerm))
                );
            }

            if (filter.ProgrammingLanguage != null)
            {
                query = query.Where(cs => cs.ProgrammingLanguage == filter.ProgrammingLanguage);
            }

            if (filter.CreatedById != null)
            {
                query = query.Where(cs => cs.CreatedById == filter.CreatedById);
            }

            // Apply sorting
            query = filter.SortBy?.ToLower() switch
            {
                "latest" => query.OrderByDescending(cs => cs.CreatedAt),
                "popular" => query.OrderByDescending(cs => cs.Likes.Count),
                "title" => query.OrderBy(cs => cs.Title),
                _ => query.OrderByDescending(cs => cs.CreatedAt)
            };

            var total = await query.CountAsync();

            var items = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(cs => new CodeSnippetDto
                {
                    Id = cs.Id,
                    Title = cs.Title,
                    Description = cs.Description,
                    ProgrammingLanguage = cs.ProgrammingLanguage,
                    CreatedAt = cs.CreatedAt,
                    CreatedById = cs.CreatedById,
                    CreatedBy = new ApplicationUserDto
                    {
                        UserName = cs.CreatedBy.UserName,
                        FirstName = cs.CreatedBy.FirstName,
                        LastName = cs.CreatedBy.LastName
                    },
                    Tags = cs.Tags.Select(t => new Tag
                    {
                        Id = t.Id,
                        Name = t.Name,
                        Area = t.Area
                    }).ToList(),
                    LikesCount = cs.Likes.Count,
                    CommentsCount = cs.Comments.Count
                })
                .ToListAsync();

            return new PaginatedResult<CodeSnippetDto>
            {
                Items = items,
                TotalCount = total,
                PageSize = filter.PageSize,
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

            // Verify ownership if needed
            if (dto.CreatedById != codeSnippet.CreatedById)
            {
                throw new UnauthorizedAccessException("You can only update your own code snippets");
            }

            codeSnippet.Title = dto.Title ?? codeSnippet.Title;
            codeSnippet.CodeContent = dto.CodeContent ?? codeSnippet.CodeContent;
            codeSnippet.Description = dto.Description ?? codeSnippet.Description;
            codeSnippet.ProgrammingLanguage = dto.ProgrammingLanguage ?? codeSnippet.ProgrammingLanguage;
            codeSnippet.FileUrl = dto.FileUrl ?? codeSnippet.FileUrl;

            // Update tags if provided
            if (dto.TagIds != null)
            {
                var newTags = await _context.Tags
                    .Where(t => dto.TagIds.Contains(t.Id))
                    .ToListAsync();
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
            var codeSnippet = await _context.CodeSnippets.FindAsync(id);

            if (codeSnippet == null)
            {
                throw new KeyNotFoundException($"Code snippet with ID {id} not found");
            }

            _context.CodeSnippets.Remove(codeSnippet);
            await _context.SaveChangesAsync();
        }
    }
}