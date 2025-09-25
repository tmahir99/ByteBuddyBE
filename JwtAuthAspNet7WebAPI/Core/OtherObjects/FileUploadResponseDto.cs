public class FileUploadResponseDto
{
    public long FileId { get; set; } // New: unique file id
    public bool IsSucceed { get; set; }
    public string Message { get; set; }
    public string FileUrl { get; set; }
    public string FileName { get; set; }
    public long FileSize { get; set; }
    public string ContentType { get; set; }
}
