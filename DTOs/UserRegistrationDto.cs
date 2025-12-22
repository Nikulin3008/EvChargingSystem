using System.ComponentModel.DataAnnotations;

namespace EvChargingSystem.API.DTOs
{
    // DTO для вхідних даних при POST /api/auth/register
    public class UserRegistrationDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(6)] // Забезпечення мінімальної безпеки пароля
        public string Password { get; set; }

        [Required]
        public string FullName { get; set; }

        // Роль задається на сервері за замовчуванням ('Driver'), 
        // але може бути передана для внутрішнього використання
        public string Role { get; set; } = "Driver";
    }
}