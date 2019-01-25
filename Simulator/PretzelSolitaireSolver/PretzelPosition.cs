﻿using System;
using System.Collections.Generic;

namespace PretzelSolitaireSolver {

    public class PretzelPosition {
        const int None = -1;
        const uint NoCard = 0; // 0 is the Ace of Spades, but aces get pulled out, so 0 is hole

        public uint[] Tableau;
        public int[] HoleIndices;
        private ushort SuitCount;
        private ushort ValueCount;
        private Random rng = new Random();

        public PretzelPosition(uint[] tableau, int[] holeIndices, ushort suitCount, ushort valueCount) {
            Tableau = tableau;
            HoleIndices = holeIndices;
            SuitCount = suitCount;
            ValueCount = valueCount;
        }

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

        public PretzelPosition(PretzelPosition previousPosition) {
            Tableau = new uint[previousPosition.Tableau.Length];
            Array.Copy(previousPosition.Tableau, Tableau, previousPosition.Tableau.Length);
            HoleIndices = new int[previousPosition.HoleIndices.Length];
            Array.Copy(previousPosition.HoleIndices, HoleIndices, previousPosition.HoleIndices.Length);
            SuitCount = previousPosition.SuitCount;
            ValueCount = previousPosition.ValueCount;
        }

        public List<PositionInfo> GetSubsequentPositions() {
            List<PositionInfo> newPositions = new List<PositionInfo>();
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
                    // NOTE: In most versions of the game, the conditional for this else is extraneous
                    //       since the next card number after a King is an Ace and all Aces were
                    //       pulled out of the deck after the shuffle.  Left in for future variants.
                    cardNumberThatFitsHole = Tableau[HoleIndices[i] - 1] + 1;
                }
                // if possible, create position resulting from moving card into hole
                if (cardNumberThatFitsHole != NoCard) {
                    PretzelPosition newPosition = new PretzelPosition(this);
                    int indexOfCardThatFitsHole = Array.IndexOf(newPosition.Tableau, cardNumberThatFitsHole);
                    newPosition.Tableau[HoleIndices[i]] = newPosition.Tableau[indexOfCardThatFitsHole];
                    newPosition.Tableau[indexOfCardThatFitsHole] = NoCard;
                    newPosition.HoleIndices[i] = indexOfCardThatFitsHole;
                    bool isAntiGoalMove = cardNumberThatFitsHole == indexOfCardThatFitsHole + 1;
                    newPositions.Add(new PositionInfo(newPosition, None, isAntiGoalMove));
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

        public override string ToString() {
            string output = string.Empty;
            // WARNING: range breaks for suit count > 20
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

}