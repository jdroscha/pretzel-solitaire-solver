using System;

namespace PretzelSolitaireSolver {

    public class CardDeck {
        public ushort SuitCount { get; }
        public ushort ValueCount { get; }
        public uint[] Cards { get; }
        private Random rng = new Random();

        public CardDeck(ushort suitCount, ushort valueCount) {
            SuitCount = suitCount;
            ValueCount = valueCount;
            Cards = new uint[suitCount * valueCount];
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
            for (int i = 0; i < Cards.Length - 1; ++i) {
                int indexToSwap = rng.Next(i + 1, Cards.Length);
                uint temp = Cards[i];
                Cards[i] = Cards[indexToSwap];
                Cards[indexToSwap] = temp;
            }
        }

        public void ShuffleBySuitDealSequentially() {
            for (int i = 0; i < SuitCount; ++i) {
                // shuffle within each suit
                uint[] cardsInSingleSuit = new uint[ValueCount];
                for (int j = 0; j < ValueCount; ++j) {
                    cardsInSingleSuit[j] = (uint)(i * ValueCount + j);
                }
                for (int j = 0; j < ValueCount - 1; ++j) {
                    int indexToSwap = rng.Next(j + 1, ValueCount);
                    uint temp = cardsInSingleSuit[j];
                    cardsInSingleSuit[j] = cardsInSingleSuit[indexToSwap];
                    cardsInSingleSuit[indexToSwap] = temp;
                }
                // distribute evenly into deck
                for (int j = 0; j < cardsInSingleSuit.Length; ++j) {
                    Cards[j * SuitCount + i] = cardsInSingleSuit[j];
                }
            }
        }

        public void ShuffleBySuitDealRandomly() {
            ShuffleBySuitDealSequentially();
            // shuffle each set of (SuitCount) cards
            for (int i = 0; i < ValueCount; ++i) {
                for (int j = 0; j < SuitCount - 1; ++j) {
                    int indexToSwap = i * SuitCount + rng.Next(j + 1, SuitCount);
                    uint temp = Cards[i * SuitCount + j];
                    Cards[i * SuitCount + j] = Cards[indexToSwap];
                    Cards[indexToSwap] = temp;
                }
            }
        }
    }

}
