using System.ComponentModel.DataAnnotations;

namespace MangaApplication.DTOs;

public record TagDto(Guid Id, string Type, string Name);
public record CreateTagDto([CheckTag]string Type, string Name);
public record UpdateTagDto([CheckTag]string? Type, string? Name);