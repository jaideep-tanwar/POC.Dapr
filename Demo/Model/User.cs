namespace Demo.Model
{
    public class User
    {
        public int Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Profilepic { get; set; }
        public string? ProfilePicUrl { get; set; }
        public byte[]? ImageBytes { get; set; }
        public IFormFile MyFile { get; set; }
    }
}
