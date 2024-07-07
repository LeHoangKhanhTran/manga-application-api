using CloudinaryDotNet.Actions;

namespace MangaApplication.Services;
public interface IImageUploader 
{
    Task<ImageUploadResult> UploadImage(IFormFile imageFile, string? folder);
    Task<DelResResult> DeleteImages(List<string> publicIds);
}