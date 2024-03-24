// PokemonApiClient.cs

using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PokemonIndex
{
    public class PokemonApiClient
    {
        private readonly HttpClient _httpClient;

        public PokemonApiClient()
        {
            _httpClient = new HttpClient();
        }

        public async Task<Pokemon> GetPokemonAsync(string pokemonName)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync($"https://pokeapi.co/api/v2/pokemon/{pokemonName}");

                if (response.IsSuccessStatusCode)
                {
                    string responseData = await response.Content.ReadAsStringAsync();

                    if (!string.IsNullOrEmpty(responseData))
                    {
                        Pokemon pokemon = JsonConvert.DeserializeObject<Pokemon>(responseData);
                        return pokemon;
                    }
                    else
                    {
                        Console.WriteLine($"Empty response received for Pokémon: {pokemonName}");
                        return null;
                    }
                }
                else
                {
                    Console.WriteLine($"API returned error status code: {response.StatusCode} for Pokémon: {pokemonName}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching Pokémon data for '{pokemonName}': {ex.Message}");
                return null;
            }
        }



    }
}

