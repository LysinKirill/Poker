namespace Poker;

public class Table
{
    public List<Card> DealtCards { get; } = new(5);
    public int NumberOfCards { get; private set; } = 0;
    public List<Participant> Players { get; init; } = new();


    public int CompareHands(Hand firstHand, Hand secondHand)
    {
        var firstCombination = GetCombination(firstHand);
        var secondCombination = GetCombination(secondHand);

        if (firstCombination != secondCombination)
            return firstCombination > secondCombination ? 1 : -1;

        var firstList = DealtCards
            .Union(new[] { firstHand.FirstCard, firstHand.SecondCard })
            .ToList();
        var secondList = DealtCards
            .Union(new[] { secondHand.FirstCard, secondHand.SecondCard })
            .ToList();

        return firstCombination switch
        {
            Combination.HighCard => CheckHighCard(firstList).CompareTo(CheckHighCard(secondList)),
            Combination.Pair => CheckPair(firstList).CompareTo(CheckPair(secondList)),
            Combination.TwoPair => CheckTwoPair(firstList).CompareTo(CheckTwoPair(secondList)),
            Combination.Set => CheckSet(firstList).CompareTo(CheckSet(secondList)),
            Combination.Straight => CheckStraight(firstList).CompareTo(CheckStraight(secondList)),
            Combination.Flush => CheckFlush(firstList).CompareTo(CheckFlush(secondList)),
            Combination.FullHouse => CheckFullHouse(firstList).CompareTo(CheckFullHouse(secondList)),
            Combination.Quads => CheckQuads(firstList).CompareTo(CheckQuads(secondList)),
            Combination.StraightFlush => CheckStraightFlush(firstList).CompareTo(CheckStraightFlush(secondList)),
            _ => throw new ArgumentOutOfRangeException()
        };
    }


    public Combination GetCombination(Hand hand)
    {
        var cards = DealtCards.Select(card => card).Union(new[] { hand.FirstCard, hand.SecondCard }).ToList();

        if (CheckStraightFlush(cards) != 0)
            return Combination.StraightFlush;
        if (CheckQuads(cards) != 0)
            return Combination.Quads;
        if (CheckFullHouse(cards).Item1 != 0)
            return Combination.FullHouse;
        if (CheckFlush(cards) != 0)
            return Combination.Flush;
        if (CheckStraight(cards) != 0)
            return Combination.Straight;
        if (CheckSet(cards).Item1 != 0)
            return Combination.Set;
        if (CheckTwoPair(cards).Item1 != 0)
            return Combination.TwoPair;
        if (CheckPair(cards).Item1 != 0)
            return Combination.Pair;
        return Combination.HighCard;
    }

    private static int CheckStraightFlush(List<Card> cards)
    {
        return cards.
            GroupBy(card => card.Suit)
            .Select(suitedGroup => CheckStraight(suitedGroup.ToList()))
            .FirstOrDefault(straight => straight != 0);
    }

    private static int CheckQuads(List<Card> cards)
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

    private static (int, int) CheckFullHouse(List<Card> cards)
    {
        if (cards.Count < 5)
            return (0, 0);

        cards = cards.OrderByDescending(card => card.CardValue).ToList();

        var currConsecutive = 0;
        var prevCardValue = 0;

        var setValue = 0;
        var pairValue = 0;

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

    private static int CheckFlush(List<Card> cards)
    {
        if (cards.Count < 5)
            return 0;

        foreach (var suitGroup in cards.GroupBy(card => card.Suit))
            if (suitGroup.Count() >= 5)
                //return suitGroup.OrderByDescending(card => card.CardValue).Select(card => card.CardValue).Take(5).ToList();
                return suitGroup.MaxBy(card => card.CardValue)!.CardValue;

        return 0;
    }

    private static int CheckStraight(List<Card> cards)
    {
        if (cards.Count < 5)
            return 0;
        cards = cards.OrderByDescending(card => card.CardValue).ToList();

        int currConsecutive = 0;
        int prevCardValue = 0;

        foreach (var card in cards)
        {
            if (card.CardValue != prevCardValue - 1 && !(card.CardValue == 14 && prevCardValue == 2))
            {
                currConsecutive = 0;
                prevCardValue = card.CardValue;
            }

            ++currConsecutive;
            if (currConsecutive == 5)
                // Not sure, maybe need to add only 4
                return prevCardValue + 4;
            //prevCardValue = card.CardValue;
            --prevCardValue;
        }

        return 0;
    }

    public static (int, int) CheckSet(List<Card> cards)
    {
        if (cards.Count < 3)
            return (0, 0);


        var t = cards.GroupBy(card => card.CardValue).OrderByDescending(x => x.Key).FirstOrDefault(x => x.Count() == 3);

        if (t is null)
            return (0, 0);

        foreach (var card in cards.OrderByDescending(card => card.CardValue))
        {
            if (card.CardValue == t.Key)
                continue;
            return (t.Key, card.CardValue);
        }

        return (t.Key, 0);
    }

    private static (int, int, int) CheckTwoPair(List<Card> cards)
    {
        if (cards.Count < 4)
            return (0, 0, 0);

        var twoGroups = cards.GroupBy(card => card.CardValue).Where(group => group.Count() == 2)
            .OrderByDescending(group => group.Key).ToList();

        if (twoGroups.Count < 2)
            return (0, 0, 0);

        var topPairs = twoGroups.Take(2).Select(group => group.Key).ToList();

        if (topPairs.Count < 2)
            return (0, 0, 0);

        foreach (var card in cards.OrderByDescending(card => card.CardValue))
        {
            if (card.CardValue == topPairs[0] || card.CardValue == topPairs[1])
                continue;
            return (topPairs[0], topPairs[1], card.CardValue);
        }

        return (topPairs[0], topPairs[1], 0);
    }

    private static (int, int) CheckPair(List<Card> cards)
    {
        if (cards.Count < 2)
            return (0, 0);

        var x = cards
            .GroupBy(card => card.CardValue)
            .Where(group => group.Count() == 2)
            .MaxBy(group => group.Key);

        if (x is null)
            return (0, 0);

        foreach (var card in cards.OrderByDescending(card => card.CardValue))
        {
            if (card.CardValue == x.Key)
                continue;
            return (x.Key, card.CardValue);
        }

        return (x.Key, 0);
    }

    private static int CheckHighCard(List<Card> cards) => cards.MaxBy(x => x.CardValue)?.CardValue ?? 0;


    private int CompareHighCards(Hand firstHand, Hand secondHand)
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

    private int ComparePairs(Hand firstHand, Hand secondHand)
    {
        (int, int) firstPair =
            CheckPair(DealtCards.Union(new[] { firstHand.FirstCard, firstHand.SecondCard }).ToList());


        (int, int) secondPair =
            CheckPair(DealtCards.Union(new[] { secondHand.FirstCard, secondHand.SecondCard }).ToList());

        if (firstPair.Item1 != secondPair.Item1)
            return firstPair.Item1 > secondPair.Item1 ? 1 : -1;

        return firstPair.Item2 > secondPair.Item2 ? 1 : firstPair.Item2 < secondPair.Item2 ? -1 : 0;
    }

    private int CompareTwoPairs(Hand firstHand, Hand secondHand)
    {
        var first = CheckTwoPair(DealtCards.Union(new[] { firstHand.FirstCard, firstHand.SecondCard }).ToList());
        var second = CheckTwoPair(DealtCards.Union(new[] { secondHand.FirstCard, secondHand.SecondCard }).ToList());

        if (first.Item1 != second.Item1)
            return first.Item1 > second.Item1 ? 1 : -1;

        if (first.Item2 != second.Item2)
            return first.Item2 > second.Item2 ? 1 : -1;

        return first.Item3 > second.Item3 ? 1 : first.Item3 < second.Item3 ? -1 : 0;
    }

    private int CompareSets(Hand firstHand, Hand secondHand)
    {
        var first = CheckSet(DealtCards.Union(new[] { firstHand.FirstCard, firstHand.SecondCard }).ToList());
        var second = CheckSet(DealtCards.Union(new[] { secondHand.FirstCard, secondHand.SecondCard }).ToList());

        if (first.Item1 != second.Item1)
            return first.Item1 > second.Item1 ? 1 : -1;

        return first.Item2 > second.Item2 ? 1 : first.Item2 < second.Item2 ? -1 : 0;
    }

    private int CompareStraights(Hand firstHand, Hand secondHand)
    {
        var first = CheckStraight(DealtCards.Union(new[] { firstHand.FirstCard, firstHand.SecondCard }).ToList());
        var second = CheckStraight(DealtCards.Union(new[] { secondHand.FirstCard, secondHand.SecondCard }).ToList());

        return first > second ? 1 : first < second ? -1 : 0;
    }

    private int CompareFlushes(Hand firstHand, Hand secondHand)
    {
        var first = CheckFlush(DealtCards.Union(new[] { firstHand.FirstCard, firstHand.SecondCard }).ToList());
        var second = CheckFlush(DealtCards.Union(new[] { secondHand.FirstCard, secondHand.SecondCard }).ToList());
        
        return first > second ? 1 : first < second ? -1 : 0;
    }

    private int CompareFullHouses(Hand firstHand, Hand secondHand)
    {
        var first = CheckFullHouse(DealtCards.Union(new[] { firstHand.FirstCard, firstHand.SecondCard }).ToList());
        var second = CheckFullHouse(DealtCards.Union(new[] { secondHand.FirstCard, secondHand.SecondCard }).ToList());

        if (first.Item1 != second.Item1)
            return first.Item1 > second.Item1 ? 1 : -1;

        return first.Item2 > second.Item2 ? 1 : first.Item2 < second.Item2 ? -1 : 0;
    }

    private int CompareQuads(Hand firstHand, Hand secondHand)
    {
        var first = CheckQuads(DealtCards.Union(new[] { firstHand.FirstCard, firstHand.SecondCard }).ToList());
        var second = CheckQuads(DealtCards.Union(new[] { secondHand.FirstCard, secondHand.SecondCard }).ToList());

        return first > second ? 1 : first < second ? -1 : 0;
    }
}