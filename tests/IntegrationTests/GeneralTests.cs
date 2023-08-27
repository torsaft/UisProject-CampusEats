namespace IntegrationTests;

public sealed class GeneralTests : BaseIntegrationTest
{
    private readonly HttpClient _client;
    public GeneralTests(CampusEatsFactory factory) : base(factory)
    {
        _client = factory.HttpClient;
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/Products")]
    [InlineData("/Orders")]
    [InlineData("/Cart")]
    public async Task AllPages_Should_RenderAsync(string url)
    {
        _ = await UseAdmin();
        var response = await _client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        response.Content.Headers.ContentType!.ToString().Should().Be("text/html; charset=utf-8");
    }
}
