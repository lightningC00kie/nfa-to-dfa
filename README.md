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