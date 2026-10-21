using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation; // 👈 Add this

namespace RaceDay.API.Models
{
    public class Event
    {
        [Key]
        public int EventId { get; set; }

        [Required]
        [ForeignKey("Organiser")]
        public int OrganiserId { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public DateTime EventDate { get; set; }

        [Required]
        public string Location { get; set; } = string.Empty;

        [Required]
        public decimal Distance { get; set; }

        [Required]
        public string EventType { get; set; } = string.Empty;

        public string? BannerImageURL { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // 👇 ADD [ValidateNever] AND [JsonIgnore] TO PREVENT ERRORS 👇
        [ValidateNever]
        [JsonIgnore]
        public Organiser? Organiser { get; set; }
        
        [ValidateNever]
        [JsonIgnore]
        public ICollection<Category> Categories { get; set; } = new List<Category>();
        
        [ValidateNever]
        [JsonIgnore]
        public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
    }
}