using System.ComponentModel.DataAnnotations;

namespace MangaApplication;

public sealed class CheckStatus : ValidationAttribute
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    public CheckStatus() 
    {
        _httpContextAccessor = new HttpContextAccessor();
    }
    public string[] ValidStatus = new string[]
    {
        "ongoing", 
        "completed", 
        "hiatus", 
        "cancelled"
    };
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if ((value is not null && ValidStatus.Contains(value.ToString().Trim().ToLower())) || (httpContext.Request.Method == "PUT" && (value is null || value == "")))
        {
            return ValidationResult.Success;
        }
        return new ValidationResult("Invalid status.");
    }
}

public sealed class CheckTag : ValidationAttribute
{
    public string[] ValidTags = new string[] 
    {
        "format",
        "genre",
        "theme",
    };
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not null && ValidTags.Contains(value.ToString().Trim().ToLower()))
        {
            return ValidationResult.Success;
        }
        return new ValidationResult("Invalid tag type.");
    }
}