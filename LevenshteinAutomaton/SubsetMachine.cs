using System;
using System.Collections.Generic;
using System.Linq;

using state = System.Int32;
using input = System.Char;

namespace LevenshteinAutomaton
{
  class SubsetMachine
  {
    private static int num = 0;

    /// <summary>
    /// Creates unique state numbers for DFA states
    /// </summary>
    /// <returns></returns>
    private static state GenNewState()
    {
      return num++;
    }

    private static void ResetState()
    {
        num = 0;
    }


    public static LenvstnDFA SubsetConstruct(LenvstnNFA nfa)
    {
        ResetState();
        LenvstnDFA dfa = new LenvstnDFA();

        // Sets of NFA states which is represented by some DFA state
        HashSet<HashSet<state>> markedStates = new HashSet<HashSet<state>>();
        HashSet<HashSet<state>> unmarkedStates = new HashSet<HashSet<state>>();

        // Gives a number to each state in the DFA
        Dictionary<HashSet<state>, state> dfaStateNum = new Dictionary<HashSet<state>, state>();

        HashSet<state> nfaInitial = new HashSet<state>();
        nfaInitial.Add(nfa.initial);

        // Initially, EpsilonClosure(nfa.initial) is the only state in the DFAs states
        // and it's unmarked.
        HashSet<state> first = EpsilonClosure(nfa, nfaInitial);
        unmarkedStates.Add(first);

        // The initial dfa state
        state dfaInitial = GenNewState();
        dfaStateNum[first] = dfaInitial;
        dfa.start = dfaInitial;

        while (unmarkedStates.Count != 0)
        {
                // Takes out one unmarked state and posteriorly mark it.
                HashSet<state> aState = unmarkedStates.First();

            // Removes from the unmarked set.
            unmarkedStates.Remove(aState);

            // Inserts into the marked set.
            markedStates.Add(aState);

            // If this state contains the NFA's final state, add it to the DFA's set of
            // final states.
            if (aState.Where(state => nfa.final.Contains(state)).Count() > 0)
                dfa.final.Add(dfaStateNum[aState]);

            IEnumerator<input> iE = nfa.inputs.GetEnumerator();

            // For each input symbol the NFA knows...
            while (iE.MoveNext())
            {
                // Next state
                HashSet<state> next = EpsilonClosure(nfa, nfa.Move(aState, iE.Current));
                if (next.Count == 0) continue;

                // If we haven't examined this state before, add it to the unmarkedStates,
                // and make up a new number for it.
                if (!unmarkedStates.Contains(next) && !markedStates.Contains(next))
                {
                    unmarkedStates.Add(next);
                    dfaStateNum.Add(next, GenNewState());
                }

                if (iE.Current != (char)LenvstnNFA.Constants.Any && iE.Current != (char)LenvstnNFA.Constants.EpsilonAny)
                {
                    KeyValuePair<state, input> transition = new KeyValuePair<state, input>(dfaStateNum[aState], iE.Current);
                    dfa.transTable[transition] = dfaStateNum[next];
                }
                else
                {
                    if (!dfa.defaultTrans.ContainsKey(dfaStateNum[aState])) 
                    {
                        dfa.defaultTrans.Add(dfaStateNum[aState], dfaStateNum[next]);
                    }
                }
            }
        }

        return dfa;
    }

    /// <summary>
    /// Builds the Epsilon closure of states for the given NFA 
    /// </summary>
    /// <param name="nfa"></param>
    /// <param name="states"></param>
    /// <returns></returns>
    static HashSet<state> EpsilonClosure(LenvstnNFA nfa, HashSet<state> states)
    {
        if (states.Count == 0) return states;

        // Push all states onto a stack
        Stack<state> uncheckedStack = new Stack<state>(states);

        // Initialize EpsilonClosure(states) to states
        HashSet<state> epsilonClosure = states;

        while (uncheckedStack.Count != 0)
        {
            // Pop state t, the top element, off the stack
            state t = uncheckedStack.Pop();

            int i = 0;

            // For each state u with an edge from t to u labeled Epsilon
            foreach (input input in nfa.transTable[t])
            {
                if (input == (char)LenvstnNFA.Constants.EpsilonAny)
                {
                    state u = Array.IndexOf(nfa.transTable[t], input, i);

                    // If u is not already in epsilonClosure, add it and push it onto stack
                    if (!epsilonClosure.Contains(u))
                    {
                        epsilonClosure.Add(u);
                        uncheckedStack.Push(u);
                    }
                }

                i = i + 1;
            }
        }

        return epsilonClosure;
    }

  }
}