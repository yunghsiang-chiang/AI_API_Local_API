namespace OllamaProxyApi.Models
{
    public class AskRequest
    {
        public string Question { get; set; }
        public int TopK { get; set; } = 15;
    }
}
