using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TouristSafetySystem.Models
{
    public class LocationHistory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TouristId { get; set; }

        [ForeignKey("TouristId")]
        public virtual Tourist Tourist { get; set; }

        [Required]
        [Range(-90, 90)]
        public double Latitude { get; set; }

        [Required]
        [Range(-180, 180)]
        public double Longitude { get; set; }

        [StringLength(200)]
        public string LocationName { get; set; }

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [StringLength(50)]
        public string Accuracy { get; set; } // High, Medium, Low

        [Range(0, 1)]
        public double RiskScoreAtLocation { get; set; }

        public bool InDangerZone { get; set; } = false;
    }
}
