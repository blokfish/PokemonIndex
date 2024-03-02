using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace PokemonIndex
{
    internal class PokemonApiClient
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private const string BaseUrl = "https://pokeapi.co/api/v2/pokemon/";

        public PokemonApiClient()
        {
            _httpClient.BaseAddress = new Uri(BaseUrl);
        }

        public async Task<Pokemon> GetPokemonAsync(string pokemonName)
        {
            if (string.IsNullOrWhiteSpace(pokemonName))
            {
                throw new ArgumentException("Pokemon name cannot be null or empty.", nameof(pokemonName));
            }

            try
            {
                // Construct the URI with the correct scheme ('https')
                var apiUrl = $"https://pokeapi.co/api/v2/pokemon/{pokemonName.ToLower()}";

                // Send the HTTP GET request
                HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);

                // Check if the response is successful
                if (response.IsSuccessStatusCode)
                {
                    // Read the response content as string
                    string jsonResponse = await response.Content.ReadAsStringAsync();

                    // Deserialize the JSON response to a Pokemon object
                    return JsonConvert.DeserializeObject<Pokemon>(jsonResponse);
                }

                return null; // Pokemon not found
            }
            catch (HttpRequestException ex)
            {
                // Handle HTTP request errors
                throw new PokemonApiException("Error occurred while calling the Pokemon API.", ex);
            }
        }



    }

    public class PokemonApiException : Exception
    {
        public PokemonApiException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}