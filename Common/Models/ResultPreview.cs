namespace Common.Models
{
    public class ResultPreview
    {
        public (string path, string body)[] Results { get; set; }

        public class Request
        {
            public string[] path { get; set; }
        }
    }
}