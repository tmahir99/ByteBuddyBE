using JwtAuthAspNet7WebAPI.Core.DbContext;
using JwtAuthAspNet7WebAPI.Core.Dtos;
using JwtAuthAspNet7WebAPI.Core.Entities;
using JwtAuthAspNet7WebAPI.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JwtAuthAspNet7WebAPI.Core.Services
{
    public class TagService : ITagService
    {
        private readonly ApplicationDbContext _context;

        public TagService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TagDto> CreateAsync(TagDto dto)
        {
            // Check if tag with same name already exists
            var existingTag = await _context.Tags
                .FirstOrDefaultAsync(t => t.Name.ToLower() == dto.Name.ToLower());

            if (existingTag != null)
            {
                throw new InvalidOperationException($"Tag with name '{dto.Name}' already exists");
            }

            var tag = new Tag
            {
                Name = dto.Name,
                Area = dto.Area
            };

            _context.Tags.Add(tag);
            await _context.SaveChangesAsync();

            return new TagDto
            {
                Id = tag.Id,
                Name = tag.Name,
                Area = tag.Area
            };
        }

        public async Task<List<string>> GetAllTagNames()
        {
            var tagNames = await _context.Tags
                .Select(t => t.Name)
                .Distinct()
                .ToListAsync();

            return tagNames;
        }

        public async Task<List<TagDto>> GetAllTagsAsync()
        {
            var tags = await _context.Tags
                .Select(t => new TagDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Area = t.Area
                })
                .ToListAsync();

            return tags;
        }

        public async Task<List<string>> GetAllTagAreas()
        {
            var tagAreas = await _context.Tags
                .Select(t => t.Area)
                .Distinct()
                .ToListAsync();

            return tagAreas;
        }

        public async Task<TagDto> GetByIdAsync(long id)
        {
            var tag = await _context.Tags
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tag == null)
            {
                throw new KeyNotFoundException($"Tag with ID {id} not found");
            }

            return new TagDto
            {
                Id = tag.Id,
                Name = tag.Name,
                Area = tag.Area
            };
        }

        public async Task<List<TagDto>> GetAllAsync()
        {
            var tags = await _context.Tags
                .Select(t => new TagDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Area = t.Area
                })
                .ToListAsync();

            return tags;
        }

        public async Task UpdateAsync(long id, TagDto dto)
        {
            var tag = await _context.Tags.FindAsync(id);

            if (tag == null)
            {
                throw new KeyNotFoundException($"Tag with ID {id} not found");
            }

            // Check if updating to a name that already exists
            var existingTag = await _context.Tags
                .FirstOrDefaultAsync(t => t.Name.ToLower() == dto.Name.ToLower() && t.Id != id);

            if (existingTag != null)
            {
                throw new InvalidOperationException($"Tag with name '{dto.Name}' already exists");
            }

            tag.Name = dto.Name;
            tag.Area = dto.Area;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(long id)
        {
            var tag = await _context.Tags
                .Include(t => t.CodeSnippets)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tag == null)
            {
                throw new KeyNotFoundException($"Tag with ID {id} not found");
            }

            // Check if tag is being used by any code snippets
            if (tag.CodeSnippets != null && tag.CodeSnippets.Any())
            {
                throw new InvalidOperationException($"Cannot delete tag '{tag.Name}' as it is being used by {tag.CodeSnippets.Count} code snippets");
            }

            _context.Tags.Remove(tag);
            await _context.SaveChangesAsync();
        }
    }
}