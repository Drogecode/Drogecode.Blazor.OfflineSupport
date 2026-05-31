using Drogecode.Blazor.OfflineSupport;

namespace OfflineSupport.Demo.Models;

public class DemoModelForStorage : ICacheableResponse
{
    public HandledBy HandledBy { get; set; }
    public string? Data { get; set; } = "Some text to store in the cache.";
}