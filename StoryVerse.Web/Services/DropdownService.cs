using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using StoryVerse.Core.Entities;
using StoryVerse.Infrastructure.Data;

namespace StoryVerse.Web.Services
{
    public class DropdownService : IDropdownService
    {
        private readonly ApplicationDbContext _context;

        public DropdownService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<DropdownOption>> GetOptionsByCategoryAsync(string category)
        {
            var categoryParam = new SqlParameter("@Category", category);
            return await _context.DropdownOptions
                .FromSqlRaw("EXEC sp_GetDropdownOptionsByCategory @Category", categoryParam)
                .ToListAsync();
        }
    }
}
