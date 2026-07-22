namespace LinkCM.DTOs
{
    public class URLCurtaRequisicao
    {
        public string Url { get; set; } = string.Empty;
        public string? CustomCode { get; set; }
        public DateTime? DataExpira { get; set; }
    }
}
