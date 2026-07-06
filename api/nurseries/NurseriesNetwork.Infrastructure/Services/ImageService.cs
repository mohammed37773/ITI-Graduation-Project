using Microsoft.Extensions.Configuration;
using NurseriesNetwork.Core.Interfaces.Services;

namespace NurseriesNetwork.Infrastructure.Services;

public class ImageService : IImageService
{
    private readonly IConfiguration _config;

    public ImageService(IConfiguration config)
    {
        _config = config;
    }

    public async Task<string> UploadImageAsync(
        Stream imageStream, string fileName)
    {
        var storagePath = _config["Images:StoragePath"]
            ?? "wwwroot/images/nurseries";

        if (!Directory.Exists(storagePath))
            Directory.CreateDirectory(storagePath);

        var uniqueFileName =
            $"{Guid.NewGuid()}_{fileName}"; // todo remove original file name from the uploaded image
        var filePath = Path.Combine(storagePath, uniqueFileName);

        using var fileStream = new FileStream(filePath, FileMode.Create);
        await imageStream.CopyToAsync(fileStream);

        return $"/images/nurseries/{uniqueFileName}";
    }

    public Task DeleteImageAsync(string imageUrl)
    {
        var fileName = Path.GetFileName(imageUrl);
        var storagePath = _config["Images:StoragePath"]
            ?? "wwwroot/images/nurseries";
        var filePath = Path.Combine(storagePath, fileName);

        if (File.Exists(filePath))
            File.Delete(filePath);

        return Task.CompletedTask;
    }
}