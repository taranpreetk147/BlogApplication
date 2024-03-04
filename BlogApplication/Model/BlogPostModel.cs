namespace BlogApplication.Model
{
    public class BlogPostModel
    {
        public int PostId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public byte[] ImageURL { get; set; }
        public int Likes { get; set; }
        public bool IsLiked { get; set; } // Indicates whether the current user has liked the post
        public string Comments { get; set; }
    }
}
