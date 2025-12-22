using System.ComponentModel.DataAnnotations;

namespace EvChargingSystem.API.DTOs
{
    // DTO для вхідних даних при POST /api/stations
    public class SiteCreateDto
    {
        [Required]
        public string SiteName { get; set; }

        [Required]
        [Range(-90.0, 90.0)]
        public decimal Latitude { get; set; }

        [Required]
        [Range(-180.0, 180.0)]
        public decimal Longitude { get; set; }

        // FK до користувача, який призначається адміністратором цієї площадки
        [Required]
        public int AdministratorId { get; set; }

        public int PointsCount { get; set; }
    }
}