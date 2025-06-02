namespace BooksApi.DTOs
{
    public class SignInResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string TokenType { get; set; } = "Bearer";
    }
}