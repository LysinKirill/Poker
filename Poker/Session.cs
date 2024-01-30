namespace Poker;

public class Session
{
    
    public List<Card> Deck { get; init; }
    public GameStage GameStage { get; private set; } = GameStage.Preflop;
    public Table Table { get; }

    private Random _random = new Random();
    
    public Session()
    {
        Deck = (from value in Enumerable.Range(2, 13)
            from suit in Enumerable.Range(1, 4)
            select new Card((Suit)suit, value)).ToList();
        Table = new Table();
    }

    public void DealRandomCard()
    {
        if (GameStage == GameStage.River)
            throw new Exception("Cannot deal another card after river.");
        
        var cardToDeal = Deck[_random.Next(Deck.Count)];
        Deck.Remove(cardToDeal);
        Table.DealtCards.Add(cardToDeal);

        GameStage++;
    }

    public void MoveToNextStage()
    {
        if (GameStage == GameStage.River)
            throw new Exception("");

    }
}