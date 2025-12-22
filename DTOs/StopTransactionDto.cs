using System.ComponentModel.DataAnnotations;

namespace EvChargingSystem.API.DTOs
{
    // DTO для POST /api/transactions/stop
    public class StopTransactionDto
    {
        [Required]
        public int TransactionId { get; set; } // Активна транзакція для завершення

        // Симулюємо дані від IoT-клієнта (спожита енергія)
        [Required]
        public decimal EnergyConsumedKwh { get; set; }
    }
}