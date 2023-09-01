using static System.Console;
using System.Collections.Generic;
using System.Text;

class intop {
    static void Main(string[] args) {

        var nfaStates = new HashSet<int> { 0, 1, 2, 3 };
        var nfaAlphabet = new List<string> { "a", "b" };
        var nfaStartingStates = new HashSet<int> { 0 };
        var nfaFinalStates = new HashSet<int> { 2, 3 };
        var nfaintransitions = new Dictionary<(int, string), List<int>> {
            { (0, "a"), new List<int> { 1 } },
            { (0, "b"), new List<int> { 0 } },
            { (1, "a"), new List<int> { 1, 2 } },
            { (1, "b"), new List<int> { 2 } },
            { (2, "a"), new List<int> { 3 } },
            { (3, "b"), new List<int> { 3 } }
        };

        var nfa = new NFA(nfaStates, nfaAlphabet, nfaStartingStates, nfaFinalStates, nfaintransitions);
        WriteLine("Starting conversion...");
        var dfa = NFAtoDFAConverter.Convert(nfa);
        WriteLine("Finished Convesion!");
        WriteLine(dfa);
    }
}

class Automata {
    public HashSet<int> states;
    public List<string> alphabet;
    public HashSet<int> finalStates;

    public Automata(HashSet<int> states, List<string> alphabet, HashSet<int> finalStates) {
        this.states = states;
        this.alphabet = alphabet;
        this.finalStates = finalStates;

    }
}

// class DFAState {

// }

class NFA : Automata {
    public HashSet<int> startingStates;
    public Dictionary<(int, string), List<int>> transitions;

    public NFA(HashSet<int> states, List<string> alphabet, HashSet<int> startingStates, 
    HashSet<int> finalStates, Dictionary<(int, string), List<int>> transitions) : 
    base(states, alphabet, finalStates)
    {
    
        this.startingStates = startingStates;
        this.transitions = transitions;
    }

    public List<int>? getNextStates(int state, string input) {
        List<int>? nextStates;
        this.transitions.TryGetValue((state, input), out nextStates);
        return nextStates;
        // return this.transitions[(state, input)];
    }
}

// class DFAState

class DFA : Automata {
    public int startingState;
    public Dictionary<(int, string), int> transitions;

    public DFA(HashSet<int> states, List<string> alphabet, int startingState, 
    HashSet<int> finalStates, Dictionary<(int, string), int> transitions) : 
    base(states, alphabet, finalStates)
    {
        this.startingState = startingState;
        this.transitions = transitions;
    }

    public int getNextState(int state, string input) {
        return this.transitions[(state, input)];
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("all states:\n");
        foreach (int s in this.states) {
            sb.Append(s + " ");
        }

        sb.Append("\n");
        sb.Append("starting state:");
        sb.Append(this.startingState + "\n");

        sb.Append("final states:\n");
        foreach (int s in this.finalStates) {
            sb.Append(s + " ");
        }

        sb.Append("\n");

        sb.Append("transitions:\n");
        foreach(var transition in this.transitions) {
            sb.Append("(" + transition.Key.Item1 + ", " + transition.Key.Item2 + ") -> " + transition.Value + "\n");
        }

        return sb.ToString();
    }
}


class NFAtoDFAConverter {
    public static DFA Convert(NFA nfa) {
        var dfaStates = new HashSet<HashSet<int>>();
        var dfaAlphabet = nfa.alphabet;
        var dfaStartingState = nfa.startingStates;
        var dfaFinalStates = new HashSet<HashSet<int>>();
        var dfaTransitions = new Dictionary<(HashSet<int>, string), HashSet<int>>();
        var unprocessedStates = new Queue<HashSet<int>>();

        unprocessedStates.Enqueue(dfaStartingState);
        // dfaStates.Add(dfaStartingState);
        while (unprocessedStates.Count >  0) {
            var currentState = unprocessedStates.Dequeue();
            foreach (string symbol in dfaAlphabet) {
                var nextStates = move(nfa, currentState, symbol);

                bool exists = false;

                foreach (HashSet<int> stateSet in dfaStates) {
                    if (stateSet.SetEquals(nextStates)) {
                        exists = true;
                        break;
                    }
                }

                if (!exists) {
                    unprocessedStates.Enqueue(nextStates);

                    HashSet<int> intersection = new HashSet<int>(nextStates);
                    intersection.IntersectWith(nfa.finalStates);
                    if (intersection.Count > 0) {
                        dfaFinalStates.Add(nextStates);
                    }

                    dfaStates.Add(nextStates);
                }


                dfaTransitions[(currentState, symbol)] = nextStates;
            }
        }
        
        var dfa = renameStates(dfaStates, dfaStartingState, dfaAlphabet, dfaFinalStates, 
        dfaTransitions);
        return dfa;
    }

    private static bool statesAreEqual(HashSet<int> state1, HashSet<int> state2) {
        return state1.SetEquals(state2);
    }

    private static bool containsState(HashSet<HashSet<int>> states, HashSet<int> state) {
        foreach (HashSet<int> currentState in states) {
            if (statesAreEqual(currentState, state)) {
                return true;
            }
        }
        return false;
    }

    private static HashSet<int> move(NFA nfa, HashSet<int> states, string symbol) {
        var nextStates = new HashSet<int>();
        foreach (int state in states) {
            var currentNextStates = nfa.getNextStates(state, symbol);
            if (currentNextStates == null) {
                continue;
            }
            foreach (int nextState in currentNextStates) {
                nextStates.Add(nextState);
            }
        }
        return nextStates;
    }

    private static string stringifyState(HashSet<int> state) {
        StringBuilder sb = new StringBuilder();
        sb.Append("{");
        foreach (int s in state) {
            sb.Append(s + ", ");
        }
        sb.Append("}");
        return sb.ToString();
    }

    private static DFA renameStates(HashSet<HashSet<int>> states, HashSet<int> startingState, 
    List<string> alphabet, HashSet<HashSet<int>> finalStates, 
    Dictionary<(HashSet<int>, string), HashSet<int>> transitions) 
    {
        var dfaStates = new HashSet<int>();
        int dfaStartingState = 0;
        var dfaFinalStates = new HashSet<int>();
        var dfaTransitions = new Dictionary<(int, string),int>();
        var stateToInt = new Dictionary<HashSet<int>, int>();

        stateToInt.Add(startingState, 0);

        int stateCounter = 1;

        foreach (HashSet<int> stateSet in states) {
            dfaStates.Add(stateCounter);
            if (finalStates.Contains(stateSet)) {
                dfaFinalStates.Add(stateCounter);
            }
            stateToInt.Add(stateSet, stateCounter++);
        }

        foreach (var transition in transitions) {
            int toState;
            bool stateExists = stateToInt.TryGetValue(transition.Value, out toState);

            if (!stateExists) {
                continue;                
            }

            dfaTransitions.Add((stateToInt[transition.Key.Item1], transition.Key.Item2), 
            toState);
        }

        return new DFA(dfaStates, alphabet, dfaStartingState, dfaFinalStates, dfaTransitions);
    }
}
