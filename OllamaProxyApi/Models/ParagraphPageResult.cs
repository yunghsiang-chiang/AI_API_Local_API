using System.Text.Json.Serialization;

namespace OllamaProxyApi.Models
{
    public class ParagraphPageResult
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int Total { get; set; }
        public List<ParagraphItem> Paragraphs { get; set; } = new();
    }

    public class ParagraphByFileResult
    {
        [JsonPropertyName("file_name")]
        public string FileName { get; set; } = string.Empty;
        public int Count { get; set; }
        public List<ParagraphItem> Paragraphs { get; set; } = new();
    }

    public class ParagraphItem
    {
        public int Pid { get; set; }
        public string Text { get; set; } = string.Empty;
        [JsonPropertyName("source_file")]
        public string SourceFile { get; set; } = "unknown";
    }
}
