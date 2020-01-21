# Welcome to Amusoft Analyzers

## CI

[![Build Status](https://taori.visualstudio.com/Amusoft.CodeAnalysis.Analyzers/_apis/build/status/taori.Amusoft.CodeAnalysis.Analyzers?branchName=master)](https://taori.visualstudio.com/Amusoft.CodeAnalysis.Analyzers/_build/latest?definitionId=4&branchName=master)

# Downloads

|Marketplace|Latest|
|---|---|
|[Download](https://marketplace.visualstudio.com/items?itemName=Amusoft.Amusoft-CodeAnalysis-Analyzers)|[Download](https://github.com/taori/Amusoft.CodeAnalysis.Analyzers/releases/latest)|

## Codefixes

### CSharp Diagnostics

|ID|Level|Description|
|---|---|---|
|CS0123|Error|Rewrites method signature or creates a signature which matches the requirement.|
|CS0161|Error|Inserts throws NotImplementedException();|
|CS0407|Error|Rewrites method signature to match the requirement.|
|CS1998|Error|Wraps expression in Task.FromResult()|
|CS4016|Error|Unwraps expression in Task.FromResult()|

### Project Diagnostics

|ID|Level|Description|
|---|---|---|
|ACA0001|Suggestion|Delegates implementation of an interface to a field/property with a collection type that also implements that interface.|
|ACA0002|Suggestion|Removes all comments within a class.|
|ACA0003|Suggestion|Removes all comments within a method.|
|ACA0004|Suggestion|Removes all comments within an array.|
|ACA0005|Suggestion|Removes all comments within a namespace.|
|ACA0006|Suggestion|Imports type as using static import.|

## References
- [Repository docs](https://github.com/taori/Amusoft.CodeAnalysis.Analyzers/tree/master/docs)
- [Features](https://github.com/taori/Amusoft.CodeAnalysis.Analyzers/tree/master/docs/FEATURES.md)
