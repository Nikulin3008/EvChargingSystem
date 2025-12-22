using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System;

namespace EvChargingSystem.API.Models
{
    public class User
    {
        // Первинний ключ (PK)
        [Key]
        public int UserId { get; set; }

        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; } // 'Driver', 'SiteAdmin', 'SystemAdmin'
        public string FullName { get; set; }
        public DateTime RegistrationDate { get; set; }

        // Навігаційні властивості (Зв'язок 1:N)
        public ICollection<Transaction> Transactions { get; set; }
        public ICollection<Site> ManagedSites { get; set; } // Для адміністраторів площадок
    }
}