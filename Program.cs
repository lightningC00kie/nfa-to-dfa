using static System.Console;
using System.Collections.Generic;
using System.Text;
using static TransitionTable;
using System.Linq;

class Top {
    static void Main(string[] args) {
        // NFA nfa1 = NFAExample2();
        // DFA dfa1 = NFAtoDFAConverter.Convert(nfa1);
       
        // GenerateTransitionTable(dfa1, "transition_table.txt");

        string regExp = "a*a(bb)*";
        WriteLine(ExpressionTree.cleanRegex(regExp));
        // var tree = ExpressionTree.buildExpressionTree(regExp);
        // WriteLine(tree);
    }

    private static NFA NFAExample1() {
        List<char> alphabet = new List<char>();
        alphabet.Add('a');
        alphabet.Add('b');
        List<State> states = new List<State>();
        states.Add(new State("q0", false, true));
        states.Add(new State("q1", false, false));
        states.Add(new State("q2", true, false));

        NFATransitions transitions = new NFATransitions(new Dictionary<(State, char), List<State>>());
        List<State> fromq0 = new List<State>();
        fromq0.Add(states[0]);
        fromq0.Add(states[1]);
        transitions.addTransition(states[0], states.GetRange(0, 1), 'a');
        transitions.addTransition(states[0], fromq0, 'b');
        transitions.addTransition(states[1], states.GetRange(2, 1), 'b');

        NFA nfa = new NFA(states, alphabet, transitions);
        return nfa;
    }

    private static NFA NFAExample2() {
        List<char> alphabet = new List<char>();
        alphabet.Add('0');
        alphabet.Add('1');
        List<State> states = new List<State>();
        states.Add(new State("q0", false, true));
        states.Add(new State("q1", false, false));
        states.Add(new State("q2", true, false));

        NFATransitions transitions = new NFATransitions(new Dictionary<(State, char), List<State>>());
        transitions.addTransition(states[0], states.GetRange(0, 1), '0');
        List<State> fromq0 = new List<State>();
        fromq0.Add(states[1]);
        fromq0.Add(states[2]);
        transitions.addTransition(states[0], fromq0, '1');
        List<State> fromq1 = new List<State>();
        fromq1.Add(states[1]);
        fromq1.Add(states[2]);
        transitions.addTransition(states[1], fromq1, '0');
        transitions.addTransition(states[1], states.GetRange(2, 1), '1');
        List<State> fromq2 = new List<State>();
        fromq2.Add(states[0]);
        fromq2.Add(states[1]);
        transitions.addTransition(states[2], fromq2, '0');
        transitions.addTransition(states[2], states.GetRange(1, 1), '1');
        // WriteLine(transitions.getNextStates(states[0], '1').Count);
        NFA nfa = new NFA(states, alphabet, transitions);
        return nfa;
    }
}

class State {
    public string stateName;
    public bool isFinal;
    public bool isStarting;

    public State(string stateName, bool isFinal, bool isStarting) {
        this.stateName = stateName;
        this.isFinal = isFinal;
        this.isStarting = isStarting;
    }

    public override string ToString()
    {
        return this.stateName;
    }
}

class DFAState : State{
    public List<State>? innerStates;
    public DFAState(string stateName, bool isFinal, bool isStarting, List<State>? innerStates) 
    : base(stateName, isFinal, isStarting)
    {
        this.isFinal = isFinal;
        this.isStarting = isStarting;
        this.innerStates = innerStates;
    }

    public static string generateStateName(List<State> innerStates) {
        if (innerStates.Count == 0) {
            return "-";
        }
        return "{" + String.Join(", ", innerStates) + "}";
    }

    public override bool Equals(object? obj)
    {
        if (!(obj is DFAState)) {
            return false;
        }

        DFAState state = (DFAState) obj;
        if (this.innerStates == null) {
            return state.innerStates == null;
        }

        if (state.innerStates == null) {
            return false;
        }
        
        return state.innerStates.ToHashSet().SetEquals(this.innerStates.ToHashSet());
    }
}

class DFATransitions {
    private Dictionary<(DFAState, char), DFAState> transitions;

    public DFATransitions(Dictionary<(DFAState, char), DFAState> transitions) {
        this.transitions = transitions;
    }

    public DFAState? getNextState(DFAState state, char symbol) {
        DFAState? nextState;
        this.transitions.TryGetValue((state, symbol), out nextState);
        return nextState;
    }

    public void addTransition(DFAState fromState, DFAState toState, char symbol) {
        this.transitions.Add((fromState, symbol), toState);
    }

    public bool transitionExists(DFAState formState, char symbol) {
        return transitions.TryGetValue((formState, symbol), out _);
    }
}

class NFATransitions {
    public Dictionary<(State, char), List<State>> transitions;

    public NFATransitions(Dictionary<(State, char), List<State>> transitions) {
        this.transitions = transitions;
    }

    // takes a state and an input symbol and returns the states that are reached on the given 
    // input symbol from the given state
    public List<State>? getNextStates(State state, char symbol) {
        List<State>? nextStates;
        this.transitions.TryGetValue((state, symbol), out nextStates);
        WriteLine("From state " + state.ToString() + " on symbol " + symbol);
        WriteLine(string.Join(", ", nextStates));
        return nextStates;
    }

    public void addTransition(State fromState, List<State> toStates, char symbol) {
        transitions.Add((fromState, symbol), toStates);
    }
}

class Automata {
    public List<State> states;
    public List<char> alphabet;

    public Automata(List<State> states, List<char> alphabet) {
        this.states = states;
        this.alphabet = alphabet;
    }

    public List<State> getStartingStates() {
        var startingStates = new List<State>();
        foreach (State s in this.states) {
            if (s.isStarting) {
                startingStates.Add(s);
            }
        }
        return startingStates;
    }

    public List<State> getFinalStates() {
        var finalStates = new List<State>();
        foreach (State s in this.states) {
            if (s.isFinal) {
                finalStates.Add(s);
            }
        }
        return finalStates;
    }
    
}

class NFA : Automata {
    public NFATransitions transitions;

    public NFA(List<State> states, List<char> alphabet, NFATransitions transitions) : 
    base(states, alphabet)
    {
        this.transitions = transitions;
    }

    
}

class DFA : Automata {
    public DFATransitions transitions;

    public DFA(List<State> states, List<char> alphabet, DFATransitions transitions) : 
    base(states, alphabet)
    {
        this.transitions = transitions;
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("all states:\n");
  
        sb.Append(String.Join(", ", this.states));

        return sb.ToString();
    }
}


class NFAtoDFAConverter {
    public static DFA Convert(NFA nfa) {
        var dfaStates = new List<State>();
        var dfaAlphabet = nfa.alphabet;
        var nfaStartingState = nfa.getStartingStates();
        var dfaTransitions = new DFATransitions(new Dictionary<(DFAState, char), DFAState>());
        var unprocessedStates = new Queue<DFAState>();
        var dfaStartingState = new DFAState(DFAState.generateStateName(nfaStartingState), false, true, nfaStartingState);
        unprocessedStates.Enqueue(dfaStartingState);
        dfaStates.Add(dfaStartingState);
        while (unprocessedStates.Count >  0) {
            
            var currentState = unprocessedStates.Dequeue();
            if (currentState.innerStates == null || currentState.innerStates.Count == 0) {
                continue;
            }

            foreach (char symbol in dfaAlphabet) {
                DFAState nextState = move(nfa, currentState, symbol);
                // WriteLine(nextState);
                bool exists = false;

                foreach (DFAState state in dfaStates) {
                    if (state.Equals(nextState)) {
                        exists = true;
                        break;
                    }
                }

                if (nextState.innerStates == null) {
                    continue;
                }

                if (!exists) {
                    unprocessedStates.Enqueue(nextState);
                    var nfaFinalStates = nfa.getFinalStates();
                    foreach (State finalState in nfaFinalStates) {
                        
                        if (nextState.innerStates.Contains(finalState)) {
                            nextState.isFinal = true;
                        }
                    }

                    dfaStates.Add(nextState);
                }

                dfaTransitions.addTransition(currentState, nextState, symbol);
            }
        }

        DFA dfa = new DFA(dfaStates, dfaAlphabet, dfaTransitions);
        return dfa;
    }

    private static DFAState move(NFA nfa, DFAState state, char symbol) { 
        DFAState returnState;
        List<State>? nextStates = new List<State>();
        foreach (State s in state.innerStates!) {
            // WriteLine(s)?W;
            var currentNextStates = nfa.transitions.getNextStates(s, symbol);
            // WriteLine("From state " + s.ToString() + " on symbol " + symbol);
            // WriteLine(nextStates.Count);
            // WriteLine(String.Join(", ", currentNextStates));
            if (currentNextStates == null) {
                continue;
            }
            
            foreach (State ss in currentNextStates) {
                if (!nextStates.Contains(ss)) {
                    nextStates.Add(ss);
                }
            }
            // nextStates.AddRange(currentNextStates);

           
        }

        if (nextStates == null) {
            return new DFAState("", false, false, null);
        }

        returnState = new DFAState(DFAState.generateStateName(nextStates), false, false, nextStates);
        return returnState;
    }
}
