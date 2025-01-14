using JwtAuthAspNet7WebAPI.Core.Dtos;
using JwtAuthAspNet7WebAPI.Core.Entities;

namespace JwtAuthAspNet7WebAPI.Core.Interfaces
{
public interface ICodeSnippetService
{
    Task<CodeSnippetDto> CreateAsync(CreateCodeSnippetDto dto);
    Task<CodeSnippetDto> GetByIdAsync(long id);
    Task<PaginatedResult<CodeSnippetDto>> GetAllAsync(CodeSnippetFilterDto filter);
    Task UpdateAsync(long id, UpdateCodeSnippetDto dto);
    Task DeleteAsync(long id);
}
}
