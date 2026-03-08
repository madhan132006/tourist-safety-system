using System.ComponentModel.DataAnnotations;

namespace TouristSafetySystem.Models
{
    public class DangerZone
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        [Required]
        [Range(-90, 90)]
        public double Latitude { get; set; }

        [Required]
        [Range(-180, 180)]
        public double Longitude { get; set; }

        [Required]
        [Range(0, 100000)]
        public int RadiusMeters { get; set; }

        [Range(0, 1)]
        public double RiskLevel { get; set; } // 0.0 to 1.0

        [StringLength(500)]
        public string Description { get; set; }

        [StringLength(50)]
        public string Reason { get; set; } // Crime Rate, Natural Disaster, Conflict, etc.

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? LastUpdated { get; set; }

        [StringLength(100)]
        public string CreatedBy { get; set; }
    }
}
