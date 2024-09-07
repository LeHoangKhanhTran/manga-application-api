using System.Net;
using CloudinaryDotNet.Actions;
using MangaApplication.DTOs;
using MangaApplication.Entities;
using MangaApplication.Services;
using MangaApplication.Services.Repositories;
using Microsoft.AspNetCore.Mvc;
namespace MangaApplication.Controllers 
{   
    [ApiController]
    [Route("api/manga")]
    public class MangaController : ControllerBase 
    {
        private readonly UnitOfWork unitOfWork;
        private readonly ILogger<MangaController> logger;
        private readonly IImageUploader imageUploader;
        private readonly IUserRepository userRepository;
        public MangaController(IUnitOfWork unitOfWork, IImageUploader imageUploader, ILogger<MangaController> logger, IUserRepository userRepository) 
        {
            this.unitOfWork = (UnitOfWork)unitOfWork;
            this.logger = logger;
            this.imageUploader = imageUploader;
            this.userRepository = userRepository;
        }
        
        [HttpGet]
        public async Task<IEnumerable<MangaDto>> GetAllManga() 
        {
            var mangaList = (await unitOfWork.mangaRepository.GetAllManga()).Select(manga => manga.AsDto());
            return mangaList;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MangaDto>> GetMangaById(Guid id) 
        {
            var manga = await unitOfWork.mangaRepository.GetMangaByID(id);
            if (manga is null) 
            {
                return NotFound();
            }
            return manga.AsDto();
        }

        [HttpPost]
        public async Task<ActionResult<MangaDto>> CreateManga([FromForm] CreateMangaDto mangaDto) 
        {
            var author = await unitOfWork.authorRepository.GetAuthorById(mangaDto.authorId);
            if (author is null)
            {
                throw new Exception($"Author with id {mangaDto.authorId} not found");
            }
            if (ModelState.IsValid)
            {
                string imageUrl = "";
                try 
                {
                    if (mangaDto.Image is not null)
                    {
                        var uploadResult = await imageUploader.UploadImage(mangaDto.Image, "manga");
                        imageUrl = uploadResult.Url.ToString();
                    }
                }
                catch (Exception e)
                {
                    logger.LogError(e, e.Message);
                }
                foreach (Guid tagId in mangaDto.TagIds)
                {
                    var tag = await unitOfWork.tagRepository.GetTagById(tagId);
                    if (tag is null)
                    {
                        throw new Exception($"Tag with id {tagId} not found");
                    }
                }   
                Manga manga = new() 
                {
                    Id = Guid.NewGuid(),
                    Title = mangaDto.Title,
                    Summary = mangaDto.Summary,
                    Author = new(author.Id, author.Name),
                    Status = mangaDto.Status,
                    ImageUrl = imageUrl,
                    Rating = new Rating(),
                    TagIds = mangaDto.TagIds,
                    CreatedDate = DateTimeOffset.UtcNow,
                    Follows = 0
                };
                await unitOfWork.mangaRepository.CreateManga(manga);
                await unitOfWork.authorRepository.AddMangaToAuthor(author, manga.Id);
                return CreatedAtAction(nameof(GetMangaById), new {id = manga.Id}, manga.AsDto());
            }
            return UnprocessableEntity(ModelState);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateManga(Guid id, [FromForm] UpdateMangaDto mangaDto)
        {
            var existingManga = await unitOfWork.mangaRepository.GetMangaByID(id);
            if (existingManga is null) 
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                var author = new Author();
                if (mangaDto.authorId is not null)
                {
                    author = await unitOfWork.authorRepository.GetAuthorById((Guid)mangaDto.authorId);
                    if (author is null)
                    {
                        throw new HttpRequestException("Author not found");
                    }
                    
                    if (author.Id != existingManga.Author.AuthorId)
                    {
                        await unitOfWork.authorRepository.AddMangaToAuthor(author, existingManga.Id);
                        Author existingAuthor = await unitOfWork.authorRepository.GetAuthorById(existingManga.Author.AuthorId);
                        await unitOfWork.authorRepository.RemoveMangaFromAuthor(existingAuthor, existingManga.Id);
                    }
                };
                if (mangaDto.TagIds is not null)
                {
                    foreach (Guid tagId in mangaDto.TagIds)
                    {
                        var tag = await unitOfWork.tagRepository.GetTagById(tagId);
                        if (tag is null)
                        {
                            throw new HttpRequestException("Tag not found");
                        }
                    }
                }
                string imageUrl = "";
                string backgroundImageUrl = "";
                try 
                {
                    if (mangaDto.Image is not null)
                    {
                        var uploadResult = await imageUploader.UploadImage(mangaDto.Image, "manga");
                        imageUrl = uploadResult.Url.ToString();
                    }
                    
                }
                catch (Exception e)
                {
                    logger.LogError(e, e.Message);
                }
                Manga manga = new() 
                {
                    Id = existingManga.Id,
                    Title = mangaDto.Title != null && mangaDto.Title.Length > 0  ? mangaDto.Title : existingManga.Title,
                    Summary = mangaDto.Summary != null && mangaDto.Summary.Length > 0 ? mangaDto.Summary : existingManga.Summary,
                    Author =  mangaDto.authorId != null ? new MangaAuthor(author.Id, author.Name) : existingManga.Author,
                    Status = mangaDto.Status != null && mangaDto.Status.Length > 0 ? mangaDto.Status : existingManga.Status,
                    ImageUrl = imageUrl != "" ? imageUrl : existingManga.ImageUrl,
                    Rating = existingManga.Rating,
                    TagIds = mangaDto.TagIds != null && mangaDto.TagIds != existingManga.TagIds ? existingManga.TagIds.Concat(mangaDto.TagIds) : existingManga.TagIds,
                    CreatedDate = existingManga.CreatedDate,
                    Follows = 0
                };
                await unitOfWork.mangaRepository.UpdateManga(manga);
                return NoContent();
            }
            return UnprocessableEntity(ModelState);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteManga(Guid id)
        {
            var existingManga = await unitOfWork.mangaRepository.GetMangaByID(id);
            if (existingManga is null) 
            {
                return NotFound();
            }
            List<string> publicIds = new();
            if (existingManga.ImageUrl != "")
            {
                publicIds.Add(PublicIdExtractor.Extract(existingManga.ImageUrl, "/manga"));
            }
            await imageUploader.DeleteImages(publicIds);
            await unitOfWork.mangaRepository.DeleteManga(id);
            if (existingManga.Author != null)
            {
                var existingAuthor = await unitOfWork.authorRepository.GetAuthorById(existingManga.Author.AuthorId);
                await unitOfWork.authorRepository.RemoveMangaFromAuthor(existingAuthor, existingManga.Id);
            }
            return NoContent();
        }

        [HttpGet("search")]
        public async Task<IEnumerable<SearchMangaDto>> GetAllMangaByTitle(string title) 
        {
            var mangaListByName = (await unitOfWork.mangaRepository.SearchMangaByTitle(title)).Select(manga => manga.AsSearchDto());
            return mangaListByName;
        }

        [HttpGet("{id}/rating")]
        public async Task<ActionResult<decimal>> GetMangaRating(Guid id)
        {
            var existingManga = await unitOfWork.mangaRepository.GetMangaByID(id);
            if (existingManga is null) 
            {
                return NotFound();
            }
            return existingManga.Rating.AverageScore;
        }

        [HttpPost("{id}/rating")]
        public async Task<ActionResult> UpdateRating(Guid id, RatingDto ratingDto)
        {
            var user = await userRepository.GetUserById(ratingDto.UserId);
            if (user is null) 
            {
                return Unauthorized();
            }
            var existingManga = await unitOfWork.mangaRepository.GetMangaByID(id);
            if (existingManga is null) 
            {
                return NotFound();
            }
            if (existingManga.Rating != null)
            { 
                if (ratingDto.PreviousScore != null && ratingDto.PreviousScore > 0) 
                {
                    existingManga.Rating.RemoveRating(ratingDto.PreviousScore);
                }
                existingManga.Rating.UpdateRating(ratingDto.Score);
                await unitOfWork.mangaRepository.UpdateManga(existingManga);   
            }
            List<UserRating> userRatings;
            if (user.UserRatings is null || !user.UserRatings.Any()) 
            {
                userRatings = new();
            }
            else 
            {
                var existingRating = user.UserRatings.Where(rating => rating.MangaId != id);
                userRatings = existingRating.ToList();
            }
            userRatings.Add(new UserRating(id, ratingDto.Score));
            User updatedUser = new() 
            {
                Id = user.Id,
                Username = user.Username,
                Password = user.Password,
                Email = user.Email,
                AvatarUrl = user.AvatarUrl,
                Role = user.Role,
                MangaFollows = user.MangaFollows,
                UserRatings = userRatings
            };
            await userRepository.UpdateUser(updatedUser);
            var updatedRating = existingManga.Rating.AverageScore;
            return Ok(new {updatedRating});
        }

        [HttpPost("{id}/remove-rating")]
        public async Task<ActionResult> RemoveRating(Guid id, RemoveRatingDto removeRatingDto)
        {
            var user = await userRepository.GetUserById(removeRatingDto.UserId);
            if (user is null) 
            {
                return Unauthorized();
            }
            var existingManga = await unitOfWork.mangaRepository.GetMangaByID(id);
            if (existingManga is null) 
            {
                return NotFound();
            }
            if (existingManga.Rating != null)
            { 
                existingManga.Rating.RemoveRating(removeRatingDto.Score);
                await unitOfWork.mangaRepository.UpdateManga(existingManga);   
            }
            User updatedUser = new() 
            {
                Id = user.Id,
                Username = user.Username,
                Password = user.Password,
                Email = user.Email,
                AvatarUrl = user.AvatarUrl,
                Role = user.Role,
                MangaFollows = user.MangaFollows,
                UserRatings = user.UserRatings.ToList().Where(userRating => userRating.MangaId != id)
            };
            await userRepository.UpdateUser(updatedUser);
            var updatedRating = existingManga.Rating.AverageScore;
            return Ok(new {updatedRating});
        }
    
    [HttpPost("{id}/follow")]
    public async Task<ActionResult> FollowManga(Guid id, FollowDto followDto)
    {
        var existingUser = await userRepository.GetUserById(followDto.UserId);
        if (existingUser is null)
        {
            return Unauthorized();
        }
        var existingManga = await unitOfWork.mangaRepository.GetMangaByID(id);
        if (existingManga is null) 
        {
            return NotFound();
        }
        User user = new()
        {
            Id = existingUser.Id,
            Username = existingUser.Username,
            Password = existingUser.Password,
            Email = existingUser.Email,
            AvatarUrl = existingUser.AvatarUrl,
            Role = existingUser.Role,
            MangaFollows = existingUser.MangaFollows is not null ? existingUser.MangaFollows.Append(id) : new List<Guid>() {id},
            UserRatings = existingUser.UserRatings
        };
        await userRepository.UpdateUser(user);
        var updatedFollow = existingManga.Follows + 1;
        Manga manga = new() 
        {
            Id = existingManga.Id,
            Title = existingManga.Title,
            Summary = existingManga.Summary,
            Author =  existingManga.Author,
            Status = existingManga.Status,
            ImageUrl = existingManga.ImageUrl,
            Rating = existingManga.Rating,
            TagIds = existingManga.TagIds,
            CreatedDate = existingManga.CreatedDate,
            Follows = updatedFollow
        };
        await unitOfWork.mangaRepository.UpdateManga(manga);
        return Ok(new {updatedFollow});
    }

    [HttpPost("{id}/unfollow")]
    public async Task<ActionResult> UnfollowManga(Guid id, FollowDto followDto)
    {

        var existingUser = await userRepository.GetUserById(followDto.UserId);
        if (existingUser is null)
        {
            return Unauthorized();
        }
        var existingManga = await unitOfWork.mangaRepository.GetMangaByID(id);
        if (existingManga is null)
        {
            return NotFound();
        }
        User user = new()
        {
            Id = existingUser.Id,
            Username = existingUser.Username,
            Password = existingUser.Password,
            Email = existingUser.Email,
            AvatarUrl = existingUser.AvatarUrl,
            Role = existingUser.Role,
            MangaFollows = existingUser.MangaFollows is not null ? existingUser.MangaFollows.ToList().Where(mangaId => mangaId != id) : new List<Guid>(),
            UserRatings = existingUser.UserRatings
        };
        await userRepository.UpdateUser(user);
        var updatedFollow = existingManga.Follows - 1;
        Manga manga = new() 
        {
            Id = existingManga.Id,
            Title = existingManga.Title,
            Summary = existingManga.Summary,
            Author =  existingManga.Author,
            Status = existingManga.Status,
            ImageUrl = existingManga.ImageUrl,
            Rating = existingManga.Rating,
            TagIds = existingManga.TagIds,
            CreatedDate = existingManga.CreatedDate,
            Follows = updatedFollow
        };
        await unitOfWork.mangaRepository.UpdateManga(manga);
        return Ok(new {updatedFollow});
    }

    [HttpGet("random")]
    public async Task<ActionResult<Guid>> GetRandomManga() 
    {
        var x = await unitOfWork.mangaRepository.GetRandomManga();
        return x.Id;
    }

    [HttpGet("list")]
    public async IAsyncEnumerable<MangaDto> GetMangaList(int numberOfItem) 
    {
        var mangaList = unitOfWork.mangaRepository.GetMangaList(numberOfItem);
        await foreach (var manga in mangaList) 
        {
            yield return manga.AsDto();
        }
    }

    [HttpGet("recently-added")]
    public async Task<IEnumerable<MangaDto>> GetMangaListByDate(int numberOfItem) 
    {
        var mangaList = (await unitOfWork.mangaRepository.GetMangaListByDate(numberOfItem)).Select(manga => manga.AsDto());
        return mangaList;
    }

    [HttpGet("top-rated")]
    public async Task<IEnumerable<MangaDto>> GetTopRatedList(int numberOfItem) 
    {
        var mangaList = (await unitOfWork.mangaRepository.GetTopRatedManga(numberOfItem)).Select(manga => manga.AsDto());
        return mangaList;
    }
    }
}
