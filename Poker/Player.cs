namespace Poker;

public class Player : Participant
{
    public string PlayerName { get; }
    
    
    public override event Action<int>? OnBet;
    public override event Action<int>? OnRaise;
    public override event Action? OnCheck;
    public override event Action? OnFold;

    public Player(string playerName, int initialMoney)
    {
        PlayerName = playerName;
        Money = initialMoney;
    }


    protected override int Bet(int bigBlindAmount)
    {
        Console.Write($"Player {PlayerName}, input the amount of your bet:");
        int betAmount;
        while(!int.TryParse(Console.ReadLine(), out betAmount)
              || betAmount > Money
              || betAmount < bigBlindAmount)
            Console.Write("Incorrect input. Input the amount of your bet: ");

        Money -= betAmount;
        OnBet?.Invoke(betAmount);
        return betAmount;
    }

    protected override int Raise(int bigBlindAmount)
    {
        Console.Write($"Player {PlayerName}, input the amount of your raise:");
        int raiseAmount;
        while(!int.TryParse(Console.ReadLine(), out raiseAmount)
              || raiseAmount > Money
              || raiseAmount < bigBlindAmount)
            Console.Write("Incorrect input. Input the amount of your raise: ");

        Money -= raiseAmount;
        OnRaise?.Invoke(raiseAmount);
        return raiseAmount;
    }

    protected override void Check()
    {
        OnCheck?.Invoke();
    }

    protected override void Fold()
    {
        OnFold?.Invoke();
    }
}