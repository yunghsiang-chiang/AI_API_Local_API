namespace OllamaProxyApi.Models
{
    public class ParagraphPageResult
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int Total { get; set; }
        public Dictionary<int, string> Paragraphs { get; set; }
    }


    public class Paragraph
    {
        public string Content { get; set; }
        public List<float> Embedding { get; set; }
    }

}
