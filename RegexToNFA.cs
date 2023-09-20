using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Text;
using static System.Console;


class RegexToNFA {
    static readonly char[] nonSymbols = new char[]{'(', ')', '+', '.', '*'};

    public static NFA? nfa;

    public static ExpressionTree BuildExpressionTree(string regExp) {
        Stack<ExpressionTree> stk = new();
        foreach (char c in regExp) {
            if (c == '+') {
                var n = new ExpressionTree(RegexCharType.Union)
                {
                    right = stk.Count > 0 ? stk.Pop() : null,
                    left = stk.Count > 0 ? stk.Pop() : null
                };
                stk.Push(n);
            }
            else if (c == '.') {
                var n = new ExpressionTree(RegexCharType.Concat)
                {
                    right = stk.Count > 0 ? stk.Pop() : null,
                    left = stk.Count > 0 ? stk.Pop() : null
                };
                stk.Push(n);
                
            }
            else if (c == '*') {
                var n = new ExpressionTree(RegexCharType.Kleene)
                {
                    left = stk.Count > 0 ? stk.Pop() : null
                };
                stk.Push(n);
            }
            else if (c == '(' || c == ')') {
                continue;
            }
            else {
                stk.Push(new ExpressionTree(RegexCharType.Symbol, c));
            }
        }
        return stk.Peek();
    }

    // add . where necessary for preprocessing before tree building
    public static string AddConcatenation(string regex) {
        List<char> res = new();
        for (int i = 0; i < regex.Length - 1; i++) {
            res.Add(regex[i]);
            if (!nonSymbols.Contains(regex[i])) {
                if (!nonSymbols.Contains(regex[i+1]) ||
                regex[i+1] == '(') {
                    res.Add('.');
                }
            }

            // concat is added to all places where . might be necessary 
            if (regex[i] == ')' && regex[i + 1] == '(') {
                res.Add('.');
            }
            if (regex[i] == '*' && regex[i + 1] == '(') {
                res.Add('.');
            }
            if (regex[i] == '*' && !nonSymbols.Contains(regex[i + 1])) {
                res.Add('.');
            }
            if (regex[i] == ')' && !nonSymbols.Contains(regex[i + 1])) {
                res.Add('.');
            }
        }
        res.Add(regex[^1]);
        return new string(res.ToArray());
    }

    // check if op1 has precedence over op2
    private static bool CompPrecedence(char op1, char op2) {
        List<char> ops = new()
        {
            '+',
            '.',
            '*'
        };
        return ops.IndexOf(op1) > ops.IndexOf(op2);
    }

    public static string ComputePostfix(string regex) {
        Stack<char> stk = new();
        string res = "";

        foreach (char c in regex) {
            if (!nonSymbols.Contains(c) || c == '*') {
                res += c;
            }
            else if (c == ')') {
                while (stk.Count > 0 && stk.Peek() != '(') {
                    res += stk.Pop();
                }

                if (stk.Count > 0) {
                    stk.Pop();
                }
            }
            else if (c == '(') {
                stk.Push(c);
            }
            else if (stk.Count == 0 || stk.Peek() == '(' || CompPrecedence(c, stk.Last())) {
                stk.Push(c);
            }
            else {
                while (stk.Count > 0 && stk.Peek() != '(' && !CompPrecedence(c, stk.Last())) {
                    res += stk.Pop();
                }
                stk.Push(c);
            }
        }

        while (stk.Count > 0) {
            res += stk.Pop();
        }

        return res;

    }

    public static string CleanRegex(string regex) {
        string reg = AddConcatenation(regex);
        string regg = ComputePostfix(reg);
        return regg;
    }

    // returns start and end states of nfa after concatenation operation
    // is applied on the given expression tree
    public static (State, State) NfaConcat(ExpressionTree tree) {
        var leftNfa = CopmuteRegex(tree.left!);
        var rightNfa = CopmuteRegex(tree.right!);

        leftNfa.Item2.nextState['$'] = GetStateList(rightNfa.Item1);

        return (leftNfa.Item1, rightNfa.Item2);
    }

    // build symbol nfa
    private static (State, State) EvalSymbol(ExpressionTree tree) {
        State start = new State("", false, false);
        State end = new State("", false, false);

        start.nextState[(char) tree.value!] = GetStateList(end);
        return (start, end);
    }

    // build union nfa
    private static (State, State) NfaUnion(ExpressionTree tree) {
        State start = new State("", false, false);
        State end = new State("", false, false);

        var firstNFA = CopmuteRegex(tree.left!);
        var secondNFA = CopmuteRegex(tree.right!);

        start.nextState['$'] = GetStateList(firstNFA.Item1, secondNFA.Item1);
        firstNFA.Item2.nextState['$'] = GetStateList(end);
        secondNFA.Item2.nextState['$'] = GetStateList(end);

        return (start, end);
    }

    // build kleene star nfa
    private static (State, State) NfaKleene(ExpressionTree tree) {
        State start = new State("", false, false);
        State end = new State("", false, false);

        var starredNFA = CopmuteRegex(tree.left!);
        start.nextState['$'] = GetStateList(starredNFA.Item1, end);
        starredNFA.Item2.nextState['$'] = GetStateList(starredNFA.Item1, end);
        return (start, end);
    }

    // takes an arbitrary number of states as arguments
    // and returns them in a List
    private static List<State> GetStateList(params State[] states) {
        return states.ToList();
    }

    public static (State, State) CopmuteRegex(ExpressionTree tree) {
        if (tree.charType == RegexCharType.Concat) {
            return NfaConcat(tree);
        }
        else if (tree.charType == RegexCharType.Union) {
            return NfaUnion(tree);
        }
        else if (tree.charType == RegexCharType.Kleene) {
            return NfaKleene(tree);
        }
        else {
            return EvalSymbol(tree);
        }
    }

    // recursively explore the state diagram and add transitions accordingly
    public static void MakeTransitions(State state, Dictionary<State, int> findName) {
        if (nfa!.states.Contains(state)) {
            return;
        }

        // state is not in the nfa, so add it to the nfa
        nfa.states.Add(state);

        foreach (var transition in state.nextState) {
            char symbol = transition.Key;
            // if the nfa alphabet does not contain the given symbol
            // then add it to the alphabet
            if (!nfa.alphabet.Contains(symbol)) {
                nfa.alphabet.Add(symbol);
            }

            // if a state that has not been seen before is found
            // then form the name of the state by incrementing the
            // largest state name found by 1 and adding Q to the beginning
            foreach (State s in state.nextState[symbol]) {
                if (!findName.ContainsKey(s)) {
                    findName.Add(s, findName.Values.Max() + 1);
                    string stateName = "Q" + findName[s];
                    s.stateName = stateName;
                }
                // add the transition to the nfa
                nfa.AddTransition(state, s, symbol);
            }

            // recursive call to add transition for the next state
            // in the state diagram
            foreach (State s in state.nextState[symbol]) {
                MakeTransitions(s, findName);
            }
        }
        
    }

    // a state is final if it is a state with no outgoing transitions except
    // to itself
    public static void SetFinalStates() {
        foreach (State s in nfa!.states) {
            bool isFinal = true;
            foreach (var transition in nfa.transitions) {
                if (transition.Key.Item1.Equals(s) && 
                (transition.Value.Count > 1 || !transition.Value[0].Equals(s))) {
                    isFinal = false;
                    break;
                }
            }
            s.isFinal = isFinal;
        }
    }

    // set starting and final states of NFA and name the states.
    public static void ArrangeNFA((State, State) s) {
        s.Item1.isStarting = true;
        nfa = new NFA(new List<State>(), new List<char>(), new Dictionary<(State, char), List<State>>());
        var findName = new Dictionary<State, int>();
        findName.Add(s.Item1, 1);
        s.Item1.stateName = "Q1";
        MakeTransitions(s.Item1, findName);
        SetFinalStates();
    }


    public static NFA Convert(string regex) {
        // converts regex into form suitable for tree building
        string cr = CleanRegex(regex);
        // builds tree from cleaned regex
        var tree = BuildExpressionTree(cr);
        // 
        var fa = CopmuteRegex(tree);
        ArrangeNFA(fa);
        return nfa!;
    }
}

class ExpressionTree {
    // charType can be symbol, or operator (Kleene, Concat, Union)
    public RegexCharType charType;
    // value holds the symbol if the node is not RegexCharType
    public char? value; 
    // every tree has a right and left child
    public ExpressionTree? right;
    public ExpressionTree? left;

    public ExpressionTree(RegexCharType charType, char? value = null) {
        this.charType = charType;
        this.value = value;
        this.right = null;
        this.left = null;
    }

    public override string ToString()
    {
        StringBuilder sb = new();

        if (this.value != null) {
            return "Tree(" + this.charType + ", " + this.value + ", left = " + this.left + 
            ", right = " + this.right + ")";
        }
        
        else {
            return "Tree(" + this.charType + ", left = " + this.left + 
            ", right = " + this.right + ")";
        }
    }


    
}
