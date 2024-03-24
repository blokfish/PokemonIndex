using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;

namespace PokemonIndex
{
    internal class WikipediaApi
    {
        private readonly HttpClient _httpClient;

        public WikipediaApi()
        {
            _httpClient = new HttpClient();
        }

        public async Task<string> GetDescription(string pokemonName)
        {
            try
            {
                string pokemonListUrl = "https://en.wikipedia.org/wiki/List_of_generation_I_Pokémon";
                HttpResponseMessage response = await _httpClient.GetAsync(pokemonListUrl);

                if (response.IsSuccessStatusCode)
                {
                    string htmlContent = await response.Content.ReadAsStringAsync();

                    HtmlDocument htmlDoc = new HtmlDocument();
                    htmlDoc.LoadHtml(htmlContent);

                    HtmlNode pokemonTable = htmlDoc.DocumentNode.SelectSingleNode("//table[contains(@class, 'wikitable')]");

                    if (pokemonTable != null)
                    {
                        HtmlNodeCollection rows = pokemonTable.SelectNodes(".//tr");
                        if (rows != null)
                        {
                            foreach (HtmlNode row in rows)
                            {
                                HtmlNodeCollection cells = row.SelectNodes(".//td");
                                if (cells != null && cells.Count >= 2)
                                {
                                    HtmlNode nameNode = row.SelectSingleNode(".//th/b");
                                    if (nameNode != null)
                                    {
                                        string name = nameNode.InnerText.Trim();
                                        if (name.StartsWith(pokemonName, StringComparison.OrdinalIgnoreCase))
                                        {
                                            string description = cells[4].InnerText.Trim();
                                            return description;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    return "Description not found.";
                }
                else
                {
                    return "Failed to retrieve data from Wikipedia.";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching data from Wikipedia: {ex.Message}");
                return "An error occurred while fetching data from Wikipedia.";
            }
        }

        public async Task<string> GetDescription2(string pokemonName)
        {
            try
            {
                // Maak een verzoek naar de Wikipedia API om de samenvatting van de pagina op te halen
                string apiUrl = $"https://en.wikipedia.org/api/rest_v1/page/summary/{pokemonName}";
                HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    // Lees de JSON-response en haal de samenvatting op
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    JObject jsonObject = JObject.Parse(jsonResponse);
                    string description = jsonObject["extract"]?.ToString();

                    if (!string.IsNullOrEmpty(description))
                    {
                        return description;
                    }
                    else
                    {
                        return "Description not found.";
                    }
                }
                else
                {
                    return "Failed to retrieve data from Wikipedia.";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching data from Wikipedia: {ex.Message}");
                return "An error occurred while fetching data from Wikipedia.";
            }
        }



    }
}
