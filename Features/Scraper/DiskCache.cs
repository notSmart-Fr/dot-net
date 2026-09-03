namespace TaskApi.Features.Scraper;

public class DiskCache
{
    private readonly string _cacheDir = Path.Combine(Directory.GetCurrentDirectory(), "cache");

    public DiskCache() => Directory.CreateDirectory(_cacheDir);

    public async Task<string?> GetAsync(string key, CancellationToken ct)
    {
        var filePath = GetPath(key);
        return File.Exists(filePath) ? await File.ReadAllTextAsync(filePath, ct) : null;
    }

    public async Task SaveAsync(string key, string content, CancellationToken ct)
    {
        await File.WriteAllTextAsync(GetPath(key), content, ct);
    }

    private string GetPath(string key) => 
        Path.Combine(_cacheDir, $"{SanitizeFileName(key)}.html");

    private static string SanitizeFileName(string name) => 
        string.Concat(name.Split(Path.GetInvalidFileNameChars()));
}