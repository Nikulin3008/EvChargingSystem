using System.ComponentModel.DataAnnotations;

namespace EvChargingSystem.API.DTOs
{
    // DTO для POST /api/transactions/start
    public class StartTransactionDto
    {
        [Required]
        public int UserId { get; set; } // Користувач, що ініціює

        [Required]
        public int PointId { get; set; } // Колонка, яку обрано
    }
}