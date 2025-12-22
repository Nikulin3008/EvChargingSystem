using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EvChargingSystem.API.Data;
using EvChargingSystem.API.DTOs;
using EvChargingSystem.API.Models;
using BCrypt.Net; // Не забудьте встановити NuGet пакет BCrypt.Net-Core

namespace EvChargingSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;

        // Впровадження AppDbContext через Dependency Injection
        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        // POST: api/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegistrationDto model)
        {
            // 1. Валідація та перевірка дублікатів
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Перевірка, чи користувач з таким Email вже існує
            if (await _context.Users.AnyAsync(u => u.Email == model.Email))
            {
                return BadRequest("Користувач з таким Email вже існує.");
            }

            // 2. Створення нового об'єкта User
            var user = new User
            {
                Email = model.Email,
                // Хешування пароля для безпечного зберігання
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                FullName = model.FullName,
                Role = model.Role, // За замовчуванням 'Driver'
                RegistrationDate = DateTime.UtcNow
            };

            // 3. Збереження в БД
            _context.Users.Add(user);
            await _context.SaveChangesAsync(); // Завдання 6: Функція роботи з БД (ORM)

            // Повернення успішного результату
            return StatusCode(201, new { Message = "Реєстрація успішна!" });
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            // 1. Пошук користувача
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);

            if (user == null)
            {
                return Unauthorized("Невірний Email або пароль.");
            }

            // 2. Перевірка пароля
            // Порівнюємо наданий пароль з хешем у БД
            bool verified = BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash);

            if (!verified)
            {
                return Unauthorized("Невірний Email або пароль.");
            }

            // 3. Успішний вхід (тут у реальному проекті генерується JWT-токен)
            // Для цілей ЛР повернемо роль та ID
            return Ok(new
            {
                Message = "Вхід успішний!",
                UserId = user.UserId,
                UserRole = user.Role
            });
        }

        // DELETE: api/auth/delete/{userId}
        [HttpDelete("delete/{userId}")]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            // 1. Пошук користувача
            var userToDelete = await _context.Users
                .Include(u => u.Transactions) // Включаємо залежні транзакції
                .Include(u => u.ManagedSites)  // Включаємо залежні площадки
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (userToDelete == null)
            {
                return NotFound($"Користувача з ID {userId} не знайдено.");
            }

            // 2. Обробка залежностей (Критично важливо для цілісності БД)
            // Ми використовуємо OnDelete(DeleteBehavior.Restrict) у DbContext,
            // тому видалення не вдасться, якщо є активні залежності.
            // Тут ми просто перевіряємо, чи є залежності.

            if (userToDelete.Transactions.Any())
            {
                return BadRequest("Неможливо видалити користувача, який має історію транзакцій. Рекомендовано деактивувати.");
            }

            if (userToDelete.ManagedSites.Any())
            {
                return BadRequest("Неможливо видалити користувача, який є адміністратором площадки. Призначте нового адміністратора спочатку.");
            }

            // 3. Видалення з БД
            _context.Users.Remove(userToDelete);
            await _context.SaveChangesAsync();

            return Ok($"Користувача з ID {userId} успішно видалено.");
        }

        // GET: api/auth/{userId}
        [HttpGet("{userId}")]
        [ProducesResponseType(typeof(UserResponseDto), 200)]
        public async Task<IActionResult> GetUserById(int userId)
        {
            // 1. Пошук користувача
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                return NotFound($"Користувача з ID {userId} не знайдено.");
            }

            // 2. Маппінг (перетворення) моделі User на DTO
            var responseDto = new UserResponseDto
            {
                UserId = user.UserId,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role,
                RegistrationDate = user.RegistrationDate
            };

            return Ok(responseDto);
        }

    }
}