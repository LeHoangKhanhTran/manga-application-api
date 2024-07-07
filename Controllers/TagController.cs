using System.Net;
using CloudinaryDotNet.Actions;
using MangaApplication.DTOs;
using MangaApplication.Entities;
using MangaApplication.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MangaApplication.Controllers;

[ApiController]
[Route("api/tag")]
public class TagController : ControllerBase
{
    private readonly ITagRepository tagRepository;
    private readonly ILogger<TagController> logger;
    public TagController(IUnitOfWork unitOfWork, ILogger<TagController> logger)
    {
        this.tagRepository = ((UnitOfWork)unitOfWork).tagRepository;
        this.logger = logger;
    }

    // [Authorize(Policy = "AdministratorPolicy")]
    [HttpGet]
    public async Task<IEnumerable<TagDto>> GetAllTags()
    {
        var tags = (await tagRepository.GetAllTags()).Select(tag => tag.AsDto());
        return tags;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TagDto>> GetTagById(Guid id)
    {
        var tag = await tagRepository.GetTagById(id);
        if (tag is null)
        {
            return NotFound();
        }
        return tag.AsDto();
    }

    [HttpPost]
    public async Task<ActionResult<TagDto>> CreateTag(CreateTagDto tagDto)
    {
        if (ModelState.IsValid)
        {
            Tag tag = new()
            {
                Id = Guid.NewGuid(),
                Type = tagDto.Type,
                Name = tagDto.Name
            };
            await tagRepository.CreateTag(tag);
            return CreatedAtAction(nameof(GetTagById), new {id = tag.Id}, tag.AsDto()); 
        }
        return UnprocessableEntity(ModelState);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateTag(Guid id, UpdateTagDto tagDto)
    {
        var exisitingTag = await tagRepository.GetTagById(id);
        if (exisitingTag is null)
        {
            return NotFound();
        }
        if (ModelState.IsValid)
        {
            Tag tag = new()
            {
                Id = exisitingTag.Id,
                Type = tagDto.Type != null && tagDto.Type.Length > 0 ? tagDto.Type : exisitingTag.Type,
                Name = tagDto.Name != null && tagDto.Name.Length > 0 ? tagDto.Name : exisitingTag.Name,
            };
            await tagRepository.UpdateTag(tag);
        }
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteTag(Guid id)
    {
        var exisitingTag = await tagRepository.GetTagById(id);
        if (exisitingTag is null)
        {
            return NotFound();
        }
        await tagRepository.DeleteTag(exisitingTag.Id);
        return NoContent();
    }
}