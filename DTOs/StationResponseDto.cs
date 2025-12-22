using System.Collections.Generic;

namespace EvChargingSystem.API.DTOs
{
    // DTO для вихідних даних про станцію
    public class StationResponseDto
    {
        public int SiteId { get; set; }
        public string SiteName { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }

        // Флаг для швидкого відображення статусу на карті
        public bool HasAvailablePorts { get; set; }

        // Актуальна ціна для pop-up вікна
        public decimal CurrentPricePerKwh { get; set; }

        // Перелік доступних портів
        public List<PortInfoDto> Ports { get; set; }
    }

    // Додатковий клас для деталізації портів
    public class PortInfoDto
    {
        public int PointId { get; set; }
        public string ConnectorType { get; set; }
        public decimal MaxPowerKw { get; set; }
        public bool IsFunctional { get; set; } // Чи може бути використаний (MF-1)
    }
}