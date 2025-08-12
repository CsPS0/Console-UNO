using System;

namespace ConsoleUno
{
    internal class Program
    {
        private static void Main()
        {
            Console.Title = "UNO!";
            var game = new Game();
            game.Start();
        }
    }
}