
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
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Windows.Media;
using System.Windows.Navigation;

namespace PokemonIndex
{
    public partial class MainWindow : Window
    {
        private readonly PokemonApiClient _apiClient;
        private AttemptData _attemptData;
        private readonly string _attemptDataFilePath = "attempt_data.json";
        private WikipediaApi PokemonTcgApi = new WikipediaApi();
        internal PokemonApiClient ApiClient => _apiClient;

        public AttemptData AttemptData
        {
            get => _attemptData;
            set => _attemptData = value;
        }

        public string AttemptDataFilePath => _attemptDataFilePath;

        public MainWindow()
        {
            InitializeComponent();
            _apiClient = new PokemonApiClient();
            InitializeAttemptData();
            LoadSeenPokemonNames();
            LoadCaughtPokemonNames();

            // Pokemonlist vullen 
            FillPokemonListBox();
            listcollection.Clear();
            foreach (string str in PokemonListBox.Items)
            {
                listcollection.Add(str);

            }
        }

        private void InitializeAttemptData()
        {
            try
            {
                string jsonData = File.ReadAllText(AttemptDataFilePath);
                AttemptData = JsonConvert.DeserializeObject<AttemptData>(jsonData);
                if (AttemptData == null)
                {
                    AttemptData = new AttemptData { NumberOfAttempts = 3 };
                }

            }
            catch (FileNotFoundException)
            {
                AttemptData = new AttemptData { NumberOfAttempts = 3 };
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading attempt data: {ex.Message}");
            }
        }

        private async Task<Pokemon> FetchPokemonData(string pokemonName, bool isShiny)
        {
            try
            {
                Pokemon pokemon = await ApiClient.GetPokemonAsync(pokemonName);

                if (pokemon != null)
                {
                    UpdateUIWithPokemonData(pokemon, isShiny);

                    string pokemonInfo = $"{pokemon.Name} ({pokemon.Id}){(isShiny ? " Shiny" : "")}";
                    if (!SeenPokemonListBox.Items.Contains(pokemonInfo))
                    {
                        SeenPokemonListBox.Items.Add(pokemonInfo);

                        // JSON file
                        string seenPokemonFilePath = "seen_pokemon.json";
                        SavePokemonNamesToJson(SeenPokemonListBox.Items.Cast<string>().ToList(), seenPokemonFilePath);
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
        } // Fetched het pokemon 


        private bool IsPokemonShiny()
        {
            double shinyProbability = 0.15; // kans op shiny 0.01 = 1% kans
            Random random = new Random();
            double randomNumber = random.NextDouble();
            return randomNumber <= shinyProbability;
        } //bepaalt het shiny van het pokemon


        private void UpdateUIWithPokemonData(Pokemon pokemon, bool isShiny)
        {
            NameLabel.Content = pokemon.Name;

            if (isShiny)
            {
                ShinyLabel.Content = "Yes";
            }
            else
            {
                ShinyLabel.Content = "No";
            }

            TypeLabel.Content = string.Join(", ", pokemon.Types.Select(t => t.Type.Name));
            WeightLabel.Content = pokemon.Weight;
            HeightLabel.Content = pokemon.Height;

            try
            {
                if (isShiny)
                {
                    PokeSprite.Source =
                        new BitmapImage(new Uri(
                            $"https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/shiny/{pokemon.Id}.png"));
                }
                else
                {
                    PokeSprite.Source =
                        new BitmapImage(new Uri(
                            $"https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/{pokemon.Id}.png"));
                }
            }
            catch (Exception)
            {
                PokeSprite.Source = null;
            }

            StatsLabel.Content = "Statistieken:";
            foreach (Stats stat in pokemon.Stats)
            {
                StatsLabel.Content += $"\n{stat.Stat.Name}: {stat.Base_Stat}";
            }
        } // de ui veranderen naar het pokemon die je hebt aangeroepen met shiny check


        private async void SeenPokemonListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SeenPokemonListBox.SelectedItem != null)
            {
                string selectedPokemonInfo = SeenPokemonListBox.SelectedItem.ToString();
                string[] infoParts = selectedPokemonInfo.Split(' ');
                string pokemonName = infoParts[0];
                bool isShiny = infoParts.Contains("Shiny"); // shinycheck

                Pokemon selectedPokemon = await ApiClient.GetPokemonAsync(pokemonName);

                if (selectedPokemon != null)
                {
                    try
                    {
                        string spriteUrl;
                        if (isShiny)
                        {
                            spriteUrl =
                                $"https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/shiny/{selectedPokemon.Id}.png";
                        }
                        else
                        {
                            spriteUrl =
                                $"https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/{selectedPokemon.Id}.png";
                        }

                        PokeSprite2.Source = new BitmapImage(new Uri(spriteUrl));
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error loading Pokemon image: {ex.Message}");
                    }
                }
            }
        } // filteren tussen gezien pokemon listbox


        private async void CaughtPokemonListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CaughtPokemonListBox.SelectedItem != null)
            {
                string selectedPokemonInfo = CaughtPokemonListBox.SelectedItem.ToString();
                string[] infoParts = selectedPokemonInfo.Split(' ');
                string pokemonName = infoParts[0];
                bool isShiny = infoParts.Contains("Shiny"); //shinycheck

                Pokemon selectedPokemon = await ApiClient.GetPokemonAsync(pokemonName);

                if (selectedPokemon != null)
                {
                    try
                    {
                        string spriteUrl;
                        if (isShiny)
                        {
                            spriteUrl =
                                $"https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/shiny/{selectedPokemon.Id}.png";
                        }
                        else
                        {
                            PokeSprite4.Source = new BitmapImage(new Uri(
                                $"https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/{selectedPokemon.Id}.png"));

                            spriteUrl =
                                $"https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/{selectedPokemon.Id}.png";
                        }

                        BitmapImage pokeImage = new BitmapImage(new Uri(spriteUrl));
                        PokeSprite3.Source = pokeImage;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error loading Pokemon image: {ex.Message}");
                    }
                }
            }
        } // filteren tussen gevangen pokemon listbox


        private void SavePokemonNamesToJson(List<string> pokemons, string filePath)
        {
            try
            {
                var pokemonNameList = new PokemonNameList { PokemonNames = pokemons };
                string jsonData = JsonConvert.SerializeObject(pokemonNameList, Formatting.Indented);
                File.WriteAllText(filePath, jsonData);
                MessageBox.Show($"Pokemon saved to '{filePath}'.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error occurred while saving Pokemon names to JSON: {ex.Message}");
            }
        } // pokemons in json files steken

        private List<string> LoadPokemonNamesFromJson(string filePath)
        {
            try
            {
                string jsonData = File.ReadAllText(filePath);
                var pokemonNameList = JsonConvert.DeserializeObject<PokemonNameList>(jsonData);
                return pokemonNameList?.PokemonNames ?? new List<string>();
            }
            catch (Exception)
            {
                return new List<string>();
            }
        } // list met de namen van pokemons



        private void LoadSeenPokemonNames()
        {
            string filePath = "seen_pokemon.json";
            var seenPokemonNames = LoadPokemonNamesFromJson(filePath);
            foreach (var pokemonName in seenPokemonNames)
            {
                SeenPokemonListBox.Items.Add(pokemonName);
            }
        } // json gezien pokemons oproepen

        private void LoadCaughtPokemonNames()
        {
            string filePath = "caught_pokemon.json";
            var caughtPokemonNames = LoadPokemonNamesFromJson(filePath);
            foreach (var pokemonName in caughtPokemonNames)
            {
                CaughtPokemonListBox.Items.Add(pokemonName);
            }
        } // json gevangen pokemons oproepen

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            var pokemonName = PokemonNameTextBox.Text.ToLower();
            bool isShiny = IsPokemonShiny();
            await FetchPokemonData(pokemonName, isShiny);
        } //pokemon opzoeken in tab 1


        private void SaveAttemptData()
        {
            try
            {
                string jsonData = JsonConvert.SerializeObject(AttemptData, Formatting.Indented);
                File.WriteAllText(AttemptDataFilePath, jsonData);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving attempt data: {ex.Message}");
            }
        } //json file 


        private async void Capture_Click(object sender, RoutedEventArgs e)
        {
            var pokemonName = NameLabel.Content as string;
            bool isShiny;
            string shinyContent = ShinyLabel.Content.ToString().ToLower();

            if (shinyContent == "yes")
            {
                isShiny = true;

            }
            else if (shinyContent == "no")
            {
                isShiny = false;
            }
            else
            {

                return;
            }

            if (AttemptData.NumberOfAttempts > 0)
            {
                var pokemon = await FetchPokemonData(pokemonName, isShiny);
                AttemptCapture(pokemon, isShiny);
                AttemptData.NumberOfAttempts--;
                SaveAttemptData();
            }
            else
            {
                MessageBox.Show("You have no attempts left.");
            }
        } //pokemon vangen knop


        private bool AttemptCapture(Pokemon pokemon, bool isShiny)
        {
            double captureChance = 0.3;
            Random random = new Random();
            double randomNumber = random.NextDouble();
            if (randomNumber <= captureChance && AttemptData.NumberOfAttempts > 0)
            {
                pokemon.IsShiny = isShiny;

                UpdateUIWithPokemonData(pokemon, isShiny);

                MessageBox.Show($"{pokemon.Name} was successfully captured! Chances have gone back to 3.");

                string caughtPokemonFilePath = "caught_pokemon.json";
                string pokemonInfo =
                    $"{pokemon.Name} ({pokemon.Id}){(pokemon.IsShiny ? " Shiny" : "")}"; // Pokemon (id) Shiny

                var caughtPokemonNames = LoadPokemonNamesFromJson(caughtPokemonFilePath);
                caughtPokemonNames.Add(pokemonInfo);
                SavePokemonNamesToJson(caughtPokemonNames, caughtPokemonFilePath);

                CaughtPokemonListBox.Items.Add(pokemonInfo);

                AttemptData.NumberOfAttempts = 3;
                return true;
            }
            else
            {
                if (AttemptData.NumberOfAttempts <= 0)
                {
                    MessageBox.Show($"You have no chances left.");
                    return false;
                }
                else
                {
                    MessageBox.Show($"{pokemon.Name} escaped! You have {AttemptData.NumberOfAttempts} chance(s) left.");
                    return false;
                }
            }
        } //probeert het pokemon te vangen 




        private void Reset_chances__Click(object sender, RoutedEventArgs e)
        {
            AttemptData = new AttemptData { NumberOfAttempts = 3 };
            SaveAttemptData();
        } //reset chances naar 3 voor slechte geluk

        private async void Randomize_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Random random = new Random();
                int randomPokemonId = random.Next(1, 151);
                string pokemonName = randomPokemonId.ToString();

                bool isShiny = IsPokemonShiny(); //shiny check
                Pokemon randomPokemon = await FetchPokemonData(pokemonName, isShiny);
                if (randomPokemon != null)
                {

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error occurred while fetching random Pokémon data: {ex.Message}");
            }
        } //randomized pokemons

        private void ClearJSON_FileSeen(object sender, RoutedEventArgs e)
        {

            string filePath2 = "seen_pokemon.json";

            File.WriteAllText(filePath2, "{}");

            PokeSprite2.Source = null;

            MessageBox.Show("JSON file Seen cleared successfully.");

            SeenPokemonListBox.Items.Clear();
            LoadSeenPokemonNames();
        } // JSON files wissen Seen

        private void ClearJSON_FileCaught(object sender, RoutedEventArgs e)
        {

            string filePath = "caught_pokemon.json";

            File.WriteAllText(filePath, "{}");

            PokeSprite3.Source = null;

            MessageBox.Show("JSON file Caught cleared successfully.");

            CaughtPokemonListBox.Items.Clear();
            LoadCaughtPokemonNames();
        } // JSON files wissen Caught

        private async void FillPokemonListBox()
        {
            string filePath = "Pokemonlist.json";
            List<Pokemon> pokemonData = LoadPokemonDataFromJson(filePath);

            List<string>
                seenPokemonNames = LoadPokemonNamesFromJson("seen_pokemon.json"); //pokemon halen van json seen pokemon

            PokemonListBox.Items.Clear();

            foreach (var pokemon in pokemonData)
            {
                string pokemonInfo = $"{pokemon.Name} ({pokemon.Id})";

                bool isSeen = seenPokemonNames.Any(name => name.ToLower().StartsWith($"{pokemon.Name.ToLower()} "));

                if (isSeen) //pokemon die gezien zijn een vinkje bij toevoegen
                {
                    pokemonInfo += " ✓";
                }

                PokemonListBox.Items.Add(pokemonInfo);
            }
        } // Tab 4 listbox vullen met pokemon


        private void RefreshPokemonListBox(List<Pokemon> pokemonData, List<string> seenPokemonNames)
        {
            PokemonListBox.Items.Clear();

            foreach (var pokemon in pokemonData)
            {
                string pokemonInfo = $"{pokemon.Name} ({pokemon.Id})";

                bool isSeen = seenPokemonNames.Any(name => name.ToLower().StartsWith($"{pokemon.Name.ToLower()} "));

                if (isSeen) //pokemon die gezien zijn een vinkje bij toevoegen
                {
                    pokemonInfo += " ✓";
                }

                PokemonListBox.Items.Add(pokemonInfo);
            }
        } // refreshed het vinkjes na je iets gezien hebt


        private List<Pokemon> LoadPokemonDataFromJson(string filePath)
        {
            try
            {
                string jsonData = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<List<Pokemon>>(jsonData);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading Pokémon data from JSON: {ex.Message}");
                return new List<Pokemon>();
            }
        } //pokemons van de json file halen





        private List<string> listcollection = new List<string>();

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = txtSearch.Text.ToLower();

            // Clear the ListBox to prepare for filtered items
            PokemonListBox.Items.Clear();

            if (string.IsNullOrEmpty(searchText))
            {
                // If the search text is empty, add all items back to the ListBox
                foreach (string str in listcollection)
                {
                    PokemonListBox.Items.Add(str);
                }
            }
            else
            {
                // Filter and add items that start with the search text to the ListBox
                foreach (string str in listcollection)
                {
                    if (str.ToLower().StartsWith(searchText))
                    {
                        PokemonListBox.Items.Add(str);
                    }
                }
            }
        } // Pokemon filteren op naam in tab 4 

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            string filePath = "Pokemonlist.json";
            List<Pokemon> pokemonData = LoadPokemonDataFromJson(filePath);
            List<string> seenPokemonNames = LoadPokemonNamesFromJson("seen_pokemon.json");
            RefreshPokemonListBox(pokemonData, seenPokemonNames);
        }

        //abdu heeft hieraan gewerkt
        private async void PokemonListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PokemonListBox.SelectedItem != null)
            {

                string selectedPokemon = PokemonListBox.SelectedItem.ToString();
                string[] parts = selectedPokemon.Split();
                string pokemonName = parts[0];

                Pokemon pokemon = await ApiClient.GetPokemonAsync(pokemonName.ToLower());
                PokeSprite4.Source = new BitmapImage(new Uri($"https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/{pokemon.Id}.png"));
                NameLabel2.Content = pokemon.Name;
                TypeLabel2.Content = string.Join(", ", pokemon.Types.Select(t => t.Type.Name));
                WeightLabel2.Content = pokemon.Weight;
                HeightLabel2.Content = pokemon.Height;

                WikipediaApi wikipediaApi = new WikipediaApi();

                string pokemonDescription = await wikipediaApi.GetDescription(pokemonName);
                string pokemonDescription2 = await wikipediaApi.GetDescription2(pokemonName);

                WikipediaInfoTextBlock.Text = pokemonDescription;
                WikipediaInfoTextBlock2.Text = pokemonDescription2;

                

            }
        }

    }



}

    public class AttemptData
    {
        public int NumberOfAttempts { get; set; }
    }
    public class PokemonNameList
    {
        public List<string> PokemonNames { get; set; }
    }

    public class Pokemon
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public PokemonType[] Types { get; set; }
        public int Weight { get; set; }
        public int Height { get; set; }
        public Stats[] Stats { get; set; }
        public bool IsShiny { get; set; }

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
