using EvChargingSystem.API.Data;
using EvChargingSystem.API.DTOs;
using EvChargingSystem.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace EvChargingSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // Нам не потрібна авторизація для перегляду карти, тому не використовуємо [Authorize]
    public class StationsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public StationsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/stations/nearest
        // Реалізує Пошук та Фільтрацію ЗС (MF-1)
        [HttpGet("nearest")]
        [ProducesResponseType(typeof(IEnumerable<StationResponseDto>), 200)]
        public async Task<IActionResult> GetNearestStations([FromQuery] StationSearchDto searchParams)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // 1. Отримуємо всі площадки та включаємо (Include) пов'язані дані (Points та Rates)
            var sites = await _context.Sites
                .Include(s => s.ChargingPoints)
                .Include(s => s.Rates)
                .ToListAsync();

            var userLat = searchParams.Latitude;
            var userLon = searchParams.Longitude;
            var radius = searchParams.RadiusKm;

            // 2. Фільтрація: Розрахунок відстані та фільтрація за радіусом (Спрощений розрахунок)
            var nearestSites = sites
                .Select(s => new
                {
                    Site = s,
                    // Приблизний розрахунок відстані (в км), можна використати Haversine, 
                    // але для ЛР достатньо: 1 градус широти ~ 111 км
                    DistanceKm = Math.Sqrt(
                        Math.Pow((double)(s.Latitude - userLat) * 111, 2) +
                        Math.Pow((double)(s.Longitude - userLon) * 111 * Math.Cos((double)userLat * Math.PI / 180), 2)
                    )
                })
                .Where(s => s.DistanceKm <= radius)
                .OrderBy(s => s.DistanceKm)
                .ToList();

            // 3. Маппінг (перетворення) моделей в DTO для відповіді клієнту
            var response = nearestSites.Select(ns => new StationResponseDto
            {
                SiteId = ns.Site.SiteId,
                SiteName = ns.Site.SiteName,
                Latitude = ns.Site.Latitude,
                Longitude = ns.Site.Longitude,

                // Визначаємо, чи є вільні та функціональні порти
                HasAvailablePorts = ns.Site.ChargingPoints.Any(p => p.IsFunctional),

                // Отримуємо актуальну (найновішу) ціну для pop-up вікна
                CurrentPricePerKwh = ns.Site.Rates
                    .OrderByDescending(r => r.ValidFrom)
                    .FirstOrDefault()?.PricePerKwh ?? 0.0m, // 0.0m якщо ціна не знайдена

                // Деталізація доступних портів
                Ports = ns.Site.ChargingPoints.Select(p => new PortInfoDto
                {
                    PointId = p.PointId,
                    ConnectorType = p.ConnectorType,
                    MaxPowerKw = p.MaxPowerKw,
                    IsFunctional = p.IsFunctional
                }).ToList()
            }).ToList();

            return Ok(response);
        }

        // POST: api/stations 
        [HttpPost]
        public async Task<IActionResult> CreateSite([FromBody] SiteCreateDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // 1. Перевірка адміністратора
            var admin = await _context.Users.FirstOrDefaultAsync(u => u.UserId == model.AdministratorId && u.Role == "SiteAdmin");

            if (admin == null)
            {
                return NotFound("Призначений адміністратор не знайдений або не має відповідної ролі.");
            }

            // 2. Створення самого об'єкта площадки (Site)
            var site = new Site
            {
                SiteName = model.SiteName,
                Latitude = model.Latitude,
                Longitude = model.Longitude,
                AdministratorId = model.AdministratorId
            };

            _context.Sites.Add(site);
            await _context.SaveChangesAsync(); // Зберігаємо, щоб отримати site.SiteId

            // 3.АВТОМАТИЧНЕ СТВОРЕННЯ КОЛОНОК
            int defaultPointsCount = model.PointsCount > 0 ? model.PointsCount : 1;

            for (int i = 0; i < defaultPointsCount; i++)
            {
                var point = new ChargingPoint
                {
                    SiteId = site.SiteId,
                    IsFunctional = true,
                    // ДОДАЄМО ОБОВ'ЯЗКОВЕ ПОЛЕ:
                    ConnectorType = "Type 2" // Або інше значення, яке підтримує ваша база (CCS, Chademo тощо)
                };
                _context.ChargingPoints.Add(point);
            }

            await _context.SaveChangesAsync(); // Зберігаємо створені колонки в БД

            // Повертаємо створений об'єкт із підтвердженням
            return StatusCode(201, new
            {
                Site = site,
                Message = $"{defaultPointsCount} колонок успішно створено та прив'язано до площадки."
            });
        }

        // DELETE: api/stations/{siteId} 
        // Дозволяє Адміністратору Системи закрити (видалити) площадку
        [HttpDelete("{siteId}")]
        public async Task<IActionResult> DeleteSite(int siteId)
        {
            var siteToDelete = await _context.Sites
                .Include(s => s.ChargingPoints)
                .FirstOrDefaultAsync(s => s.SiteId == siteId);

            if (siteToDelete == null)
            {
                return NotFound($"Площадку з ID {siteId} не знайдено.");
            }

            // Перевірка залежностей: якщо є активні колонки, не видаляємо
            if (siteToDelete.ChargingPoints.Any())
            {
                return BadRequest("Неможливо видалити площадку, яка містить зарядні колонки. Видаліть колонки спочатку.");
            }

            // Видаляємо площадку
            _context.Sites.Remove(siteToDelete);
            await _context.SaveChangesAsync();

            return Ok($"Площадку з ID {siteId} успішно видалено.");
        }

        // GET: api/stations/{siteId}
        // Повертає повну інформацію про конкретну площадку та її порти
        [HttpGet("{siteId}")]
        [ProducesResponseType(typeof(StationResponseDto), 200)]
        public async Task<IActionResult> GetSiteById(int siteId)
        {
            // 1. Отримуємо площадку та включаємо (Include) пов'язані дані (Points та Rates)
            var site = await _context.Sites
                .Include(s => s.ChargingPoints)
                .Include(s => s.Rates)
                .FirstOrDefaultAsync(s => s.SiteId == siteId);

            if (site == null)
            {
                return NotFound($"Зарядна площадка з ID {siteId} не знайдена.");
            }

            // 2. Маппінг (перетворення) моделі в DTO для відповіді клієнту
            var response = new StationResponseDto
            {
                SiteId = site.SiteId,
                SiteName = site.SiteName,
                Latitude = site.Latitude,
                Longitude = site.Longitude,

                // Визначаємо, чи є вільні та функціональні порти
                HasAvailablePorts = site.ChargingPoints.Any(p => p.IsFunctional),

                // Отримуємо актуальну (найновішу) ціну
                CurrentPricePerKwh = site.Rates
                    .OrderByDescending(r => r.ValidFrom)
                    .FirstOrDefault()?.PricePerKwh ?? 0.0m, // 0.0m якщо ціна не знайдена

                // Деталізація доступних портів
                Ports = site.ChargingPoints.Select(p => new PortInfoDto
                {
                    PointId = p.PointId,
                    ConnectorType = p.ConnectorType,
                    MaxPowerKw = p.MaxPowerKw,
                    IsFunctional = p.IsFunctional
                }).ToList()
            };

            return Ok(response);
        }


    }
}