using System.Security.Cryptography;

namespace LinkCM.Services
{
    public class GeradorCodigosCurtos
    {
        public const int TamanhoPadrao = 6;
        private const string Caracteres = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        public string GerarCodigoCurto(int tamanho = TamanhoPadrao)
        {
            var codigoCurto = new char[tamanho];

            for (int i = 0; i < tamanho; i++)
            {
                codigoCurto[i] = Caracteres[RandomNumberGenerator.GetInt32(Caracteres.Length)];
            }

            return new string(codigoCurto);
        }

        public static bool CodigoPersonalizadoValido(string codigo)
        {
            return codigo.Length == TamanhoPadrao &&
                codigo.All(Caracteres.Contains);
        }
    }
}
