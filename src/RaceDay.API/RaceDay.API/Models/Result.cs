using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace RaceDay.API.Models
{
    public class Result
    {
        [Key]
        public int ResultId { get; set; }

        [Required]
        [ForeignKey("Enrolment")]
        public int EnrolmentId { get; set; }

        public TimeSpan? FinishTime { get; set; }

        public int? FinishingPosition { get; set; }

        // 👇 THIS PREVENTS THE VALIDATION ERROR 👇
        [ValidateNever]
        [JsonIgnore]
        public Enrolment? Enrolment { get; set; }
    }
}