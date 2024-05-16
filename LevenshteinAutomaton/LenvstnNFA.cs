using System;
using SCG = System.Collections.Generic;


using state = System.Int32;
using input = System.Byte;
using System.Collections.Generic;

namespace LevenshteinAutomaton
{
    class Constants
    {
        public static byte None = (byte)0;
        public static byte EpsilonAny = (byte)1;
        public static byte Any = (byte)2;
    }

  /// <summary>
  /// Implements a non-deterministic finite automata
  /// </summary>
    class LenvstnNFA
    {
        public state initial;
        public HashSet<state> final;
        // Inputs this NFA responds to
        public SortedSet<input> inputs; // SORTEDARRAY
        public input[][] transTable;
        
        private int size;


        private LenvstnNFA()
        {
        }

        /// <summary>
        /// Constructed with the NFA size (amount of states), the initial state and the
        /// final state
        /// </summary>
        /// <param name="size_">Amount of states.</param>
        /// <param name="initial_">Initial state.</param>
        /// <param name="final_">Final state.</param>
        private LenvstnNFA(int size_, state initial_, HashSet<state> final_)
        {
            initial = initial_;
            final = final_;
            size = size_;

            inputs = new SortedSet<input>();

            // Initializes transTable with an "empty graph", no transitions between its
            // states
            transTable = new input[size][];

            for (int i = 0; i < size; ++i)
                transTable[i] = new input[size];
        }

        /// <summary>
        /// NFA building functions, build a NFA to find all words within given levenshtein distance from given word.
        /// </summary>
        /// <param name="str">The input word</param>
        /// <param name="maxDist">The max levenshtein distance from input word</param>
        /// <returns></returns>
        public static LenvstnNFA BuildNFA(String str, int maxDist)
        {
            int width = str.Length + 1;
            int height = maxDist + 1;
            int size = width * height;

            HashSet<state> final = new HashSet<state>();
            for (int i = 1; i <= height; ++i)
                final.Add(i * width - 1);
            LenvstnNFA nfa = new LenvstnNFA(size, 0, final);

            //Every state except those in right most coulmn in the matrix
            for (int e = 0; e < height; ++e)
            {
                for (int i = 0; i < width - 1; ++i)
                {
                    //trans to right
                    nfa.AddTrans(e * width + i, e * width + i + 1, (byte)str[i]);
                    if (e < (height - 1))
                    {
                        //trans to upper
                        nfa.AddTrans(e * width + i, (e + 1) * width + i, Constants.Any);
                        //trans to diagonal upper
                        nfa.AddTrans(e * width + i, (e + 1) * width + i + 1, Constants.EpsilonAny);
                    }
                }
            }

            //right most column
            for (int k = 1; k < height; ++k)
            {
                //trans to upper
                nfa.AddTrans(k * width - 1, (k + 1) * width - 1, Constants.Any);
            }
            return nfa;
        }

        /// <summary>
        /// Adds a transition between two states.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="in"></param>
        public void AddTrans(state from, state to, input @in)
        {
            transTable[from][to] = @in;
            inputs.Add(@in);
        }

        /// <summary>
        /// Returns a set of NFA states from which there is a transition on input symbol
        /// inp from some state s in states.
        /// </summary>
        /// <param name="states"></param>
        /// <param name="inp"></param>
        /// <returns></returns>
        public HashSet<state> Move(HashSet<state> states, input inp)
        {
            HashSet<state> result = new HashSet<state>();

            // The result to return include every state reachable by inp or Epsilon or Any. 
            // But if parameter inp is a normal letter, will only return the result when truly found one bridge which equals to inp.
            bool needNormalLetter = false;
            bool findNormalLetter = false;
            if (inp != Constants.Any && inp != Constants.EpsilonAny)
            {
                needNormalLetter = true;
            }

            // For each state in the set of states
            foreach (state state in states)
            {
                // For each transition from this state
                for (int j = 0; j < size; ++j)
                {
                    // If the transition is on input inp, add it to the resulting set
                    if (transTable[state][j] == inp || transTable[state][j] == Constants.Any || transTable[state][j] == Constants.EpsilonAny)
                    {
                        if (needNormalLetter && transTable[state][j] == inp) findNormalLetter = true;
                        result.Add(j);
                    }
                }
            }

            if (needNormalLetter && !findNormalLetter) result.Clear();
            return result;
        }

        /// <summary>
        /// Prints out the NFA.
        /// </summary>
        public void Show()
        {
            Console.WriteLine("This NFA has {0} states: 0 - {1}", size, size - 1);
            Console.WriteLine("The initial state is {0}", initial);
            Console.WriteLine("The final state is {0}\n", final);

            for (state from = 0; from < size; ++from)
            {
                for (state to = 0; to < size; ++to)
                {
                    input @in = transTable[from][to];

                    if (@in != (char)Constants.None)
                    {
                        Console.Write("Transition from {0} to {1} on input ", from, to);

                        if (@in == (char)Constants.EpsilonAny)
                            Console.Write("EpsilonAny\n");
                        else if (@in == (char)Constants.Any)
                            Console.Write("Any\n");
                        else
                            Console.Write("{0}\n", @in);
                    }
                }
            }
            Console.Write("\n\n");
        }

        
    }

}