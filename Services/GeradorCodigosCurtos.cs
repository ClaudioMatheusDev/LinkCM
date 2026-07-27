namespace LinkCM.Services
{
    public class GeradorCodigosCurtos
    {
        private const string Caracteres = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        public string GerarCodigoCurto(int tamanho = 6)
        {
            var random = new Random();
            var codigoCurto = new char[tamanho];

            for (int i = 0; i < tamanho; i++)
            {
                codigoCurto[i] = Caracteres[random.Next(0, Caracteres.Length)];
            }

            return new string(codigoCurto);
        }

    }
}