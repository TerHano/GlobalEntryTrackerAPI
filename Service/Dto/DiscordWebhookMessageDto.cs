namespace Service.Dto;

public class DiscordWebhookMessageDto
{
    public string? Username { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Content { get; set; }
    public List<Embed>? Embeds { get; set; }


    public class Embed
    {
        public int Color { get; set; }
        public Author? Author { get; set; }
        public string? Title { get; set; }
        public string? Url { get; set; }
        public string? Description { get; set; }
        public List<Field>? Fields { get; set; }
    }


    public class Author
    {
        public string? Name { get; set; }
        public string? Url { get; set; }
        public string? IconUrl { get; set; }
    }


    public class Field
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public bool Inline { get; set; }
    }
}