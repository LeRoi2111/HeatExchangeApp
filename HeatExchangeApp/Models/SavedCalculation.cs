using System;
using System.ComponentModel.DataAnnotations;

namespace HeatExchangeApp.Models
{
    public class SavedCalculation
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = "Расчет от " + DateTime.Now.ToString("dd.MM.yyyy");

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Required]
        public string CalculationDataJson { get; set; } = "";

        public string ResultsJson { get; set; } = "";

        // Для анонимных пользователей оставим null
        public string? UserId { get; set; }
    }
}