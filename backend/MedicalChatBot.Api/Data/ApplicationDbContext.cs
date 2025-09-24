using Microsoft.EntityFrameworkCore;
using MedicalChatBot.Api.Models;

namespace MedicalChatBot.Api.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ChatHistory> ChatHistories { get; set; }
        public DbSet<User> Users { get; set; }
    }
}
