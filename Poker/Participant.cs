namespace Poker;

public abstract class Participant
{
    public string ParticipantName { get; init; } = "Empty name";
    public int Money { get; protected set; }

    public int MoneySpentDuringRound { get; set; } = 0;
    // public int TotalExpenses {get; protected set}
    public Hand? Hand { get; set; }

    public abstract event Action<int>? OnCall;
    public abstract event Action<int>? OnBet;
    public abstract event Action<int>? OnRaise;
    public abstract event Action? OnCheck;
    public abstract event Action? OnFold;

    public abstract event Action<int>? OnAllIn;



    public abstract void RemoveAllListeners();
    
    public void ShowCards()
    {
        Console.WriteLine($"Participant {ParticipantName}'s cards:");
        Console.WriteLine(Hand!.FirstCard);
        Console.WriteLine(Hand!.SecondCard);
    }
    
    public void CollectWinnings(int winAmount)
    {
        if (winAmount <= 0)
            throw new ArgumentException("Who are you trying to scam, boy!");
        Money += winAmount;
    }
    

    public int ForcedBet(int forcedBetAmount)     // FIX THIS!
    {
        if (Money <= forcedBetAmount)
        {
            AllIn();
            return 0;
        }

        MoneySpentDuringRound += forcedBetAmount;
        Money -= forcedBetAmount;
        return forcedBetAmount;
        //OnBet.Invoke();
    }
    
    protected abstract void Call(int currentBet);
    protected abstract bool Bet(int bigBlindAmount);
    protected abstract bool Raise(int bigBlindAmount, int currentBet);
    protected abstract void Check(); // ???
    protected abstract void Fold(); // ???
    protected abstract void AllIn(); // ???
    public abstract void MakeMove(int currentBet, int bigBlind);
    
    protected bool IsValidMove(Move move, int currentBet)
    {
        return move switch
        {
            Move.Check =>  MoneySpentDuringRound == currentBet,
            Move.Call => MoneySpentDuringRound != currentBet && Money != 0,
            Move.Bet => Money != 0 && currentBet == MoneySpentDuringRound,
            Move.Raise => Money != 0,
            Move.Fold => true,
            Move.AllIn => true,
            _ => throw new ArgumentOutOfRangeException(nameof(move), move, null)
        };
    }


    public override string ToString()
    {
        return $"Participant {ParticipantName}. Money: ${Money}";
    }
}