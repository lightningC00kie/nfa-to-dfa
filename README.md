## NFA to DFA Converter

### Introduction

This repository proposes the development of a C# program designed to convert non-deterministic finite automata (NFA) to deterministic finite automata (DFA). The program also includes functionality for transforming arbitrary regular expressions into NFAs. The project's primary goal is to automate the conversion process, enabling efficient analysis and manipulation of automata in fields such as formal languages, compiler design, and pattern matching.

### Objectives

The project's main objectives are outlined below:

- **Design and Implementation**: Create a C# program that can convert a given NFA into a DFA.
- **Regular Expression Conversion**: Develop the capability to convert arbitrary regular expressions into NFAs.
- **Clear Representation**: Present resulting DFA and NFA models using transition representations to facilitate understanding.
- **Efficiency and Scalability**: Optimize conversion algorithms to ensure efficiency and scalability for larger automata.
- **Input Validation**: Ensure the program handles diverse edge cases and validates input for accuracy.

### Scope

This program focuses on the following key features:

- **NFA to DFA Conversion**: The program takes a user-provided NFA as input and produces the corresponding DFA. It handles NFA states, transitions, and final states to derive the DFA.

- **Regular Expression to NFA Conversion**: The program accepts a regular expression and converts it into an NFA representation.

- **Transition-Based Representation**: Both resulting DFA and NFA models are presented in terms of transitions. This representation empowers users to comprehend automata behavior and facilitates in-depth analysis.

- **Textual Input/Output**: Input and output are exclusively text-based. 

- **Command Line Usage**: The program reads two file names as command line arguments. The first file contains the input, allowing for easy editing or tweaking. The program generates an output file specified by the second command line argument.

### Usage

To use the program, follow these steps:

1. Compile the C# program using your preferred compiler.
2. Run the compiled program from the command line with the following arguments:
   
   ```
   program.exe input.txt output.txt
   ```

   - `input.txt`: Contains the input automaton description in a specific format (see user documentation).
   - `output.txt`: Will hold the resulting DFA or NFA description.