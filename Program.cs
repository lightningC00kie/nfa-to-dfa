using static System.Console;
using System.Collections.Generic;
using System.Text;
using static TransitionTable;
using System.Linq;

class Top
{
    static void Main(string[] args)
    {

        if (args.Length != 3)
        {
            // if type of conversion, input and output files are not specified as arguments
            // then write the usage of the program and exit.
            WriteUsage();
            return;
        }

        if (args[0] == "-R" || args[0] == "-N")
        {
            string inputFile = args[1];
            // assert that input file is of type json
            if (!inputFile.EndsWith(".json"))
            {
                WriteLine("Input file specified does not have a json extension");
                WriteUsage();
                return;
            }
            string outputFile = args[2];

            string jsonContent = File.ReadAllText(inputFile);
            if (args[0] == "-R")
            {
                // read regex from json file using JsonToRegex from JsonParser class
                string regex = JsonToRegex.Convert(jsonContent);
                // use the converter to convert to nfa
                NFA nfa = RegexToNFA.Convert(regex);
                // generate the transition table using the TransitionTable class
                GenerateNfaTransitionTable(nfa, outputFile);
            }

            else if (args[0] == "-N")
            {
                // read NFA from json file using JsonToNfa from JsonParser class
                NFA nfa = JsonToNfa.Convert(jsonContent);
                // use the converter to convert to DFA
                DFA dfa = NFAtoDFAConverter.Convert(nfa);
                // generate the transition table using the TransitionTable class
                GenerateDfaTransitionTable(dfa, outputFile);
            }
        }
        else
        {
            // if the conversion flag is not -R or -N then it is not recognized
            // and usage is written to the console.
            WriteUsage();
            return;
        }
    }

    private static void WriteUsage()
    {
        WriteLine("Usage: program.exe [-R|-N] input.json output");

        WriteLine("Options:");
        WriteLine("-R    Convert regex from input file to a NFA");
        WriteLine("-N    Convert NFA from input file to a DFA");
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

    // nextState used in Regex to NFA conversion only to simplify the conversion algorithm
    // otherwise normal transition dictionary is used as defined in NFA and DFA respectively
    public Dictionary<char, List<State>> nextState = new Dictionary<char, List<State>>();

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
    // innerStates holds the states of the NFA that the DFA state corresponds to
    // after conversion using the powerset algorithm
    public List<State>? innerStates;
    public DFAState(string stateName, bool isFinal, bool isStarting, List<State>? innerStates)
    : base(stateName, isFinal, isStarting)
    {
        this.isFinal = isFinal;
        this.isStarting = isStarting;
        this.innerStates = innerStates;
    }

    public static string generateStateName(List<State> innerStates)
    {
        if (innerStates.Count == 0)
        {
            return "-";
        }
        return "{" + String.Join(", ", innerStates) + "}";
    }

    // two DFAState instances are equal
    // if they contain the same elements
    // in their innerStates attributes
    public override bool Equals(object? obj)
    {
        if (!(obj is DFAState))
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
        return 0;
    }
}


class Automata
{
    public List<State> states;
    public List<char> alphabet;

    public Automata(List<State> states, List<char> alphabet)
    {
        this.states = states;
        this.alphabet = alphabet;
    }

    // return a list that contains only states where
    // isStarting is true
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

    // return a list that contains only states where
    // isFinal is true
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
    // dictionary representing transition function in the NFA
    // takes (current_state, alphabet_symbol) as key and produces next_state as value
    public Dictionary<(State, char), List<State>> transitions;

    public NFA(List<State> states, List<char> alphabet, Dictionary<(State, char), List<State>> transitions) :
    base(states, alphabet)
    {
        this.transitions = transitions;
    }

    // gets a list of next states from the transition dictionary
    // given a current_state and alphabet_symbol
    public List<State>? GetNextStates(State state, char symbol)
    {
        List<State>? nextStates;
        this.transitions.TryGetValue((state, symbol), out nextStates);
        return nextStates;
    }

    // adds a transition to the transitions dictionary of the NFA
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

    // adds a transition to the transitions dictionary of the NFA
    // this function is different from the AddTransition function defined above
    // because it takes a list as the second argument instead of just a state
    public void AddTransition(State fromState, List<State> toStates, char symbol)
    {
        if (transitions.ContainsKey((fromState, symbol)))
        {
            var existingStates = transitions[(fromState, symbol)];
            // if the key of (fromState, symbol) already exists in the dictionary
            // then we only add states to the list of already existing values if
            // its not already included there
            foreach (State s in toStates)
            {
                if (!existingStates.Contains(s))
                {
                    // we use the AddTransition function that takes as a second argument
                    // a single state instead of a list to add the transition to the dictionary
                    AddTransition(fromState, s, symbol);
                }
            }
        }
        else
        {
            // if the (fromState, symbol) key is not present then just add the key to the dictionary
            // with the given values
            transitions.Add((fromState, symbol), toStates);
        }
        transitions.Add((fromState, symbol), toStates);
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
    // dictionary representing the transition function for the DFA
    // takes a key in the form (current_state, alphabet_symbol) and returns
    // a next_state of type DFAState
    public Dictionary<(DFAState, char), DFAState> transitions;

    public DFA(List<State> states, List<char> alphabet, Dictionary<(DFAState, char), DFAState> transitions) :
    base(states, alphabet)
    {
        this.transitions = transitions;
    }

    // return the next state in the DFA given a current_state and an alphabet symbol
    // returns null if the transition doesn't exist
    public DFAState? GetNextState(DFAState state, char symbol)
    {
        DFAState? nextState;
        this.transitions.TryGetValue((state, symbol), out nextState);
        return nextState;
    }

    // add a transition to the dictionary
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
        var unprocessedStates = new Queue<DFAState>();
        var dfaStartingState = new DFAState(DFAState.generateStateName(nfaStartingState), false, true, nfaStartingState);

        /**
        we first get the power set of the nfa states to get dfa states.
        Then for each state in the dfa, we append the states where the nfa states in 
        it transition to and take their union.
        The start states of the DFA are the start states of the NFA.
        The final states of DFA consist of all the states that have at least one 
        final NFA state. We include them in the dfa final states.
        **/

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
                DFAState nextState = move(nfa, currentState, symbol);
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
        return dfa;
    }

    // move
    // returns a new DFAState with innerStates
    // corresponding the a list of all the states
    // reachable from the innerStates of the given DFAState in the nfa
    private static DFAState move(NFA nfa, DFAState state, char symbol)
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

        returnState = new DFAState(DFAState.generateStateName(nextStates), false, false, nextStates);
        return returnState;
    }
}
