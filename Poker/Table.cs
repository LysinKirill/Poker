namespace Poker;

public class Table
{
    public List<Card> DealtCards { get; } = new List<Card>(5);
    public int NumberOfCards { get; private set; } = 0;

    public int CompareHands(Hand firstHand, Hand secondHand)
    {
        return 0;
    }

    public int CheckStraightFlush(List<Card> cards)
    {
        if (cards.Count < 5)
            return 0;
        cards = cards.OrderByDescending(card => card.CardValue).ToList();

        int currConsecutive = 0;
        //for(int i = 0; i < cards.)
        return 1;
    }
}
