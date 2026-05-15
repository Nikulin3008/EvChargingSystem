using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EvChargingSystem.API.Data;
using EvChargingSystem.API.DTOs;
using EvChargingSystem.API.Models;

namespace EvChargingSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TransactionsController(AppDbContext context)
        {
            _context = context;
        }

        // POST: api/transactions/start (MF-3: Керування сесією)
        [HttpPost("start")]
        public async Task<IActionResult> StartTransaction([FromBody] StartTransactionDto model)
        {
            // 1. Перевірка доступності колонки
            var point = await _context.ChargingPoints
                .FirstOrDefaultAsync(p => p.PointId == model.PointId && p.IsFunctional == true);

            if (point == null)
            {
                return BadRequest("Обрана колонка не існує або несправна.");
            }

            // 2. Створення нової транзакції
            var transaction = new Transaction
            {
                UserId = model.UserId,
                PointId = model.PointId,
                StartTime = DateTime.UtcNow,
                // EndTime та Cost будуть заповнені пізніше
                TotalCost = 0.0m,
                EnergyKwh = 0.0m
            };

            // 3. Збереження в БД та зміна статусу колонки (симуляція)
            _context.Transactions.Add(transaction);
            // point.IsFunctional = false; // У реальності це робить IoT-клієнт
            await _context.SaveChangesAsync();

            // Повернення ID активної транзакції
            return Ok(new { TransactionId = transaction.TransactionId, Message = "Зарядка розпочата. Очікуйте підтвердження IoT-клієнта." });
        }

        // POST: api/transactions/stop (MF-3, MF-4: Обробка оплати та фіксація)
        [HttpPost("stop")]
        public async Task<IActionResult> StopTransaction([FromBody] StopTransactionDto model)
        {
            // 1. Пошук транзакції
            var transaction = await _context.Transactions
                .Include(t => t.ChargingPoint)
                    .ThenInclude(p => p.Site)
                        .ThenInclude(s => s.Rates)
                .FirstOrDefaultAsync(t => t.TransactionId == model.TransactionId);

            if (transaction == null) return NotFound("Транзакція не знайдена.");

            // 2. Вибір енергії: беремо з бази (від Wokwi), якщо в Swagger залишили 0
            decimal finalEnergy = (transaction.EnergyKwh > 0 && model.EnergyConsumedKwh == 0)
                                  ? transaction.EnergyKwh
                                  : (decimal)model.EnergyConsumedKwh;

            // 3. Розрахунок вартості (з дефолтним тарифом 15.0)
            var rate = transaction.ChargingPoint.Site.Rates.OrderByDescending(r => r.ValidFrom).FirstOrDefault();
            decimal pricePerKwh = rate?.PricePerKwh ?? 15.0m;

            // 4. Оновлення даних (ВИКОРИСТОВУЄМО UtcNow для уникнення помилки 500)
            transaction.EndTime = DateTime.UtcNow;
            transaction.EnergyKwh = finalEnergy;
            transaction.TotalCost = finalEnergy * pricePerKwh;

            // 5. Збереження
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Помилка БД: {ex.Message}");
            }

            return Ok(new
            {
                Message = "Зарядка завершена успішно.",
                TotalCost = transaction.TotalCost,
                EnergyConsumed = transaction.EnergyKwh,
                PricePerKwh = pricePerKwh
            });
        }

        // GET: api/transactions/history/{userId} (MF-4: Облік історії)
        [HttpGet("history/{userId}")]
        public async Task<ActionResult<IEnumerable<Transaction>>> GetUserHistory(int userId)
        {
            // Отримуємо всі завершені транзакції користувача, включаючи деталі колонки
            var history = await _context.Transactions
                .Include(t => t.ChargingPoint)
                    .ThenInclude(p => p.Site)
                .Where(t => t.UserId == userId && t.EndTime != DateTime.MinValue)
                .OrderByDescending(t => t.StartTime)
                .ToListAsync();

            if (history == null || !history.Any())
            {
                return NotFound("Історія транзакцій не знайдена.");
            }

            // У реальному проекті тут використовувався б TransactionHistoryDto
            return Ok(history);
        }

        // GET: api/transactions/all (Для статистики в Панелі Адміністратора)
        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<Transaction>>> GetAllTransactions()
        {
            // Отримуємо всі транзакції, де є хоч якась оплата або спожита енергія
            var allTransactions = await _context.Transactions
                .Include(t => t.ChargingPoint)
                    .ThenInclude(p => p.Site)
                .Where(t => t.TotalCost > 0 || t.EnergyKwh > 0)
                .OrderByDescending(t => t.StartTime)
                .ToListAsync();

            // Повертаємо пустий масив, якщо транзакцій ще немає (щоб React не видавав червону помилку)
            if (allTransactions == null || !allTransactions.Any())
            {
                return Ok(new List<Transaction>());
            }

            return Ok(allTransactions);
        }

        [HttpPost("update-telemetry")]
        public async Task<IActionResult> UpdateTelemetry([FromBody] TelemetryDto model)
        {
            // Шукаємо транзакцію для цієї точки, яка ще не завершена (TotalCost == 0)
            var transaction = await _context.Transactions
                .Where(t => t.PointId == model.PointId && t.TotalCost == 0)
                .OrderByDescending(t => t.TransactionId)
                .FirstOrDefaultAsync();

            if (transaction == null)
            {
                return NotFound("Активна сесія не знайдена. Запустіть Start у Swagger!");
            }

            // Оновлюємо енергію
            transaction.EnergyKwh = (decimal)model.Energy;

            await _context.SaveChangesAsync();

            // Лог для перевірки в консолі Visual Studio
            Console.WriteLine($"[IoT] Оновлено енергію для транзакції {transaction.TransactionId}: {transaction.EnergyKwh} kWh");

            return Ok(new { Message = "Дані оновлено", CurrentEnergy = transaction.EnergyKwh });
        }
    }
}