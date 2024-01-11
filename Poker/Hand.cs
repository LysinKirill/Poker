namespace Poker;

public class Hand
{
    public Card FirstCard { get; }
    public Card SecondCard { get; }

    public Hand(Card firstCard, Card secondCard)
    {
        FirstCard = firstCard;
        SecondCard = secondCard;
    }
}
