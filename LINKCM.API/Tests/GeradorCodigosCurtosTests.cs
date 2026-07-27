using LinkCM.Services;
using Xunit;

namespace LinkCM.Tests
{
    public class GeradorCodigosCurtosTests
    {
        [Fact]
        public void DeveGerarCodigoComTamanhoPadrao()
        {
            var gerador = new GeradorCodigosCurtos();

            var codigo = gerador.GerarCodigoCurto();

            Assert.Equal(GeradorCodigosCurtos.TamanhoPadrao, codigo.Length);
            Assert.True(GeradorCodigosCurtos.CodigoPersonalizadoValido(codigo));
        }

        [Theory]
        [InlineData("abc123", true)]
        [InlineData("ABC123", true)]
        [InlineData("abc12", false)]
        [InlineData("abc1234", false)]
        [InlineData("abc-12", false)]
        [InlineData("abc 12", false)]
        public void DeveValidarCodigoPersonalizado(string codigo, bool esperado)
        {
            var resultado = GeradorCodigosCurtos.CodigoPersonalizadoValido(codigo);

            Assert.Equal(esperado, resultado);
        }
    }
}
