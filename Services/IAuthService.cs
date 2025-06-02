using BooksApi.DTOs;

namespace BooksApi.Services
{
    public interface IAuthService
    {
        Task<SignInResponse> SignInAsync(SignInRequest request);
        Task<SignInResponse> RegisterAsync(RegisterRequest request);
    }
}