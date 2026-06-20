using Hotel_Booking_API.Domain.Interfaces;
using MediatR;

namespace Hotel_Booking_API.Application.Common.Behaviors
{
    /// <summary>
    /// Pipeline behavior that wraps each MediatR request in a database transaction.
    ///
    /// This ensures atomicity:
    /// - If the handler succeeds ? transaction is committed.
    /// - If the handler fails ? transaction is rolled back.
    ///
    /// Useful for commands that modify multiple tables or perform several operations that
    /// must be treated as one logical unit.
    /// </summary>
    public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IUnitOfWork _unitOfWork;

        public TransactionBehavior(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Wraps the handler execution inside a database transaction.
        /// </summary>
        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            // Skip transactions for queries
            if (typeof(TRequest).Name.EndsWith("Query"))
            {
                return await next();
            }

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var response = await next();

                await _unitOfWork.CommitTransactionAsync();

                return response;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
    }
}
