using System;

namespace PretzelSolitaireSolver {

    public struct SolveResults {
        public bool Solvable;
        public ushort MinimumMoves;
        public ushort AntiGoalMoves;
    }

    public class PositionInfo {
        public PretzelPosition Position { get; }
        public int ParentIndex { get; set; }
        public bool IsAntiGoalMove { get; }
        public PositionInfo(PretzelPosition position, int parentIndex, bool isAntiGoalMove) {
            Position = position;
            ParentIndex = parentIndex;
            IsAntiGoalMove = isAntiGoalMove;
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
