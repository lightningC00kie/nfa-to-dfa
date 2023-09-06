using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Text;
using static System.Console;


class RegexToNFA {
    static char[] nonSymbols = new char[]{'(', ')', '+', '.', '*'};

    public static NFA nfa;

    public static ExpressionTree buildExpressionTree(string regExp) {
        Stack<ExpressionTree> stk = new Stack<ExpressionTree>();
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

    public static string addConcatenation(string regex) {
        List<char> res = new List<char>();
        for (int i = 0; i < regex.Length - 1; i++) {
            res.Add(regex[i]);
            if (!nonSymbols.Contains(regex[i])) {
                if (!nonSymbols.Contains(regex[i+1]) ||
                regex[i+1] == '(') {
                    res.Add('.');
                }
            }

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

    private static bool compPrecedence(char op1, char op2) {
        List<char> ops = new List<char>
        {
            '+',
            '.',
            '*'
        };
        return ops.IndexOf(op1) > ops.IndexOf(op2);
    }

    public static string computePostfix(string regex) {
        Stack<char> stk = new Stack<char>();
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
            else if (stk.Count == 0 || stk.Peek() == '(' || compPrecedence(c, stk.Last())) {
                stk.Push(c);
            }
            else {
                while (stk.Count > 0 && stk.Peek() != '(' && !compPrecedence(c, stk.Last())) {
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

    public static string cleanRegex(string regex) {
        string reg = addConcatenation(regex);
        string regg = computePostfix(reg);
        return regg;
    }


    public static (State, State) nfaConcat(ExpressionTree tree) {
        var leftNfa = copmuteRegex(tree.left!);
        var rightNfa = copmuteRegex(tree.right!);

        leftNfa.Item2.nextState['$'] = getStateList(rightNfa.Item1);

        return (leftNfa.Item1, rightNfa.Item2);
    }

    private static (State, State) evalSymbol(ExpressionTree tree) {
        State start = new State("", false, false);
        State end = new State("", false, false);

        start.nextState[(char) tree.value!] = getStateList(end);
        return (start, end);
    }

    private static (State, State) nfaUnion(ExpressionTree tree) {
        State start = new State("", false, false);
        State end = new State("", false, false);

        var firstNFA = copmuteRegex(tree.left!);
        var secondNFA = copmuteRegex(tree.right!);

        start.nextState['$'] = getStateList(firstNFA.Item1, secondNFA.Item1);
        firstNFA.Item2.nextState['$'] = getStateList(end);
        secondNFA.Item2.nextState['$'] = getStateList(end);

        return (start, end);
    }

    private static (State, State) nfaKleene(ExpressionTree tree) {
        State start = new State("", false, false);
        State end = new State("", false, false);

        var starredNFA = copmuteRegex(tree.left!);
        start.nextState['$'] = getStateList(starredNFA.Item1, end);
        starredNFA.Item2.nextState['$'] = getStateList(starredNFA.Item1, end);
        return (start, end);
    }

    private static List<State> getStateList(params State[] states) {
        return states.ToList();
    }

    public static (State, State) copmuteRegex(ExpressionTree tree) {
        if (tree.charType == RegexCharType.Concat) {
            return nfaConcat(tree);
        }
        else if (tree.charType == RegexCharType.Union) {
            return nfaUnion(tree);
        }
        else if (tree.charType == RegexCharType.Kleene) {
            return nfaKleene(tree);
        }
        else {
            return evalSymbol(tree);
        }
    }

    public static void makeTransitions(State state, Dictionary<State, int> findName) {
        // WriteLine("here");
        if (nfa.states.Contains(state)) {
            return;
        }

        nfa.states.Add(state);

        foreach (var transition in state.nextState) {
            char symbol = transition.Key;
            if (!nfa.alphabet.Contains(symbol)) {
                nfa.alphabet.Add(symbol);
            }

            foreach (State s in state.nextState[symbol]) {
                if (!findName.ContainsKey(s)) {
                    findName.Add(s, findName.Values.Max() + 1);
                    string stateName = "Q" + findName[s];
                    s.stateName = stateName;
                }
                nfa.transitions.addTransition(state, s, symbol);
            }

            foreach (State s in state.nextState[symbol]) {
                makeTransitions(s, findName);
            }
        }
        
    }

    public static void setFinalStates() {
        foreach (State s in nfa.states) {
            bool isFinal = true;
            foreach (var transition in nfa.transitions.transitions) {
                if (transition.Key.Item1.Equals(s) && 
                (transition.Value.Count > 1 || !transition.Value[0].Equals(s))) {
                    isFinal = false;
                    break;
                }
            }
            s.isFinal = isFinal;
        }
    }
    public static void arrangeNFA((State, State) s) {
        s.Item1.isStarting = true;
        nfa = new NFA(new List<State>(), new List<char>(), new NFATransitions());
        var findName = new Dictionary<State, int>();
        findName.Add(s.Item1, 1);
        s.Item1.stateName = "Q1";
        makeTransitions(s.Item1, findName);
        setFinalStates();
    }


    public static void Convert(string regex) {
        string cr = cleanRegex(regex);
        var tree = buildExpressionTree(cr);
        var fa = copmuteRegex(tree);
        arrangeNFA(fa);
        WriteLine(nfa);
    }
}

class ExpressionTree {
    public RegexCharType charType;
    public char? value;
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
        StringBuilder sb = new StringBuilder();

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
