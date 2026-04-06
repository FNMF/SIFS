using DotNetEnv;
using SIFS.Infrastructure.Persistence.Models;
using SIFS.Shared.Helpers;

namespace SIFS.Infrastructure
{
    public class LocalfileService: ILocalfileService
    {
        private readonly IWebHostEnvironment _env;

        public LocalfileService(IWebHostEnvironment env)
        {
            _env = env;
        }
        public async Task<string> LocalSaveAsync(IFormFile file)
        {
            var folder = Path.Combine(_env.ContentRootPath, "Files");

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            var fileName = UuidV7.NewUuidV7() + Path.GetExtension(file.FileName);
            var fullPath = Path.Combine(folder, fileName);

            using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);

            return "/Files/" + fileName;
        }
    }
}
