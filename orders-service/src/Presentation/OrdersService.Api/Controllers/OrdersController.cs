using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrdersService.Api.ViewModels;
using OrdersService.Application.Commands;
using OrdersService.Application.Entities;
using OrdersService.Application.Exceptions;
using OrdersService.Application.Queries;
using OrdersService.Domain.Events.Base;
using OrdersService.Domain.Models;

namespace OrdersService.Api.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly ISender _sender;

    public OrdersController(ISender sender, IMapper mapper)
    {
        _sender = sender;
        _mapper = mapper;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<OrderVM>>> Get(string? fuzzySearchString = null)
    {
        IEnumerable<OrderEntity> orderEntities = await _sender.Send(new OrdersRetrievalQuery { FuzzySearchString = fuzzySearchString });
        var orderVMs = _mapper.Map<IEnumerable<OrderVM>>(orderEntities);

        return Ok(orderVMs);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OrderVM>> Add([FromBody] OrderCreationVM orderCreationVM)
    {
        var orderCreationCommand = _mapper.Map<OrderCreationCommand>(orderCreationVM);

        Order order;
        try
        {
            order = await _sender.Send(orderCreationCommand);
        }
        catch (AlreadyExistsException<Order, OrderEvent> alreadyExistsException)
        {
            return BadRequest($"Order with id '{alreadyExistsException.Aggregate.Id}' already exists.");
        }

        var orderVM = _mapper.Map<OrderVM>(order);
        return Ok(orderVM);
    }

    /// <summary>
    /// Confirm
    /// </summary>
    [HttpPost("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderVM>> Confirm(Guid id)
    {
        var orderConfirmationCommand = new OrderConfirmationCommand{ OrderId = id };

        Order order;
        try
        {
            order = await _sender.Send(orderConfirmationCommand);
        }
        catch (IsNotFoundException isNotFoundException)
        {
            return NotFound($"Order with id '{isNotFoundException.AggregateId}' does not exist.");
        }

        var orderVM = _mapper.Map<OrderVM>(order);
        return Ok(orderVM);
    }

    [HttpPost("{id:guid}/subscriptions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderVM>> SubscribeToUpdates([FromRoute] Guid id)
    {
        var orderSubscriptionAdditionCommand = new OrderSubscriptionAdditionCommand { OrderId = id };

        Order order;
        try
        {
            order = await _sender.Send(orderSubscriptionAdditionCommand);
        }
        catch (IsNotFoundException isNotFoundException)
        {
            return NotFound($"Order with id '{isNotFoundException.AggregateId}' does not exist.");
        }

        var orderVM = _mapper.Map<OrderVM>(order);
        return Ok(orderVM);
    }

    [HttpDelete("{id:guid}/subscriptions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderVM>> UnsubscribeFromUpdates([FromRoute] Guid id)
    {
        var orderSubscriptionRemovalCommand = new OrderSubscriptionRemovalCommand { OrderId = id };

        Order order;
        try
        {
            order = await _sender.Send(orderSubscriptionRemovalCommand);
        }
        catch (IsNotFoundException isNotFoundException)
        {
            return NotFound($"Order with id '{isNotFoundException.AggregateId}' does not exist.");
        }

        var orderVM = _mapper.Map<OrderVM>(order);
        return Ok(orderVM);
    }
}