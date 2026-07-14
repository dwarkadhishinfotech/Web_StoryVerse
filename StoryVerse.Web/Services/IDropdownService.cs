using System.Collections.Generic;
using System.Threading.Tasks;
using StoryVerse.Core.Entities;

namespace StoryVerse.Web.Services
{
    public interface IDropdownService
    {
        Task<List<DropdownOption>> GetOptionsByCategoryAsync(string category);
    }
}
