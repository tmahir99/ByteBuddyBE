using JwtAuthAspNet7WebAPI.Core.Dtos;
using JwtAuthAspNet7WebAPI.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JwtAuthAspNet7WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PageController : ControllerBase
    {
        private readonly IPageService _pageService;

        public PageController(IPageService pageService)
        {
            _pageService = pageService ?? throw new ArgumentNullException(nameof(pageService));
        }

        /// <summary>
        /// Create a new page
        /// </summary>
        /// <param name="dto">Page creation data</param>
        /// <returns>Created page</returns>
        [HttpPost]
        [ProducesResponseType(typeof(PageDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<PageDto>> CreatePage([FromBody] CreatePageDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User not authenticated");
                }

                dto.CreatedById = userId;
                var result = await _pageService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetPageById), new { id = result.Id }, result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the page", details = ex.Message });
            }
        }

        /// <summary>
        /// Get page by ID
        /// </summary>
        /// <param name="id">Page ID</param>
        /// <returns>Page details</returns>
        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PageDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<PageDto>> GetPageById(long id)
        {
            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var result = await _pageService.GetByIdAsync(id, currentUserId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the page", details = ex.Message });
            }
        }

        /// <summary>
        /// Get all pages with filtering and pagination
        /// </summary>
        /// <param name="searchTerm">Search term for title and description</param>
        /// <param name="createdById">Filter by creator user ID</param>
        /// <param name="createdAfter">Filter by creation date (after)</param>
        /// <param name="createdBefore">Filter by creation date (before)</param>
        /// <param name="sortBy">Sort by: latest, popular, title</param>
        /// <param name="page">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Paginated list of pages</returns>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(JwtAuthAspNet7WebAPI.Core.Dtos.PaginatedResult<PageDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<JwtAuthAspNet7WebAPI.Core.Dtos.PaginatedResult<PageDto>>> GetAllPages(
            [FromQuery] string searchTerm = null,
            [FromQuery] string createdById = null,
            [FromQuery] DateTime? createdAfter = null,
            [FromQuery] DateTime? createdBefore = null,
            [FromQuery] string sortBy = "latest",
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var filter = new PageFilterDto
                {
                    SearchTerm = searchTerm,
                    CreatedById = createdById,
                    CreatedAfter = createdAfter,
                    CreatedBefore = createdBefore,
                    SortBy = sortBy,
                    Page = page,
                    PageSize = pageSize
                };

                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var result = await _pageService.GetAllAsync(filter, currentUserId);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving pages", details = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing page
        /// </summary>
        /// <param name="id">Page ID</param>
        /// <param name="dto">Page update data</param>
        /// <returns>Success response</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> UpdatePage(long id, [FromBody] UpdatePageDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User not authenticated");
                }

                dto.CreatedById = userId;
                await _pageService.UpdateAsync(id, dto);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the page", details = ex.Message });
            }
        }

        /// <summary>
        /// Delete a page
        /// </summary>
        /// <param name="id">Page ID</param>
        /// <returns>Success response</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> DeletePage(long id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User not authenticated");
                }

                await _pageService.DeleteAsync(id, userId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the page", details = ex.Message });
            }
        }

        /// <summary>
        /// Get pages created by a specific user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of user's pages</returns>
        [HttpGet("user/{userId}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<PageDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<List<PageDto>>> GetUserPages(string userId)
        {
            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var result = await _pageService.GetUserPagesAsync(userId, currentUserId);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving user pages", details = ex.Message });
            }
        }

        /// <summary>
        /// Toggle like/unlike on a page
        /// </summary>
        /// <param name="id">Page ID</param>
        /// <returns>Updated page with like status</returns>
        [HttpPost("{id}/like")]
        [ProducesResponseType(typeof(PageDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<PageDto>> TogglePageLike(long id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User not authenticated");
                }

                var result = await _pageService.ToggleLikeAsync(id, userId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while toggling page like", details = ex.Message });
            }
        }

        /// <summary>
        /// Get users who liked a specific page
        /// </summary>
        /// <param name="id">Page ID</param>
        /// <returns>List of users who liked the page</returns>
        [HttpGet("{id}/likers")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<ApplicationUserDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<List<ApplicationUserDto>>> GetPageLikers(long id)
        {
            try
            {
                var result = await _pageService.GetPageLikersAsync(id);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving page likers", details = ex.Message });
            }
        }
    }
}
