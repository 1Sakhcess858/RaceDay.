namespace RaceDay.Web.DTOs
{
    public class EnrolmentDto
    {
        public int EnrolmentId { get; set; }
        public int ParticipantId { get; set; }
        public string? ParticipantName { get; set; }
        public int EventId { get; set; }
        public string? EventName { get; set; } // 👈 Added back
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public DateTime EnrolmentDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public ResultDto? Result { get; set; }
    }
}