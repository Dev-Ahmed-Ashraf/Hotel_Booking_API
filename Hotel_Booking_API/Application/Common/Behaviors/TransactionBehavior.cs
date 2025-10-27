using MediatR;
using Hotel_Booking_API.Domain.Interfaces;

namespace Hotel_Booking_API.Application.Common.Behaviors
{
    public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IUnitOfWork _unitOfWork;

        public TransactionBehavior(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (request is not IRequest<TResponse>)
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
