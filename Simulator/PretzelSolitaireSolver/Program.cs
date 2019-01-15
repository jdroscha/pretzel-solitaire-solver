using System;
using System.Collections.Generic;

namespace PretzelSolitaireSolver {

    class CardDeck {
        public ushort SuitCount { get; }
        public ushort ValueCount { get; }
        public uint[] Cards { get; }

        public CardDeck(ushort suitCount, ushort valueCount) {
            SuitCount = suitCount;
            ValueCount = valueCount;
            Cards = new uint[suitCount * valueCount];
            for (ushort cardNumber = 0; cardNumber < suitCount * valueCount; ++cardNumber) {
                Cards[cardNumber] = cardNumber;
            }
        }

        public void Shuffle() {
            Random rng = new Random();
            for (int i = 0; i < Cards.Length - 1; ++i) {
                int indexToSwap = rng.Next(i + 1, Cards.Length);
                uint temp = Cards[i];
                Cards[i] = Cards[indexToSwap];
                Cards[indexToSwap] = temp;
            }
        }
    }

    class PretzelPosition {
        const int NoCard = 0; // 0 is the Ace of Spades, but aces get pulled out, so 0 is hole
        uint[] Tableau;
        int[] HoleIndices;
        ushort SuitCount;
        ushort ValueCount;

        public PretzelPosition(CardDeck deck) {
            Tableau = deck.Cards;
            HoleIndices = new int[deck.SuitCount];
            SuitCount = deck.SuitCount;
            ValueCount = deck.ValueCount;
            ushort holeCounter = 0;
            for (int i = 0; i < Tableau.Length; ++i) {
                // pull out aces, leaving holes
                if (Tableau[i] % ValueCount == 0) {
                    Tableau[i] = NoCard;
                    HoleIndices[holeCounter] = i;
                    ++holeCounter;
                }
            }
        }

        public PretzelPosition(PretzelPosition precedingPosition) {
            Tableau = new uint[precedingPosition.Tableau.Length];
            Array.Copy(precedingPosition.Tableau, Tableau, precedingPosition.Tableau.Length);
            HoleIndices = new int[precedingPosition.HoleIndices.Length];
            Array.Copy(precedingPosition.HoleIndices, HoleIndices, precedingPosition.HoleIndices.Length);
            SuitCount = precedingPosition.SuitCount;
            ValueCount = precedingPosition.ValueCount;
        }

        private bool EqualTo(PretzelPosition position) {
            // check if holes match first, to give a quick out in most cases
            for (int i = 0; i < HoleIndices.Length; ++i) {
                if (HoleIndices[i] != position.HoleIndices[i]) {
                    return false;
                }
            }
            for (int i = 0; i < Tableau.Length; ++i) {
                if (Tableau[i] != position.Tableau[i]) {
                    return false;
                }
            }
            return true;
        }

        public List<PretzelPosition> GetSubsequentPositions() {
            List<PretzelPosition> newPositions = new List<PretzelPosition>();
            for (int i = 0; i < SuitCount; ++i) {
                // find card that fits this hole
                uint cardNumberThatFitsHole = NoCard;
                if (HoleIndices[i] % ValueCount == 0) {
                    // hole is in first column, so the 2 of row's suit fits hole
                    cardNumberThatFitsHole = (uint)HoleIndices[i] + 1;
                } else if (Tableau[HoleIndices[i] - 1] == NoCard) {
                    // hole follows another hole
                    cardNumberThatFitsHole = NoCard; // NOTE: redundant
                } else if (Tableau[HoleIndices[i] - 1] % ValueCount < (ValueCount - 1)) {
                    // card before hole is not King, so next sequential card fits hole
                    cardNumberThatFitsHole = Tableau[HoleIndices[i] - 1] + 1;
                }
                // if possible, create position resulting from moving card into hole
                if (cardNumberThatFitsHole != NoCard) {
                    PretzelPosition newPosition = new PretzelPosition(this);
                    int indexOfCardThatFitsHole = Array.IndexOf(newPosition.Tableau, cardNumberThatFitsHole);
                    newPosition.Tableau[HoleIndices[i]] = newPosition.Tableau[indexOfCardThatFitsHole];
                    newPosition.Tableau[indexOfCardThatFitsHole] = NoCard;
                    newPosition.HoleIndices[i] = indexOfCardThatFitsHole;
                    newPositions.Add(newPosition);
                }
            }
            return newPositions;
        }

        public bool IsSolved() {
            bool solved = true;
            int i = 0;
            while (solved && (i < Tableau.Length)) {
                if ((Tableau[i] != NoCard) && (Tableau[i] != i + 1)) {
                    solved = false;
                }
                ++i;
            }
            return solved;
        }

        private class AttainablePosition {
            public readonly PretzelPosition Position;
            public readonly int ParentIndex; 
            public AttainablePosition(PretzelPosition position, int parentIndex) {
                Position = position;
                ParentIndex = parentIndex;
            }
        }

        // NOTE: returns minimum moves required to solve; returns -1 if not solvable
        public short Solve(PretzelPosition position) {
            const int None = -1;
            List<AttainablePosition> positionNodes = new List<AttainablePosition>();
            positionNodes.Add(new AttainablePosition(position, None));
            int solutionIndex = None;
            int currentIndex = 0;
            while ((solutionIndex == None) && (currentIndex < positionNodes.Count)) {
                // Console.WriteLine(attainablePositions[currentPositionIndex].ToString());
                if (positionNodes[currentIndex].Position.IsSolved()) {
                    solutionIndex = currentIndex;
                    break;
                } else {
                    List<PretzelPosition> subsequentPositions = positionNodes[currentIndex].Position.GetSubsequentPositions();
                    for (int i = 0; i < subsequentPositions.Count; ++i) {
                        // check if position has already been attained
                        bool positionFound = false;
                        int j = positionNodes.Count - 1;
                        while (!positionFound && (j >= 0)) {
                            if (subsequentPositions[i].EqualTo(positionNodes[j].Position)) {
                                positionFound = true;
                            }
                            --j;
                        }
                        // if new position attained, throw it at the end of node list
                        if (!positionFound) {
                            positionNodes.Add(new AttainablePosition(subsequentPositions[i], currentIndex));
                        }
                    }
                }
                ++currentIndex;
            }
            // log results
            Console.WriteLine(positionNodes.Count + " Attainable Positions Explored");
            if (solutionIndex != None) {
                short solutionMoveCount = 0;
                while (solutionIndex != None) {
                    Console.WriteLine(positionNodes[solutionIndex].Position.ToString());
                    solutionIndex = positionNodes[solutionIndex].ParentIndex;
                    ++solutionMoveCount;
                }
                --solutionMoveCount; // do not count starting position
                Console.WriteLine(solutionMoveCount.ToString() + " Minimum Moves Required");
                return solutionMoveCount;
            } else {
                Console.WriteLine("No Solution Found");
                return -1;
            }
        }

        public override string ToString() {
            string output = string.Empty;
            // NOTE: range breaks for suit count > 20
            string[] suitNames = { "S", "H", "D", "C", "A", "B", "E", "F", "G", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "T" };
            for (int i = 0; i < Tableau.Length; ++i) {
                if (Tableau[i] > 0) {
                    ushort suit = (ushort)(Tableau[i] / ValueCount);
                    ushort value = (ushort)(Tableau[i] % ValueCount);
                    output += suitNames[suit] + (value + 1).ToString() + " ";
                } else {
                    output += "-- "; // hole
                }
                if (i % ValueCount == ValueCount - 1) {
                    output += "* "; // end of row
                }
            }
            return output;
        }
    }

    class MainClass {
        public static void Main(string[] args) {
            Console.WriteLine("Pretzel Solitaire Solver");
            CardDeck deck = new CardDeck(suitCount: 4, valueCount: 4);
            DateTime solveStartTime = DateTime.UtcNow;
            ushort solvedPretzels = 0;
            long totalMinimumMovesRequired = 0;
            ushort iterations = 1000;
            for (int i = 0; i < iterations; ++i) {
                Console.WriteLine("Iteration " + i.ToString());
                deck.Shuffle();
                PretzelPosition position = new PretzelPosition(deck);
                Console.WriteLine(position.ToString());
                short minimumMovesRequired = position.Solve(position);
                if (minimumMovesRequired > -1) {
                    ++solvedPretzels;
                    totalMinimumMovesRequired += minimumMovesRequired;
                }
            }
            DateTime solveEndTime = DateTime.UtcNow;
            Console.WriteLine("");
            string resultsText = solvedPretzels.ToString() + " / " + iterations.ToString();
            resultsText += " = " + ((double)solvedPretzels / (double)iterations).ToString("F2") + " ";
            resultsText += deck.SuitCount.ToString() + "x" + deck.ValueCount.ToString();
            resultsText += " Pretzels Solved";
            Console.WriteLine(resultsText);
            Console.WriteLine(((double)totalMinimumMovesRequired / (double)solvedPretzels).ToString("F2") + " Mean Minimum Moves Required");
            Console.WriteLine((solveEndTime - solveStartTime).ToString() + " Elapsed");
            if (iterations != 1000) {
                double multiplier = 1000 / (double)iterations;
                TimeSpan estimatedTimePer1000 = TimeSpan.FromTicks((long)((solveEndTime - solveStartTime).Ticks * multiplier));
                Console.WriteLine(estimatedTimePer1000.ToString() + " Estimated Time Per 1000 Iterations");
            }
            Console.Write((char)7); // play bell
            Console.Write((char)7); // play bell
            Console.Write((char)7); // play bell
            Console.Write((char)7); // play bell
            Console.Write((char)7); // play bell
        }
    }
}
