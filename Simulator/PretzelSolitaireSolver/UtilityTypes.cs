using System;
using System.Collections.Generic;

namespace PretzelSolitaireSolver {

    public struct SolveResults {
        public bool Solvable;
        public ushort Moves;
        public ushort Deadends;
    }

    public class PositionInfo {
        public PretzelPosition Position { get; }
        public List<int> ParentIndexes { get; } // first element always traces back shortest path; others will exist only for FullTree analysis
        public PositionInfo(PretzelPosition position, int parentIndex) {
            Position = position;
            ParentIndexes = new List<int> { parentIndex };
        }
    }

    public static class TypeExtensions {
        public static ushort Clamp(this ushort value, ushort inclusiveMinimum, ushort inclusiveMaximum) {
            if (value < inclusiveMinimum) { return inclusiveMinimum; }
            if (value > inclusiveMaximum) { return inclusiveMaximum; }
            return value;
        }
    }

}
