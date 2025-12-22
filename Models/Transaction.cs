using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace EvChargingSystem.API.Models
{
    public class Transaction
    {
        [Key]
        public int TransactionId { get; set; }

        // FK до Користувача
        [ForeignKey("User")]
        public int UserId { get; set; }
        public User User { get; set; }

        // FK до Колонки
        [ForeignKey("ChargingPoint")]
        public int PointId { get; set; }
        public ChargingPoint ChargingPoint { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; } // Критично важливе для білінгу
        public decimal EnergyKwh { get; set; }
        public decimal TotalCost { get; set; }
    }
}