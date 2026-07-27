using Xunit;

namespace LinkCM.Tests
{
    public class ValidatorURL
    {
        [Theory]
        [InlineData("https://www.exemplo.com", true)]
        [InlineData("http://www.exemplo.com", true)]
        [InlineData("htp://www.exemplo.com", false)]
        [InlineData("www.exemplo.com", false)]
        public void DeveValidarUrlHttpOuHttps(string url, bool esperado)
        {
            var resultado = Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
                            (uri.Scheme == Uri.UriSchemeHttp ||
                             uri.Scheme == Uri.UriSchemeHttps);

            Assert.Equal(esperado, resultado);
        }

    }
}