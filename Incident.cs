using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TouristSafetySystem.Models
{
    public class Incident
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TouristId { get; set; }

        [ForeignKey("TouristId")]
        public virtual Tourist Tourist { get; set; }

        [Required]
        [StringLength(50)]
        public string Type { get; set; } // theft, accident, assault, medical, lost, other

        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [Required]
        [Range(-90, 90)]
        public double Latitude { get; set; }

        [Required]
        [Range(-180, 180)]
        public double Longitude { get; set; }

        [StringLength(200)]
        public string LocationName { get; set; }

        [StringLength(50)]
        public string Severity { get; set; } = "Medium"; // Low, Medium, High, Critical

        public bool IsHandled { get; set; } = false;

        public DateTime? ResolvedDate { get; set; }

        [StringLength(500)]
        public string ResolutionNotes { get; set; }

        [StringLength(100)]
        public string ReportedBy { get; set; } // Who reported it - self, authorities, etc.

        // AI Risk Assessment
        [Range(0, 1)]
        public double RiskScore { get; set; } // 0.0 to 1.0

        public bool NotificationSent { get; set; } = false;

        public DateTime? NotificationSentTime { get; set; }

        // Navigation properties
        public virtual ICollection<IncidentResponse> IncidentResponses { get; set; } = new List<IncidentResponse>();
    }
}