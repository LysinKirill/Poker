using System.Globalization;
using System.Text;

namespace Poker;


class Program
{
    public static void Main(string[] args)
    {
        Console.InputEncoding = Encoding.Unicode;
        Console.OutputEncoding = Encoding.Unicode;
        Card card = Card.ParseCard(Console.ReadLine());
        
        Console.WriteLine(card);
    }
}

