using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EvChargingSystem.API.Models
{
    public class Site
    {
        // Первинний ключ (PK)
        [Key]
        public int SiteId { get; set; }

        public string SiteName { get; set; }
        public decimal Latitude { get; set; } // DECIMAL для координат
        public decimal Longitude { get; set; }

        // Зовнішній ключ (FK) для Адміністратора площадки
        [ForeignKey("Administrator")]
        public int AdministratorId { get; set; }

        // Навігаційна властивість (Зв'язок N:1 до Users)
        public User Administrator { get; set; }

        // Навігаційні властивості (Зв'язок 1:N)
        public ICollection<ChargingPoint> ChargingPoints { get; set; }
        public ICollection<ElectricityRate> Rates { get; set; }
    }
}