using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Text;
using static System.Console;


class RegexToNFA {

}

class ExpressionTree {
    static char[] nonSymbols = new char[]{'(', ')', '+', '.', '*'};
    RegexCharType charType;
    char? value;
    ExpressionTree? right;
    ExpressionTree? left;
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
            sb.Append("(" + this.value);
        }
        
        else {
            sb.Append("(" + this.charType.ToString());
        }

        sb.Append("(left:" + (this.left == null ? "null" : this.left.ToString()) + ")");
        sb.Append("(right:" + (this.right == null ? null : this.right.ToString()) + ")");
        return sb.ToString();

    }


    public static ExpressionTree buildExpressionTree(string regExp) {
        Stack<ExpressionTree> stk = new Stack<ExpressionTree>();
        foreach (char c in regExp) {
            WriteLine(c);
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
                    res.Append('.');
                }
            }

            if (regex[i] == ')' && regex[i + 1] == '(') {
                res.Add('.');
            }
            if (regex[i] == '*' && regex[i + 1] == '(') {
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
        List<char> ops = new List<char>();
        ops.Add('+');
        ops.Add('.');
        ops.Add('*');
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
                while (stk.Count > 0 && stk.Last() != '(') {
                    res += stk.Pop();
                }
                stk.Pop();
            }
            else if (c == '(') {
                stk.Push(c);
            }
            else {
                while (stk.Count > 0 && stk.Last() != '(' && !compPrecedence(c, stk.Last())) {
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

    // public static NFA Convert(string regex) {
    //     Stack<NFA> stk = new Stack<NFA>();
    //     Stack<char> opStk = new Stack<char>();

    //     foreach (char c in regex) {
    //         if (!nonSymbols.Contains(c)) {
    //             NFA nfa = createNFA(c);
    //             stk.Push(nfa);
    //         }
    //         else if (c == '*') {
    //             NFA nfa = stk.Pop();
    //             NFA newNFA = createKleeneClosure(nfa);
    //             stk.Push(newNFA);
    //         }
    //         else if (c == '+') {
    //             opStk.Push(c);
    //         }
    //         else if (c == '.') {
    //             while (opStk.Count > 0 && opStk.Peek() == '.') {
    //                 opStk.Pop();
    //                 NFA nfa2 = stk.Pop();
    //                 NFA nfa1 = stk.Pop();
    //                 NFA newNFA = createConcatenation(nfa1, nfa2);
    //                 stk.Push(newNFA);
    //             }
    //             opStk.Push(c);
    //         }
    //     }
    // }
}

public enum Operator {
    Union, 
    Concat,
    Kleene
}