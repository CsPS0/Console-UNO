using System;
using System.Text;

namespace ConsoleUno
{
    internal class Program
    {
        private static void Main()
        {
            try
            {
                Console.OutputEncoding = Encoding.UTF8;
                Console.Title = "Console UNO";
            }
            catch { }

            var game = new Game();
            game.Start();
        }
    }
}
