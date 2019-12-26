# Welcome to Amusoft Analyzers

## CI

[![Build Status](https://taori.visualstudio.com/Amusoft.CodeAnalysis.Analyzers/_apis/build/status/taori.Amusoft.CodeAnalysis.Analyzers?branchName=master)](https://taori.visualstudio.com/Amusoft.CodeAnalysis.Analyzers/_build/latest?definitionId=4&branchName=master)

# Downloads

|Marketplace|Latest|
|---|---|
|[![Build Status](https://taori.vsrm.visualstudio.com/_apis/public/Release/badge/8682a196-f5ae-4f86-9d50-b067e3280f9d/1/1)](https://marketplace.visualstudio.com/items?itemName=Amusoft.Amusoft-CodeAnalysis-Analyzers)|[![Build Status](https://taori.visualstudio.com/Amusoft.CodeAnalysis.Analyzers/_apis/build/status/taori.Amusoft.CodeAnalysis.Analyzers?branchName=master)](https://github.com/taori/Amusoft.CodeAnalysis.Analyzers/releases/latest)|
### Marketplace



### latest

## Features

### Forward implementation to fields/properties

##### Description
Compositional programming styles sometimes require you to forward method calls to items of a field/property which is IEnumerable. This code fix makes this process a breeze

##### Requirements
- Your type has to implement the interface of a property to be a candidate for this code fix
- Your type members' type must implement `IEnumerable<T>`
- Your method must be empty or `throw new NotImplementedException`

## References
- [Repository docs](https://github.com/taori/Amusoft.CodeAnalysis.Analyzers/docs)
