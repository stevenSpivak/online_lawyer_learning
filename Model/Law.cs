namespace Model;

public class Law : BaseEntity
{
    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public DateTime? CreatedAt { get; set; }
}