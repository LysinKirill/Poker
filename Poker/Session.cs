namespace Poker;

public class Session
{
    public int Pot { get; set; }
    public int CurrentBet
    {
        get { return _currentBet; }
        private set { _currentBet = value; }
    }
    private int SmallBlind { get; set; }
    private List<Card> Deck { get; set; } = new List<Card>();
    private GameStage GameStage { get; set; } = GameStage.Preflop;
    private Table Table { get; }
    private HashSet<Participant> AllInPlayers { get; set; }
    private HashSet<Participant> FoldedPlayers { get; set; }

    private static readonly Random Random = new Random();
    private int _currentBet = 0;

    public Session(List<Participant> participants, int smallBlind)
    {
        Table = new Table(participants);
        AllInPlayers = new HashSet<Participant>();
        FoldedPlayers = new HashSet<Participant>();
        SmallBlind = smallBlind;
    }


    private Participant GetParticipant(int index) => Table.Participants[index % Table.Participants.Count];

    public void StartSession()
    {
        RemoveListeners();
        Subscribe();

        int dealerIndex = 0;
        int roundCount = 1;
        while (Table.Participants.Count(participant => participant.Money > 0) > 1)
        {
            if (roundCount != 1 && roundCount % Table.Participants.Count == 1)
                SmallBlind *= 2;
            Console.WriteLine($"Round №{roundCount++}");
            PlayRound(dealerIndex, SmallBlind);
            ++dealerIndex;
        }

        var winner = Table.Participants.First(participant => participant.Money > 0);
        Console.WriteLine(winner);
    }

    private void PlayRound(int dealerIndex, int smallBlind)
    {
        GameStage = GameStage.GameStart;
        //int currentBet = 0;
        AllInPlayers.Clear();
        FoldedPlayers.Clear();

        CurrentBet = 0;
        Pot = 0;
        Deck = GetStandardDeck();
        ShuffleDeck();


        foreach (Participant participant in Table.Participants)
            participant.MoneySpentDuringRound = 0;
        DealCardsToParticipants();

        Table.Participants.ForEach(x => x.ShowCards());



        int firstForcedBet = GetParticipant(dealerIndex + 1).ForcedBet(smallBlind); // Small blind
        Pot += firstForcedBet;
        ColorPrompt($"Participant {GetParticipant(dealerIndex + 1).ParticipantName} puts a small blind. [-${firstForcedBet}]", ConsoleColor.Cyan);

        int secondForcedBet = GetParticipant(dealerIndex + 2).ForcedBet(smallBlind * 2); // Big blind
        Pot += secondForcedBet;
        ColorPrompt($"Participant {GetParticipant(dealerIndex + 2).ParticipantName} puts a big blind. [-${secondForcedBet}]", ConsoleColor.Cyan);

        CurrentBet = smallBlind * 2;


        Console.WriteLine("Preflop.");
        if (WaitForParticipantMoves(dealerIndex + 2, smallBlind))
        {
            Pot = 0;
            return;
        }

        ++GameStage;
        ++dealerIndex;

        Console.WriteLine("Flop.");
        DealRandomCardToTable();
        DealRandomCardToTable();
        DealRandomCardToTable();
        ShowDealtCards();
        if (WaitForParticipantMoves(dealerIndex + 1, smallBlind))
        {
            Pot = 0;
            return;
        }

        ++GameStage;
        ++dealerIndex;

        Console.WriteLine("Turn.");
        DealRandomCardToTable();
        ShowDealtCards();
        if (WaitForParticipantMoves(dealerIndex + 2, smallBlind))
        {
            Pot = 0;
            return;
        }

        ++GameStage;
        ++dealerIndex;

        Console.WriteLine("River.");
        DealRandomCardToTable();
        ShowDealtCards();
        if (WaitForParticipantMoves(dealerIndex + 3, smallBlind))
        {
            Pot = 0;
            return;
        }

        ++GameStage;
        ++dealerIndex;

        Console.WriteLine("It's time to show cards...");
        foreach (Participant participant in Table.Participants.Except(FoldedPlayers))
        {
            Console.WriteLine($"Participant {participant.ParticipantName}:");
            Console.WriteLine(participant.Hand!.FirstCard);
            Console.WriteLine(participant.Hand!.SecondCard);
            Console.WriteLine("__________________________________________");
        }

        Console.WriteLine();
        var winners = DetermineWinners(Table.Participants.Except(FoldedPlayers).ToList());
        var winAmount = Pot / winners.Count;

        winners.ForEach(winner =>
        {
            ColorPrompt($"Participant {winner.ParticipantName} won ({Table.GetCombination(winner.Hand!)}). [+${winAmount}]", ConsoleColor.Cyan);
            winner.CollectWinnings(winAmount);
        });
        Console.WriteLine();
        Pot = 0;
    }


    private void RemoveListeners() => Table.Participants.ForEach(participant => participant.RemoveAllListeners());

    private void Subscribe() // And leave a comment
    {
        foreach (Participant participant in Table.Participants)
        {
            participant.OnAllIn += amount =>
            {
                ColorPrompt($"{participant.ParticipantName} goes all in. [-${amount}]", ConsoleColor.Cyan);
                if (participant.MoneySpentDuringRound > _currentBet)
                {
                    _currentBet = participant.MoneySpentDuringRound;
                    //AddPreviousPlayersToIncreaseTheirBet();
                }

                AllInPlayers.Add(participant);
                Pot += amount;
            };

            participant.OnBet += amount =>
            {
                ColorPrompt($"{participant.ParticipantName} bets. [-${amount}]", ConsoleColor.Cyan);
                if (participant.MoneySpentDuringRound > _currentBet)
                {
                    _currentBet = participant.MoneySpentDuringRound;
                    //AddPreviousPlayersToIncreaseTheirBet();
                }

                Pot += amount;
            };

            participant.OnCall += amount =>
            {
                ColorPrompt($"{participant.ParticipantName} calls. [-${amount}]", ConsoleColor.Cyan);
                Pot += amount;
            };

            participant.OnCheck += () => Console.WriteLine($"{participant.ParticipantName} checks.");
            participant.OnFold += () =>
            {
                Console.WriteLine($"{participant.ParticipantName} folds.");
                FoldedPlayers.Add(participant);
            };

            participant.OnRaise += amount =>
            {
                ColorPrompt($"{participant.ParticipantName} raises. [-{amount}$]", ConsoleColor.Cyan);
                _currentBet = participant.MoneySpentDuringRound;
                Pot += amount;
            };
        }
    }

    private List<Participant> DetermineWinners(List<Participant> candidates)
    {
        List<Participant> winners = new List<Participant>() { candidates[0] };

        Participant currentWinner = candidates[0];
        for (int i = 1; i < candidates.Count; i++)
        {
            Participant participant = candidates[i];
            int compare = Table.CompareHands(participant.Hand!, currentWinner.Hand!);
            switch (compare)
            {
                case < 0:
                    continue;
                case > 0:
                    winners.Clear();
                    currentWinner = participant;
                    break;
            }

            winners.Add(participant);
        }

        return winners;
    }

    private void ShowDealtCards() => Table.DealtCards.ForEach(Console.WriteLine);

    private static List<Card> GetStandardDeck()
    {
        return (from value in Enumerable.Range(2, 13)
            from suit in Enumerable.Range(1, 4)
            select new Card((Suit)suit, value)).ToList();
    }

    private void ShuffleDeck()
    {
        Deck = Deck
            .OrderBy(x => Random.Next())
            .ToList();
    }

    private void DealRandomCard(out Card dealtCard)
    {
        var cardToDeal = Deck[Random.Next(Deck.Count)];
        Deck.Remove(cardToDeal);
        dealtCard = cardToDeal;
    }

    private void DealRandomCardToTable()
    {
        if (GameStage == GameStage.River)
            throw new Exception("Cannot deal another card after river.");

        DealRandomCard(out Card cardToDeal);
        Table.DealtCards.Add(cardToDeal);
        //GameStage++;
    }

    private void DealCardsToParticipants()
    {
        foreach (Participant participant in Table.Participants)
        {
            DealRandomCard(out Card firstCard);
            DealRandomCard(out Card secondCard);

            participant.Hand = new Hand(firstCard, secondCard);
        }
    }

    private bool WaitForParticipantMoves(int dealerIndex, int smallBlind)
    {

        //int currIndex = dealerIndex + 1;

        for (int currIndex = dealerIndex + 1;
             currIndex <= dealerIndex + Table.Participants.Count && Table.Participants.Count - FoldedPlayers.Count != 1;
             ++currIndex)
        {
            ColorPrompt($"Current table bet: ${_currentBet}", ConsoleColor.Cyan);
            Participant participant = GetParticipant(currIndex);
            if (FoldedPlayers.Contains(participant))
            {
                ++currIndex;
                continue;
            }

            if (AllInPlayers.Contains(participant))
            {
                Console.WriteLine($"Player {participant.ParticipantName} checks. (All-in)");
                ++currIndex;
                continue;
            }

            GetParticipant(currIndex).MakeMove(_currentBet, smallBlind * 2);
        }

        Console.WriteLine();

        if (Table.Participants.Count - FoldedPlayers.Count != 1)
            return false;

        Participant winner = Table.Participants.Except(AllInPlayers).First();
        ColorPrompt($"Participant {winner.ParticipantName} wins. [+{Pot}$]\n", ConsoleColor.Cyan);

        // Side pot ?????????????????????????????????

        winner.CollectWinnings(Pot);
        Pot = 0;
        return true;

    }


    public void MoveToNextStage()
    {
        switch (GameStage)
        {
            case GameStage.GameStart:
                Console.WriteLine("Game started.");
                break;
            case GameStage.Preflop:
                DealRandomCardToTable();
                DealRandomCardToTable();
                DealRandomCardToTable();
                break;
            case GameStage.Flop:
                DealRandomCardToTable();
                break;
            case GameStage.Turn:
                DealRandomCardToTable();
                break;
            case GameStage.River:
                Console.WriteLine("Results:");
                break;
            case GameStage.GameEnd:
                Console.WriteLine("Game ended.");
                return;
            default:
                throw new ArgumentOutOfRangeException();
        }

        ++GameStage;
    }

    private static void ColorPrompt(string str, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(str);
        Console.ResetColor();
    }
}
