# EdwardHsu.CircuitBreaker

CircuitBreaker is a .NET library that provides a circuit breaker pattern implementation for methods. It is designed to prevent a method from being executed too frequently or too intensively, which can lead to resource exhaustion or other issues.

## Features

- Supports both instance and static methods
- Provides different circuit breaker strategies:
    - `TimeSlidingWindowCountFuse`: Limits the number of executions within a sliding time window at any given time.
    - `TimeWindowCountFuse`: Limits the number of executions within a fixed time window.
- Allows manual tripping and resetting of the circuit breaker

## Usage

1. Create an instance of the desired circuit breaker strategy (e.g., `TimeSlidingWindowCountFuse` or `TimeWindowCountFuse`).
2. Create an instance of `CircuitBreaker`, passing the fuse instance and a lambda expression that represents the method to be monitored.
3. Use the `Status` property to check the current status of the circuit breaker.
4. Call the `On()` method to reset the circuit breaker and allow the monitored method to be executed.
5. Call the `Off()` method to manually trip the circuit breaker and prevent the monitored method from being executed.

### Example

```csharp
// Create a fuse instance, e.g., TimeSlidingWindowCountFuse
// This fuse allows up to 10 calls per second
var fuse = new TimeSlidingWindowCountFuse(10, TimeSpan.FromSeconds(1));

// Create a test instance
var testInstance = new TestModel();

// Create a circuit breaker for the Method1
var breaker = new CircuitBreaker(fuse, () => testInstance.Method1(""));

for (int i = 0; i < 100; i++)
{
    testInstance.Method1(""); // This will throw an exception after 10 calls per second
}

// ... do something else ...

// Trip the circuit breaker manually
breaker.Off();

// Reset the circuit breaker
breaker.On();
```

## Contributing
Contributions are welcome! If you find any issues or have suggestions for improvements, please open an issue or submit a pull request.

## License
This project is licensed under the MIT License.