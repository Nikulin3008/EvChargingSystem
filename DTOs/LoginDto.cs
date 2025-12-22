using System.ComponentModel.DataAnnotations;

namespace EvChargingSystem.API.DTOs
{
    // DTO для вхідних даних при POST /api/auth/login
    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}