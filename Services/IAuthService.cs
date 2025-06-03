using BooksApi.DTOs;

namespace BooksApi.Services
{
    public interface IAuthService
    {
        Task<SignInResponse> SignInAsync(SignInRequest request);
        Task<SignInResponse> RegisterAsync(RegisterRequest request);
        Task<UserListResponse> GetUsersAsync(
            string? roleFilter = "user",
            string search = "",
            string sort = "asc",
            int page = 1,
            int pageSize = 10);
    }
}