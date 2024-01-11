namespace Poker;

public class Card
{
    public Suit Suit { get; init; }
    public int CardValue { get; init; }


    private static readonly Dictionary<char, int> CardValues = new()
    {
        { '2', 2 },
        { '3', 3 },
        { '4', 4 },
        { '5', 5 },
        { '6', 6 },
        { '7', 7 },
        { '8', 8 },
        { '9', 9 },
        { 'T', 10 },
        { 'J', 11 },
        { 'Q', 12 },
        { 'K', 13 },
        { 'A', 14 },
    };

    private static readonly Dictionary<Suit, char> SuitToString = new()
    {
        { Suit.Hearts, '\u2665' },
        { Suit.Spades, '\u2660' },
        { Suit.Diamonds, '\u2666' },
        { Suit.Clubs, '\u2663' },
    };

    public Card(Suit suit, int cardValue)
    {
        Suit = suit;
        CardValue = cardValue;
    }

    public static Card ParseCard(string s)
    {
        s = s.ToUpper();
        if (s.Length != 2)
            throw new ArgumentException($"String representation of a card should have a length of 2. Found length = {s.Length}");

        var cardValue = CardValues[s[0]];

        var cardSuit = s[1] switch
        {
            'S' or '\u2660' => Suit.Spades,
            'C' or '\u2663' => Suit.Clubs,
            'D' or '\u2666' => Suit.Diamonds,
            'H' or '\u2665' => Suit.Hearts,
            _ => throw new ArgumentException($"Available suits: S C D H, Found: {s[1]}")
        };

        return new Card(cardSuit, cardValue);
    }

    public override string ToString() => $"{CardValues.First(x => x.Value == CardValue).Key}{SuitToString[Suit]}";
}
