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

            string summaryHeader = "\nTrial \tIters \tSolved \tMoves";
            if (Approach == ApproachType.FullTree)
                summaryHeader += " \tNoFail \tDeadends_for_Solvable";
            if (Output != OutputType.Verbose)
                Console.WriteLine(summaryHeader);

            CardDeck deck = new CardDeck(SuitCount, ValueCount);
            DateTime solveStartTime = DateTime.UtcNow;
            string moveCounts = string.Empty;
            ushort totalSolvedPretzels = 0;
            ulong totalMoves = 0;
            ushort totalUnlosablePretzels = 0;
            ulong totalDeadendsForSolvablePretzels = 0;
            for (short t = 0; t < Trials; ++t) {
                ushort trialSolvedPretzels = 0;
                ulong trialMoves = 0;
                ushort trialUnlosablePretzels = 0;
                ulong trailDeadendsForSolvablePretzels = 0;
                for (short i = 0; i < Iterations; ++i) {
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
                        if (Output == OutputType.SummaryWithMoveCounts) {
                            moveCounts += results.Moves.ToString() + ", ";
                        }
                        trailDeadendsForSolvablePretzels += results.Deadends;
                        if (results.Deadends == 0) {
                            ++trialUnlosablePretzels;
                        }
                    }
                }
                totalSolvedPretzels += trialSolvedPretzels;
                totalMoves += trialMoves;
                totalUnlosablePretzels += trialUnlosablePretzels;
                totalDeadendsForSolvablePretzels += trailDeadendsForSolvablePretzels;
                // output trial results
                if (Output == OutputType.Verbose)
                    Console.WriteLine(summaryHeader);
                string trialResults = t.ToString();
                trialResults += "\t" + Iterations.ToString();
                trialResults += "\t" + trialSolvedPretzels.ToString();
                trialResults += "\t" + trialMoves.ToString();
                if (Approach == ApproachType.FullTree) {
                    trialResults += "\t" + trialUnlosablePretzels.ToString();
                    trialResults += "\t" + trailDeadendsForSolvablePretzels.ToString();
                }
                Console.WriteLine(trialResults);
                if (Output == OutputType.SummaryWithMoveCounts) {
                    moveCounts += "\n\n";
                }
            }
            // output total results
            DateTime solveEndTime = DateTime.UtcNow;
            Console.WriteLine("");
            string totalResults = "TOTAL: ";
            totalResults += "\t" + (Trials * Iterations).ToString();
            totalResults += "\t" + totalSolvedPretzels.ToString();
            totalResults += "\t" + totalMoves.ToString();
            if (Approach == ApproachType.FullTree) {
                totalResults += "\t" + totalUnlosablePretzels.ToString();
                totalResults += "\t" + totalDeadendsForSolvablePretzels.ToString();
            }
            Console.WriteLine(totalResults);
            if (Output == OutputType.SummaryWithMoveCounts) {
                Console.WriteLine("\nMove Counts for All Solved Pretzels, Grouped by Trial");
                Console.WriteLine(moveCounts);
            }
            Console.WriteLine("");
            Console.WriteLine((solveEndTime - solveStartTime).ToString() + " Elapsed");
            Console.Write((char)7); // play bell
        }

        // WARNING: stores all attained positions in memory in two shapes, and can cause memory issues on some platforms
        public SolveResults Solve(PretzelPosition position, ApproachType approach, OutputType output) {
            List<PositionInfo> attainablePositionList = new List<PositionInfo>(); // ordered list of positions known to be attainable (explored or not yet explored)
            Dictionary<ushort, object> attainedPositions = new Dictionary<ushort, object>(); // trie of all positions attained (explored)
            // add initial position to list
            attainablePositionList.Add(new PositionInfo(position, None));
            // add initial position to trie
            int lastTableauIndex = position.Tableau.Length - 1;
            Dictionary<ushort, object> childNode = new Dictionary<ushort, object> { { position.Tableau[lastTableauIndex], 0 } };
            for (int i = lastTableauIndex - 1; i >= 0; --i) {
                Dictionary<ushort, object> trieNode = new Dictionary<ushort, object> { { position.Tableau[i], childNode } };
                childNode = trieNode;
            }
            int solutionIndex = None;
            List<int> deadendIndexes = new List<int>();
            int currentIndex = 0;
            while (((solutionIndex == None) || (approach == ApproachType.FullTree)) && (currentIndex < attainablePositionList.Count)) {
                if (attainablePositionList[currentIndex].Position.IsSolved()) {
                    solutionIndex = currentIndex;
                } else {
                    List<PretzelPosition> subsequentPositions = attainablePositionList[currentIndex].Position.GetSubsequentPositions();
                    if ((approach == ApproachType.RandomPlay) && (subsequentPositions.Count > 0)) {
                        short randomPlayIndex = (short)rng.Next(0, subsequentPositions.Count);
                        attainablePositionList.Add(new PositionInfo(subsequentPositions[randomPlayIndex], currentIndex));
                    } else if (subsequentPositions.Count > 0) {
                        for (short i = 0; i < subsequentPositions.Count; ++i) {
                            // check if position has already been attained
                            bool positionPreviouslyAttained = true;
                            int attainablePositionListIndex = None;
                            ushort currentTableauIndex = 0;
                            Dictionary<ushort, object> currentNode = attainedPositions;
                            object childNodeObject = null;
                            while (positionPreviouslyAttained && (currentTableauIndex <= lastTableauIndex)) {
                                if (currentNode.TryGetValue((ushort)subsequentPositions[i].Tableau[currentTableauIndex], out childNodeObject)) {
                                    if (currentTableauIndex < lastTableauIndex) {
                                        currentNode = (Dictionary<ushort, object>)childNodeObject;
                                    } else {
                                        attainablePositionListIndex = (int)childNodeObject; // unbox index of position in attainablePositionList from position's last grid position node in trie
                                    }
                                    ++currentTableauIndex;
                                } else {
                                    positionPreviouslyAttained = false;
                                    // add remainder of position to trie, starting at the last grid position and chaining forward to divergent node
                                    // NOTE: last grid position in trie contains boxed index of this position within attainablePositionList
                                    childNode = new Dictionary<ushort, object> { { subsequentPositions[i].Tableau[lastTableauIndex], currentIndex } };
                                    for (int j = lastTableauIndex - 1; j > currentTableauIndex; --j) {
                                        Dictionary<ushort, object> newNode = new Dictionary<ushort, object> { { subsequentPositions[i].Tableau[j], childNode } };
                                        childNode = newNode;
                                    }
                                    currentNode.Add((ushort)subsequentPositions[i].Tableau[currentTableauIndex], childNode);
                                }
                            }
                            // if new position attained, queue it at the end of attainable position list
                            if (!positionPreviouslyAttained) {
                                attainablePositionList.Add(new PositionInfo(subsequentPositions[i], currentIndex));
                            } else if (approach == ApproachType.FullTree) {
                                // position already reached; add new path to it if analyzing full decision tree
                                attainablePositionList[attainablePositionListIndex].ParentIndexes.Add(currentIndex);
                            }
                        }
                    } else {
                        // deadend
                        deadendIndexes.Add(currentIndex);
                    }
                }
                ++currentIndex;
            }
            // output and return results
            if (output == OutputType.Verbose) {
                Console.WriteLine(attainablePositionList.Count + " Attainable Positions Explored");
                Console.WriteLine(deadendIndexes.Count.ToString() + " Dead Ends");
            }
            if (solutionIndex != None) {
                if (output == OutputType.Verbose)
                    Console.WriteLine("Solution (read last line to first):");
                ushort solutionMoveCount = 0;
                while (solutionIndex != None) {
                    if (output == OutputType.Verbose)
                        Console.WriteLine(attainablePositionList[solutionIndex].Position.ToString());
                    solutionIndex = attainablePositionList[solutionIndex].ParentIndexes[0];
                    ++solutionMoveCount;
                }
                --solutionMoveCount; // do not count starting position
                if (output == OutputType.Verbose)
                    Console.WriteLine(solutionMoveCount.ToString() + " Moves");
                return new SolveResults { Solvable = true, Moves = solutionMoveCount, Deadends = (ushort)deadendIndexes.Count };
            } else {
                if (output == OutputType.Verbose)
                    Console.WriteLine("No Solution Found");
                return new SolveResults { Solvable = false, Moves = 0, Deadends = (ushort)deadendIndexes.Count };
            }
        }

    }

}
