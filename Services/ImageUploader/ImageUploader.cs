using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace MangaApplication.Services;
public class CloudinaryImageUploader : IImageUploader
{
    private Cloudinary cloudinary;
    public CloudinaryImageUploader(Cloudinary cloudinary)
    {
        this.cloudinary = cloudinary;
    }

    public async Task<ImageUploadResult> UploadImage(IFormFile imageFile, string? folder)
    {
        ImageUploadResult uploadResult;
        using (var stream = imageFile.OpenReadStream()) 
        {
            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(imageFile.FileName, stream),
                UseFilename = true,
                UniqueFilename = true,
                Overwrite = true,
                Folder = folder
            };
            uploadResult = await cloudinary.UploadAsync(uploadParams);
        }
        return uploadResult;
    }
    public async Task<DelResResult> DeleteImages(List<string> publicIds)
    {
        var delParams = new DelResParams()
        {
            PublicIds = publicIds,
            ResourceType = ResourceType.Image
        };
        return await cloudinary.DeleteResourcesAsync(delParams);
    }
}