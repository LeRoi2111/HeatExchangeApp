using System.Text.Json;
using HeatExchangeApp.Models;
using System.Collections.Generic;

namespace HeatExchangeApp.Services
{
    public class CalculationService
    {
        public List<CalculationResult> Calculate(CalculationModel model)
        {
            var results = new List<CalculationResult>();

            // Площадь сечения аппарата
            double area = Math.PI * Math.Pow(model.ApparatusDiameter / 2, 2);

            // Объемный расход газа (м³/с)
            double gasVolumeFlow = model.GasVelocity * area;

            // Переводим кДж в Дж для корректных расчетов
            double materialCapacityJ = model.MaterialHeatCapacity * 1000; // Дж/(кг·К)
            double gasCapacityJ = model.GasHeatCapacity * 1000; // Дж/(м³·К)

            // Отношение теплоемкостей потоков (m)
            double m = (model.MaterialFlowRate * materialCapacityJ)
                      / (gasVolumeFlow * gasCapacityJ);

            // Максимальная относительная высота (Y0)
            double Y0 = (model.HeatTransferCoefficient * model.Height)
                       / (gasVolumeFlow * gasCapacityJ);

            // Разбиваем высоту на 10 точек для расчета
            for (int i = 0; i <= 10; i++)
            {
                double y = model.Height * i / 10.0;

                // Текущая относительная высота (Y)
                double Y = (model.HeatTransferCoefficient * y)
                          / (gasVolumeFlow * gasCapacityJ);

                // Расчет безразмерных температур
                double expTerm = Math.Exp((m - 1) * Y / m);
                double expTerm0 = Math.Exp((m - 1) * Y0 / m);

                double thetaMaterial = (1 - expTerm) / (1 - m * expTerm0);
                double thetaGas = (1 - m * expTerm) / (1 - m * expTerm0);

                // Расчет фактических температур
                double tMaterial = model.MaterialInitialTemp +
                                 (model.GasInitialTemp - model.MaterialInitialTemp) * thetaMaterial;
                double tGas = model.MaterialInitialTemp +
                             (model.GasInitialTemp - model.MaterialInitialTemp) * thetaGas;

                results.Add(new CalculationResult
                {
                    Height = Math.Round(y, 2),
                    MaterialTemperature = Math.Round(tMaterial, 1),
                    GasTemperature = Math.Round(tGas, 1),
                    TemperatureDifference = Math.Round(tGas - tMaterial, 1)
                });
            }

            return results;
        }

        // Метод для сериализации данных
        public string SerializeCalculation(CalculationModel model)
        {
            return JsonSerializer.Serialize(model);
        }

        public string SerializeResults(List<CalculationResult> results)
        {
            return JsonSerializer.Serialize(results);
        }

        // Метод для десериализации
        public CalculationModel? DeserializeCalculation(string json)
        {
            return JsonSerializer.Deserialize<CalculationModel>(json);
        }

        public List<CalculationResult>? DeserializeResults(string json)
        {
            return JsonSerializer.Deserialize<List<CalculationResult>>(json);
        }
    }
}