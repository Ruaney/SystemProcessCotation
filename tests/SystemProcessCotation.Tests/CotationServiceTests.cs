using System.Net;

namespace SystemProcessCotation.Tests;

public class CotationServiceTests
{
    [Fact]
    public async Task GetCotationAsync_ParsesBrazilianPriceAndNormalizesSymbol()
    {
        using var client = new HttpClient(new StubHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                    <html>
                      <body>
                        <table>
                          <tr>
                            <td class="data destaque w3"><span class="txt">1.234,56</span></td>
                          </tr>
                        </table>
                      </body>
                    </html>
                    """)
            }));
        var service = new global::CotationService(client);

        var result = await service.GetCotationAsync(" petr4 ");

        Assert.Equal("PETR4", result.Symbol);
        Assert.Equal(1234.56, result.Price);
    }

    [Fact]
    public async Task GetCotationAsync_ParsesCurrencyFormattedPrice()
    {
        using var client = new HttpClient(new StubHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                    <html>
                      <body>
                        <table>
                          <tr>
                            <td>Cotação</td>
                            <td><span class="txt">R$ 1.234,56</span></td>
                          </tr>
                        </table>
                      </body>
                    </html>
                    """)
            }));
        var service = new global::CotationService(client);

        var result = await service.GetCotationAsync("PETR4");

        Assert.Equal(1234.56, result.Price);
    }

    [Fact]
    public async Task GetCotationAsync_ThrowsWhenResponseIsNotSuccessful()
    {
        using var client = new HttpClient(new StubHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.InternalServerError)));
        var service = new global::CotationService(client);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.GetCotationAsync("PETR4"));

        Assert.Contains("Erro ao buscar cotação", exception.Message);
    }

    [Fact]
    public async Task GetCotationAsync_RejectsEmptySymbol()
    {
        using var client = new HttpClient(new StubHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)));
        var service = new global::CotationService(client);

        await Assert.ThrowsAsync<ArgumentException>(() => service.GetCotationAsync(" "));
    }

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _factory;

        public StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> factory)
        {
            _factory = factory;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(_factory(request));
    }
}
