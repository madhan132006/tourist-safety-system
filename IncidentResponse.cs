using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TouristSafetySystem.Models
{
    public class IncidentResponse
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int IncidentId { get; set; }

        [ForeignKey("IncidentId")]
        public virtual Incident Incident { get; set; }

        [Required]
        [StringLength(100)]
        public string ResponderName { get; set; }

        [StringLength(50)]
        public string ResponderRole { get; set; } // Police, Medical, Guard, etc.

        [StringLength(500)]
        public string ActionTaken { get; set; }

        [Required]
        public DateTime ResponseDateTime { get; set; } = DateTime.UtcNow;

        public int? EstimatedMinutesToArrive { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "In Progress"; // Pending, In Progress, Completed

        public DateTime? CompletionDateTime { get; set; }

        [StringLength(500)]
        public string Notes { get; set; }
    }
}
