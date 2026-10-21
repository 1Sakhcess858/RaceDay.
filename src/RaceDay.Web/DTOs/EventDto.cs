namespace RaceDay.Web.DTOs
{
    public class EventDto
    {
        public int EventId { get; set; }
        public int OrganiserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime EventDate { get; set; }
        public string Location { get; set; } = string.Empty;
        public decimal Distance { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string? BannerImageURL { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? OrganiserName { get; set; }
        public List<CategoryDto>? Categories { get; set; }
        public List<EnrolmentDto>? Enrolments { get; set; }
    }
}