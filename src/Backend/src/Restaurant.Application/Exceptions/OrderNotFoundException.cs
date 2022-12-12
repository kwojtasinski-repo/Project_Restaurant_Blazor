﻿namespace Restaurant.Application.Exceptions
{
    public sealed class OrderNotFoundException : BusinessException
    {
        public Guid OrderId { get; }

        public OrderNotFoundException(Guid orderId) : base($"Order with id: '{orderId}' was not found")
        {
            OrderId = orderId;
        }
    }
}
