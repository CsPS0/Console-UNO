using System;

namespace ConsoleUno
{
    public class Card
    {
        public string Color { get; set; }
        public string Value { get; }

        public Card(string color, string value)
        {
            Color = color;
            Value = value;
        }

        public ConsoleColor GetConsoleColor() => Color switch
        {
            "Red" => ConsoleColor.Red,
            "Blue" => ConsoleColor.Cyan,
            "Green" => ConsoleColor.Green,
            "Yellow" => ConsoleColor.Yellow,
            "Wild" => ConsoleColor.Magenta,
            _ => ConsoleColor.White
        };

        public string GetSymbol() => Value switch
        {
            "Skip" => "X",
            "Reverse" => "<>",
            "+2" => "+2",
            "+4" => "+4",
            "Wild" => "W",
            _ => Value
        };

        public string GetColorAbbr() => Color switch
        {
            "Red" => "RED",
            "Blue" => "BLU",
            "Green" => "GRN",
            "Yellow" => "YEL",
            "Wild" => "WLD",
            _ => "---"
        };

        public string GetLocalizedName(bool hungarian)
        {
            string colorName = Color switch
            {
                "Red" => hungarian ? "Piros" : "Red",
                "Blue" => hungarian ? "Kek" : "Blue",
                "Green" => hungarian ? "Zold" : "Green",
                "Yellow" => hungarian ? "Sarga" : "Yellow",
                "Wild" => hungarian ? "Szinvalaszto" : "Wild",
                _ => Color
            };

            string valName = Value switch
            {
                "Skip" => hungarian ? "Kimaradsz" : "Skip",
                "Reverse" => hungarian ? "Fordito" : "Reverse",
                "+2" => "+2",
                "+4" => hungarian ? "+4 Vad" : "+4 Wild",
                "Wild" => hungarian ? "Vad" : "Wild",
                _ => Value
            };

            return Color == "Wild" && Value != "+4"
                ? (hungarian ? "Vad (Szinvalaszto)" : "Wild Card")
                : $"{colorName} {valName}";
        }

        public string[] GetAsciiLines(bool selected = false)
        {
            string sym = GetSymbol();
            string topSym = sym.PadRight(3);
            if (topSym.Length > 3) topSym = topSym.Substring(0, 3);

            string botSym = sym.PadLeft(3);
            if (botSym.Length > 3) botSym = botSym.Substring(botSym.Length - 3, 3);

            string mid = GetColorAbbr().PadRight(3);
            if (mid.Length > 3) mid = mid.Substring(0, 3);

            if (selected)
            {
                return new[]
                {
                    "╔═════╗",
                    $"║{topSym}  ║",
                    $"║ {mid} ║",
                    $"║  {botSym}║",
                    "╚═════╝"
                };
            }

            return new[]
            {
                "┌─────┐",
                $"│{topSym}  │",
                $"│ {mid} │",
                $"│  {botSym}│",
                "└─────┘"
            };
        }

        public override string ToString()
        {
            return $"[{Color} {Value}]";
        }
    }
}
