using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace GastroWaga.Data
{
    public static class DbInitializer
    {
        public static async Task EnsureCreatedAsync()
        {
            using var db = new AppDbContext();
            // Na MVP używamy EnsureCreated (prościej niż migracje)
            await db.Database.EnsureCreatedAsync();
        }
    }
}
