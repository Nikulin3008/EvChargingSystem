using System.ComponentModel.DataAnnotations;

namespace EvChargingSystem.API.DTOs
{
    // DTO для вхідних параметрів при GET /api/stations/nearest
    public class StationSearchDto
    {
        [Required]
        [Range(-90.0, 90.0)]
        public decimal Latitude { get; set; } // Поточна широта користувача

        [Required]
        [Range(-180.0, 180.0)]
        public decimal Longitude { get; set; } // Поточна довгота користувача

        [Range(1, 100)]
        public int RadiusKm { get; set; } = 5; // Радіус пошуку за замовчуванням
    }
}