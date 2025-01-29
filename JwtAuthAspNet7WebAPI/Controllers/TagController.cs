using JwtAuthAspNet7WebAPI.Core.Dtos;
using JwtAuthAspNet7WebAPI.Core.Interfaces;
using JwtAuthAspNet7WebAPI.Core.OtherObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JwtAuthAspNet7WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TagsController : ControllerBase
    {
        private readonly ITagService _tagService;

        public TagsController(ITagService tagService)
        {
            _tagService = tagService;
        }

        [HttpPost]
        [Authorize(Roles = StaticUserRoles.GUEST + "," + StaticUserRoles.ADMIN)]
        //[Authorize(Roles = StaticUserRoles.ADMIN)]
        [ProducesResponseType(typeof(TagDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<TagDto>> Create([FromBody] TagDto dto)
        {
            try
            {
                var result = await _tagService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(TagDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TagDto>> GetById(long id)
        {
            try
            {
                var result = await _tagService.GetByIdAsync(id);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpGet("names")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<TagDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllTagNames()
        {
            var tagNames = await _tagService.GetAllTagNames();
            return Ok(tagNames);
        }

        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<TagDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllTagsAsync()
        {
            var tagNames = await _tagService.GetAllTagsAsync();
            return Ok(tagNames);
        }

        [HttpGet("areas")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<TagDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllTagAreas()
        {
            var tagAreas = await _tagService.GetAllTagAreas();
            return Ok(tagAreas);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = StaticUserRoles.USER + "," + StaticUserRoles.ADMIN)]
        //[Authorize(Roles = StaticUserRoles.ADMIN)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(long id, [FromBody] TagDto dto)
        {
            try
            {
                await _tagService.UpdateAsync(id, dto);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = StaticUserRoles.USER + "," + StaticUserRoles.ADMIN)]
        //[Authorize(Roles = StaticUserRoles.ADMIN)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Delete(long id)
        {
            try
            {
                await _tagService.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}