namespace LinkCM.DTOs
{
    public class RespostaURLCurta
    {
        public int Id { get; set; }
        public string UrlCurta { get; set; } = string.Empty;
        public string UrlOriginal { get; set; } = string.Empty;
        public string ShortCode { get; set; } = string.Empty;
        public DateTime DataCriacao { get; set; }
        public DateTime? DataExpira { get; set; }
        public int ContasAcessadas { get; set; }
    }
}
