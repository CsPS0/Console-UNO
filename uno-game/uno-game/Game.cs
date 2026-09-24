using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;

namespace ConsoleUno
{
    public class Game
    {
        private readonly List<string> _playerNames = new();
        private readonly Random _random = new();
        private bool _isHungarian = true;

        private bool _soundEnabled = true;
        private int _effectsVolume = 100;
        private int _musicVolume = 100;
        private SoundManager _soundManager;
        private bool _musicPlaying = true;

        private List<Card> _deck = new();
        private List<Card> _discardPile = new();
        private string _lastActionMessage = "";
        private ConsoleColor _lastActionColor = ConsoleColor.White;

        public Game()
        {
            try
            {
                Console.CursorVisible = false;
            }
            catch { }

            LoadSettingsCache();
            _soundManager = new SoundManager(_soundEnabled, _effectsVolume, _musicVolume);
            if (_musicPlaying)
            {
                StartMainMenuMusic();
            }
        }

        #region Deck & Game Logic
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

            ShuffleDeck(deck);
            return deck;
        }

        private static void ShuffleDeck(List<Card> deck)
        {
            Random rng = new();
            int n = deck.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                (deck[k], deck[n]) = (deck[n], deck[k]);
            }
        }

        private Card? DrawCard(List<Card>? hand, bool animate = true)
        {
            if (_deck.Count == 0)
            {
                if (_discardPile.Count > 0)
                {
                    _lastActionMessage = _isHungarian
                        ? "A pakli elfogyott! A dobopakli ujra lett keverve."
                        : "Deck was empty! Reshuffled discard pile into draw deck.";
                    _lastActionColor = ConsoleColor.Magenta;

                    _deck.AddRange(_discardPile);
                    _discardPile.Clear();
                    ShuffleDeck(_deck);
                }
                else
                {
                    return null;
                }
            }

            Card card = _deck[0];
            _deck.RemoveAt(0);

            if (hand != null)
            {
                hand.Add(card);
                if (animate)
                {
                    _soundManager.PlayCardDraw();
                }
            }

            return card;
        }

        private static bool CanPlayCard(Card card, Card? currentCard)
        {
            if (currentCard == null) return true;
            return card.Color == "Wild" ||
                   card.Color == currentCard.Color ||
                   card.Value == currentCard.Value;
        }

        private Card HandleWildCard(Card wildCard, bool isAI)
        {
            string[] colors = { "Red", "Blue", "Green", "Yellow" };
            int colorChoice;

            if (isAI)
            {
                colorChoice = _random.Next(colors.Length);
                string chosenColor = colors[colorChoice];
                _lastActionMessage = _isHungarian
                    ? $"AI valasztott szine: {TranslateColor(chosenColor)}"
                    : $"AI chose color: {chosenColor}";
                _lastActionColor = GetColorConsole(chosenColor);
                Thread.Sleep(800);
            }
            else
            {
                ClearConsole();
                DrawHeader();
                Console.WriteLine();
                CenterColoredText(_isHungarian ? "=== VALASSZ SZINT! ===" : "=== CHOOSE A COLOR! ===", ConsoleColor.Yellow);
                Console.WriteLine();

                string[] options = _isHungarian
                    ? new[] { "[1] PIROS", "[2] KEK", "[3] ZOLD", "[4] SARGA" }
                    : new[] { "[1] RED", "[2] BLUE", "[3] GREEN", "[4] YELLOW" };

                ConsoleColor[] optColors = { ConsoleColor.Red, ConsoleColor.Cyan, ConsoleColor.Green, ConsoleColor.Yellow };

                int startX = Math.Max(2, (GetWindowWidth() - 48) / 2);
                SafeSetCursorPosition(startX, Console.CursorTop);
                for (int i = 0; i < options.Length; i++)
                {
                    Console.ForegroundColor = optColors[i];
                    Console.Write($"{options[i]}    ");
                }
                Console.ResetColor();
                Console.WriteLine("\n");

                do
                {
                    var key = SafeReadKey();
                    colorChoice = key.KeyChar - '1';
                } while (colorChoice < 0 || colorChoice >= colors.Length);

                _soundManager.PlayMenuBeep();
            }

            return new Card(colors[colorChoice], wildCard.Value);
        }

        private static string TranslateColor(string color) => color switch
        {
            "Red" => "Piros",
            "Blue" => "Kek",
            "Green" => "Zold",
            "Yellow" => "Sarga",
            _ => color
        };

        private static ConsoleColor GetColorConsole(string color) => color switch
        {
            "Red" => ConsoleColor.Red,
            "Blue" => ConsoleColor.Cyan,
            "Green" => ConsoleColor.Green,
            "Yellow" => ConsoleColor.Yellow,
            "Wild" => ConsoleColor.Magenta,
            _ => ConsoleColor.White
        };

        private static int GetNextPlayerIndex(int currentPlayer, int numberOfPlayers, bool clockwise)
        {
            if (clockwise)
                return (currentPlayer + 1) % numberOfPlayers;

            return (currentPlayer - 1 + numberOfPlayers) % numberOfPlayers;
        }
        #endregion

        #region UI & Rendering
        private static int GetWindowWidth()
        {
            try
            {
                return Math.Max(60, Console.WindowWidth);
            }
            catch
            {
                return 80;
            }
        }

        private static void SafeSetCursorPosition(int left, int top)
        {
            try
            {
                int maxLeft = Math.Max(0, Console.WindowWidth - 1);
                int maxTop = Math.Max(0, Console.WindowHeight - 1);
                Console.SetCursorPosition(Math.Clamp(left, 0, maxLeft), Math.Clamp(top, 0, maxTop));
            }
            catch { }
        }

        private void CenterColoredText(string text, ConsoleColor color)
        {
            int left = Math.Max(0, (GetWindowWidth() - text.Length) / 2);
            SafeSetCursorPosition(left, Console.CursorTop);
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ResetColor();
        }

        private void ClearConsole()
        {
            try
            {
                Console.Clear();
            }
            catch { }
        }

        private void DrawHeader()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            string title = "[ C O N S O L E   U N O ]";
            int left = Math.Max(0, (GetWindowWidth() - title.Length) / 2);
            SafeSetCursorPosition(left, 0);
            Console.WriteLine(title);

            Console.ForegroundColor = ConsoleColor.DarkGray;
            string sub = _isHungarian
                ? "Fejleszto: Solti Csongor Peter"
                : "Developer: Solti Csongor Peter";
            left = Math.Max(0, (GetWindowWidth() - sub.Length) / 2);
            SafeSetCursorPosition(left, 1);
            Console.WriteLine(sub);
            Console.ResetColor();
            Console.WriteLine(new string('-', GetWindowWidth() - 1));
        }

        private void DrawGameBoard(List<Card> playerHand, Card? currentCard, int currentPlayerIndex, bool isSinglePlayer, bool clockwise, int selectedIndex, int pageStart)
        {
            ClearConsole();
            DrawHeader();

            string turnName = _playerNames[currentPlayerIndex];
            string dirStr = clockwise ? "-> Oramutato szerint" : "<- Ellentetes irany";
            if (!_isHungarian)
            {
                dirStr = clockwise ? "-> Clockwise" : "<- Counter-Clockwise";
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"  {(_isHungarian ? "KOVETKEZO" : "TURN")}: ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"{turnName,-14}");

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($" |  {(_isHungarian ? "IRANY" : "DIR")}: ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"{dirStr,-22}");

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($" |  {(_isHungarian ? "PAKLIK" : "DECK")}: ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{_deck.Count} {(_isHungarian ? "lap" : "cards")}");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write("  " + (_isHungarian ? "Jatekosok:" : "Players:") + " ");
            for (int i = 0; i < _playerNames.Count; i++)
            {
                bool isCurrent = i == currentPlayerIndex;
                if (isCurrent)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write($"[{_playerNames[i]}] ");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write($"{_playerNames[i]} ");
                }
            }
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(new string('-', GetWindowWidth() - 1));
            Console.ResetColor();

            Console.WriteLine();
            int centerStartX = Math.Max(2, (GetWindowWidth() - 44) / 2);

            string[] drawPileBox =
            {
                "┌─────┐",
                "│░░░░░│",
                "│ UNO │",
                "│░░░░░│",
                "└─────┘"
            };

            string[] discardPileBox = currentCard?.GetAsciiLines(false) ?? new[]
            {
                "┌─────┐",
                "│     │",
                "│ --- │",
                "│     │",
                "└─────┘"
            };

            ConsoleColor discardColor = currentCard?.GetConsoleColor() ?? ConsoleColor.White;

            SafeSetCursorPosition(centerStartX, Console.CursorTop);
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("  [ " + (_isHungarian ? "HUZOPAKLI" : "DRAW PILE") + " ]           [ " + (_isHungarian ? "DOBOPAKLI" : "DISCARD PILE") + " ]");
            Console.ResetColor();
            Console.WriteLine();

            for (int row = 0; row < 5; row++)
            {
                SafeSetCursorPosition(centerStartX, Console.CursorTop);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write($"    {drawPileBox[row]}");
                Console.ResetColor();

                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write(row == 2 ? "     --->     " : "              ");
                Console.ResetColor();

                Console.ForegroundColor = discardColor;
                Console.WriteLine(discardPileBox[row]);
                Console.ResetColor();
            }

            string colorName = currentCard != null
                ? (_isHungarian ? TranslateColor(currentCard.Color) : currentCard.Color)
                : "None";
            string colorInfo = $"{(_isHungarian ? "Aktiv szin" : "Active Color")}: [ {colorName} ]";
            SafeSetCursorPosition(centerStartX + 20, Console.CursorTop);
            Console.ForegroundColor = discardColor;
            Console.WriteLine(colorInfo);
            Console.ResetColor();

            if (!string.IsNullOrEmpty(_lastActionMessage))
            {
                Console.WriteLine();
                CenterColoredText($">> {_lastActionMessage}", _lastActionColor);
            }
            else
            {
                Console.WriteLine();
            }

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(new string('-', GetWindowWidth() - 1));
            Console.ResetColor();

            int pageSize = 7;
            int pageEnd = Math.Min(pageStart + pageSize, playerHand.Count);

            string handHeader = _isHungarian
                ? $"LAPJAID ({playerHand.Count} db) - [{selectedIndex + 1}. kivalasztva]:"
                : $"YOUR HAND ({playerHand.Count} cards) - [Card {selectedIndex + 1} selected]:";
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"  {handHeader}");
            Console.ResetColor();

            List<string[]> renderedCards = new();
            for (int i = pageStart; i < pageEnd; i++)
            {
                renderedCards.Add(playerHand[i].GetAsciiLines(i == selectedIndex));
            }

            Console.Write("   ");
            if (pageStart > 0)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write(" << ");
                Console.ResetColor();
            }
            else
            {
                Console.Write("    ");
            }

            for (int i = pageStart; i < pageEnd; i++)
            {
                if (i == selectedIndex)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write($" >[{i + 1}]<  ");
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write($"   [{i + 1}]   ");
                    Console.ResetColor();
                }
            }

            if (pageEnd < playerHand.Count)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write(" >>");
                Console.ResetColor();
            }
            Console.WriteLine();

            for (int row = 0; row < 5; row++)
            {
                Console.Write("   ");
                Console.Write(pageStart > 0 && row == 2 ? " <- " : "    ");

                for (int col = 0; col < renderedCards.Count; col++)
                {
                    int cardIdx = pageStart + col;
                    Console.ForegroundColor = playerHand[cardIdx].GetConsoleColor();
                    Console.Write(renderedCards[col][row] + "  ");
                    Console.ResetColor();
                }

                if (pageEnd < playerHand.Count && row == 2)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write("->");
                    Console.ResetColor();
                }
                Console.WriteLine();
            }

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(new string('-', GetWindowWidth() - 1));
            Console.ForegroundColor = ConsoleColor.White;
            if (_isHungarian)
            {
                Console.WriteLine("  [<- / ->]: Lap kivalasztasa  |  [ENTER]: Lerakas  |  [D]: Huzas  |  [Q]: Kilepes");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("  (Gyorsbillentyuk: Nyomd le a lap szamat [1-9] kozvetlenul a lerakashoz)");
            }
            else
            {
                Console.WriteLine("  [<- / ->]: Select Card  |  [ENTER]: Play Card  |  [D]: Draw Card  |  [Q]: Quit");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("  (Tip: Press number keys 1-9 to play that card immediately)");
            }
            Console.ResetColor();
        }
        #endregion

        #region Game Loop
        private void PlayGame(int numberOfPlayers)
        {
            StopMainMenuMusic();
            ClearConsole();

            bool isSinglePlayer = _playerNames.Count == 0;
            if (isSinglePlayer)
            {
                _playerNames.Clear();
                _playerNames.Add(_isHungarian ? "Te (Jatekos 1)" : "You (Player 1)");
                string[] botNames = { "Byte", "Pixel", "Cipher" };
                for (int i = 1; i < numberOfPlayers; i++)
                {
                    _playerNames.Add($"AI {botNames[(i - 1) % botNames.Length]}");
                }
            }

            _deck = InitializeDeck();
            _discardPile.Clear();
            _lastActionMessage = "";

            var playerHands = new List<List<Card>>();
            for (int i = 0; i < numberOfPlayers; i++)
            {
                playerHands.Add(new List<Card>());
                for (int j = 0; j < 7; j++)
                {
                    DrawCard(playerHands[i], animate: false);
                }
            }

            Card? currentCard = DrawCard(null, animate: false);
            while (currentCard != null && currentCard.Value == "+4")
            {
                _deck.Add(currentCard);
                ShuffleDeck(_deck);
                currentCard = DrawCard(null, animate: false);
            }
            if (currentCard != null && currentCard.Color == "Wild")
            {
                currentCard.Color = "Red";
            }

            int currentPlayer = 0;
            bool clockwise = true;

            while (true)
            {
                bool isAI = isSinglePlayer && currentPlayer > 0;

                if (!isSinglePlayer && !isAI)
                {
                    ShowPassScreen(_playerNames[currentPlayer]);
                }

                int selectedIndex = 0;
                int pageSize = 7;
                bool turnCompleted = false;
                Card? playedCard = null;

                if (isAI)
                {

                    DrawGameBoard(playerHands[currentPlayer], currentCard, currentPlayer, isSinglePlayer, clockwise, 0, 0);
                    Thread.Sleep(900);
                    playedCard = HandleAITurn(playerHands[currentPlayer], ref currentCard);
                    turnCompleted = true;
                }
                else
                {

                    while (!turnCompleted)
                    {
                        int pageStart = (selectedIndex / pageSize) * pageSize;
                        DrawGameBoard(playerHands[currentPlayer], currentCard, currentPlayer, isSinglePlayer, clockwise, selectedIndex, pageStart);

                        var key = SafeReadKey();

                        if (key.Key == ConsoleKey.Q)
                        {
                            StartMainMenuMusic();
                            _playerNames.Clear();
                            return;
                        }

                        if (key.Key == ConsoleKey.LeftArrow)
                        {
                            if (selectedIndex > 0)
                            {
                                selectedIndex--;
                                _soundManager.PlayMenuBeep();
                            }
                        }
                        else if (key.Key == ConsoleKey.RightArrow)
                        {
                            if (selectedIndex < playerHands[currentPlayer].Count - 1)
                            {
                                selectedIndex++;
                                _soundManager.PlayMenuBeep();
                            }
                        }
                        else if (key.Key == ConsoleKey.Enter || key.Key == ConsoleKey.Spacebar)
                        {
                            if (playerHands[currentPlayer].Count > 0)
                            {
                                Card candidate = playerHands[currentPlayer][selectedIndex];
                                if (CanPlayCard(candidate, currentCard))
                                {
                                    playedCard = candidate;
                                    playerHands[currentPlayer].RemoveAt(selectedIndex);
                                    if (currentCard != null) _discardPile.Add(currentCard);
                                    currentCard = playedCard;

                                    if (currentCard.Color == "Wild")
                                    {
                                        currentCard = HandleWildCard(currentCard, isAI: false);
                                    }

                                    _soundManager.PlayCardPlay();
                                    _lastActionMessage = _isHungarian
                                        ? $"{_playerNames[currentPlayer]} kijatszotta: {currentCard.GetLocalizedName(true)}"
                                        : $"{_playerNames[currentPlayer]} played: {currentCard.GetLocalizedName(false)}";
                                    _lastActionColor = currentCard.GetConsoleColor();
                                    turnCompleted = true;
                                }
                                else
                                {
                                    _soundManager.PlayInvalid();
                                    _lastActionMessage = _isHungarian
                                        ? "Ervenytelen lepes! A lap nem egyezik szinben vagy ertekben."
                                        : "Invalid move! Card must match color or value.";
                                    _lastActionColor = ConsoleColor.Red;
                                }
                            }
                        }
                        else if (key.Key == ConsoleKey.D)
                        {
                            Card? drawn = DrawCard(playerHands[currentPlayer], animate: true);
                            if (drawn != null)
                            {
                                _lastActionMessage = _isHungarian
                                    ? $"{_playerNames[currentPlayer]} huzott egy lapot: {drawn.GetLocalizedName(true)}"
                                    : $"{_playerNames[currentPlayer]} drew a card: {drawn.GetLocalizedName(false)}";
                                _lastActionColor = ConsoleColor.Cyan;

                                selectedIndex = playerHands[currentPlayer].Count - 1;

                                if (CanPlayCard(drawn, currentCard))
                                {
                                    int newPageStart = (selectedIndex / pageSize) * pageSize;
                                    DrawGameBoard(playerHands[currentPlayer], currentCard, currentPlayer, isSinglePlayer, clockwise, selectedIndex, newPageStart);
                                    Console.ForegroundColor = ConsoleColor.Yellow;
                                    Console.WriteLine(_isHungarian
                                        ? "\n  A huzott lap kijatszhato! Megjatszod? [ENTER: Igen / Barmely mas gomb: Megtartom]"
                                        : "\n  The drawn card can be played! Play it now? [ENTER: Yes / Any other key: Pass]");
                                    Console.ResetColor();

                                    var decision = SafeReadKey();
                                    if (decision.Key == ConsoleKey.Enter || decision.Key == ConsoleKey.Spacebar)
                                    {
                                        playedCard = drawn;
                                        playerHands[currentPlayer].RemoveAt(selectedIndex);
                                        if (currentCard != null) _discardPile.Add(currentCard);
                                        currentCard = playedCard;

                                        if (currentCard.Color == "Wild")
                                        {
                                            currentCard = HandleWildCard(currentCard, isAI: false);
                                        }

                                        _soundManager.PlayCardPlay();
                                        _lastActionMessage = _isHungarian
                                            ? $"{_playerNames[currentPlayer]} azonnal kijatszotta a huzott lapot: {currentCard.GetLocalizedName(true)}"
                                            : $"{_playerNames[currentPlayer]} immediately played the drawn card: {currentCard.GetLocalizedName(false)}";
                                        _lastActionColor = currentCard.GetConsoleColor();
                                    }
                                }
                            }
                            else
                            {
                                _lastActionMessage = _isHungarian ? "Nincs tobb huzhato lap!" : "No cards left to draw!";
                                _lastActionColor = ConsoleColor.Red;
                            }
                            turnCompleted = true;
                        }
                        else if (int.TryParse(key.KeyChar.ToString(), out int num) && num >= 1 && num <= 9)
                        {
                            int targetIdx = pageStart + (num - 1);
                            if (targetIdx < playerHands[currentPlayer].Count)
                            {
                                selectedIndex = targetIdx;
                                Card candidate = playerHands[currentPlayer][selectedIndex];
                                if (CanPlayCard(candidate, currentCard))
                                {
                                    playedCard = candidate;
                                    playerHands[currentPlayer].RemoveAt(selectedIndex);
                                    if (currentCard != null) _discardPile.Add(currentCard);
                                    currentCard = playedCard;

                                    if (currentCard.Color == "Wild")
                                    {
                                        currentCard = HandleWildCard(currentCard, isAI: false);
                                    }

                                    _soundManager.PlayCardPlay();
                                    _lastActionMessage = _isHungarian
                                        ? $"{_playerNames[currentPlayer]} kijatszotta: {currentCard.GetLocalizedName(true)}"
                                        : $"{_playerNames[currentPlayer]} played: {currentCard.GetLocalizedName(false)}";
                                    _lastActionColor = currentCard.GetConsoleColor();
                                    turnCompleted = true;
                                }
                                else
                                {
                                    _soundManager.PlayInvalid();
                                    _lastActionMessage = _isHungarian
                                        ? "Ervenytelen lepes! A lap nem egyezik szinben vagy ertekben."
                                        : "Invalid move! Card must match color or value.";
                                    _lastActionColor = ConsoleColor.Red;
                                }
                            }
                        }
                    }
                }

                if (playerHands[currentPlayer].Count == 1)
                {
                    _soundManager.PlayUnoAlert();
                    ShowUnoBanner(_playerNames[currentPlayer]);
                }

                if (playerHands[currentPlayer].Count == 0)
                {
                    ShowVictoryScreen(_playerNames[currentPlayer]);
                    break;
                }

                if (playedCard != null)
                {
                    switch (playedCard.Value)
                    {
                        case "Skip":
                            _soundManager.PlaySpecialAction();
                            int skippedPlayer = GetNextPlayerIndex(currentPlayer, numberOfPlayers, clockwise);
                            _lastActionMessage = _isHungarian
                                ? $"[KIMARAD] {_playerNames[skippedPlayer]} kimarad a korbol!"
                                : $"[SKIPPED] {_playerNames[skippedPlayer]} was skipped!";
                            _lastActionColor = ConsoleColor.Yellow;
                            currentPlayer = skippedPlayer;
                            break;

                        case "Reverse":
                            _soundManager.PlaySpecialAction();
                            clockwise = !clockwise;
                            if (numberOfPlayers == 2)
                            {

                                _lastActionMessage = _isHungarian
                                    ? "[FORDITO] (2 jatekosnal kimaradaskent mukodik)"
                                    : "[REVERSE] (Acts as Skip in 2-player match)";
                                currentPlayer = GetNextPlayerIndex(currentPlayer, numberOfPlayers, clockwise);
                            }
                            else
                            {
                                _lastActionMessage = _isHungarian
                                    ? $"[FORDITO] Uj irany: {(clockwise ? "Oramutato szerint" : "Ellentetes")}"
                                    : $"[REVERSE] New direction: {(clockwise ? "Clockwise" : "Counter-Clockwise")}";
                            }
                            _lastActionColor = ConsoleColor.Cyan;
                            break;

                        case "+2":
                            _soundManager.PlaySpecialAction();
                            int penaltyPlayer = GetNextPlayerIndex(currentPlayer, numberOfPlayers, clockwise);
                            DrawCard(playerHands[penaltyPlayer], animate: false);
                            DrawCard(playerHands[penaltyPlayer], animate: false);
                            _lastActionMessage = _isHungarian
                                ? $"[+2] {_playerNames[penaltyPlayer]} huz 2 lapot es kimarad!"
                                : $"[+2] {_playerNames[penaltyPlayer]} draws 2 cards and skips turn!";
                            _lastActionColor = ConsoleColor.Red;
                            currentPlayer = penaltyPlayer;
                            break;

                        case "+4":
                            _soundManager.PlaySpecialAction();
                            int plus4Player = GetNextPlayerIndex(currentPlayer, numberOfPlayers, clockwise);
                            for (int k = 0; k < 4; k++)
                            {
                                DrawCard(playerHands[plus4Player], animate: false);
                            }
                            _lastActionMessage = _isHungarian
                                ? $"[+4] {_playerNames[plus4Player]} huz 4 lapot es kimarad!"
                                : $"[+4] {_playerNames[plus4Player]} draws 4 cards and skips turn!";
                            _lastActionColor = ConsoleColor.Red;
                            currentPlayer = plus4Player;
                            break;
                    }
                }

                currentPlayer = GetNextPlayerIndex(currentPlayer, numberOfPlayers, clockwise);
            }

            StartMainMenuMusic();
            _playerNames.Clear();
        }

        private Card? HandleAITurn(List<Card> aiHand, ref Card? currentCard)
        {
            int playIndex = -1;

            for (int i = 0; i < aiHand.Count; i++)
            {
                if (CanPlayCard(aiHand[i], currentCard) && aiHand[i].Value is "Skip" or "Reverse" or "+2" or "+4" or "Wild")
                {
                    playIndex = i;
                    break;
                }
            }

            if (playIndex == -1)
            {
                for (int i = 0; i < aiHand.Count; i++)
                {
                    if (CanPlayCard(aiHand[i], currentCard))
                    {
                        playIndex = i;
                        break;
                    }
                }
            }

            if (playIndex != -1)
            {
                Card played = aiHand[playIndex];
                aiHand.RemoveAt(playIndex);
                if (currentCard != null) _discardPile.Add(currentCard);
                currentCard = played;

                if (currentCard.Color == "Wild")
                {
                    currentCard = HandleWildCard(currentCard, isAI: true);
                }

                _soundManager.PlayCardPlay();
                _lastActionMessage = _isHungarian
                    ? $"AI kijatszotta: {currentCard.GetLocalizedName(true)}"
                    : $"AI played: {currentCard.GetLocalizedName(false)}";
                _lastActionColor = currentCard.GetConsoleColor();
                return played;
            }

            Card? drawn = DrawCard(aiHand, animate: false);
            _soundManager.PlayCardDraw();

            if (drawn != null && CanPlayCard(drawn, currentCard))
            {
                Thread.Sleep(600);
                aiHand.RemoveAt(aiHand.Count - 1);
                if (currentCard != null) _discardPile.Add(currentCard);
                currentCard = drawn;

                if (currentCard.Color == "Wild")
                {
                    currentCard = HandleWildCard(currentCard, isAI: true);
                }

                _soundManager.PlayCardPlay();
                _lastActionMessage = _isHungarian
                    ? $"AI huzott es azonnal lerakta: {currentCard.GetLocalizedName(true)}"
                    : $"AI drew and immediately played: {currentCard.GetLocalizedName(false)}";
                _lastActionColor = currentCard.GetConsoleColor();
                return drawn;
            }

            _lastActionMessage = _isHungarian ? "AI huzott egy lapot." : "AI drew a card.";
            _lastActionColor = ConsoleColor.Cyan;
            return null;
        }

        private void ShowPassScreen(string nextPlayerName)
        {
            ClearConsole();
            DrawHeader();
            Console.WriteLine("\n\n");
            CenterColoredText("===============================================", ConsoleColor.DarkGray);
            CenterColoredText(_isHungarian ? "KOVETKEZO JATEKOS FORDULOJA!" : "PASS TO NEXT PLAYER!", ConsoleColor.Yellow);
            Console.WriteLine();
            CenterColoredText($"[ {nextPlayerName} ]", ConsoleColor.Cyan);
            Console.WriteLine();
            CenterColoredText(_isHungarian
                ? "(Nyomj meg egy gombot, amikor keszen allsz a lapjaid megtekintesere!)"
                : "(Press any key when ready to view your cards!)", ConsoleColor.Gray);
            CenterColoredText("===============================================", ConsoleColor.DarkGray);
            SafeReadKey();
            _soundManager.PlayMenuBeep();
        }

        private void ShowUnoBanner(string playerName)
        {
            ClearConsole();
            DrawHeader();
            Console.WriteLine("\n\n");
            CenterColoredText("=================================================", ConsoleColor.Yellow);
            CenterColoredText("       █    █  █    █  ██████  █  ", ConsoleColor.Red);
            CenterColoredText("       █    █  ██   █  █    █  █  ", ConsoleColor.Red);
            CenterColoredText("       █    █  █ █  █  █    █  █  ", ConsoleColor.Yellow);
            CenterColoredText("       █    █  █  █ █  █    █  █  ", ConsoleColor.Yellow);
            CenterColoredText("       ██████  █   ██  ██████  █  ", ConsoleColor.Green);
            Console.WriteLine();
            CenterColoredText($"*** {playerName} {(_isHungarian ? "mar csak 1 lappal rendelkezik!" : "has only 1 card remaining!")} ***", ConsoleColor.Cyan);
            CenterColoredText("=================================================", ConsoleColor.Yellow);
            Thread.Sleep(1600);
        }

        private void ShowVictoryScreen(string winner)
        {
            ClearConsole();
            DrawHeader();
            _soundManager.PlayVictory();

            string[] trophy =
            {
                "             ___________             ",
                "            '._==_==_=_.'            ",
                "            .-\\:      /-.            ",
                "           | (|:.     |) |           ",
                "            '-|:.     |-'            ",
                "              \\::.    /              ",
                "               '::. .'               ",
                "                 ) (                 ",
                "               _.' '._               ",
                "              `\"\"\"\"\"\"\"`              "
            };

            Console.WriteLine();
            foreach (var line in trophy)
            {
                CenterColoredText(line, ConsoleColor.Yellow);
            }

            Console.WriteLine();
            CenterColoredText(_isHungarian ? "*** GYOZELEM! ***" : "*** VICTORY! ***", ConsoleColor.Green);
            Console.WriteLine();
            CenterColoredText($"{winner} {(_isHungarian ? "megnyerte a jatekot!" : "won the match!")}", ConsoleColor.Cyan);
            Console.WriteLine();
            CenterColoredText(_isHungarian ? "Gratulalunk a remek taktikanak!" : "Congratulations on a fantastic game!", ConsoleColor.White);
            Console.WriteLine();
            CenterColoredText(_isHungarian ? "Nyomj meg egy gombot a fomenube valo visszatereshez..." : "Press any key to return to Main Menu...", ConsoleColor.DarkGray);
            SafeReadKey();
        }
        #endregion

        #region Menus
        public void Start()
        {
            while (true)
            {
                DrawMainMenu();
                var key = SafeReadKey();
                _soundManager.PlayMenuBeep();

                switch (key.KeyChar)
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
                        StopMainMenuMusic();
                        return;
                }
            }
        }

        private void DrawMainMenu()
        {
            ClearConsole();
            DrawHeader();
            Console.WriteLine("\n");

            string[] menuItems = _isHungarian
                ? new[]
                {
                    "┌─────────────────────────────────────┐",
                    "│  1. Egyjatekos Mod (vs AI)          │",
                    "│  2. Helyi Tobbjatekos (2-4 jatekos) │",
                    "│  3. Beallitasok (Settings)          │",
                    "│  4. Jatekszabalyok & Utmutato       │",
                    "│  5. Kilepes                         │",
                    "└─────────────────────────────────────┘"
                }
                : new[]
                {
                    "┌─────────────────────────────────────┐",
                    "│  1. Singleplayer (vs AI)            │",
                    "│  2. Local Multiplayer (2-4 players) │",
                    "│  3. Settings                        │",
                    "│  4. How to Play & Tutorial          │",
                    "│  5. Exit                            │",
                    "└─────────────────────────────────────┘"
                };

            foreach (var line in menuItems)
            {
                CenterColoredText(line, ConsoleColor.Cyan);
            }

            Console.WriteLine();
            CenterColoredText(_isHungarian ? "Valassz a szamgombokkal (1-5):" : "Select an option using numbers (1-5):", ConsoleColor.DarkGray);
        }

        private void ShowSinglePlayerMenu()
        {
            while (true)
            {
                ClearConsole();
                DrawHeader();
                Console.WriteLine("\n");

                CenterColoredText(_isHungarian ? "=== EGYJATEKOS MOD ===" : "=== SINGLEPLAYER MODE ===", ConsoleColor.Yellow);
                Console.WriteLine();

                string[] options = _isHungarian
                    ? new[]
                    {
                        "┌───────────────────────────┐",
                        "│  1. 1v1 (Te vs 1 AI)      │",
                        "│  2. 1v1v1 (Te vs 2 AI)    │",
                        "│  3. 1v1v1v1 (Te vs 3 AI)  │",
                        "│  4. Vissza a Fomenube     │",
                        "└───────────────────────────┘"
                    }
                    : new[]
                    {
                        "┌───────────────────────────┐",
                        "│  1. 1v1 (You vs 1 AI)     │",
                        "│  2. 1v1v1 (You vs 2 AI)   │",
                        "│  3. 1v1v1v1 (You vs 3 AI) │",
                        "│  4. Back to Main Menu     │",
                        "└───────────────────────────┘"
                    };

                foreach (var line in options)
                {
                    CenterColoredText(line, ConsoleColor.Cyan);
                }

                var key = SafeReadKey();
                _soundManager.PlayMenuBeep();

                switch (key.KeyChar)
                {
                    case '1':
                        PlayGame(2);
                        return;
                    case '2':
                        PlayGame(3);
                        return;
                    case '3':
                        PlayGame(4);
                        return;
                    case '4':
                        return;
                }
            }
        }

        private void ShowMultiplayerMenu()
        {
            while (true)
            {
                ClearConsole();
                DrawHeader();
                Console.WriteLine("\n");

                CenterColoredText(_isHungarian ? "=== HELYI TOBBJATEKOS ===" : "=== LOCAL MULTIPLAYER ===", ConsoleColor.Yellow);
                Console.WriteLine();

                string[] options = _isHungarian
                    ? new[]
                    {
                        "┌─────────────────────────────────────┐",
                        "│  1. Helyi jatek inditasa (2-4 fo)   │",
                        "│  2. Vissza a Fomenube               │",
                        "└─────────────────────────────────────┘"
                    }
                    : new[]
                    {
                        "┌─────────────────────────────────────┐",
                        "│  1. Start Local Game (2-4 players)  │",
                        "│  2. Back to Main Menu               │",
                        "└─────────────────────────────────────┘"
                    };

                foreach (var line in options)
                {
                    CenterColoredText(line, ConsoleColor.Cyan);
                }

                var key = SafeReadKey();
                _soundManager.PlayMenuBeep();

                if (key.KeyChar == '2') return;

                if (key.KeyChar == '1')
                {
                    GetPlayerNamesForLocalMultiplayer();
                    return;
                }
            }
        }

        private void GetPlayerNamesForLocalMultiplayer()
        {
            ClearConsole();
            DrawHeader();
            Console.WriteLine("\n");

            CenterColoredText(_isHungarian ? "Hany jatekos jatszik? (2-4):" : "How many players? (2-4):", ConsoleColor.Yellow);

            int playerCount;
            while (!int.TryParse(SafeReadKey().KeyChar.ToString(), out playerCount) || playerCount < 2 || playerCount > 4)
            {
                _soundManager.PlayInvalid();
            }

            _soundManager.PlayMenuBeep();
            _playerNames.Clear();

            ClearConsole();
            DrawHeader();
            Console.WriteLine("\n");
            CenterColoredText(_isHungarian ? $"Add meg a(z) {playerCount} jatekos nevet:" : $"Enter names for {playerCount} players:", ConsoleColor.Yellow);
            Console.WriteLine();

            for (int i = 0; i < playerCount; i++)
            {
                try { Console.CursorVisible = true; } catch { }
                int left = Math.Max(2, (GetWindowWidth() - 30) / 2);
                SafeSetCursorPosition(left, Console.CursorTop);
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"{(_isHungarian ? "Jatekos" : "Player")} {i + 1}: ");
                Console.ForegroundColor = ConsoleColor.Cyan;
                string? name = Console.ReadLine()?.Trim();
                Console.ResetColor();

                if (string.IsNullOrWhiteSpace(name))
                    name = $"{(_isHungarian ? "Jatekos" : "Player")} {i + 1}";

                _playerNames.Add(name);
                try { Console.CursorVisible = false; } catch { }
            }

            PlayGame(playerCount);
        }

        private void ShowSettings()
        {
            while (true)
            {
                ClearConsole();
                DrawHeader();
                Console.WriteLine("\n");

                CenterColoredText(_isHungarian ? "=== BEALLITASOK ===" : "=== SETTINGS ===", ConsoleColor.Yellow);
                Console.WriteLine();

                string langName = _isHungarian ? "Magyar" : "English";
                string soundState = _soundEnabled ? "BE" : "KI";
                if (!_isHungarian) soundState = _soundEnabled ? "ON" : "OFF";
                string musicState = _musicPlaying ? "BE" : "KI";
                if (!_isHungarian) musicState = _musicPlaying ? "ON" : "OFF";

                string[] items = _isHungarian
                    ? new[]
                    {
                        "┌───────────────────────────────────────────────┐",
                        $"│  1. Nyelv (Language): {langName,-23} │",
                        $"│  2. Hanghatasok (Sound): {soundState,-20} │",
                        $"│  3. Fomenu Zene (Music): {musicState,-20} │",
                        $"│  4. Effektek Hangero: {_effectsVolume + "%",-23} │",
                        $"│  5. Zene Hangero: {_musicVolume + "%",-27} │",
                        "│  6. Vissza a Fomenube                         │",
                        "└───────────────────────────────────────────────┘"
                    }
                    : new[]
                    {
                        "┌───────────────────────────────────────────────┐",
                        $"│  1. Language (Nyelv): {langName,-23} │",
                        $"│  2. Sound Effects: {soundState,-26} │",
                        $"│  3. Main Menu Music: {musicState,-24} │",
                        $"│  4. Effects Volume: {_effectsVolume + "%",-25} │",
                        $"│  5. Music Volume: {_musicVolume + "%",-27} │",
                        "│  6. Back to Main Menu                         │",
                        "└───────────────────────────────────────────────┘"
                    };

                foreach (var line in items)
                {
                    CenterColoredText(line, ConsoleColor.Cyan);
                }

                var key = SafeReadKey();
                _soundManager.PlayMenuBeep();

                switch (key.KeyChar)
                {
                    case '1':
                        _isHungarian = !_isHungarian;
                        SaveSettingsCache();
                        break;
                    case '2':
                        _soundEnabled = !_soundEnabled;
                        _soundManager = new SoundManager(_soundEnabled, _effectsVolume, _musicVolume);
                        if (!_soundEnabled) StopMainMenuMusic();
                        else if (_musicPlaying) StartMainMenuMusic();
                        SaveSettingsCache();
                        break;
                    case '3':
                        if (_musicPlaying) StopMainMenuMusic();
                        else StartMainMenuMusic();
                        SaveSettingsCache();
                        break;
                    case '4':
                        ChangeEffectsVolume();
                        SaveSettingsCache();
                        break;
                    case '5':
                        ChangeMusicVolume();
                        SaveSettingsCache();
                        break;
                    case '6':
                        SaveSettingsCache();
                        return;
                }
            }
        }

        private void ChangeEffectsVolume()
        {
            _effectsVolume = (_effectsVolume + 20) % 120;
            if (_effectsVolume == 0 && _soundEnabled) _effectsVolume = 20;
            _soundManager.UpdateEffectsVolume(_effectsVolume);
        }

        private void ChangeMusicVolume()
        {
            _musicVolume = (_musicVolume + 20) % 120;
            if (_musicVolume == 0 && _soundEnabled) _musicVolume = 20;
            _soundManager.UpdateMusicVolume(_musicVolume);
            if (_musicPlaying)
            {
                StopMainMenuMusic();
                StartMainMenuMusic();
            }
        }

        private void ShowTutorial()
        {
            ClearConsole();
            DrawHeader();
            Console.WriteLine("\n");

            CenterColoredText(_isHungarian ? "=== JATEKSZABALYOK ES IRANYITAS ===" : "=== HOW TO PLAY & CONTROLS ===", ConsoleColor.Yellow);
            Console.WriteLine();

            if (_isHungarian)
            {
                CenterColoredText("CEL: Szabadulj meg az osszes lapodtol elsokent!", ConsoleColor.Green);
                Console.WriteLine();

                CenterColoredText("Iranyitas a jatekban:", ConsoleColor.White);
                CenterColoredText("- [<-] es [->] Nyilak: Kartyak lapozasa es kivalasztasa", ConsoleColor.DarkCyan);
                CenterColoredText("- [ENTER] vagy [SZOKOZ]: Kivalasztott lap lerakasa", ConsoleColor.DarkCyan);
                CenterColoredText("- [1] - [9] Szamgombok: Lap azonnali kijatszasa", ConsoleColor.DarkCyan);
                CenterColoredText("- [D] Gomb: Lap huzasa a paklibol", ConsoleColor.DarkCyan);
                CenterColoredText("- [Q] Gomb: Kilepes a meccsbol", ConsoleColor.DarkCyan);
                Console.WriteLine();

                CenterColoredText("Specialis Kartyak:", ConsoleColor.White);
                CenterColoredText("- [KIMARAD] (Skip): A kovetkezo jatekos kimarad a korbol", ConsoleColor.Yellow);
                CenterColoredText("- [FORDITO] (Reverse): Megforditja a jatek iranyat", ConsoleColor.Yellow);
                CenterColoredText("- [+2]: A kovetkezo jatekos huz 2 lapot es kimarad", ConsoleColor.Yellow);
                CenterColoredText("- [VAD] (Wild): Barmire lerakhato, uj szint valaszthatsz", ConsoleColor.Magenta);
                CenterColoredText("- [+4 VAD]: Uj szin valasztasa + a kovetkezo jatekos huz 4 lapot", ConsoleColor.Magenta);
            }
            else
            {
                CenterColoredText("GOAL: Be the first player to discard all of your cards!", ConsoleColor.Green);
                Console.WriteLine();

                CenterColoredText("In-Game Controls:", ConsoleColor.White);
                CenterColoredText("- [<-] and [->] Arrow Keys: Browse and select cards in your hand", ConsoleColor.DarkCyan);
                CenterColoredText("- [ENTER] or [SPACE]: Play the highlighted card", ConsoleColor.DarkCyan);
                CenterColoredText("- [1] - [9] Numbers: Instantly play corresponding card", ConsoleColor.DarkCyan);
                CenterColoredText("- [D] Key: Draw a card from the deck", ConsoleColor.DarkCyan);
                CenterColoredText("- [Q] Key: Quit current game", ConsoleColor.DarkCyan);
                Console.WriteLine();

                CenterColoredText("Special Action Cards:", ConsoleColor.White);
                CenterColoredText("- [SKIP]: The next player loses their turn", ConsoleColor.Yellow);
                CenterColoredText("- [REVERSE]: Reverses the direction of play", ConsoleColor.Yellow);
                CenterColoredText("- [+2]: Next player draws 2 cards and misses their turn", ConsoleColor.Yellow);
                CenterColoredText("- [WILD]: Playable anytime, allows choosing a new active color", ConsoleColor.Magenta);
                CenterColoredText("- [+4 WILD]: Choose a new color + next player draws 4 cards", ConsoleColor.Magenta);
            }

            Console.WriteLine("\n");
            CenterColoredText(_isHungarian ? "Nyomj meg egy gombot a visszatereshez..." : "Press any key to return...", ConsoleColor.DarkGray);
            SafeReadKey();
            _soundManager.PlayMenuBeep();
        }
        #endregion

        #region Music Control
        private void StartMainMenuMusic()
        {
            if (_musicPlaying || !_soundEnabled || _musicVolume == 0) return;
            _musicPlaying = true;
            _soundManager.PlayMainMenuMusic(loop: true);
        }

        private void StopMainMenuMusic()
        {
            _musicPlaying = false;
            _soundManager.StopMusic();
        }
        #endregion

        #region Settings Cache
        private static string GetSettingsFilePath()
        {
            try
            {
                string localPath = Path.Combine(Directory.GetCurrentDirectory(), "settings.json");
                if (File.Exists(localPath)) return localPath;
            }
            catch { }

            try
            {
                string basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");
                if (File.Exists(basePath)) return basePath;
            }
            catch { }

            try
            {
                return Path.Combine(Directory.GetCurrentDirectory(), "settings.json");
            }
            catch
            {
                return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");
            }
        }

        private void LoadSettingsCache()
        {
            try
            {
                string path = GetSettingsFilePath();
                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    var settings = JsonSerializer.Deserialize<GameSettings>(json);
                    if (settings != null)
                    {
                        _isHungarian = settings.IsHungarian;
                        _soundEnabled = settings.SoundEnabled;
                        _effectsVolume = Math.Clamp(settings.EffectsVolume, 0, 100);
                        _musicVolume = Math.Clamp(settings.MusicVolume, 0, 100);
                        _musicPlaying = settings.MusicEnabled && _soundEnabled && _musicVolume > 0;
                    }
                }
            }
            catch
            {

            }
        }

        private void SaveSettingsCache()
        {
            try
            {
                var settings = new GameSettings
                {
                    IsHungarian = _isHungarian,
                    SoundEnabled = _soundEnabled,
                    MusicEnabled = _musicPlaying,
                    EffectsVolume = _effectsVolume,
                    MusicVolume = _musicVolume
                };

                string path = GetSettingsFilePath();
                string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(path, json);
            }
            catch
            {

            }
        }

        private static ConsoleKeyInfo SafeReadKey()
        {
            if (Console.IsInputRedirected)
            {
                try
                {
                    int ch = Console.Read();
                    if (ch == -1) return new ConsoleKeyInfo('\0', ConsoleKey.NoName, false, false, false);
                    char c = (char)ch;
                    ConsoleKey key = c switch
                    {
                        '1' => ConsoleKey.D1,
                        '2' => ConsoleKey.D2,
                        '3' => ConsoleKey.D3,
                        '4' => ConsoleKey.D4,
                        '5' => ConsoleKey.D5,
                        '6' => ConsoleKey.D6,
                        '7' => ConsoleKey.D7,
                        '8' => ConsoleKey.D8,
                        '9' => ConsoleKey.D9,
                        'd' or 'D' => ConsoleKey.D,
                        'q' or 'Q' => ConsoleKey.Q,
                        '\r' or '\n' => ConsoleKey.Enter,
                        ' ' => ConsoleKey.Spacebar,
                        _ => ConsoleKey.NoName
                    };
                    return new ConsoleKeyInfo(c, key, false, false, false);
                }
                catch
                {
                    return new ConsoleKeyInfo('\0', ConsoleKey.NoName, false, false, false);
                }
            }

            try
            {
                return Console.ReadKey(true);
            }
            catch (InvalidOperationException)
            {
                try
                {
                    int ch = Console.Read();
                    if (ch == -1) return new ConsoleKeyInfo('\0', ConsoleKey.NoName, false, false, false);
                    char c = (char)ch;
                    return new ConsoleKeyInfo(c, ConsoleKey.NoName, false, false, false);
                }
                catch
                {
                    return new ConsoleKeyInfo('\0', ConsoleKey.NoName, false, false, false);
                }
            }
        }
        #endregion
    }
}
