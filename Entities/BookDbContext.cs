using BooksApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BooksApi.Entities
{
    public class BookDbContext : DbContext
    {
        public BookDbContext(DbContextOptions<BookDbContext> options) : base(options)
        {
        }

        public DbSet<Book> Books { get; set; }
    }
}