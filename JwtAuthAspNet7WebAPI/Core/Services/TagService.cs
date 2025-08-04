using JwtAuthAspNet7WebAPI.Core.DbContext;
using JwtAuthAspNet7WebAPI.Core.Dtos;
using JwtAuthAspNet7WebAPI.Core.Entities;
using JwtAuthAspNet7WebAPI.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JwtAuthAspNet7WebAPI.Core.Services
{
    public class TagService : ITagService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TagService> _logger;

        public TagService(ApplicationDbContext context, ILogger<TagService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<TagDto> CreateAsync(TagDto dto)
        {
            try
            {
                if (dto == null)
                {
                    _logger.LogWarning("CreateAsync called with null dto");
                    throw new ArgumentNullException(nameof(dto));
                }

                if (string.IsNullOrWhiteSpace(dto.Name))
                {
                    _logger.LogWarning("CreateAsync called with empty tag name");
                    throw new ArgumentException("Tag name cannot be empty", nameof(dto));
                }

                if (string.IsNullOrWhiteSpace(dto.Area))
                {
                    _logger.LogWarning("CreateAsync called with empty tag area");
                    throw new ArgumentException("Tag area cannot be empty", nameof(dto));
                }

                _logger.LogInformation("Creating tag: {Name} in area: {Area}", dto.Name, dto.Area);

                // Check if tag with same name already exists
                var existingTag = await _context.Tags
                    .FirstOrDefaultAsync(t => t.Name.ToLower() == dto.Name.ToLower());

                if (existingTag != null)
                {
                    _logger.LogWarning("Tag creation failed: Tag '{Name}' already exists", dto.Name);
                    throw new InvalidOperationException($"Tag with name '{dto.Name}' already exists");
                }

                var tag = new Tag
                {
                    Name = dto.Name.Trim(),
                    Area = dto.Area.Trim()
                };

                _context.Tags.Add(tag);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Tag created successfully with ID: {Id}", tag.Id);

                return new TagDto
                {
                    Id = tag.Id,
                    Name = tag.Name,
                    Area = tag.Area
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating tag: {Name} in area: {Area}", dto?.Name, dto?.Area);
                throw;
            }
        }

        public async Task<List<string>> GetAllTagNames()
        {
            try
            {
                _logger.LogInformation("Getting all tag names");
                var tagNames = await _context.Tags
                    .Select(t => t.Name)
                    .Distinct()
                    .ToListAsync();

                _logger.LogInformation("Retrieved {Count} unique tag names", tagNames.Count);
                return tagNames;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all tag names");
                throw;
            }
        }

        public async Task<List<TagDto>> GetAllTagsAsync()
        {
            try
            {
                _logger.LogInformation("Getting all tags");
                var tags = await _context.Tags
                    .Select(t => new TagDto
                    {
                        Id = t.Id,
                        Name = t.Name,
                        Area = t.Area
                    })
                    .ToListAsync();

                _logger.LogInformation("Retrieved {Count} tags", tags.Count);
                return tags;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all tags");
                throw;
            }
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
            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning("GetByIdAsync called with invalid ID: {Id}", id);
                    throw new ArgumentException("ID must be greater than 0", nameof(id));
                }

                _logger.LogInformation("Getting tag by ID: {Id}", id);
                var tag = await _context.Tags
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (tag == null)
                {
                    _logger.LogWarning("Tag not found with ID: {Id}", id);
                    throw new KeyNotFoundException($"Tag with ID {id} not found");
                }

                return new TagDto
                {
                    Id = tag.Id,
                    Name = tag.Name,
                    Area = tag.Area
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tag by ID: {Id}", id);
                throw;
            }
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

                if (string.IsNullOrWhiteSpace(dto.Name))
                {
                    _logger.LogWarning("UpdateAsync called with empty tag name");
                    throw new ArgumentException("Tag name cannot be empty", nameof(dto));
                }

                _logger.LogInformation("Updating tag with ID: {Id}", id);
                var tag = await _context.Tags.FindAsync(id);

                if (tag == null)
                {
                    _logger.LogWarning("Tag not found for update with ID: {Id}", id);
                    throw new KeyNotFoundException($"Tag with ID {id} not found");
                }

                // Check if updating to a name that already exists
                var existingTag = await _context.Tags
                    .FirstOrDefaultAsync(t => t.Name.ToLower() == dto.Name.ToLower() && t.Id != id);

                if (existingTag != null)
                {
                    _logger.LogWarning("Tag update failed: Tag '{Name}' already exists", dto.Name);
                    throw new InvalidOperationException($"Tag with name '{dto.Name}' already exists");
                }

                tag.Name = dto.Name.Trim();
                tag.Area = dto.Area?.Trim();

                await _context.SaveChangesAsync();
                _logger.LogInformation("Tag updated successfully with ID: {Id}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating tag with ID: {Id}", id);
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

                _logger.LogInformation("Deleting tag with ID: {Id}", id);
                var tag = await _context.Tags
                    .Include(t => t.CodeSnippets)
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (tag == null)
                {
                    _logger.LogWarning("Tag not found for deletion with ID: {Id}", id);
                    throw new KeyNotFoundException($"Tag with ID {id} not found");
                }

                // Check if tag is being used by any code snippets
                if (tag.CodeSnippets != null && tag.CodeSnippets.Any())
                {
                    _logger.LogWarning("Cannot delete tag '{Name}' as it is being used by {Count} code snippets", tag.Name, tag.CodeSnippets.Count);
                    throw new InvalidOperationException($"Cannot delete tag '{tag.Name}' as it is being used by {tag.CodeSnippets.Count} code snippets");
                }

                _context.Tags.Remove(tag);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Tag deleted successfully with ID: {Id}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting tag with ID: {Id}", id);
                throw;
            }
        }
    }
}