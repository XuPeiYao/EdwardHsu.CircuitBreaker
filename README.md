EdwardHsu.CircuitBreaker
=====
[![Unit Test](https://github.com/XuPeiYao/EdwardHsu.CircuitBreaker/actions/workflows/unit-test.yaml/badge.svg)](https://github.com/XuPeiYao/EdwardHsu.CircuitBreaker/actions/workflows/unit-test.yaml)
[![codecov](https://codecov.io/gh/XuPeiYao/EdwardHsu.CircuitBreaker/graph/badge.svg?token=IY5K44AVW1)](https://codecov.io/gh/XuPeiYao/EdwardHsu.CircuitBreaker)
[![NuGet Version](https://img.shields.io/nuget/v/EdwardHsu.CircuitBreaker.svg)](https://www.nuget.org/packages/EdwardHsu.CircuitBreaker/)
[![NuGet Download](https://img.shields.io/nuget/dt/EdwardHsu.CircuitBreaker.svg)](https://www.nuget.org/packages/EdwardHsu.CircuitBreaker/)
[![Github license](https://img.shields.io/github/license/XuPeiYao/EdwardHsu.CircuitBreaker.svg)](https://www.nuget.org/packages/EdwardHsu.CircuitBreaker/)

CircuitBreaker is a .NET library that provides a circuit breaker pattern implementation for methods. It is designed to prevent a method from being executed too frequently or too intensively, which can lead to resource exhaustion or other issues.

## Features
* Circuit Breaker: The CircuitBreaker class represents the circuit breaker and provides methods to control its state (on, tripped off, and off).
* Fuse: The IFuse interface defines the triggering conditions of the circuit breaker. The library provides two implementations:
    * ExecutionLimitFuse: Trips the circuit breaker when the number of executions exceeds a specified limit.
    * TimeSlidingWindowLimitFuse: Trips the circuit breaker when the number of executions within a specified time window exceeds a limit.
* Injection: The CircuitBreakerHookInjector class allows you to inject the circuit breaker into existing methods without modifying the code.


## Installation

```bash
#main package
dotnet add package EdwardHsu.CircuitBreaker

#hook injector package
dotnet add package EdwardHsu.CircuitBreaker.HookInjector
```

## Usage

1. Create a Circuit Breaker.

```csharp
var fuse = new ExecutionLimitFuse(5);
var breaker = new CircuitBreaker(fuse);
```

2. Execute a Method through the Circuit Breaker.

```csharp
try
{
    breaker.Execute(new object[] { arg1, arg2 }); // Arguments can be passed to Fuse, which can determine whether to trigger based on these parameters.
}
catch (InvalidOperationException)
{
    // Circuit breaker is tripped off
}
```

3. Control the Circuit Breaker.

```csharp
breaker.On(); // Turn on the circuit breaker
breaker.Off(); // Turn off the circuit breaker
```

4. Inject the Circuit Breaker into an Existing Method.

```csharp
breaker.Inject(() => myInstance.MyMethod(arg));
breaker.Inject(() => MyClass.MyStaticMethod(arg));
```


## Contributing
Contributions are welcome! If you find any issues or have suggestions for improvements, please feel free to open an issue or submit a pull request.

## License
This project is licensed under the MIT License.