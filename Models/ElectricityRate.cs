using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace EvChargingSystem.API.Models
{
    public class ElectricityRate
    {
        [Key]
        public int RateId { get; set; }

        public decimal PricePerKwh { get; set; }
        public string RateName { get; set; }
        public DateTime ValidFrom { get; set; } // Додаткове поле для визначення актуальності

        // Зовнішній ключ (FK) до Площадки
        [ForeignKey("Site")]
        public int SiteId { get; set; }

        // Навігаційна властивість (Зв'язок N:1 до Sites)
        public Site Site { get; set; }
    }
}