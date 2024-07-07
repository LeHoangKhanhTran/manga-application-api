using MangaApplication.DTOs;
using MangaApplication.Entities;
using Microsoft.AspNetCore.Mvc;

namespace MangaApplication.Services
{
    public interface IMangaRepository
    {
        Task<IEnumerable<Manga>> GetAllManga();
        Task<Manga> GetMangaByID(Guid id);
        Task CreateManga(Manga entity);
        Task DeleteManga(Guid id);
        Task UpdateManga(Manga entity);
        Task<IEnumerable<Manga>> SearchMangaByTitle(string name);
        Task<Manga> GetRandomManga();
        IAsyncEnumerable<Manga> GetMangaList(int numberOfItem);
        Task<IEnumerable<Manga>> GetMangaListByDate(int numberOfItem);
        Task<IEnumerable<Manga>> GetTopRatedManga(int numberOfItem);
    }

}