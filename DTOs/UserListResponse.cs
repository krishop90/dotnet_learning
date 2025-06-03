using BooksApi.DTOs;

public class UserListResponse
{
    public IEnumerable<UserModel> Users { get; set; } = new List<UserModel>();
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
}