namespace EvChargingSystem.API.DTOs
{
    // DTO для вихідних даних про користувача (що повертаються клієнту)
    public class UserResponseDto
    {
        public int UserId { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }
        public DateTime RegistrationDate { get; set; }
    }
}