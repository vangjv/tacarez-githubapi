namespace TacarEZGithubAPI.Models
{
    public class GithubContent
    {
        public string message { get; set; }
        public string content { get; set; }
        public string sha { get; set; }
        public Committer committer { get; set; }

    }

    public class Committer
    {
        public string name { get; set; }
        public string email { get; set; }
        public string date { get; set; }
        public Author author { get; set; }
    }

    public class Author
    {
        public string name { get; set; }
        public string email { get; set; }
        public string date { get; set; }
    }
}
