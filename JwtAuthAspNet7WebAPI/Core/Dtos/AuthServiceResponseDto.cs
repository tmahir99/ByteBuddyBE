namespace JwtAuthAspNet7WebAPI.Core.Dtos
{
    public class AuthServiceResponseDto
    {
        public bool IsSucceed { get; set; }
        public string Message { get; set; }
        public IList<string> Roles { get; set; }
    }
}
