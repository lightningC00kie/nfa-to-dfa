using static System.Console;
using System.Text;
using static TransitionTable;

class Top
{
    static void Main(string[] args)
    {

        if (args.Length != 3)
        {
            WriteUsage();
            return;
        }

        if (args[0] == "-r" || args[0] == "-n")
        {
            string inputFile = args[1];
            if (!inputFile.EndsWith(".json"))
            {
                WriteLine("Input file specified does not have a json extension");
                WriteUsage();
                return;
            }
            string outputFile = args[2];

            string jsonContent = File.ReadAllText(inputFile);
            if (args[0] == "-r")
            {
                string regex = JsonToRegex.Convert(jsonContent);
                NFA nfa = RegexToNFA.Convert(regex);
                GenerateNfaTransitionTable(nfa, outputFile);
            }

            else if (args[0] == "-n")
            {
                NFA nfa = JsonToNfa.Convert(jsonContent);
                DFA dfa = NFAtoDFAConverter.Convert(nfa);
                GenerateDfaTransitionTable(dfa, outputFile);
            }
        }
        else
        {
            WriteUsage();
            return;
        }
    }

    private static void WriteUsage()
    {
        WriteLine("Usage: program.exe [-r|-n] input.json output");

        WriteLine("Options:");
        WriteLine("-r    Convert regex from input file to a NFA");
        WriteLine("-n    Convert NFA from input file to a DFA");
        WriteLine("input.json   Path to the input JSON file to be processed.");
        WriteLine("output   Path to the output text file to store the results.");

        WriteLine("Example:");
        WriteLine("program.exe -r input.json output.txt");
    }
}

class State
{
    public string stateName;
    public bool isFinal;
    public bool isStarting;

    public Dictionary<char, List<State>> nextState = new();

    public State(string stateName = "", bool isFinal = false, bool isStarting = false)
    {
        this.stateName = stateName;
        this.isFinal = isFinal;
        this.isStarting = isStarting;
    }

    public override string ToString()
    {
        return this.stateName;
    }
}

class DFAState : State
{
    public List<State>? innerStates;
    public DFAState(string stateName, bool isFinal, bool isStarting, List<State>? innerStates)
    : base(stateName, isFinal, isStarting)
    {
        this.isFinal = isFinal;
        this.isStarting = isStarting;
        this.innerStates = innerStates;
    }

    public static string GenerateStateName(List<State> innerStates)
    {
        if (innerStates.Count == 0)
        {
            return "-";
        }
        return "{" + String.Join(", ", innerStates) + "}";
    }

    public override bool Equals(object? obj)
    {
        if (obj is not DFAState)
        {
            return false;
        }

        DFAState state = (DFAState)obj;
        if (this.innerStates == null)
        {
            return state.innerStates == null;
        }

        if (state.innerStates == null)
        {
            return false;
        }

        return state.innerStates.ToHashSet().SetEquals(this.innerStates.ToHashSet());
    }

    public override int GetHashCode()
     {
         return GetHashCode();
     }


}


class Automata
{
    public List<State> states;
    public List<char> alphabet;

    public Automata(List<State> states = default!, List<char> alphabet = default!)
    {
        this.states = states;
        this.alphabet = alphabet;
    }

    public List<State> GetStartingStates()
    {
        var startingStates = new List<State>();
        foreach (State s in this.states)
        {
            if (s.isStarting)
            {
                startingStates.Add(s);
            }
        }
        return startingStates;
    }

    public List<State> GetFinalStates()
    {
        var finalStates = new List<State>();
        foreach (State s in this.states)
        {
            if (s.isFinal)
            {
                finalStates.Add(s);
            }
        }
        return finalStates;
    }
}

class NFA : Automata
{
    public Dictionary<(State, char), List<State>> transitions = new();

    public NFA(List<State> states=default!, List<char> alphabet=default!, 
    Dictionary<(State, char), List<State>> transitions = default!) :
    base(states, alphabet)
    {
        this.transitions = transitions;
    }

    public List<State>? GetNextStates(State state, char symbol)
    {
        List<State>? nextStates;
        this.transitions.TryGetValue((state, symbol), out nextStates);
        return nextStates;
    }

    public void AddTransition(State fromState, List<State> toStates, char symbol)
    {
        if (transitions.ContainsKey((fromState, symbol)))
        {
            var existingStates = transitions[(fromState, symbol)];
            foreach (State s in toStates)
            {
                if (!existingStates.Contains(s))
                {
                    AddTransition(fromState, s, symbol);
                }
            }
        }
        else
        {
            transitions.Add((fromState, symbol), toStates);
        }
        transitions.Add((fromState, symbol), toStates);
    }

    public void AddTransition(State fromState, State toState, char symbol)
    {
        if (transitions.ContainsKey((fromState, symbol)))
        {
            transitions[(fromState, symbol)].Add(toState);
        }
        else
        {
            var stateList = new List<State>() {
                toState
            };
            transitions.Add((fromState, symbol), stateList);
        }
    }

    public bool TransitionExists(State formState, char symbol)
    {
        return transitions.TryGetValue((formState, symbol), out _);
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("States:\n");
        sb.Append(string.Join(", ", this.states));

        sb.Append("Transitions\n");
        foreach (var transition in this.transitions)
        {
            var fromState = transition.Key.Item1;
            sb.Append((fromState.isStarting ? "->" : "") + (fromState.isFinal ? "<-" : "") + fromState + "-(" + transition.Key.Item2 + ")->{" + string.Join(", ", transition.Value) + "}\n");
        }
        sb.Append("Final States:\n");
        foreach (State s in this.GetFinalStates())
        {
            sb.Append(s);
        }
        return sb.ToString();
    }

}

class DFA : Automata
{
    public Dictionary<(DFAState, char), DFAState> transitions;

    public DFA(List<State> states, List<char> alphabet, Dictionary<(DFAState, char), DFAState> transitions) :
    base(states, alphabet)
    {
        this.transitions = transitions;
    }

    public DFAState? GetNextState(DFAState state, char symbol)
    {
        DFAState? nextState;
        this.transitions.TryGetValue((state, symbol), out nextState);
        return nextState;
    }

    public void AddTransition(DFAState fromState, DFAState toState, char symbol)
    {
        this.transitions.Add((fromState, symbol), toState);
    }

    public bool TransitionExists(DFAState formState, char symbol)
    {
        return transitions.TryGetValue((formState, symbol), out _);
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("all states:\n");

        sb.Append(String.Join(", ", this.states));

        return sb.ToString();
    }
}


class NFAtoDFAConverter
{
    public static DFA Convert(NFA nfa)
    {
        DFA dfa = new DFA(new List<State>(), new List<char>(), new Dictionary<(DFAState, char), DFAState>());

        var dfaStates = new List<State>();
        var dfaAlphabet = nfa.alphabet;
        var nfaStartingState = nfa.GetStartingStates();
        // var dfaTransitions = new DFATransitions(new Dictionary<(DFAState, char), DFAState>());
        var unprocessedStates = new Queue<DFAState>();
        var dfaStartingState = new DFAState(DFAState.GenerateStateName(nfaStartingState), false, true, nfaStartingState);
        unprocessedStates.Enqueue(dfaStartingState);
        dfaStates.Add(dfaStartingState);
        while (unprocessedStates.Count > 0)
        {

            var currentState = unprocessedStates.Dequeue();
            if (currentState.innerStates == null || currentState.innerStates.Count == 0)
            {
                continue;
            }

            foreach (char symbol in dfaAlphabet)
            {
                DFAState nextState = Move(nfa, currentState, symbol);
                // WriteLine(nextState);
                bool exists = false;

                foreach (DFAState state in dfaStates)
                {
                    if (state.Equals(nextState))
                    {
                        exists = true;
                        break;
                    }
                }

                if (nextState.innerStates == null)
                {
                    continue;
                }

                if (!exists)
                {
                    unprocessedStates.Enqueue(nextState);
                    var nfaFinalStates = nfa.GetFinalStates();
                    foreach (State finalState in nfaFinalStates)
                    {

                        if (nextState.innerStates.Contains(finalState))
                        {
                            nextState.isFinal = true;
                        }
                    }

                    dfaStates.Add(nextState);
                }

                dfa.AddTransition(currentState, nextState, symbol);
            }
        }
        dfa.states = dfaStates;
        dfa.alphabet = dfaAlphabet;
        // DFA dfa = new DFA(dfaStates, dfaAlphabet, dfaTransitions);
        return dfa;
    }

    private static DFAState Move(NFA nfa, DFAState state, char symbol)
    {
        DFAState returnState;
        List<State>? nextStates = new List<State>();
        foreach (State s in state.innerStates!)
        {
            var currentNextStates = nfa.GetNextStates(s, symbol);
            if (currentNextStates == null)
            {
                continue;
            }

            foreach (State ss in currentNextStates)
            {
                if (!nextStates.Contains(ss))
                {
                    nextStates.Add(ss);
                }
            }
        }

        if (nextStates == null)
        {
            return new DFAState("", false, false, null);
        }

        returnState = new DFAState(DFAState.GenerateStateName(nextStates), false, false, nextStates);
        return returnState;
    }
}
