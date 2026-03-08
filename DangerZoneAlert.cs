using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TouristSafetySystem.Models
{
    public class DangerZoneAlert
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TouristId { get; set; }

        [ForeignKey("TouristId")]
        public virtual Tourist Tourist { get; set; }

        [Required]
        [StringLength(200)]
        public string ZoneName { get; set; }

        [Required]
        [Range(-90, 90)]
        public double ZoneLatitude { get; set; }

        [Required]
        [Range(-180, 180)]
        public double ZoneLongitude { get; set; }

        [Required]
        [Range(0, 10000)]
        public int RadiusMeters { get; set; }

        [Range(0, 1)]
        public double RiskLevel { get; set; } // 0.0 to 1.0

        [StringLength(500)]
        public string AlertReason { get; set; }

        [Required]
        public DateTime AlertDateTime { get; set; } = DateTime.UtcNow;

        public DateTime? DismissedDateTime { get; set; }

        public bool IsActive { get; set; } = true;

        [StringLength(50)]
        public string AlertType { get; set; } = "Danger"; // Danger, Warning, Info
    }
}
