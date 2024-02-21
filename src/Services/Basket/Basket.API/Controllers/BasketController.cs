using AutoMapper;
using Basket.API.Entities;
using Basket.API.GrpcServices;
using Basket.API.Repositories;
using EventBus.Messages.Events;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Basket.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class BasketController: ControllerBase
    {
        private readonly IBasketRepository _repository;
        private readonly DiscountGrpcServices _discountGrpcServices;
        private readonly IMapper _mapper;
        private readonly IPublishEndpoint _publishEndpoint;

        public BasketController(IBasketRepository repository, DiscountGrpcServices discountGrpcServices, IMapper mapper, IPublishEndpoint publishEndpoint)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _discountGrpcServices = discountGrpcServices ?? throw new ArgumentNullException(nameof(discountGrpcServices));
            _mapper = mapper;
            _publishEndpoint = publishEndpoint;
        }

        [HttpGet("{userName}", Name = "GetBasket")]
        [ProducesResponseType(typeof(ShoppingCart), (int) HttpStatusCode.OK)]
        public async Task<ActionResult<ShoppingCart>> GetBasket(string userName)
        {
            var basket = await _repository.GetBasket(userName);

            return Ok(basket ?? new ShoppingCart(userName));
            //Here if the basket is empty, we are creating new Basket with username. 
        }

        [HttpPost]
        [ProducesResponseType(typeof(ShoppingCart), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<ShoppingCart>> UpdateBasket([FromBody]ShoppingCart basket)
        {
            foreach (var item in basket.Items) {
                var coupon = await _discountGrpcServices.GetDiscount(item.ProductName);
                item.Price -= coupon.Amount; 
            }

            return Ok(await _repository.UpdateBasket(basket));
        }

        [HttpDelete]
        [ProducesResponseType(typeof(void),(int)HttpStatusCode.OK)]
        public async Task<ActionResult> DeleteBasket(string userName)
        {
            await _repository.DeleteBasket(userName);
            return Ok();
        }


        [Route("[action]")]
        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.Accepted)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Checkout([FromBody] BasketCheckout basketCheckout)
        {
            //get existing basket with totalPrice
            var basket = await _repository.GetBasket(basketCheckout.UserName);
            if(basket == null) {
                return BadRequest();
            }

            //Create basketChecoutevent -- Set TotalPrice on basketCheckout eventMsg
            var eventMessage = _mapper.Map<BasketCheckoutEvent>(basketCheckout);
            eventMessage.TotalPrice = basket.TotalPrice;
            //Send checkout event to rabbitMQ
            await _publishEndpoint.Publish(eventMessage);

            //Remove the basket
            await _repository.DeleteBasket(basketCheckout.UserName);

            return Accepted();
        }

    }
}
