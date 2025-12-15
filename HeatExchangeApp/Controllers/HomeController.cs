using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HeatExchangeApp.Models;
using HeatExchangeApp.Services;
using HeatExchangeApp.Data;

namespace HeatExchangeApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly CalculationService _calculationService;
        private readonly ApplicationDbContext _context;

        public HomeController(CalculationService calculationService, ApplicationDbContext context)
        {
            _calculationService = calculationService;
            _context = context;
        }

        // Главная страница со списком сохраненных расчетов
        public async Task<IActionResult> Index()
        {
            var calculations = await _context.SavedCalculations
                .OrderByDescending(c => c.CreatedAt)
                .Take(20)
                .ToListAsync();

            return View(calculations);
        }

        // Страница калькулятора
        [HttpGet]
        public IActionResult Calculator(int? id)
        {
            CalculationModel model = new CalculationModel();

            // Если передан ID, загружаем сохраненный расчет
            if (id.HasValue)
            {
                var saved = _context.SavedCalculations.Find(id.Value);
                if (saved != null)
                {
                    var loadedModel = _calculationService.DeserializeCalculation(saved.CalculationDataJson);
                    if (loadedModel != null)
                    {
                        model = loadedModel;
                        model.CalculationName = saved.Name;

                        // Передаем результаты в TempData для отображения
                        var results = _calculationService.DeserializeResults(saved.ResultsJson);
                        if (results != null)
                        {
                            TempData["Results"] = results;
                        }
                    }
                }
            }

            return View(model);
        }

        // Обработка расчета
        [HttpPost]
        public async Task<IActionResult> Calculate(CalculationModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Calculator", model);
            }

            // Выполнение расчета
            var results = _calculationService.Calculate(model);

            // Сохранение в базу данных
            var savedCalculation = new SavedCalculation
            {
                Name = model.CalculationName,
                CalculationDataJson = _calculationService.SerializeCalculation(model),
                ResultsJson = _calculationService.SerializeResults(results),
                UserId = User.Identity?.IsAuthenticated == true ? User.Identity.Name : null
            };

            _context.SavedCalculations.Add(savedCalculation);
            await _context.SaveChangesAsync();

            // Передача результатов в представление
            ViewBag.Results = results;
            ViewBag.SavedId = savedCalculation.Id;

            return View("Calculator", model);
        }

        // Удаление расчета
        [HttpPost]
        public async Task<IActionResult> DeleteCalculation(int id)
        {
            var calculation = await _context.SavedCalculations.FindAsync(id);
            if (calculation != null)
            {
                _context.SavedCalculations.Remove(calculation);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        // Показ результатов без сохранения
        [HttpPost]
        public IActionResult ShowResults(CalculationModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Calculator", model);
            }

            var results = _calculationService.Calculate(model);
            ViewBag.Results = results;

            return View("Calculator", model);
        }
    }
}