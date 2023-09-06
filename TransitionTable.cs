using static System.Console;
using System;
using System.Collections.Generic;
using System.Linq;

class TransitionTable {
    public static void GenerateDfaTransitionTable(DFA dfa, string fileName)
    {
        try
        {
            using (StreamWriter sw = new StreamWriter(fileName))
            {

                int longestStateName = dfa.states.Max(s => s.stateName.Length);
                // Write the header row with input symbols
                sw.Write("|" + addPadding("State", longestStateName));

                foreach (char symbol in dfa.alphabet)
                {
                    sw.Write("|" + addPadding(symbol.ToString(), longestStateName));
                }
                sw.WriteLine();
                // Write the transition table
                foreach (DFAState state in dfa.states)
                {
                    if (state.innerStates!.Count == 0) {
                        continue;
                    }
                    sw.Write("|" + addPadding(state.ToString(), longestStateName));
                    foreach (char symbol in dfa.alphabet)
                    {
                        if (dfa.TransitionExists(state, symbol))
                        {
                            sw.Write("|" + addPadding(dfa
                            .GetNextState(state, symbol)!
                            .ToString(), longestStateName));
                        }
                        else
                        {
                            sw.Write(addPadding("-\t", longestStateName));
                        }
                    }
                    sw.WriteLine();

                }
                sw.WriteLine();
                sw.WriteLine("Starting States:");
                sw.WriteLine(string.Join(", ", dfa.GetStartingStates()));
                sw.WriteLine("Final States:");
                sw.WriteLine(string.Join(", ", dfa.GetFinalStates()));
            }

            Console.WriteLine("Transition table written to " + fileName);
        }
        catch (Exception e)
        {
            Console.WriteLine("An error occurred: " + e.Message);
        }
    }

    public static void GenerateNfaTransitionTable(NFA nfa, string fileName)
    {
        try
        {
            using (StreamWriter sw = new StreamWriter(fileName))
            {
                List<State> longestList = nfa.transitions.Values.
                OrderByDescending(list => list.Count).FirstOrDefault()!;

                // int longestStateList = nfa.transitions.transitions.Max(t => t.Value.Count);
                int longestStateName = stringifyStateList(longestList).Length;

                // WriteLine(longestStateList);
                // Write the header row with input symbols
                sw.Write("|" + addPadding("State", longestStateName));
                foreach (char symbol in nfa.alphabet)
                {
                    sw.Write("|" + addPadding(symbol.ToString(), longestStateName));
                }
                sw.WriteLine();
                // Write the transition table
                foreach (State state in nfa.states)
                {
                    sw.Write("|" + addPadding(state.ToString(), longestStateName));
                    foreach (char symbol in nfa.alphabet)
                    {
                        if (nfa.TransitionExists(state, symbol))
                        {
                            var nextStates = nfa.GetNextStates(state, symbol)!;

                            sw.Write("|" + addPadding(string.Join(", ", stringifyStateList(nextStates)), longestStateName));
                        }
                        else
                        {
                            sw.Write("|" + addPadding("-", longestStateName));
                        }
                    }
                    sw.WriteLine();
                }
                
                sw.WriteLine();
                sw.WriteLine("Starting States:");
                sw.WriteLine(string.Join(", ", nfa.GetStartingStates()));
                sw.WriteLine("Final States:");
                sw.WriteLine(string.Join(", ", nfa.GetFinalStates()));
            }
            Console.WriteLine("Transition table written to " + fileName);
        }
        catch (Exception e)
        {
            Console.WriteLine("An error occurred: " + e.Message);
        }
    }

    private static string stringifyStateList(List<State> states) {
        return "{" + string.Join(", ", states) + "}";
    }

    private static string addPadding(string text, int length) {
        if (text.Length > length) {
            return text;
        }

        int totalPadding = length - text.Length;
        string paddedText = text + new string(' ', totalPadding + 1);
        return paddedText;
    }
}