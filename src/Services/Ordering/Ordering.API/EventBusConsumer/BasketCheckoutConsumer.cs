using AutoMapper;
using EventBus.Messages.Events;
using MassTransit;
using MediatR;
using Ordering.Application.Features.Orders.Commands.CheckoutOrder;

namespace Ordering.API.EventBusConsumer
{
    public class BasketCheckoutConsumer : IConsumer<BasketCheckoutEvent>
    {
        private readonly IMapper _mapper;
        private readonly IMediator _mediator; //Remember to use MediatR package and not MassTransit.Mediator
        private readonly ILogger<BasketCheckoutConsumer> _logger;

        public BasketCheckoutConsumer(IMapper mapper, IMediator mediator, ILogger<BasketCheckoutConsumer> logger)
        {
            _mapper = mapper;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<BasketCheckoutEvent> context)
        {
            try
            {                
                //Mapper not working for some reason
                //var command = _mapper.Map<CheckoutOrderCommand>(context.Message);
                var command = new CheckoutOrderCommand { 
                    AddressLine = context.Message.AddressLine,
                    CardName = context.Message.CardName,
                    CardNumber = context.Message.CardNumber,
                    Country = context.Message.Country,
                    CVV = context.Message.CVV,
                    EmailAddress = context.Message.EmailAddress,
                    Expiration = context.Message.Expiration,
                    FirstName = context.Message.FirstName,
                    LastName = context.Message.LastName,
                    PaymentMethod = context.Message.PaymentMethod,
                    State = context.Message.State,
                    TotalPrice = context.Message.TotalPrice,
                    UserName = context.Message.UserName,
                    ZipCode = context.Message.ZipCode
                };
                var result = await _mediator.Send(command);
                _logger.LogInformation($"BasketCheckoutEvent consumer successfully. Created order id : {result}.");
            }
            catch (Exception ex)
            {

                throw ex;
            }
            
            

            
        }
    }
}
