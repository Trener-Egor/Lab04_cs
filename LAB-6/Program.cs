using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;

// Структура для хранения информации о погоде
public struct Weather
{
    public string Country { get; set; }         // Страна
    public string Name { get; set; }            // Название населенного пункта
    public float Temp { get; set; }             // Температура в градусах Цельсия
    public string Description { get; set; }     // Описание погоды
}

class Program
{
    private static readonly HttpClient httpClient = new HttpClient(); // Объект HttpClient для выполнения запросов
    private const string ApiKey = "62f7fc8498e7b8cbbc8753ff3543e09c"; // API ключ для доступа к OpenWeather API

    static async Task Main(string[] args)
    {
        List<Weather> weatherData = new List<Weather>();    // Список для хранения данных о погоде
        Random random = new Random();                       // Генератор случайных чисел для широты и долготы

        // Генерация 25 случайных координат для получения погоды
        while (weatherData.Count < 25)
        {
            float lat = (float)(random.NextDouble() * 180 - 90);    // Генерация случайной широты
            float lon = (float)(random.NextDouble() * 360 - 180);   // Генерация случайной долготы

            // Формирование URL для запроса погоды по координатам и API ключу
            string url = $"https://api.openweathermap.org/data/2.5/weather?lat={lat}&lon={lon}&appid={ApiKey}&units=metric";
            var weather = await GetWeatherData(url); // Получение данных о погоде

            // Проверяем полученные данные на наличие необходимых значений
            if (weather.HasValue
                && !string.IsNullOrEmpty(weather.Value.Country)
                && !string.IsNullOrEmpty(weather.Value.Name))
            {
                weatherData.Add(weather.Value); // Добавляем данные в список, если они получены
                Console.WriteLine($"Добавлен элемент {weatherData.Count}");
            }

            // Задержка в 2 секунды между запросами
            await Task.Delay(2000);
        }

        // 1. Нахождение страны с максимальной и минимальной температурой
        var maxTempCountry = weatherData.OrderByDescending(w => w.Temp).FirstOrDefault();
        var minTempCountry = weatherData.OrderBy(w => w.Temp).FirstOrDefault();

        // Вывод информации о максимальной и минимальной температурах
        Console.WriteLine($"Страна с максимальной температурой: {maxTempCountry.Country}, Температура: {maxTempCountry.Temp}°C");
        Console.WriteLine($"Страна с минимальной температурой: {minTempCountry.Country}, Температура: {minTempCountry.Temp}°C");

        // 2. Вычисление средней температуры
        var averageTemp = weatherData.Average(w => w.Temp);
        Console.WriteLine($"Средняя температура в мире: {averageTemp}°C");

        // 3. Подсчет количества уникальных стран
        var uniqueCountriesCount = weatherData.Select(w => w.Country).Distinct().Count();
        Console.WriteLine($"Количество уникальных стран: {uniqueCountriesCount}");

        // 4. Поиск первой найденной страны с определенными описаниями погоды
        var descriptions = new[] { "clear sky", "rain", "few clouds" };
        var firstMatch = weatherData.FirstOrDefault(w => descriptions.Contains(w.Description));

        // Вывод информации о первой найденной стране с определённым описанием
        if (!string.IsNullOrEmpty(firstMatch.Name))
        {
            Console.WriteLine($"Первая найдетая страна с описанием: {firstMatch.Country}, Местность: {firstMatch.Name}, Описание: {firstMatch.Description}");
        }
        else
        {
            Console.WriteLine("Нет найденных описаний.");
        }
    }

    // Асинхронный метод для получения данных о погоде по URL
    private static async Task<Weather?> GetWeatherData(string url)
    {
        try
        {
            Console.WriteLine($"Запрос к URL: {url}"); // Логирование URL
            // Получение данных из API и десериализация в объект WeatherResponse
            var response = await httpClient.GetFromJsonAsync<WeatherResponse>(url);

            // Выводим весь ответ в формате JSON
            // string jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true });
            // Console.WriteLine("Ответ API:");
            // Console.WriteLine(jsonResponse);

            // Проверка данных на наличие обязательных полей
            if (response != null
                && response.Main != null
                && response.Sys != null
                && response.Weather != null
                && response.Name != ""
                && response.Weather.Count > 0)
            {
                // Возвращаем объект Weather с заполненными данными
                return new Weather
                {
                    Country = response.Sys.Country ?? "Неизвестно",     // Устанавливаем значение по умолчанию
                    Name = response.Name ?? "Неизвестно",               // Устанавливаем значение по умолчанию
                    Temp = response.Main.Temp,                          // Температура уже в градусах Цельсия с параметром units=metric
                    Description = response.Weather[0].Description       // Первое описание погоды
                };
            }
            else
            {
                Console.WriteLine("Ответ API не содержит необходимых данных."); // Логирование проблемы с данными
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Ошибка запроса: {ex.Message}"); // Логирование ошибок при запросе
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Общая ошибка: {ex.Message}"); // Логирование других ошибок
        }
        return null; // Возвращаем null, если не удалось получить данные
    }
}

// Класс для десериализации ответа от API
public class WeatherResponse
{
    public Main? Main { get; set; } // Модификатор ? позволяет принимать значение null
    public Sys? Sys { get; set; }   // Система данных, например, страна
    public List<WeatherDescription>? Weather { get; set; } // Список описаний погоды
    public string? Name { get; set; } // Название местности, позволяющее принимать значение null
}

// Класс для хранения данных о температуре
public class Main
{
    public float Temp { get; set; } // Температура в градусах Цельсия
}

// Класс для хранения данных о системе
public class Sys
{
    public string Country { get; set; } // Код страны
}

// Класс для хранения описания погоды
public class WeatherDescription
{
    public string Description { get; set; } // Описание погодных условий
}