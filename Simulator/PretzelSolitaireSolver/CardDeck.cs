using System;

namespace PretzelSolitaireSolver {

    public class CardDeck {
        public ushort SuitCount { get; }
        public ushort ValueCount { get; }
        public ushort[] Cards { get; }
        private Random rng = new Random();

        public CardDeck(ushort suitCount, ushort valueCount) {
            SuitCount = suitCount;
            ValueCount = valueCount;
            Cards = new ushort[suitCount * valueCount];
            for (ushort cardNumber = 0; cardNumber < suitCount * valueCount; ++cardNumber) {
                Cards[cardNumber] = cardNumber;
            }
        }

        public void Shuffle(DealType deal) {
            switch (deal) {
                case DealType.SequentialSuits: { ShuffleBySuitDealSequentially(); break; }
                case DealType.RandomSuits: { ShuffleBySuitDealRandomly(); break; }
                default: { ShuffleStandard(); break; }
            }
        }

        public void ShuffleStandard() {
            for (short i = 0; i < Cards.Length - 1; ++i) {
                short indexToSwap = (short)rng.Next(i + 1, Cards.Length);
                ushort temp = Cards[i];
                Cards[i] = Cards[indexToSwap];
                Cards[indexToSwap] = temp;
            }
        }

        public void ShuffleBySuitDealSequentially() {
            for (short i = 0; i < SuitCount; ++i) {
                // shuffle within each suit
                ushort[] cardsInSingleSuit = new ushort[ValueCount];
                for (short j = 0; j < ValueCount; ++j) {
                    cardsInSingleSuit[j] = (ushort)(i * ValueCount + j);
                }
                for (short j = 0; j < ValueCount - 1; ++j) {
                    short indexToSwap = (short)rng.Next(j + 1, ValueCount);
                    ushort temp = cardsInSingleSuit[j];
                    cardsInSingleSuit[j] = cardsInSingleSuit[indexToSwap];
                    cardsInSingleSuit[indexToSwap] = temp;
                }
                // distribute evenly shorto deck
                for (short j = 0; j < cardsInSingleSuit.Length; ++j) {
                    Cards[j * SuitCount + i] = cardsInSingleSuit[j];
                }
            }
        }

        public void ShuffleBySuitDealRandomly() {
            ShuffleBySuitDealSequentially();
            // shuffle each set of (SuitCount) cards
            for (short i = 0; i < ValueCount; ++i) {
                for (short j = 0; j < SuitCount - 1; ++j) {
                    short indexToSwap = (short)(i * SuitCount + rng.Next(j + 1, SuitCount));
                    ushort temp = Cards[i * SuitCount + j];
                    Cards[i * SuitCount + j] = Cards[indexToSwap];
                    Cards[indexToSwap] = temp;
                }
            }
        }
    }

}
