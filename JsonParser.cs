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
    // JsonToRegex
    // converts a given string formatted in JSON
    // to an instance of the JsonRegex class
    // if the string is not formatted correctly
    // then throw an exception and exit
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
    // JsonToNfa
    // converts a given string formatted in JSON
    // to an instance of the JsonNfa class
    public static NFA Convert(string json) {
        JsonNfa jsonNfa = JsonConvert.DeserializeObject<JsonNfa>(json)!;

        // build the a new NFA instance from the JsonNfa class instance
        List<char> alphabet = new List<char>();
        foreach (string symbol in jsonNfa.alphabet!) {
            // convert the alphabet to char
            alphabet.Add(char.Parse(symbol));
        }

        // build list of states of the NFA
        // findState is a dictionary that takes the state name as key
        // and returns the state itself as value
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

        // new NFA instance initialized with all the states and alphabet
        NFA nfa = new NFA(states, alphabet, new Dictionary<(State, char), List<State>>());

        // building the transitions for the NFA and adding it to the transitions dictionary
        // using the AddTransition function
        foreach (var transition in jsonNfa.transition_function!) {
            State fromState = findState[transition[0]];
            char symbol = char.Parse(transition[1]);
            State toState = findState[transition[2]];
            nfa.AddTransition(fromState, toState, symbol);
        }
        
        return nfa;
    }
}