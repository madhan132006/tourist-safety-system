using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TouristSafetySystem.Models
{
    public class Tourist
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Phone]
        public string PhoneNumber { get; set; }

        // Blockchain address for digital identity
        [StringLength(256)]
        public string BlockchainAddress { get; set; }

        // Location tracking
        [Range(-90, 90)]
        public double Latitude { get; set; }

        [Range(-180, 180)]
        public double Longitude { get; set; }

        // Profile information
        [StringLength(100)]
        public string EmergencyContact { get; set; }

        [Phone]
        public string EmergencyPhoneNumber { get; set; }

        [Range(1, 120)]
        public int Age { get; set; }

        [StringLength(100)]
        public string Nationality { get; set; }

        // Travel information
        public bool FirstTimeVisitor { get; set; }

        [Range(0, 365)]
        public int DaysInCountry { get; set; }

        public DateTime ArrivalDate { get; set; }

        public DateTime DepartureDate { get; set; }

        // Firebase notification token
        [StringLength(500)]
        public string FirebaseToken { get; set; }

        // Risk profile
        [StringLength(50)]
        public string RiskLevel { get; set; } = "Low"; // Low, Medium, High

        // Account management
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ICollection<Incident> Incidents { get; set; } = new List<Incident>();
        public virtual ICollection<LocationHistory> LocationHistories { get; set; } = new List<LocationHistory>();
        public virtual ICollection<DangerZoneAlert> DangerZoneAlerts { get; set; } = new List<DangerZoneAlert>();
    }
}