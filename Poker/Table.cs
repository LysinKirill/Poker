namespace Poker;

public class Table
{
    public List<Card> DealtCards { get; } = new List<Card>(5);
    public int NumberOfCards { get; private set; } = 0;

    public int CompareHands(Hand firstHand, Hand secondHand)
    {
        var firstCombination = GetCombination(firstHand);
        var secondCombination = GetCombination(secondHand);

        if (firstCombination != secondCombination)
            return firstCombination > secondCombination ? 1 : -1;


        
    }


    public Combination GetCombination(Hand hand)
    {
        var cards = DealtCards.Select(card => card).Union(new []{hand.FirstCard, hand.SecondCard}).ToList();

        if (CheckStraightFlush(cards) != 0)
            return Combination.StraightFlush;
        if (CheckQuads(cards) != 0)
            return Combination.Quads;
        if (CheckFullHouse(cards).Item1 != 0)
            return Combination.FullHouse;
        if (CheckFlush(cards).Count != 0)
            return Combination.Flush;
        if (CheckStraight(cards) != 0)
            return Combination.Straight;
        if (CheckSet(cards) != 0)
            return Combination.Set;
        if (CheckTwoPair(cards).Item1 != 0)
            return Combination.TwoPair;
        if (CheckPair(cards) != 0)
            return Combination.Pair;
        return Combination.HighCard;
    }
    public static int CheckStraightFlush(List<Card> cards)
    {
        var flush = CheckFlush(cards);
        var straight = CheckStraight(cards);

        if (flush.Count == 0 || straight == 0)
            return 0;
        
        return straight;
    }

    public static int CheckQuads(List<Card> cards)
    {
        if (cards.Count < 4)
            return 0;

        cards = cards.OrderByDescending(card => card.CardValue).ToList();
        int numberOfCardsToQuads = 4;
        int previousCardValue = 0;

        foreach (var card in cards)
        {
            if (card.CardValue != previousCardValue)
            {
                numberOfCardsToQuads = 4;
                previousCardValue = card.CardValue;
            }

            --numberOfCardsToQuads;
            if (numberOfCardsToQuads == 0)
                return previousCardValue;
        }

        return 0;
    }

    public static (int, int) CheckFullHouse(List<Card> cards)
    {
        if (cards.Count < 5)
            return (0, 0);

        cards = cards.OrderByDescending(card => card.CardValue).ToList();

        int currConsecutive = 0;
        int prevCardValue = 0;

        int setValue = 0;
        int pairValue = 0;

        foreach (var card in cards)
        {
            if (card.CardValue != prevCardValue)
            {
                if (currConsecutive == 3 && setValue == 0)
                    setValue = prevCardValue;
                if (currConsecutive == 2 && pairValue == 0)
                    pairValue = prevCardValue;

                currConsecutive = 0;
                prevCardValue = card.CardValue;
            }

            ++currConsecutive;
        }

        if (currConsecutive == 3 && setValue == 0)
            setValue = prevCardValue;
        if (currConsecutive == 2 && pairValue == 0)
            pairValue = prevCardValue;
        return setValue != 0 && pairValue != 0 ? (setValue, pairValue) : (0, 0);
    }

    public static List<int> CheckFlush(List<Card> cards)
    {
        if (cards.Count < 5)
            return new();

        foreach (var suitGroup in cards.GroupBy(card => card.Suit))
            if (suitGroup.Count() >= 5)
                return suitGroup.OrderByDescending(card => card.CardValue).Select(card => card.CardValue).Take(5).ToList();

        return new();
    }

    public static int CheckStraight(List<Card> cards)
    {
        if (cards.Count < 5)
            return 0;
        cards = cards.OrderByDescending(card => card.CardValue).ToList();

        int currConsecutive = 0;
        int prevCardValue = 0;

        foreach (var card in cards)
        {
            if (card.CardValue != prevCardValue - 1)
            {
                currConsecutive = 0;
                prevCardValue = card.CardValue;
            }

            ++currConsecutive;
            if (currConsecutive == 5)
                // Not sure, maybe need to add only 4
                return prevCardValue + 5;
            prevCardValue = card.CardValue;
        }

        return 0;
    }

    public static int CheckSet(List<Card> cards)
    {
        if (cards.Count < 3)
            return 0;
        var t = cards.GroupBy(card => card.CardValue).OrderByDescending(x => x.Key).FirstOrDefault(x => x.Count() == 3);
        return t?.Key ?? 0;
    }

    public static (int, int) CheckTwoPair(List<Card> cards)
    {
        if (cards.Count < 4)
            return (0, 0);

        var twoGroups = cards.GroupBy(card => card.CardValue).Where(group => group.Count() == 2).OrderByDescending(group => group.Key).ToList();
        if (twoGroups is null && twoGroups.Count < 2)
            return (0, 0);
        var topPairs = twoGroups.Take(2).Select(group => group.Key).ToList();
        return (topPairs[0], topPairs[1]);
    }

    public static int CheckPair(List<Card> cards)
    {
        if (cards.Count < 2)
            return 0;

        var x = cards.GroupBy(card => card.CardValue).Where(group => group.Count() == 2).MaxBy(group => group.Key);
        return x?.Key ?? 0;
    }

    public static int CheckHighCard(List<Card> cards) => cards.MaxBy(x => x.CardValue)?.CardValue ?? 0;


    public int CompareHighCards(Hand firstHand, Hand secondHand)
    {
        var maxDealtCard = DealtCards.MaxBy(card => card.CardValue)?.CardValue ?? 0;
        var maxFirst = Math.Max(firstHand.FirstCard.CardValue, firstHand.SecondCard.CardValue);
        var maxSecond = Math.Max(secondHand.FirstCard.CardValue, secondHand.SecondCard.CardValue);

        if (maxDealtCard > maxFirst && maxDealtCard > maxSecond || maxFirst == maxSecond)
            return 0;

        if (maxFirst > maxSecond)
            return 1;
        return -1;
    }

    public int ComparePairs(Hand firstHand, Hand secondHand)
    {
        int firstPair = CheckPair(firstHand)
    }
    
}
