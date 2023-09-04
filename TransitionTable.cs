using static System.Console;

class TransitionTable {
    public static void GenerateTransitionTable(DFA dfa, string fileName)
    {
        try
        {
            using (StreamWriter sw = new StreamWriter(fileName))
            {

                int longestStateName = dfa.states.Max(s => s.stateName.Length);
                string rightShift = "  ";
                // Write the header row with input symbols
                sw.Write(rightShift + "|" + addPadding("State", longestStateName));

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
                    sw.Write(
                    (state.isStarting ? "->" : "") + (state.isFinal ? "<-" : "") + 
                    (!state.isFinal && !state.isStarting ? rightShift : "") + "|" + 
                    addPadding(state.ToString(), longestStateName)
                    );
                    foreach (char symbol in dfa.alphabet)
                    {
                        if (dfa.transitions.transitionExists(state, symbol))
                        {
                            sw.Write("|" + addPadding(dfa.transitions
                            .getNextState(state, symbol)!
                            .ToString(), longestStateName));
                        }
                        else
                        {
                            sw.Write(addPadding("-\t", longestStateName));
                        }
                    }
                    sw.WriteLine();
                }
            }

            Console.WriteLine("Transition table written to " + fileName);
        }
        catch (Exception e)
        {
            Console.WriteLine("An error occurred: " + e.Message);
        }
    }

    private static string addPadding(string text, int length) {
        int totalPadding = length - text.Length;
        string paddedText = text + new string(' ', totalPadding + 1);
        return paddedText;
    }
}