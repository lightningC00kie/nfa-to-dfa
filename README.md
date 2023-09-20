## NFA to DFA and Regex to NFA Converter

### Introduction

This repository proposes the development of a C# program designed to convert non-deterministic finite automata (NFA) to deterministic finite automata (DFA). The program also includes functionality for transforming arbitrary regular expressions into NFAs.

### Objectives

The project's main objectives are outlined below:

- **Design and Implementation**: Create a C# program that can convert a given NFA into a DFA.
- **Regular Expression Conversion**: Develop the capability to convert arbitrary regular expressions into NFAs.
- **Clear Representation**: Present resulting DFA and NFA models using transition representations to facilitate understanding.

### Scope

This program focuses on the following key features:

- **NFA to DFA Conversion**: The program takes a user-provided NFA as input and produces the corresponding DFA. It handles NFA states, transitions, and final states to derive the DFA.

- **Regular Expression to NFA Conversion**: The program accepts a regular expression and converts it into an NFA representation.

- **Transition-Based Representation**: Both resulting DFA and NFA models are presented in terms of transitions.

- **JSON Input**: Input is represented in terms of JSON.

- **Textual Output**: The output is printed into the specified output file in the form of a transition
table along with the starting and final states of the automaton.

## Input JSON Format

To convert from a NFA to DFA, you should provide input data in the following JSON format:

```json
{
  "states": [
    "Q1",
    "Q2",
    "Q3"
  ],
  "alphabet": [
    "$",
    "a",
    "b"
  ],
  "transition_function": [
    ["Q1", "$", "Q2"],
    ["Q1", "$", "Q3"]
  ],
  "start_states": [],
  "final_states": []
}
```

To convert from a regular expression to a NFA, you should provide input data in the following JSON format:
```json
{
  "regex": "a+b(ab)*"
}
```

This JSON structure represents the components of a finite automaton:

   - "states": An array of strings representing the states of the automaton.
   - "alphabet": An array of strings representing the alphabet symbols.
   - "transition_function": An array of arrays where each inner array consists of three strings: the current state, the input symbol, and the next state (transition function).
   - "start_states": An array of strings representing the initial states.
   - "final_states": An array of strings representing the final (accepting) states.

### Program Usage

**Usage:** `dotnet run [-R|-N] input.json output`

**Options:**
- `-R`    Convert regex from input file to a NFA
- `-N`    Convert NFA from input file to a DFA
- `input.json`   Path to the input JSON file to be processed.
- `output`   Path to the output text file to store the results.

**Example:**
```bash
dotnet run -R input.json output.txt
```
#
# Developer Documentation
## The program contains 4 main parts:
- **NFA to DFA converter**
- **Regex to NFA converter**
- **JSON reading/parsing**
- **Writing transition tables**

## NFA to DFA converter
- NFA is read from JSON file.
- DFA and NFA classes inherit from parent Automata class.
- Attributes of each of NFA and DFA come from the formal definition of the corresponding automata from automata theory.
- I use the powerset algorithm in order to convert any given NFA to a DFA. 
- DFAState class inherits from state. This class is used to add the additional innerStates attribute present only in DFA states.
- DFAs have an innerStates attribute that holds the states that it represents in the NFA.
- Automata class is a parent class for NFA and DFA classes. Automata class contain alphabet and a list of states, which is common to both NFA and DFA.
- NFA class contains transitions field represented as a dictionary, taking a pair of current state and character as key and next states as list of values.
- DFA class contains a similar transitions field, except it the value is just one state instead of a list of states.
- AddTransition function adds new transition to the dictionary. This function takes 3 arguments, the current state, and either a list of next states or a singular next state, followed by an alphabet symbol.
- Converter implements the powerset algorithm to build the DFA from the given NFA.
- The resulting DFA is then written to the specified output file using the GenerateDfaTransitionTable function is the TransitionTable class.

## Regex to NFA converter
- Regex is read from a JSON file.
- CleanRegex function adds concatenations where necessary and then converts the given regex into postfix notation in order to be able to build an expression tree with it.
- The cleaned regex is then passed to BuildExpressionTree, which returns an ExpressionTree object.
- ExpressionTree contains charType attribute, which can be anyone of the types defined in the RegexCharType enum.
- ExpressionTree contains value attribute for when the charType is a symbol, meaning it is a symbol of the language alphabet.
- Each ExpressionTree can have right and left children that are also of type ExpressionTree.
- After building the expression tree from the cleaned regex, we pass the expression tree to the ComputeRegex function, which calls the necessary functions that build small NFAs recursively by exploring the tree.
- The resulting automaton is then arranged using the ArrangeNFA function, which gives all the states names, adds transitions to the automaton, and sets the starting and final states.
- The resulting NFA is then written to the specified output file using the GenerateNfaTransitionTable function is the TransitionTable class.





