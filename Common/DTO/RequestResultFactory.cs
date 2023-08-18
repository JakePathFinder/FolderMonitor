namespace Common.DTO
{
    public static class RequestResponseFactory
    {
        public static RequestResponse CreateFromException(Exception ex)
        {
            return new RequestResponse
            {
                Success = false,
                Message = ex.Message
            };
        }

        public static RequestResponse CreateSuccess(string? message = null)
        {
            return new RequestResponse
            {
                Success = true,
                Message = message
            };
        }
    }
}
