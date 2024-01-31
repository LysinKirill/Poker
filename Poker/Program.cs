using System.Globalization;
using System.Text;

namespace Poker;


class Program
{
    public static void Main(string[] args)
    {
        Console.InputEncoding = Encoding.Unicode;
        Console.OutputEncoding = Encoding.Unicode;

        List<Participant> players = new List<Participant>()
        {
            new Player("Amogus", 1000),
            new Player("Sus", 1000),
        };
        
        Session session = new Session(players, 10);
        session.StartSession();
        
        players.ForEach(Console.WriteLine);
    }
}

