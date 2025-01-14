using JwtAuthAspNet7WebAPI.Core.Dtos;
using JwtAuthAspNet7WebAPI.Core.Entities;
using JwtAuthAspNet7WebAPI.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JwtAuthAspNet7WebAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CodeSnippetsController : ControllerBase
    {
        private readonly ICodeSnippetService _service;

        public CodeSnippetsController(ICodeSnippetService service)
        {
            _service = service;
        }

        /// Creates a new code snippet
        [HttpPost]
        [ProducesResponseType(typeof(CodeSnippetDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CodeSnippetDto>> Create([FromBody] CreateCodeSnippetDto dto)
        {
            // Get current user's ID from the token
            dto.CreatedById = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var result = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        /// Gets a specific code snippet by ID
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(CodeSnippetDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CodeSnippetDto>> GetById(long id)
        {
            try
            {
                var result = await _service.GetByIdAsync(id);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        /// Gets a paginated list of code snippets with optional filtering
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedResult<CodeSnippetDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginatedResult<CodeSnippetDto>>> GetAll([FromQuery] CodeSnippetFilterDto filter)
        {
            var result = await _service.GetAllAsync(filter);
            return Ok(result);
        }

        /// Updates an existing code snippet
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Update(long id, [FromBody] UpdateCodeSnippetDto dto)
        {
            try
            {
                // Get current user's ID from the token
                dto.CreatedById = User.FindFirstValue(ClaimTypes.NameIdentifier);

                await _service.UpdateAsync(id, dto);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        /// Deletes a code snippet
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(long id)
        {
            try
            {
                // Optional: Check if the user owns the snippet before deletion
                var snippet = await _service.GetByIdAsync(id);
                if (snippet.CreatedById != User.FindFirstValue(ClaimTypes.NameIdentifier))
                {
                    return Forbid();
                }

                await _service.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        /// Gets code snippets for the current user
        [HttpGet("my")]
        [ProducesResponseType(typeof(PaginatedResult<CodeSnippetDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginatedResult<CodeSnippetDto>>> GetMySnippets([FromQuery] CodeSnippetFilterDto filter)
        {
            filter.CreatedById = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _service.GetAllAsync(filter);
            return Ok(result);
        }

        /// Gets code snippets by tag
        [HttpGet("bytag/{tagName}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PaginatedResult<CodeSnippetDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginatedResult<CodeSnippetDto>>> GetByTag(
            string tagName,
            [FromQuery] CodeSnippetFilterDto filter)
        {
            filter.SearchTerm = tagName;  // Use the existing search functionality to filter by tag
            var result = await _service.GetAllAsync(filter);
            return Ok(result);
        }
    }
}