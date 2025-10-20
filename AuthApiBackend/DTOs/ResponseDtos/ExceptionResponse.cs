namespace AuthApiBackend.DTOs.ResponseDtos
{
    public class ExceptionResponse
    {
        public string ErrorMessage { get; set; } = null!;
        public int ErrorCode { get; set; }
    }
}
