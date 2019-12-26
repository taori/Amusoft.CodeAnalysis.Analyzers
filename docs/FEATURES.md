# Features


## Forward implementation to fields/properties



### Description


Compositional programming styles sometimes require you to forward method calls to items of a field/property which is IEnumerable. This code fix makes this process a breeze



### Requirements


- Your type has to implement the interface of a property to be a candidate for this code fix

- Your type members' type must implement `IEnumerable<T>`

- Your method must be empty or `throw new NotImplementedException`

