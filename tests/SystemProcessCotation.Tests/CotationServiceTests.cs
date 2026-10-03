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
    public async Task GetCotationAsync_SendsFundamentusRequestHeaders()
    {
        string? requestUri = null;
        string? userAgent = null;
        string? acceptLanguage = null;

        using var client = new HttpClient(new StubHttpMessageHandler(request =>
        {
            requestUri = request.RequestUri?.ToString();
            userAgent = string.Join(" ", request.Headers.GetValues("User-Agent"));
            acceptLanguage = string.Join(",", request.Headers.GetValues("Accept-Language"));

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                    <html>
                      <body>
                        <table>
                          <tr>
                            <td>Cotação</td>
                            <td>31,42</td>
                          </tr>
                        </table>
                      </body>
                    </html>
                    """)
            };
        }));
        var service = new global::CotationService(client);

        await service.GetCotationAsync(" petr4 ");

        Assert.Equal("https://www.fundamentus.com.br/detalhes.php?papel=PETR4", requestUri);
        Assert.Contains("Mozilla/5.0", userAgent);
        Assert.Contains("pt-BR", acceptLanguage);
    }

    [Fact]
    public async Task GetCotationAsync_NormalizesB3SuffixBeforeRequest()
    {
        string? requestUri = null;

        using var client = new HttpClient(new StubHttpMessageHandler(request =>
        {
            requestUri = request.RequestUri?.ToString();

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                    <html>
                      <body>
                        <table>
                          <tr>
                            <td>Cotação</td>
                            <td>31,42</td>
                          </tr>
                        </table>
                      </body>
                    </html>
                    """)
            };
        }));
        var service = new global::CotationService(client);

        var result = await service.GetCotationAsync(" petr4.sa ");

        Assert.Equal("PETR4", result.Symbol);
        Assert.Equal("https://www.fundamentus.com.br/detalhes.php?papel=PETR4", requestUri);
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
    public async Task GetCotationAsync_PrefersNestedPriceTextWhenCellHasVariation()
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
                            <td><span class="txt">31,42</span><span>+0,50%</span></td>
                          </tr>
                        </table>
                      </body>
                    </html>
                    """)
            }));
        var service = new global::CotationService(client);

        var result = await service.GetCotationAsync("PETR4");

        Assert.Equal(31.42, result.Price);
    }

    [Fact]
    public async Task GetCotationAsync_ScansLaterPriceTextSpans()
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
                            <td>
                              <span class="txt"> </span>
                              <span class="txt">+0,50%</span>
                              <span class="txt">31,42</span>
                            </td>
                          </tr>
                        </table>
                      </body>
                    </html>
                    """)
            }));
        var service = new global::CotationService(client);

        var result = await service.GetCotationAsync("PETR4");

        Assert.Equal(31.42, result.Price);
    }

    [Fact]
    public async Task GetCotationAsync_FallsBackToCellTextWhenPriceSpanHasOnlyVariation()
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
                            <td><span class="txt">+0,50%</span> 31,42</td>
                          </tr>
                        </table>
                      </body>
                    </html>
                    """)
            }));
        var service = new global::CotationService(client);

        var result = await service.GetCotationAsync("PETR4");

        Assert.Equal(31.42, result.Price);
    }

    [Fact]
    public async Task GetCotationAsync_ParsesPlainCotationCell()
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
                            <td>31,42</td>
                          </tr>
                        </table>
                      </body>
                    </html>
                    """)
            }));
        var service = new global::CotationService(client);

        var result = await service.GetCotationAsync("PETR4");

        Assert.Equal(31.42, result.Price);
    }

    [Fact]
    public async Task GetCotationAsync_ParsesHeaderCotationLabel()
    {
        using var client = new HttpClient(new StubHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                    <html>
                      <body>
                        <table>
                          <tr>
                            <th>Cotação</th>
                            <td>31,42</td>
                          </tr>
                        </table>
                      </body>
                    </html>
                    """)
            }));
        var service = new global::CotationService(client);

        var result = await service.GetCotationAsync("PETR4");

        Assert.Equal(31.42, result.Price);
    }

    [Fact]
    public async Task GetCotationAsync_ParsesCotationLabelWithoutAccentOrCaseMatch()
    {
        using var client = new HttpClient(new StubHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                    <html>
                      <body>
                        <table>
                          <tr>
                            <td>cotacao:</td>
                            <td>31,42</td>
                          </tr>
                        </table>
                      </body>
                    </html>
                    """)
            }));
        var service = new global::CotationService(client);

        var result = await service.GetCotationAsync("PETR4");

        Assert.Equal(31.42, result.Price);
    }

    [Fact]
    public async Task GetCotationAsync_ParsesCurrentCotationLabelVariant()
    {
        using var client = new HttpClient(new StubHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                    <html>
                      <body>
                        <table>
                          <tr>
                            <td>Cotação atual</td>
                            <td>31,42</td>
                          </tr>
                        </table>
                      </body>
                    </html>
                    """)
            }));
        var service = new global::CotationService(client);

        var result = await service.GetCotationAsync("PETR4");

        Assert.Equal(31.42, result.Price);
    }

    [Fact]
    public async Task GetCotationAsync_ParsesCotationLabelWithCurrencyUnit()
    {
        using var client = new HttpClient(new StubHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                    <html>
                      <body>
                        <table>
                          <tr>
                            <td>Cotação (R$)</td>
                            <td>31,42</td>
                          </tr>
                        </table>
                      </body>
                    </html>
                    """)
            }));
        var service = new global::CotationService(client);

        var result = await service.GetCotationAsync("PETR4");

        Assert.Equal(31.42, result.Price);
    }

    [Theory]
    [InlineData("Preço atual")]
    [InlineData("Valor atual")]
    [InlineData("Último preço")]
    [InlineData("Ultimo valor")]
    public async Task GetCotationAsync_ParsesCurrentPriceLabelVariants(string label)
    {
        using var client = new HttpClient(new StubHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent($"""
                    <html>
                      <body>
                        <table>
                          <tr>
                            <td>{label}</td>
                            <td>31,42</td>
                          </tr>
                        </table>
                      </body>
                    </html>
                    """)
            }));
        var service = new global::CotationService(client);

        var result = await service.GetCotationAsync("PETR4");

        Assert.Equal(31.42, result.Price);
    }

    [Fact]
    public async Task GetCotationAsync_ParsesInlineLabeledPriceCell()
    {
        using var client = new HttpClient(new StubHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                    <html>
                      <body>
                        <table>
                          <tr>
                            <td><span>Cotação:</span><span class="txt">31,42</span></td>
                          </tr>
                        </table>
                      </body>
                    </html>
                    """)
            }));
        var service = new global::CotationService(client);

        var result = await service.GetCotationAsync("PETR4");

        Assert.Equal(31.42, result.Price);
    }

    [Fact]
    public async Task GetCotationAsync_ParsesPlainInlineLabeledPriceCell()
    {
        using var client = new HttpClient(new StubHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                    <html>
                      <body>
                        <table>
                          <tr>
                            <td>Cotação: 31,42</td>
                          </tr>
                        </table>
                      </body>
                    </html>
                    """)
            }));
        var service = new global::CotationService(client);

        var result = await service.GetCotationAsync("PETR4");

        Assert.Equal(31.42, result.Price);
    }

    [Theory]
    [InlineData("Cotação R$ 31,42")]
    [InlineData("Cotação BRL 31,42")]
    [InlineData("Preço atual 31,42")]
    public async Task GetCotationAsync_ParsesInlineLabeledPriceWithoutColon(string labelAndPrice)
    {
        using var client = new HttpClient(new StubHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent($"""
                    <html>
                      <body>
                        <table>
                          <tr>
                            <td>{labelAndPrice}</td>
                          </tr>
                        </table>
                      </body>
                    </html>
                    """)
            }));
        var service = new global::CotationService(client);

        var result = await service.GetCotationAsync("PETR4");

        Assert.Equal(31.42, result.Price);
    }

    [Fact]
    public async Task GetCotationAsync_FallsBackWhenLabeledCotationCellIsBlank()
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
                            <td>   </td>
                            <td class="data destaque w3"><span class="txt">31,42</span></td>
                          </tr>
                        </table>
                      </body>
                    </html>
                    """)
            }));
        var service = new global::CotationService(client);

        var result = await service.GetCotationAsync("PETR4");

        Assert.Equal(31.42, result.Price);
    }

    [Fact]
    public async Task GetCotationAsync_FallsBackWhenLabeledCotationCellIsMalformed()
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
                            <td><span class="txt">indisponível</span></td>
                          </tr>
                          <tr>
                            <td class="data destaque w3"><span class="txt">31,42</span></td>
                          </tr>
                        </table>
                      </body>
                    </html>
                    """)
            }));
        var service = new global::CotationService(client);

        var result = await service.GetCotationAsync("PETR4");

        Assert.Equal(31.42, result.Price);
    }

    [Fact]
    public async Task GetCotationAsync_FallsBackToLaterHighlightedPriceCells()
    {
        using var client = new HttpClient(new StubHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                    <html>
                      <body>
                        <table>
                          <tr>
                            <td class="data destaque w3"><span class="txt">indisponível</span></td>
                          </tr>
                        </table>
                        <table>
                          <tr>
                            <td class="data destaque w3"><span class="txt">31,42</span></td>
                          </tr>
                        </table>
                      </body>
                    </html>
                    """)
            }));
        var service = new global::CotationService(client);

        var result = await service.GetCotationAsync("PETR4");

        Assert.Equal(31.42, result.Price);
    }

    [Fact]
    public async Task GetCotationAsync_FallsBackToHighlightedPriceCellTextWithoutSpan()
    {
        using var client = new HttpClient(new StubHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                    <html>
                      <body>
                        <table>
                          <tr>
                            <td class="data destaque w3">31,42 +0,50%</td>
                          </tr>
                        </table>
                      </body>
                    </html>
                    """)
            }));
        var service = new global::CotationService(client);

        var result = await service.GetCotationAsync("PETR4");

        Assert.Equal(31.42, result.Price);
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

    [Theory]
    [InlineData("PETR 4")]
    [InlineData("PETR-4")]
    [InlineData("PETR4.BR")]
    public async Task GetCotationAsync_RejectsSymbolsWithInvalidCharacters(string symbol)
    {
        using var client = new HttpClient(new StubHttpMessageHandler(_ =>
            throw new InvalidOperationException("HTTP should not be called for invalid symbols.")));
        var service = new global::CotationService(client);

        var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.GetCotationAsync(symbol));

        Assert.Contains("letras e números", exception.Message);
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
