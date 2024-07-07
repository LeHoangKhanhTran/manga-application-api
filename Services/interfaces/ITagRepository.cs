using MangaApplication.DTOs;
using MangaApplication.Entities;

namespace MangaApplication.Services;

public interface ITagRepository 
{
    Task<IEnumerable<Tag>> GetAllTags();
    Task<Tag> GetTagById(Guid id);
    Task CreateTag(Tag tag);
    Task UpdateTag(Tag tag);
    Task DeleteTag(Guid id);

}