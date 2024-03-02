
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Windows;
using System.Xml;
using System.Linq;
using System.Windows.Media.Imaging;
using Newtonsoft.Json.Serialization;
using System.Windows.Controls;
using Formatting = Newtonsoft.Json.Formatting;

namespace PokemonIndex
{
    public partial class MainWindow : Window
    {
        private readonly PokemonApiClient _apiClient;

        public MainWindow()
        {
            InitializeComponent();
            _apiClient = new PokemonApiClient();
        }   

        private async Task<Pokemon> FetchPokemonData(string pokemonName)
        {
            try
            {
                Pokemon pokemon = await _apiClient.GetPokemonAsync(pokemonName);

                if (pokemon != null)
                {
                    // Pokemon stats 
                    UpdateUIWithPokemonData(pokemon);
                    // Pokemon die nog niet gezien zijn geweest registreren in de pokedex als "Seen"
                    string pokemonInfo = $"{pokemon.Name} ({pokemon.Id})";
                    if (!SeenPokemonListBox.Items.Contains(pokemonInfo))
                    {
                        SeenPokemonListBox.Items.Add(pokemonInfo);
                    }
                }
                else
                {
                    MessageBox.Show($"Pokémon '{pokemonName}' not found.");
                }

                return pokemon;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error occurred while fetching Pokémon data: {ex.Message}");
                return null;
            }
        }

        private void UpdateUIWithPokemonData(Pokemon pokemon)
        {
            NameLabel.Content = $"{pokemon.Name}";
            TypeLabel.Content = $"{string.Join(", ", pokemon.Types.Select(t => t.Type.Name))}";
            WeightLabel.Content = $"{pokemon.Weight}";
            HeightLabel.Content = $"{pokemon.Height}";
            StatsLabel.Content = "Statistieken:";

            try
            {
                BitmapImage pokeImage = new BitmapImage(new Uri(
                    $"https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/{pokemon.Id}.png"));
                PokeSprite.Source = pokeImage;
            }
            catch (Exception)
            {
                PokeSprite.Source = null;
            }

            // Maakt een lijst aan van stats die de pokemon heeft. (meer voor een mooiere UI)
            foreach (Stats stat in pokemon.Stats)
            {
                StatsLabel.Content += $"\n{stat.Stat.Name}: {stat.Base_Stat}";
            }
        }

        private async void SeenPokemonListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Check if an item is selected
            if (SeenPokemonListBox.SelectedItem != null)
            {
                // Extract the Pokemon name from the selected ListBoxItem
                string selectedPokemonInfo = SeenPokemonListBox.SelectedItem.ToString();
                string[] infoParts = selectedPokemonInfo.Split(' '); // Assuming the format is "Name (ID)"
                string pokemonName = infoParts[0]; // Get the Pokemon name
                // Fetch data for the selected Pokemon
                await FetchPokemonData(pokemonName);
            }
        }



        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            Chances = 3;
            string pokemonName = PokemonNameTextBox.Text.ToLower();
            FetchPokemonData(pokemonName);
        }

        private void Shinyfy_Click(object sender, RoutedEventArgs e)
        {

        }

        private int Chances = 3;

        private async void Capture_Click(object sender, RoutedEventArgs e)
        {
            var pokemonName = NameLabel.Content as string;

            var pokemon = await FetchPokemonData(pokemonName); // Fetch Pokemon data
            if (pokemon != null)
            {
                bool captured = await AttemptCapture(pokemon); // Attempt to capture the Pokemon
                if (captured)
                {
                    // Handle successful capture
                }
                else
                {
                    // Handle failed capture
                }
            }
            if (pokemon != null && pokemon.Id > 0)
            {
                string filePath = "captured_pokemon.json"; // Specify the file path
                SaveCapturedPokemonToJson(pokemon, filePath);
            }
        }

        private void SaveCapturedPokemonToJson(Pokemon pokemon, string filePath)
        {
            try
            {
                // Serialize the Pokemon object to JSON
                string jsonData = JsonConvert.SerializeObject(pokemon, (Formatting)Formatting.Indented);

                // Write JSON data to the file
                File.WriteAllText(filePath, jsonData);

                MessageBox.Show($"Captured Pokemon '{pokemon.Name}' saved to '{filePath}'.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error occurred while saving captured Pokemon to JSON: {ex.Message}");
            }
        }

        private async Task<bool> AttemptCapture(Pokemon pokemon)
        {
            double captureChance = 0.3;
            Random random = new Random();
            double randomNumber = random.NextDouble();
            Chances--;

            if (randomNumber <= captureChance && Chances > 0)
            {
                await FetchPokemonData(pokemon.Name.ToLower());
                CaughtPokemonListBox.Items.Add($"{pokemon.Name} ({pokemon.Id})");
                MessageBox.Show($"{pokemon.Name} was successfully captured! ");
                Chances = 3;
                return true;
            }
            else
            {
                if (Chances <= 0)
                {
                    MessageBox.Show($"You have no chances left. ");
                    return false;

                }
                else
                {
                    MessageBox.Show($"{pokemon.Name} escaped! You have {Chances} chance(s) left. ");
                    return false;
                }
            }
        }

}

    public class Pokemon
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public PokemonType[] Types { get; set; }
        public int Weight { get; set; }
        public int Height { get; set; }
        public Stats[] Stats { get; set; }
    }

    public class PokemonType
    {
        public Type Type { get; set; }
    }

    public class Type
    {
        public string Name { get; set; }
    }

    public class Stats
    {
        public int Base_Stat { get; set; }
        public StatInfo Stat { get; set; }
    }

    public class StatInfo
    {
        public string Name { get; set; }
    }
}