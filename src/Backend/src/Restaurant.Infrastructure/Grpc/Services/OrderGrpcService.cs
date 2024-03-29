﻿using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Restaurant.Application.Abstractions;
using Restaurant.Application.Exceptions;
using Restaurant.Shared.OrderProto;

namespace Restaurant.Infrastructure.Grpc.Services
{
	[Authorize]
	internal class OrderGrpcService : Orders.OrdersBase
	{
		private readonly IServiceProvider _serviceProvider;

		public OrderGrpcService(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		public override async Task<AddOrderResponse> AddOrder(AddOrderRequest request, ServerCallContext context)
		{
			using var scope = _serviceProvider.CreateScope();
			var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();
			var dto = request.AsDto();
			await orderService.AddAsync(dto);
			return new AddOrderResponse { Id = dto.Id.ToString() };
		}

		public override async Task<Empty> UpdateOrder(AddOrderRequest request, ServerCallContext context)
		{
			using var scope = _serviceProvider.CreateScope();
			var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();
			await orderService.UpdateAsync(request.AsDto());
			return new Empty();
		}

		public override async Task<Empty> DeleteOrder(DeleteOrderRequest request, ServerCallContext context)
		{
			using var scope = _serviceProvider.CreateScope();
			var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();
			await orderService.DeleteAsync(request.Id.AsGuid());
			return new Empty();
		}

		public override async Task<Empty> DeleteOrderWithPositions(DeleteOrderWithPositionsRequest request, ServerCallContext context)
		{
			using var scope = _serviceProvider.CreateScope();
			var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();
			await orderService.DeleteWithPositionsAsync(request.Id.AsGuid());
			return new Empty();
		}

		public override async Task<OrderDetails> GetOrder(GetOrderRequest request, ServerCallContext context)
		{
			using var scope = _serviceProvider.CreateScope();
			var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();
			var id = request.Id.AsGuid();
			var order = await orderService.GetAsync(id);

			if (order is null)
			{
				throw new OrderNotFoundException(id);
			}

			var response = new OrderDetails
			{
				Id = request.Id,
				Email = order.Email,
				OrderNumber = order.OrderNumber,
				Price = order.Price.ToString(),
				Created = Timestamp.FromDateTime(order.Created),
				Note = order.Note ?? ""
			};
			response.Products.AddRange(order.Products.Select(p => new ProductSale
			{
				Id = p.Id.ToString(),
				Email = p.Email,
				ProductSaleState = p.ProductSaleState,
				EndPrice = p.EndPrice.ToString(),
				Product = new Product { Id = p.Product.Id.ToString(), Price = p.Product.Price.ToString(), ProductName = p.Product.ProductName, ProductKind = p.Product.ProductKind },
				Addition = p.Addition is not null ?
					new Addition { Id = p.Addition.Id.ToString(), AdditionName = p.Addition.AdditionName, AdditionKind = p.Addition.AdditionKind, Price = p.Addition.Price.ToString() }
					: null
			}));
			return response;
		}

		public override async Task<GetOrdersResponse> GetOrders(Empty request, ServerCallContext context)
		{
			using var scope = _serviceProvider.CreateScope();
			var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();
			var orders = await orderService.GetAllAsync();
			var response = new GetOrdersResponse();
			response.Orders.AddRange(orders.Select(o => new Order
			{
				Id = o.Id.ToString(),
				OrderNumber = o.OrderNumber,
				Email = o.Email,
				Price = o.Price.ToString(),
				Created = Timestamp.FromDateTime(o.Created),
				Note = o.Note ?? ""
			}));
			return response;
		}
	}
}
