namespace LinkCM.Models
{
    public class URLCurta
    {
        public int Id { get; set; }
        public string UrlOriginal { get; set; } = string.Empty;
        public string UrlOtimizada { get; set; } = string.Empty;
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
        public DateTime DataExpira { get; set; }
        public int QuantidadeCliques { get; set; } = 0;
        public DateTime UltimoAcesso { get; set; }
        public string ShortCode { get; set; } = string.Empty;
        public bool Ativo { get; set; }
    }
}
