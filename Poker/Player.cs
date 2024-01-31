namespace Poker;

public class Player : Participant
{
    //public string PlayerName { get; }

    public override event Action<int>? OnCall;
    public override event Action<int>? OnBet;
    public override event Action<int>? OnRaise;
    public override event Action? OnCheck;
    public override event Action? OnFold;
    public override event Action<int>? OnAllIn;
    public override void RemoveAllListeners()
    {
        OnBet = null;
        OnRaise = null;
        OnCheck = null;
        OnFold = null;
        OnAllIn = null;
        OnCall  = null;
    }



    public Player(string playerName, int initialMoney)
    {
        ParticipantName = playerName;
        Money = initialMoney;
    }


    protected override void Call(int currentBet)
    {
        int amountToAdd = currentBet - MoneySpentDuringRound;
        if (Money > amountToAdd)
        {
            MoneySpentDuringRound += amountToAdd;
            Money -= amountToAdd;
            OnCall?.Invoke(amountToAdd);
            return;
        }
        
        AllIn();
    }
    
    protected override bool Bet(int bigBlindAmount)
    {
        Console.Write($"Player {ParticipantName}, input the amount of your bet: ");

        if (!int.TryParse(Console.ReadLine(), out int betAmount)
            || betAmount > Money
            || betAmount < bigBlindAmount)
        {
            Console.Write("Incorrect input.");
            return false;
        }

        if (betAmount == Money)
        {
            AllIn();
            return true;
        }
        MoneySpentDuringRound += betAmount;
        Money -= betAmount;
        OnBet?.Invoke(betAmount);
        return true;
    }

    protected override bool Raise(int bigBlindAmount, int currentBet)
    {
        Console.Write($"Player {ParticipantName}, input the amount of your raise: ");

        if (!int.TryParse(Console.ReadLine(), out int raiseAmount)
            || raiseAmount > Money
            || raiseAmount < bigBlindAmount
            || MoneySpentDuringRound + raiseAmount <= currentBet)
        {
            Console.Write("Incorrect input.");
            return false;
        }

        if (raiseAmount == Money)
        {
            AllIn();
            return true;
        }
        MoneySpentDuringRound += raiseAmount;
        Money -= raiseAmount;
        OnRaise?.Invoke(raiseAmount);
        return true;
    }
    
    

    protected override void Check()
    {
        OnCheck?.Invoke();
    }

    protected override void Fold()
    {
        OnFold?.Invoke();
    }

    protected override void AllIn()
    {
        int allInAmount = Money;
        MoneySpentDuringRound += Money;
        Money = 0;
        OnAllIn?.Invoke(allInAmount);
    }


    private Move GetMove(int currentBet)
    {
        while (true)
        {
            Console.WriteLine("Choose what move you want to make:");
            Console.WriteLine("Moves:\n" +
                              "  1. Check\n" +
                              "  2. Call\n" +
                              "  3. Bet\n" +
                              "  4. Raise\n" +
                              "  5. Fold\n");

            if (int.TryParse(Console.ReadLine(), out int chosenNumber)
                && Enum.IsDefined(typeof(Move), chosenNumber)
                && IsValidMove((Move)chosenNumber, currentBet))
                return (Move)chosenNumber;
            Console.WriteLine("Not a valid move.");
        }
    }
    
    public override void MakeMove(int currentBet, int bigBlind)
    {
        Console.WriteLine($"Player {ParticipantName}, it's your turn to make a move.");

        while (true)
        {
            Move move = GetMove(currentBet);
            switch (move)
            {
                case Move.Check:
                    Check();
                    return;
                case Move.Call:
                    Call(currentBet);
                    return;
                case Move.Bet:
                    bool betSuccessful = Bet(bigBlind);
                    if(!betSuccessful)
                        continue;
                    //potSize += betAmount;
                    return;
                case Move.Raise:
                    bool raiseSuccessful = Raise(bigBlind, currentBet);
                    if(!raiseSuccessful)
                        continue;
                    //potSize += raiseAmount;
                    return;
                case Move.Fold:
                    Fold();
                    return;
                case Move.AllIn:
                    AllIn();
                    return;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}