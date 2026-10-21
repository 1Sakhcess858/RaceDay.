namespace RaceDay.Web.DTOs
{
    public class CategoryDto
    {
        public int CategoryId { get; set; } // 👈 Added this
        public int EventId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}