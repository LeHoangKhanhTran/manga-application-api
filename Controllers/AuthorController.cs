using MangaApplication.DTOs;
using MangaApplication.Entities;
using MangaApplication.Services;
using Microsoft.AspNetCore.Mvc;

namespace MangaApplication.Controllers
{
    [ApiController]
    [Route("api/author")]
    public class AuthorController : ControllerBase
    {
        private readonly UnitOfWork unitOfWork;
        private readonly IImageUploader imageUploader;
        private readonly ILogger<AuthorController> logger;
        public AuthorController(IUnitOfWork unitOfWork, IImageUploader imageUploader, ILogger<AuthorController> logger)
        {
            this.unitOfWork = (UnitOfWork)unitOfWork;
            this.imageUploader = imageUploader;
            this.logger = logger;
        }

        [HttpGet]
        public async Task<IEnumerable<AuthorDto>> GetAllAuthors()
        {
            var authorList = (await unitOfWork.authorRepository.GetAllAuthors()).Select(author => author.AsDto());
            return authorList;
        } 

        [HttpGet("search")]
        public async Task<IEnumerable<AuthorDto>> SearchAuthorsByName(string name)
        {
            var authorList = (await unitOfWork.authorRepository.SearchAuthorsByName(name)).Select(author => author.AsDto());
            return authorList;
        }
        
        [HttpGet("{id}")]
        public async Task<ActionResult<AuthorDto>> GetAuthorById(Guid id)
        {
            var author = await unitOfWork.authorRepository.GetAuthorById(id);
            if (author is null)
            {
                return NoContent();
            }
            return author.AsDto();
            
        }

        [HttpPost]
        public async Task<ActionResult<AuthorDto>> CreateAuthor([FromForm] CreateAuthorDto authorDto)
        {
            string avatarUrl = "";
            try 
            {
                if (authorDto.Avatar is not null)
                {
                    var uploadResult = await imageUploader.UploadImage(authorDto.Avatar, "author");
                    avatarUrl = uploadResult.Url.ToString();
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
            }
            Author author = new()
            {
                Id = Guid.NewGuid(),
                Name = authorDto.Name,
                Works = new List<Guid>(),
                Biography = authorDto.Biography != null && authorDto.Biography.Length > 0 ? authorDto.Biography : "",
                AvatarUrl = avatarUrl
            };
            await unitOfWork.authorRepository.CreateAuthor(author);
            return CreatedAtAction(nameof(GetAuthorById), new {id = author.Id}, author.AsDto());
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateAuthor(Guid id, [FromForm] UpdateAuthorDto authorDto)
        {
            var existingAuthor = await unitOfWork.authorRepository.GetAuthorById(id);
            if (existingAuthor is null)
            {
                return NotFound();
            }
            if (authorDto.Name != null && authorDto.Name != existingAuthor.Name)
            {
                foreach (Guid mangaId in existingAuthor.Works)
                {
                    var existingManga = await unitOfWork.mangaRepository.GetMangaByID(mangaId);
                    Manga manga = new()
                    {
                        Id = mangaId,
                        Title = existingManga.Title,
                        Summary = existingManga.Summary,
                        Author = new MangaAuthor(id, authorDto.Name),
                        Status = existingManga.Status,
                        ImageUrl = existingManga.ImageUrl,
                        Rating = existingManga.Rating,
                        CreatedDate = existingManga.CreatedDate
                    };
                    await unitOfWork.mangaRepository.UpdateManga(manga);
                }
            }
            string avatarUrl = "";
            try 
            {
                if (authorDto.Avatar is not null)
                {
                    var uploadResult = await imageUploader.UploadImage(authorDto.Avatar, "author");
                    avatarUrl = uploadResult.Url.ToString();
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
            }
            Author author = new()
            { 
                Id = existingAuthor.Id,
                Name = authorDto.Name != null && authorDto.Name.Length > 0 ? authorDto.Name : existingAuthor.Name,
                Works = existingAuthor.Works, 
                Biography = authorDto.Biography != null && authorDto.Biography.Length > 0 ? authorDto.Biography : existingAuthor.Biography,
                AvatarUrl = avatarUrl != "" ? avatarUrl : existingAuthor.AvatarUrl
            };
            await unitOfWork.authorRepository.UpdateAuthor(author);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAuthor(Guid id)
        {
            var existingAuthor = await unitOfWork.authorRepository.GetAuthorById(id);
            if (existingAuthor is null)
            {
               return NotFound();
            }
            await unitOfWork.authorRepository.DeleteAuthor(id);
            return NoContent();
        }
    }
}