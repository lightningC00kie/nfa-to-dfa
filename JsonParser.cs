using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using static System.Console;
public class JsonNfa
{
    public List<string>? states { get; set; }
    public List<string>? alphabet { get; set; }
    public List<List<string>>? transition_function { get; set; }
    public List<string>? start_states { get; set; }
    public List<string>? final_states { get; set; }
}

class JsonRegex {
    public string? regex { get; set; }
}

class JsonToRegex {
    public static string Convert(string json) {
        try {
            JsonRegex jsonRegex = JsonConvert.DeserializeObject<JsonRegex>(json)!;
            return jsonRegex.regex!;
        }
        catch (Exception e) {
            WriteLine("JSON format is incorrect: " + e.Message);
            System.Environment.Exit(1);
        }
        return "";
        
    }
}

class JsonToNfa {
    public static NFA Convert(string json) {
        NFA nfa = new NFA();

        // JsonNfa jsonNfa;
        try {
            JsonNfa jsonNfa = JsonConvert.DeserializeObject<JsonNfa>(json)!;
            List<char> alphabet = new List<char>();
            foreach (string symbol in jsonNfa.alphabet!) {
                alphabet.Add(char.Parse(symbol));
            }

            var findState = new Dictionary<string, State>();
            List<State> states = new List<State>();
            foreach (string s in jsonNfa.states!) {
                State state = new State();
                if (jsonNfa.start_states!.Contains(s)) {
                    state.isStarting = true;
                }
                if (jsonNfa.final_states!.Contains(s)) {
                    state.isFinal = true;
                }
                state.stateName = s;
                findState.Add(s, state);
                states.Add(state);
            }


            // NFATransitions NfaTransitions = new NFATransitions();
            foreach (var transition in jsonNfa.transition_function!) {
                State fromState = findState[transition[0]];
                char symbol = char.Parse(transition[1]);
                State toState = findState[transition[2]];
                nfa.AddTransition(fromState, toState, symbol);
            }
        }
        catch (Exception e) {
            WriteLine("JSON format is incorrect: " + e.Message);
            System.Environment.Exit(1);
        }

        
        return nfa;
    }
}