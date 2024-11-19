//Austin Casselman - Acasselman2648
// PROG3175 - Assignment 2 - 18/11/2024
using System.Text.Json;

namespace Acasselman_A2_Console
{
    class Program
    {
        private static readonly HttpClient client = new HttpClient();

        static async Task Main(string[] args)
        {
            // base URL of the API
            string apiUrl = "http://localhost:3000/api/";

            try
            {
                // fetch the tables
                var timesOfDay = await GetTimesOfDay(apiUrl);
                var languages = await GetLanguages(apiUrl);

                // preset tones
                var tones = new List<string> { "Formal", "Casual" };

                // prompt user to select time of day
                Console.WriteLine("Available times of day:");
                for (int i = 0; i < timesOfDay.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {timesOfDay[i]}");
                }

                Console.Write("Please select a time of day by number: ");
                int timeOfDayChoice = int.Parse(Console.ReadLine() ?? "");
                string selectedTimeOfDay = timesOfDay[timeOfDayChoice - 1];

                // prompt user to select language
                Console.WriteLine("\nAvailable languages:");
                for (int i = 0; i < languages.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {languages[i]}");
                }

                Console.Write("Please select a language by number: ");
                int languageChoice = int.Parse(Console.ReadLine() ?? "");
                string selectedLanguage = languages[languageChoice - 1];

                // prompt to select tone
                Console.WriteLine("\nAvailable tones:");
                for (int i = 0; i < tones.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {tones[i]}");
                }

                Console.Write("Please select a tone by number (default is 'Formal'): ");
                string? toneInput = Console.ReadLine();

                // default to "Formal" if no input
                string selectedTone = string.IsNullOrWhiteSpace(toneInput) ? "Formal" : tones[int.Parse(toneInput) - 1];

                // request the greeting message based on the users' selection
                await GetGreeting(apiUrl, selectedTimeOfDay, selectedLanguage, selectedTone);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        // fetch times of day
        private static async Task<List<string>> GetTimesOfDay(string apiUrl)
        {
            HttpResponseMessage response = await client.GetAsync($"{apiUrl}times-of-day");
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();

            // Deserialize into the matching model
            var timesOfDayResponse = JsonSerializer.Deserialize<TimesOfDayResponse>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return timesOfDayResponse?.TimesOfDay ?? new List<string>();
        }

        // fetch available languages
        private static async Task<List<string>> GetLanguages(string apiUrl)
        {
            HttpResponseMessage response = await client.GetAsync($"{apiUrl}languages");
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();

            var languagesResponse = JsonSerializer.Deserialize<LanguagesResponse>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return languagesResponse?.Languages ?? new List<string>();
        }

        // request the greeting based on user inputs for time of day, language, and tone
        private static async Task GetGreeting(string apiUrl, string timeOfDay, string language, string tone)
        {
            // create request object
            var requestData = new
            {
                timeOfDay,
                language,
                tone
            };

            // put the request data into JSON
            var content = new StringContent(JsonSerializer.Serialize(requestData), System.Text.Encoding.UTF8, "application/json");

            // send the post request to the greet endpoint
            HttpResponseMessage response = await client.PostAsync($"{apiUrl}greet", content);
            // make sure it works
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Request: {JsonSerializer.Serialize(requestData)}");
                Console.WriteLine($"Response: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                string error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error: {response.StatusCode} ({response.ReasonPhrase}). Details: {error}");
            }

            // parse the greeting response
            string responseBody = await response.Content.ReadAsStringAsync();
            var greetingResponse = JsonSerializer.Deserialize<GreetingResponse>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Console.WriteLine($"\nGreeting: {greetingResponse?.GreetingMessage}");
        }
    }

    // classes to map the API response
    public class TimesOfDayResponse
    {
        public List<string> TimesOfDay { get; set; } = new();
    }

    public class LanguagesResponse
    {
        public List<string> Languages { get; set; } = new();
    }

    public class GreetingResponse
    {
        public string? GreetingMessage { get; set; }
    }

    
}