using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EvChargingSystem.API.Models
{
    public class ChargingPoint
    {
        [Key]
        public int PointId { get; set; }

        // Зовнішній ключ (FK) до Площадки
        [ForeignKey("Site")]
        public int SiteId { get; set; }

        // Навігаційна властивість (Зв'язок N:1 до Sites)
        public Site Site { get; set; }

        public bool IsFunctional { get; set; } // Статус працездатності для Адміністратора
        public string ConnectorType { get; set; }
        public decimal MaxPowerKw { get; set; }

        // Навігаційна властивість (Зв'язок 1:N)
        public ICollection<Transaction> Transactions { get; set; }
    }
}