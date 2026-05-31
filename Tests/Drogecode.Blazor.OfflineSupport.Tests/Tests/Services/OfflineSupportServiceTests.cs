namespace Drogecode.Blazor.OfflineSupport.Tests.Tests.Services;

public class OfflineSupportServiceTests : IDisposable
{
    private readonly IOfflineSupportService _offlineSupportService;

    public OfflineSupportServiceTests(IOfflineSupportService offlineSupportService)
    {
        _offlineSupportService = offlineSupportService;
        OfflineSupportService.LogToConsole = true;
    }

    public void Dispose()
    {
        OfflineSupportService.Postfix = null;
        OfflineSupportService.LogToConsole = false;
        // We can't easily reset IsOffline because it has a private setter and no public reset method, 
        // but we can trigger it to be false by a successful request.
    }

    [Fact]
    public async Task MinimalRequestTest()
    {
        const string cacheKey = "MinimalRequestTest";
        var response = await _offlineSupportService.CachedRequestAsync<string>(cacheKey, () => Task.FromResult("test"), clt: TestContext.Current.CancellationToken);
        Assert.NotNull(response);
        response.Should().Be("test");
    }

    [Fact]
    public async Task ByFunctionTest()
    {
        const string cacheKey = "ByFunctionTest";
        var response = await _offlineSupportService.CachedRequestAsync<TestStringResponse>(cacheKey, () => Task.FromResult(new TestStringResponse
        {
            Data = "test"
        }), clt: TestContext.Current.CancellationToken);
        Assert.NotNull(response?.Data);
        response.HandledBy.Should().Be(HandledBy.Function);
        response.Data.Should().Be("test");
    }

    [Fact]
    public async Task FromCacheTest()
    {
        const string cacheKey = "FromCacheTest";
        var addToCache = await _offlineSupportService.CachedRequestAsync<TestStringResponse>(cacheKey, () => Task.FromResult(new TestStringResponse
            {
                Data = "test"
            }),
            clt: TestContext.Current.CancellationToken);
        Assert.NotNull(addToCache?.Data);
        addToCache.HandledBy.Should().Be(HandledBy.Function);
        var response = await _offlineSupportService.CachedRequestAsync<TestStringResponse>(cacheKey, () => Task.FromResult(new TestStringResponse
            {
                Data = "not called"
            }),
            new CachedRequest { OneCallPerLocalStorage = true },
            clt: TestContext.Current.CancellationToken);
        Assert.NotNull(response?.Data);
        response.HandledBy.Should().Be(HandledBy.LocalStorage);
        response.Data.Should().Be("test");
    }

    [Fact]
    public async Task PostfixTest()
    {
        const string cacheKey = "PostfixTest";
        OfflineSupportService.Postfix = "user1";
        await _offlineSupportService.CachedRequestAsync<TestStringResponse>(cacheKey, () => Task.FromResult(new TestStringResponse { Data = "data1" }), clt: TestContext.Current.CancellationToken);

        OfflineSupportService.Postfix = "user2";
        var response2 = await _offlineSupportService.CachedRequestAsync<TestStringResponse>(cacheKey, () => Task.FromResult(new TestStringResponse { Data = "data2" }), clt: TestContext.Current.CancellationToken);

        Assert.NotNull(response2?.Data);
        response2.Data.Should().Be("data2");
        response2.HandledBy.Should().Be(HandledBy.Function);

        OfflineSupportService.Postfix = "user1";
        var response1FromCache = await _offlineSupportService.CachedRequestAsync<TestStringResponse>(cacheKey, () => Task.FromResult(new TestStringResponse { Data = "not called" }), new CachedRequest { OneCallPerLocalStorage = true }, clt: TestContext.Current.CancellationToken);
        Assert.NotNull(response1FromCache?.Data);
        response1FromCache.Data.Should().Be("data1");
        response1FromCache.HandledBy.Should().Be(HandledBy.LocalStorage);
    }

    [Fact]
    public async Task IgnoreCacheTest()
    {
        const string cacheKey = "IgnoreCacheTest";
        await _offlineSupportService.CachedRequestAsync<TestStringResponse>(cacheKey, () => Task.FromResult(new TestStringResponse { Data = "cached" }), new CachedRequest { OneCallPerLocalStorage = true }, clt: TestContext.Current.CancellationToken);

        var response = await _offlineSupportService.CachedRequestAsync<TestStringResponse>(cacheKey, () => Task.FromResult(new TestStringResponse { Data = "fresh" }), new CachedRequest { IgnoreCache = true }, clt: TestContext.Current.CancellationToken);
        Assert.NotNull(response?.Data);
        response.Data.Should().Be("fresh");
        response.HandledBy.Should().Be(HandledBy.Function);
    }

    [Fact]
    public async Task HttpRequestExceptionSetsOfflineTest()
    {
        const string cacheKey = "OfflineTest";
        try
        {
            await _offlineSupportService.CachedRequestAsync<TestStringResponse>(cacheKey, () => throw new HttpRequestException(), clt: TestContext.Current.CancellationToken);
        }
        catch (HttpRequestException) { }

        OfflineSupportService.IsOffline.Should().BeTrue();

        // Recover
        await _offlineSupportService.CachedRequestAsync<TestStringResponse>(cacheKey, () => Task.FromResult(new TestStringResponse { Data = "recovered" }), clt: TestContext.Current.CancellationToken);
        OfflineSupportService.IsOffline.Should().BeFalse();
    }

    [Fact]
    public async Task CancellationTokenTest()
    {
        const string cacheKey = "CancellationTokenTest";
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var response = await _offlineSupportService.CachedRequestAsync<TestStringResponse>(cacheKey, () => Task.FromResult(new TestStringResponse { Data = "test" }), clt: cts.Token);

        response.Should().BeNull();
    }

    [Fact]
    public async Task OneCallPerSessionTest()
    {
        const string cacheKey = "OneCallPerSessionTest";
        var response = await _offlineSupportService.CachedRequestAsync<string>(cacheKey, () => Task.FromResult("test"), new CachedRequest{OneCallPerSession = true, IgnoreCache = false, CachedAndReplace = false}, clt: TestContext.Current.CancellationToken);
        Assert.NotNull(response);
        response.Should().Be("test");
    }
}