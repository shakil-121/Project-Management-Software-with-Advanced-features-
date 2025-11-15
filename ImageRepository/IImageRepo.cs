namespace FastPMS.ImageRepository
{
    public interface IImageRepo
    {
        Task<string> UploadAsync(IFormFile file);
    }
}
