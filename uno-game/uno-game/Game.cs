using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ConsoleUno
{
    internal class Game
    {
        #region TheUnoGame
        private readonly ConsoleColor[] _cardColors = { ConsoleColor.Red, ConsoleColor.Blue, ConsoleColor.Green, ConsoleColor.Yellow };
        private readonly List<string> _playerNames = new();
        private readonly Random _random = new();
        private bool _animationsEnabled = true;

        public Game()
        {
            try
            {
                Console.CursorVisible = false;
            }
            catch (System.IO.IOException ex)
            {
                Console.WriteLine($"Warning: Could not set Console.CursorVisible: {ex.Message}");
                // Continue without setting cursor visible
            }
            _soundManager = new SoundManager(_soundEnabled, _effectsVolume, _musicVolume);
            _soundManager.UpdateEffectsVolume(_effectsVolume);
            _soundManager.UpdateMusicVolume(_musicVolume);
            StartMainMenuMusic();
        }

        //Kinézet és generálás
        private static List<Card> InitializeDeck()
        {
            var deck = new List<Card>();
            string[] colors = { "Red", "Blue", "Green", "Yellow" };
            string[] values = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "Skip", "Reverse", "+2" };

            foreach (var color in colors)
            {
                foreach (var value in values)
                {
                    deck.Add(new Card(color, value));
                    if (value != "0")
                    {
                        deck.Add(new Card(color, value));
                    }
                }
            }

            for (int i = 0; i < 4; i++)
            {
                deck.Add(new Card("Wild", "Wild"));
                deck.Add(new Card("Wild", "+4"));
            }

            Random rng = new Random();
            int n = deck.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                (deck[k], deck[n]) = (deck[n], deck[k]);
            }

            return deck;
        }

        private Card? DrawCard(List<Card>? hand, List<Card> deck)
        {
            if (deck.Count == 0)
                return null;

            Card card = deck[0];
            deck.RemoveAt(0);

            if (hand != null)
            {
                hand.Add(card);
                AnimateCardDraw(card);
            }

            return card;
        }

        private void AnimateCardDraw(Card card)
        {
            _soundManager.PlaySound(600, 100);
            Console.WriteLine($"Drew: {card}");
            if (_animationsEnabled)
            {
                Thread.Sleep(500);
            }
        }

        private void DrawGameScreen(List<Card> playerHand, Card? currentCard, bool isAI = false)
        {
            ClearConsole();
            Console.WriteLine($"Current Card: {currentCard?.ToString() ?? "No card"}");

            if (!isAI)
            {
                Console.WriteLine("\nYour Cards (1-9):");
                for (int i = 0; i < Math.Min(9, playerHand.Count); i++)
                {
                    ConsoleColor originalColor = Console.ForegroundColor;
                    Console.ForegroundColor = GetCardColor(playerHand[i].Color);
                    Console.Write($"{i + 1}. {playerHand[i]} ");
                    Console.ForegroundColor = originalColor;
                }

                if (playerHand.Count > 9)
                {
                    Console.WriteLine("\n\nOther Cards (can't be played right now):");
                    for (int i = 9; i < playerHand.Count; i++)
                    {
                        ConsoleColor originalColor = Console.ForegroundColor;
                        Console.ForegroundColor = GetCardColor(playerHand[i].Color);
                        Console.Write($"{playerHand[i]} ");
                        Console.ForegroundColor = originalColor;
                    }
                }

                Console.WriteLine("\n\nControls:");
                Console.WriteLine("1-9: Play card");
                Console.WriteLine("D: Draw card");
                Console.WriteLine("Q: Quit game");
            }
            else
            {
                Console.WriteLine($"\nAI has {playerHand.Count} cards");
            }
        }

        private ConsoleColor GetCardColor(string color) => color switch
        {
            "Red" => ConsoleColor.Red,
            "Blue" => ConsoleColor.Blue,
            "Green" => ConsoleColor.Green,
            "Yellow" => ConsoleColor.Yellow,
            _ => ConsoleColor.White
        };

        private void ShowVictoryScreen(string winner)
        {
            ClearConsole();
            string[] victoryText = {
        "▄█    █▄   ▄█  ▄████████     ███      ▄██████▄     ▄████████  ▄██   ▄   ",
        "███    ███ ███  ███    ███ ▀█████████▄ ███    ███   ███    ███ ███   ██▄ ",
        "███    ███ ███▌ ███    █▀     ▀███▀▀██ ███    ███   ███    ███ ███▄▄▄███ ",
        "███    ███ ███▌ ███            ███   ▀ ███    ███  ▄███▄▄▄▄██▀ ▀▀▀▀▀▀███ ",
        "███    ███ ███▌ ███            ███     ███    ███ ▀▀███▀▀▀▀▀   ▄██   ███ ",
        "███    ███ ███  ███    █▄      ███     ███    ███ ▀███████████ ███   ███ ",
        "███    ███ ███  ███    ███     ███     ███    ███   ███    ███ ███   ███ ",
        " ▀██████▀  █▀   ████████▀     ▄████▀    ▀██████▀    ███    ███  ▀█████▀  "
    };

            foreach (string line in victoryText)
            {
                CenterText(line);
            }

            Console.WriteLine($"\n{winner} wins!");
            _soundManager.PlaySound(1500, 500);
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey(true);
        }

        //Logika és Működés

        private Card HandleWildCard(Card wildCard)
        {
            string[] colors = { "Red", "Blue", "Green", "Yellow" };
            int colorChoice;

            int currentPlayer = Array.IndexOf(_playerNames.ToArray(), _playerNames.FirstOrDefault(n => n.StartsWith("AI")));
            bool isAI = currentPlayer > 0;

            if (isAI)
            {
                colorChoice = _random.Next(colors.Length);
                Console.WriteLine($"AI chose: {colors[colorChoice]}");
                Thread.Sleep(1000);
            }
            else
            {
                Console.WriteLine("\nChoose a color:");
                Console.WriteLine("1. Red");
                Console.WriteLine("2. Blue");
                Console.WriteLine("3. Green");
                Console.WriteLine("4. Yellow");

                do
                {
                    var key = Console.ReadKey(true);
                    colorChoice = key.KeyChar - '1';
                } while (colorChoice < 0 || colorChoice >= colors.Length);
            }

            return new Card(colors[colorChoice], wildCard.Value);
        }

        private static bool CanPlayCard(Card card, Card? currentCard)
        {
            if (currentCard == null) return true;
            return card.Color == "Wild" ||
                   card.Color == currentCard.Color ||
                   card.Value == currentCard.Value;
        }

        private void PlayCard(List<Card> hand, int index, ref Card? currentCard)
        {
            _soundManager.PlaySound(1200, 150);
            currentCard = hand[index];
            hand.RemoveAt(index);

            if (currentCard.Color == "Wild")
            {
                currentCard = HandleWildCard(currentCard);
            }
        }

        private void HandleAITurn(List<Card> aiHand, ref Card? currentCard, List<Card> deck)
        {
            for (int i = 0; i < aiHand.Count; i++)
            {
                if (CanPlayCard(aiHand[i], currentCard))
                {
                    if (aiHand[i].Value is "Skip" or "Reverse" or "+2" or "+4" or "Wild")
                    {
                        PlayCard(aiHand, i, ref currentCard);
                        Console.WriteLine($"AI played: {currentCard}");
                        Thread.Sleep(1000);
                        return;
                    }
                }
            }

            for (int i = 0; i < aiHand.Count; i++)
            {
                if (CanPlayCard(aiHand[i], currentCard))
                {
                    PlayCard(aiHand, i, ref currentCard);
                    Console.WriteLine($"AI played: {currentCard}");
                    Thread.Sleep(1000);
                    return;
                }
            }

            Card? drawnCard = DrawCard(aiHand, deck);
            Console.WriteLine("AI draws a card");
            Thread.Sleep(1000);

            if (drawnCard != null && CanPlayCard(drawnCard, currentCard))
            {
                PlayCard(aiHand, aiHand.Count - 1, ref currentCard);
                Console.WriteLine($"AI plays drawn card: {currentCard}");
                Thread.Sleep(1000);
            }
        }

        private bool HandleGameInput(List<Card> playerHand, ref Card? currentCard, List<Card> deck)
        {
            var key = Console.ReadKey(true);

            if (key.Key == ConsoleKey.Q)
            {
                return false;
            }

            if (key.Key == ConsoleKey.D)
            {
                DrawCard(playerHand, deck);
                return true;
            }

            if (int.TryParse(key.KeyChar.ToString(), out int cardIndex) &&
                cardIndex > 0 && cardIndex <= playerHand.Count)
            {
                Card selectedCard = playerHand[cardIndex - 1];
                if (CanPlayCard(selectedCard, currentCard))
                {
                    PlayCard(playerHand, cardIndex - 1, ref currentCard);
                }
                else
                {
                    Console.WriteLine("\nInvalid move! Press any key to continue...");
                    Console.ReadKey(true);
                }
            }
            return true;
        }

        private static int GetNextPlayer(int currentPlayer, int numberOfPlayers, bool clockwise)
        {
            if (clockwise)
                return (currentPlayer + 1) % numberOfPlayers;

            return (currentPlayer - 1 + numberOfPlayers) % numberOfPlayers;
        }

        private void PlayGame(int numberOfPlayers)
        {
            StopMainMenuMusic();
            ClearConsole();

            bool isSinglePlayer = _playerNames.Count == 0;
            if (isSinglePlayer)
            {
                _playerNames.Clear();
                _playerNames.Add("You");
                for (int i = 1; i < numberOfPlayers; i++)
                {
                    _playerNames.Add($"AI {i}");
                }
            }

            var deck = InitializeDeck();
            var playerHands = new List<List<Card>>();

            for (int i = 0; i < numberOfPlayers; i++)
            {
                playerHands.Add(new List<Card>());
                for (int j = 0; j < 7; j++)
                {
                    DrawCard(playerHands[i], deck);
                }
            }

            Card? currentCard = DrawCard(null, deck);
            int currentPlayer = 0;
            bool clockwise = true;

            while (true)
            {
                bool isAI = isSinglePlayer && currentPlayer > 0;

                DrawGameScreen(playerHands[currentPlayer], currentCard, isAI);
                Console.WriteLine($"\n{_playerNames[currentPlayer]}'s turn");

                if (isAI)
                {
                    Thread.Sleep(1000);
                    HandleAITurn(playerHands[currentPlayer], ref currentCard, deck);
                }
                else if (!HandleGameInput(playerHands[currentPlayer], ref currentCard, deck))
                {
                    break;
                }

                if (playerHands[currentPlayer].Count == 0)
                {
                    ShowVictoryScreen(_playerNames[currentPlayer]);
                    break;
                }

                if (deck.Count == 0)
                    if (deck.Count == 0)
                    {
                        Console.WriteLine("\nNo more cards in the deck! Game Over!");
                        Thread.Sleep(2000);
                        break;
                    }

                if (currentCard != null)
                {
                    switch (currentCard.Value)
                    {
                        case "Skip":
                            currentPlayer = GetNextPlayer(currentPlayer, numberOfPlayers, clockwise);
                            break;
                        case "Reverse":
                            clockwise = !clockwise;
                            break;
                        case "+2":
                            int nextPlayer = GetNextPlayer(currentPlayer, numberOfPlayers, clockwise);
                            DrawCard(playerHands[nextPlayer], deck);
                            DrawCard(playerHands[nextPlayer], deck);
                            break;
                        case "+4":
                            nextPlayer = GetNextPlayer(currentPlayer, numberOfPlayers, clockwise);
                            for (int i = 0; i < 4; i++)
                            {
                                DrawCard(playerHands[nextPlayer], deck);
                            }
                            break;
                    }
                }

                currentPlayer = GetNextPlayer(currentPlayer, numberOfPlayers, clockwise);
            }
            StartMainMenuMusic();
            _playerNames.Clear();
        }

        private void PlayerNamesLogo()
        {
            string[] local_logo = {
                "██╗░░░░░░█████╗░░█████╗░░█████╗░██╗░░░░░",
                "██║░░░░░██╔══██╗██╔══██╗██╔══██╗██║░░░░░",
                "██║░░░░░██║░░██║██║░░╚═╝███████║██║░░░░░",
                "██║░░░░░██║░░██║██║░░██╗██╔══██║██║░░░░░",
                "███████╗╚█████╔╝╚█████╔╝██║░░██║███████╗",
                "╚══════╝░╚════╝░░╚════╝░╚═╝░░╚═╝╚══════╝"
            };

            foreach (var line in local_logo)
            {
                CenterText(line);
            }
        }

        private void PlayerNamesAnimation()
        {
            string[] local_menu = {
                "How many players? (2-4):"
            };

            foreach (var line in local_menu)
            {
                CenterText(line);
            }
        }

        private void GetPlayerNamesForLocalMultiplayer()
        {
            ClearConsole();
            PlayerNamesLogo();
            Console.WriteLine("\n\n");
            PlayerNamesAnimation();

            int playerCount;
            while (!int.TryParse(Console.ReadKey(true).KeyChar.ToString(), out playerCount)
                   || playerCount < 2 || playerCount > 4)
            {
                _soundManager.PlaySound(400, 100);
            }

            _soundManager.PlaySound(800, 100);
            _playerNames.Clear();

            Console.Clear();
            Console.WriteLine($"Enter names for {playerCount} players:");

            for (int i = 0; i < playerCount; i++)
            {
                Console.CursorVisible = true;
                Console.Write($"Player {i + 1}: ");
                string name = Console.ReadLine()?.Trim() ?? $"Player {i + 1}";
                if (string.IsNullOrWhiteSpace(name))
                    name = $"Player {i + 1}";
                _playerNames.Add(name);
                Console.CursorVisible = false;
            }

            PlayGame(playerCount);
        }
        #endregion

        #region Sounds
        private bool _soundEnabled = true;
        private int _effectsVolume = 100;
        private int _musicVolume = 100;
        private SoundManager _soundManager;
        private bool _musicPlaying = false;
        private Thread? _musicThread = null;

        private void StartMainMenuMusic()
        {
            if (_musicPlaying || !_soundEnabled) return;

            _musicPlaying = true;
            _musicThread = new Thread(() =>
            {
                _soundManager.PlayMainMenuMusic(true);
            });
            _musicThread.IsBackground = true;
            _musicThread.Start();
        }

        private void StopMainMenuMusic()
        {
            _musicPlaying = false;
            _musicThread?.Join(500);
            _soundManager.StopMusic();
        }

        private void AnimateText(string text)
        {
            if (_animationsEnabled)
            {
                foreach (char c in text)
                {
                    Console.Write(c);
                    Thread.Sleep(5);
                }
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine(text);
            }
        }

        private void CenterText(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                Console.WriteLine();
                return;
            }
            int leftPadding = (Console.WindowWidth - text.Length) / 2;
            if (leftPadding < 0) leftPadding = 0;
            Console.SetCursorPosition(leftPadding, Console.CursorTop);
            AnimateText(text);
        }

        private void ClearConsole()
        {
            try
            {
                Console.Clear();
            }
            catch (System.IO.IOException ex)
            {
                Console.WriteLine($"Warning: Could not clear console: {ex.Message}");
            }
        }
        #endregion

        #region Main Menu
        private void MainMenuLogo()
        {
            string[] main_logo = {
                "",
                "██╗░░░██╗███╗░░██╗░█████╗░██╗",
                "██║░░░██║████╗░██║██╔══██╗██║",
                "██║░░░██║██╔██╗██║██║░░██║██║",
                "██║░░░██║██║╚████║██║░░██║╚═╝",
                "╚██████╔╝██║░╚███║╚█████╔╝██╗",
                "░╚═════╝░╚═╝░░╚══╝░╚════╝░╚═╝"
            };

            foreach (var line in main_logo)
            {
                CenterText(line);
            }
        }

        private void MainMenuAnimation()
        {
            string[] main_menu = {
                "█▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀█",
                "█  1. Singleplayer    █",
                "█  2. Multiplayer     █",
                "█  3. Settings        █",
                "█  4. Tutorial        █",
                "█  5. Exit            █",
                "█▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄█"
            };

            foreach (var line in main_menu)
            {
                CenterText(line);
            }
        }

        private void DrawMainMenu()
        {
            ClearConsole();
            Console.SetCursorPosition(0, 0);
            MainMenuLogo();
            Console.WriteLine("\n\n");
            MainMenuAnimation();
        }

        public void Start()
        {
            while (true)
            {
                DrawMainMenu();
                var choice = Console.ReadKey(true).KeyChar;
                _soundManager.PlaySound(800, 100);

                switch (choice)
                {
                    case '1':
                        ShowSinglePlayerMenu();
                        break;
                    case '2':
                        ShowMultiplayerMenu();
                        break;
                    case '3':
                        ShowSettings();
                        break;
                    case '4':
                        ShowTutorial();
                        break;
                    case '5':
                        ClearConsole();
                        return;
                }
            }
        }
        #endregion

        #region SinglePlayerMenu
        private void SinglePlayerMenuLogo()
        {
            string[] singleplayer_logo = {
                "░██████╗██╗███╗░░██╗░██████╗░██╗░░░░░███████╗██████╗░██╗░░░░░░█████╗░██╗░░░██╗███████╗██████╗░",
                "██╔════╝██║████╗░██║██╔════╝░██║░░░░░██╔════╝██╔══██╗██║░░░░░██╔══██╗╚██╗░██╔╝██╔════╝██╔══██╗",
                "╚█████╗░██║██╔██╗██║██║░░██╗░██║░░░░░█████╗░░██████╔╝██║░░░░░███████║░╚████╔╝░█████╗░░██████╔╝",
                "░╚═══██╗██║██║╚████║██║░░╚██╗██║░░░░░██╔══╝░░██╔═══╝░██║░░░░░██╔══██║░░╚██╔╝░░██╔══╝░░██╔══██╗",
                "██████╔╝██║██║░╚███║╚██████╔╝███████╗███████╗██║░░░░░██║░░░░░██║░░██║░░░██║░░░███████╗██║░░██║",
                "╚═════╝░╚═╝╚═╝░░╚══╝░╚═════╝░╚══════╝╚══════╝╚═╝░░░░░╚══════╝╚═╝░░╚═╝░░░╚═╝░░░╚══════╝╚═╝░░╚═╝"
            };

            foreach (var line in singleplayer_logo)
            {
                CenterText(line);
            }
        }

        private void SinglePlayerMenuAnimation()
        {
            string[] singleplayer_menu = {
                "█▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀█",
                "█  1. 1v1                █",
                "█  2. 1v1v1              █",
                "█  3. 1v1v1v1            █",
                "█  4. Back to Main Menu  █",
                "█▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄█"
            };

            foreach (var line in singleplayer_menu)
            {
                CenterText(line);
            }
        }

        private void ShowSinglePlayerMenu()
        {
            ClearConsole();
            SinglePlayerMenuLogo();
            Console.WriteLine("\n\n");
            SinglePlayerMenuAnimation();

            while (true)
            {
                var key = Console.ReadKey(true);
                _soundManager.PlaySound(800, 100);

                switch (key.KeyChar)
                {
                    case '1':
                    case '2':
                    case '3':
                        PlayGame(int.Parse(key.KeyChar.ToString()) + 1);
                        return;
                    case '4':
                        return;
                }
            }
        }
        #endregion

        #region MultiPlayerMenu
        private void MultiPlayerMenuLogo()
        {
            string[] multiplayer_logo = {
                "███╗░░░███╗██╗░░░██╗██╗░░░░░████████╗██╗██████╗░██╗░░░░░░█████╗░██╗░░░██╗███████╗██████╗░",
                "████╗░████║██║░░░██║██║░░░░░╚══██╔══╝██║██╔══██╗██║░░░░░██╔══██╗╚██╗░██╔╝██╔════╝██╔══██╗",
                "██╔████╔██║██║░░░██║██║░░░░░░░░██║░░░██║██████╔╝██║░░░░░███████║░╚████╔╝░█████╗░░██████╔╝",
                "██║╚██╔╝██║██║░░░██║██║░░░░░░░░██║░░░██║██╔═══╝░██║░░░░░██╔══██║░░╚██╔╝░░██╔══╝░░██╔══██╗",
                "██║░╚═╝░██║╚██████╔╝███████╗░░░██║░░░██║██║░░░░░███████╗██║░░██║░░░██║░░░███████╗██║░░██║",
                "╚═╝░░░░░╚═╝░╚═════╝░╚══════╝░░░╚═╝░░░╚═╝╚═╝░░░░░╚══════╝╚═╝░░╚═╝░░░╚═╝░░░╚══════╝╚═╝░░╚═╝"
            };

            foreach (var line in multiplayer_logo)
            {
                CenterText(line);
            }
        }

        private void MultiPlayerMenuAnimation()
        {
            string[] multiplayer_menu = {
                "█▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀█",
                "█  1. Local              █",
                "█  2. Ethernal (N/A)     █",
                "█  3. Back to Main Menu  █",
                "█▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄█"
            };

            foreach (var line in multiplayer_menu)
            {
                CenterText(line);
            }
        }

        private void ShowMultiplayerMenu()
        {
            ClearConsole();
            MultiPlayerMenuLogo();
            Console.WriteLine("\n\n");
            MultiPlayerMenuAnimation();

            while (true)
            {
                var key = Console.ReadKey(true);
                _soundManager.PlaySound(800, 100);

                if (key.KeyChar == '3')
                    return;

                if (key.KeyChar == '1')
                {
                    GetPlayerNamesForLocalMultiplayer();
                    return;
                }

                if (key.KeyChar == '2')
                {
                    Console.WriteLine("\nThis feature is coming soon!");
                    Thread.Sleep(1500);
                }
            }
        }
        #endregion

        #region SettingsMenu
        private void SettingsMenuLogo()
        {
                    string[] settings_logo = {
                "░██████╗███████╗████████╗████████╗██╗███╗░░██╗░██████╗░░██████╗",
                "██╔════╝██╔════╝╚══██╔══╝╚══██╔══╝██║████╗░██║██╔════╝░██╔════╝",
                "╚█████╗░█████╗░░░░░██║░░░░░░██║░░░██║██╔██╗██║██║░░██╗░╚█████╗░",
                "░╚═══██╗██╔══╝░░░░░██║░░░░░░██║░░░██║██║╚████║██║░░╚██╗░╚═══██╗",
                "██████╔╝███████╗░░░██║░░░░░░██║░░░██║██║░╚███║╚██████╔╝██████╔╝",
                "╚═════╝░╚══════╝░░░╚═╝░░░░░░╚═╝░░░╚═╝╚═╝░░╚══╝░╚═════╝░╚═════╝░"
                    };

                foreach (var line in settings_logo)
                {
                    CenterText(line);
                }
        }

        private void SettingsMenuAnimation()
        {
            string[] settings_menu = {
                "█▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀█",
                $"█ 1. Sound: {(_soundEnabled ? "ON" : "OFF")}            █",
                $"█ 2. Animations: {(_animationsEnabled ? "ON" : "OFF")}       █",
                $"█ 3. Main Menu Music: {(_musicPlaying ? "ON" : "OFF")}  █",
                string.Format("█ 4. Effects Volume: {0,-10} █", _soundEnabled ? $"{_effectsVolume}%" : "disabled"),
                string.Format("█ 5. Music Volume: {0,-10}   █", _soundEnabled ? $"{_musicVolume}%" : "disabled"),
                "█ 6. Back to Main Menu    █",
                "█▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄█"
            };

            foreach (var line in settings_menu)
            {
                CenterText(line);
            }
        }

        private void ShowSettings()
        {
            while (true)
            {
                ClearConsole();
                SettingsMenuLogo();
                Console.WriteLine("\n\n");
                SettingsMenuAnimation();

                var key = Console.ReadKey(true);
                _soundManager.PlaySound(800, 100);

                switch (key.KeyChar)
                {
                    case '1':
                        _soundEnabled = !_soundEnabled;
                        _soundManager = new SoundManager(_soundEnabled, _effectsVolume, _musicVolume);
                        _soundManager.UpdateEffectsVolume(_effectsVolume);
                        _soundManager.UpdateMusicVolume(_musicVolume);
                        if (!_soundEnabled)
                            StopMainMenuMusic();
                        else if (_musicPlaying)
                            StartMainMenuMusic();
                        break;
                    case '2':
                        _animationsEnabled = !_animationsEnabled;
                        break;
                    case '3':
                        if (_musicPlaying)
                            StopMainMenuMusic();
                        else
                            StartMainMenuMusic();
                        break;
                    case '4':
                        ChangeEffectsVolume();
                        break;
                    case '5':
                        ChangeMusicVolume();
                        break;
                    case '6':
                        return;
                }
            }
        }

        private void ChangeEffectsVolume()
        {
            if (!_soundEnabled)
            {
                Console.WriteLine("\nSound is disabled. Enable sound first.");
                Thread.Sleep(1500);
                return;
            }

            ClearConsole();
            Console.WriteLine($"Adjust Effects Volume (Current: {_effectsVolume}%). Use +/-, then Enter to confirm:");
            int newVolume = _effectsVolume;
            while (true)
            {
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write($"Adjust Effects Volume (Current: {newVolume}%). Use +/-, then Enter to confirm: ");
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Add || key.KeyChar == '+' || key.Key == ConsoleKey.OemPlus)
                {
                    newVolume = Math.Clamp(newVolume + 10, 10, 100);
                }
                else if (key.Key == ConsoleKey.Subtract || key.KeyChar == '-' || key.Key == ConsoleKey.OemMinus)
                {
                    newVolume = Math.Clamp(newVolume - 10, 10, 100);
                }
                else if (key.Key == ConsoleKey.Enter)
                {
                    _effectsVolume = newVolume;
                    _soundManager.UpdateEffectsVolume(_effectsVolume);
                    break;
                }
            }
        }

        private void ChangeMusicVolume()
        {
            if (!_soundEnabled)
            {
                Console.WriteLine("\nSound is disabled. Enable sound first.");
                Thread.Sleep(1500);
                return;
            }

            ClearConsole();
            Console.WriteLine($"Adjust Music Volume (Current: {_musicVolume}%). Use +/-, then Enter to confirm:");
            int newVolume = _musicVolume;
            while (true)
            {
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write($"Adjust Music Volume (Current: {newVolume}%). Use +/-, then Enter to confirm: ");
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Add || key.KeyChar == '+')
                {
                    newVolume = Math.Clamp(newVolume + 10, 10, 100);
                }
                else if (key.Key == ConsoleKey.Subtract || key.KeyChar == '-')
                {
                    newVolume = Math.Clamp(newVolume - 10, 10, 100);
                }
                else if (key.Key == ConsoleKey.Enter)
                {
                    _musicVolume = newVolume;
                    _soundManager.UpdateMusicVolume(_musicVolume);

                    if (_musicPlaying)
                    {
                        StopMainMenuMusic();
                        StartMainMenuMusic();
                    }
                    break;
                }
            }
        }
        #endregion

    #region TutorialMenu
        private void TutorialMenuLogo()
        {
            string[] tutorial_logo = {
                @" _   _ _   _ _____ ___ ___  _   _      _",
                @"| | | | | | |_   _|_ _/ _ \| \ | |    | |",
                @"| |_| | | | | | |  | | | | |  \| |    | |",
                @"|  _  | |_| | | |  | | |_| | |\  |    |_|",
                @"|_| |_|\___/  |_| |___\_/|_| \_|    (_)"
            };
            foreach (var line in tutorial_logo)
            {
                CenterText(line);
            }
        }

        private void ShowTutorial()
        {
            ClearConsole();
            TutorialMenuLogo();
            Console.WriteLine("\n\n");

            AnimateText("Menu Navigation:");
            AnimateText("- Numbers (1-9): Navigate menus");
            AnimateText("\nIn-Game Commands:");
            AnimateText("- Numbers (1-9): Play corresponding card");
            AnimateText("- D: Draw a card");
            AnimateText("- Q: Quit game");
            AnimateText("\nSettings Controls:");
            AnimateText("- +: Increase volume");
            AnimateText("- -: Decrease volume");
            AnimateText("- Enter: Confirm volume");

            AnimateText("\n\nPress any key to return to Main Menu...");
            Console.ReadKey(true);
        }
        #endregion
    }
}
