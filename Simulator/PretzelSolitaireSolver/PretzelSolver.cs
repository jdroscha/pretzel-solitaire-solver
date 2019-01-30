using System;
using System.Collections.Generic;

namespace PretzelSolitaireSolver {

    public class Simulator {
        const int None = -1;

        // these local values get set in constructor based on command line arguments
        private ushort SuitCount;
        private ushort ValueCount;
        private ushort Trials;
        private ushort Iterations;
        private DealType Deal;
        private ApproachType Approach;
        private OutputType Output;
        private Random rng = new Random();

        public Simulator(ushort suitCount,
                         ushort valueCount,
                         ushort trials,
                         ushort iterations,
                         DealType deal,
                         ApproachType approach,
                         OutputType output) {
            SuitCount = suitCount;
            ValueCount = valueCount;
            Trials = trials;
            Iterations = iterations;
            Deal = deal;
            Approach = approach;
            Output = output;
        }

        public void RunTrials() {
            Console.WriteLine("Pretzel Solitaire Solver");
            string resultsHeader = "Suits: " + SuitCount.ToString();
            resultsHeader += ", Values: " + ValueCount.ToString();
            resultsHeader += ", Deal: " + Deal.ToString();
            resultsHeader += ", Approach: " + Approach.ToString();
            resultsHeader += ", Ouput: " + Output.ToString();
            Console.WriteLine(resultsHeader);
            if (Output != OutputType.Verbose)
                Console.WriteLine("\nTrial \tIters \tSolved \tMoves \tAntiGoalMoves");

            CardDeck deck = new CardDeck(SuitCount, ValueCount);
            DateTime solveStartTime = DateTime.UtcNow;
            string moveCounts = string.Empty;
            string antiGoalMoveCounts = string.Empty;
            ushort totalSolvedPretzels = 0;
            ulong totalMoves = 0;
            ulong totalAntiGoalMoves = 0;
            for (int t = 0; t < Trials; ++t) {
                ushort trialSolvedPretzels = 0;
                ulong trialMoves = 0;
                ulong trialAntiGoalMoves = 0;
                for (int i = 0; i < Iterations; ++i) {
                    if (Output == OutputType.Verbose)
                        Console.WriteLine("\nTrial " + t.ToString() + " Iteration " + i.ToString());
                    deck.Shuffle(Deal);
                    PretzelPosition position = new PretzelPosition(deck);
                    if (Output == OutputType.Verbose)
                        Console.WriteLine(position.ToString());
                    SolveResults results = Solve(position, Approach, Output);
                    if (results.Solvable) {
                        ++trialSolvedPretzels;
                        trialMoves += results.Moves;
                        trialAntiGoalMoves += results.AntiGoalMoves;
                        if (Output == OutputType.SummaryWithMoveCounts) {
                            moveCounts += results.Moves.ToString() + ", ";
                            antiGoalMoveCounts += results.AntiGoalMoves.ToString() + ", ";
                        }
                    }
                }
                totalSolvedPretzels += trialSolvedPretzels;
                totalMoves += trialMoves;
                totalAntiGoalMoves += trialAntiGoalMoves;
                // output trial results
                if (Output == OutputType.Verbose)
                    Console.WriteLine("\nTrial \tIters \tSolved \tMoves \tAntiGoalMoves");
                string trialResults = t.ToString();
                trialResults += "\t" + Iterations.ToString();
                trialResults += "\t" + trialSolvedPretzels.ToString();
                trialResults += "\t" + trialMoves.ToString();
                trialResults += "\t" + trialAntiGoalMoves.ToString();
                Console.WriteLine(trialResults);
                if (Output == OutputType.SummaryWithMoveCounts) {
                    moveCounts += "\n\n";
                    antiGoalMoveCounts += "\n\n";
                }
            }
            // output total results
            DateTime solveEndTime = DateTime.UtcNow;
            Console.WriteLine("");
            string totalResults = "TOTAL: ";
            totalResults += "\t" + (Trials * Iterations).ToString();
            totalResults += "\t" + totalSolvedPretzels.ToString();
            totalResults += "\t" + totalMoves.ToString();
            totalResults += "\t" + totalAntiGoalMoves.ToString();
            Console.WriteLine(totalResults);
            if (Output == OutputType.SummaryWithMoveCounts) {
                Console.WriteLine("\nMove Counts for All Solved Pretzels, Grouped by Trial");
                Console.WriteLine(moveCounts);
                Console.WriteLine("Anti-Goal Move Counts for All Solved Pretzels, Grouped by Trial");
                Console.WriteLine(antiGoalMoveCounts);
            }
            Console.WriteLine("");
            Console.WriteLine((solveEndTime - solveStartTime).ToString() + " Elapsed");
            Console.Write((char)7); // play bell
        }

        // WARNING: stores all attained positions in memory in two shapes, and can cause memory issues on some platforms
        public SolveResults Solve(PretzelPosition position, ApproachType approach, OutputType output) {
            List<PositionInfo> attainablePositionList = new List<PositionInfo>(); // ordered list of positions attained
            Dictionary<uint, object> attainedPositions = new Dictionary<uint, object>(); // trie of all positions attained
            // add initial position to list
            attainablePositionList.Add(new PositionInfo(position, None, false));
            // add initial position to trie
            Dictionary<uint, object> childNode = null;
            for (int i = position.Tableau.Length - 1; i >= 0; --i) {
                Dictionary<uint, object> trieNode = new Dictionary<uint, object>();
                trieNode.Add(position.Tableau[i], childNode);
                childNode = trieNode;
            }
            int solutionIndex = None;
            int currentIndex = 0;
            while ((solutionIndex == None) && (currentIndex < attainablePositionList.Count)) {
                if (attainablePositionList[currentIndex].Position.IsSolved()) {
                    solutionIndex = currentIndex;
                } else {
                    List<PositionInfo> subsequentPositions = attainablePositionList[currentIndex].Position.GetSubsequentPositions();
                    if ((approach == ApproachType.RandomPlay) && (subsequentPositions.Count > 0)) {
                        int randomPlayIndex = rng.Next(0, subsequentPositions.Count);
                        subsequentPositions[randomPlayIndex].ParentIndex = currentIndex;
                        attainablePositionList.Add(subsequentPositions[randomPlayIndex]);
                    } else {
                        for (int i = 0; i < subsequentPositions.Count; ++i) {
                            // check if position has already been attained
                            bool positionPreviouslyAttained = true;
                            uint currentGridLocation = 0;
                            Dictionary<uint, object> currentNode = attainedPositions;
                            object childNodeObject = null;
                            while (positionPreviouslyAttained && (currentGridLocation < subsequentPositions[i].Position.Tableau.Length)) {
                                if (currentNode.TryGetValue(subsequentPositions[i].Position.Tableau[currentGridLocation], out childNodeObject)) {
                                    currentNode = (Dictionary<uint, object>)childNodeObject;
                                    ++currentGridLocation;
                                } else {
                                    positionPreviouslyAttained = false;
                                    // add remainder of position to trie
                                    childNode = null;
                                    for (int j = subsequentPositions[i].Position.Tableau.Length - 1; j > currentGridLocation; --j) {
                                        Dictionary<uint, object> newNode = new Dictionary<uint, object>();
                                        newNode.Add(subsequentPositions[i].Position.Tableau[j], childNode);
                                        childNode = newNode;
                                    }
                                    currentNode.Add(subsequentPositions[i].Position.Tableau[currentGridLocation], childNode);
                                }
                            }
                            // if new position attained, queue it at the end of attainable position list
                            if (!positionPreviouslyAttained) {
                                subsequentPositions[i].ParentIndex = currentIndex;
                                attainablePositionList.Add(subsequentPositions[i]);
                            }
                        }
                    }
                }
                ++currentIndex;
            }
            // output and return results
            if (output == OutputType.Verbose)
                Console.WriteLine(attainablePositionList.Count + " Attainable Positions Explored");
            if (solutionIndex != None) {
                if (output == OutputType.Verbose)
                    Console.WriteLine("Solution (read last line to first):");
                ushort solutionMoveCount = 0;
                ushort solutionAntiGoalMoveCount = 0;
                while (solutionIndex != None) {
                    if (output == OutputType.Verbose)
                        Console.WriteLine(attainablePositionList[solutionIndex].Position.ToString());
                    if (attainablePositionList[solutionIndex].IsAntiGoalMove)
                        ++solutionAntiGoalMoveCount;
                    solutionIndex = attainablePositionList[solutionIndex].ParentIndex;
                    ++solutionMoveCount;
                }
                --solutionMoveCount; // do not count starting position
                if (output == OutputType.Verbose)
                    Console.WriteLine(solutionMoveCount.ToString() + " Moves, including " + solutionAntiGoalMoveCount.ToString() + " Anti-Goal Moves");
                return new SolveResults { Solvable = true, Moves = solutionMoveCount, AntiGoalMoves = solutionAntiGoalMoveCount };
            } else {
                if (output == OutputType.Verbose)
                    Console.WriteLine("No Solution Found");
                return new SolveResults { Solvable = false, Moves = 0, AntiGoalMoves = 0 };
            }
        }

    }

}
