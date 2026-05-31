namespace Drogecode.Blazor.OfflineSupport.Models;

public class ExpiryStorageModel<T>
{
    public long Ttl { get; set; }
    public required T Data { get; set; }
}
