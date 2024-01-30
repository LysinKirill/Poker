namespace Poker;

public abstract class Participant
{
    public int Money { get; protected set; }
    public Hand? Hand { get; protected set; }

    public event Action<int>? OnCall;
    public abstract event Action<int>? OnBet;
    public abstract event Action<int>? OnRaise;
    public abstract event Action? OnCheck;
    public abstract event Action? OnFold;

    private int Call(int currentBet)
    {
        if (Money >= currentBet)
        {
            Money -= currentBet;
            OnCall?.Invoke(currentBet);
            return currentBet;
        }

        var bet = Money;
        Money = 0;
        OnCall?.Invoke(bet);
        return bet;
    }


    protected abstract int Bet(int bigBlindAmount);
    protected abstract int Raise(int bigBlindAmount);
    protected abstract void Check(); // ???
    protected abstract void Fold(); // ???
}