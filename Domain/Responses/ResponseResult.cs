namespace Domain.Responses;

public class ResponseResult
{
    public bool Succeeded { get; set; }
    public int StatusCode { get; set; }
    public string? Error { get; set; }
}
