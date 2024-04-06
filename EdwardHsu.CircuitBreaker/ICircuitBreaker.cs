namespace EdwardHsu.CircuitBreaker
{
    /// <summary>
    /// Interface for circuit breaker.
    /// </summary>
    public interface ICircuitBreaker
    {
        /// <summary>
        /// Gets the fuse.
        /// </summary>
        IFuse Fuse { get; }

        /// <summary>
        /// Gets the status.
        /// </summary>
        CircuitBreakerStatus Status { get; }

        /// <summary>
        /// Occurs when status changed.
        /// </summary>
        event Action<ICircuitBreaker> StatusChanged;

        /// <summary>
        /// Try to pass the circuit breaker.
        /// </summary>
        /// <param name="arguements">arguments</param>
        void Execute(object[] arguements);

        /// <summary>
        /// Turn on the circuit breaker.
        /// </summary>
        void On();

        /// <summary>
        /// Turn off the circuit breaker.
        /// </summary>
        void Off();
    }
}
