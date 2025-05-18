﻿using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.ApplicationCore.Entities.BasketAggregate;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.ApplicationCore.Specifications;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using System.Reflection;

namespace Microsoft.eShopWeb.ApplicationCore.Services;

public class OrderService : IOrderService
{
    private readonly IRepository<Order> _orderRepository;
    private readonly IUriComposer _uriComposer;
    private readonly IRepository<Basket> _basketRepository;
    private readonly IRepository<CatalogItem> _itemRepository;

    public OrderService(IRepository<Basket> basketRepository,
        IRepository<CatalogItem> itemRepository,
        IRepository<Order> orderRepository,
        IUriComposer uriComposer)
    {
        _orderRepository = orderRepository;
        _uriComposer = uriComposer;
        _basketRepository = basketRepository;
        _itemRepository = itemRepository;
    }

    public async Task CreateOrderAsync(int basketId, Address shippingAddress)
    {
        var basketSpec = new BasketWithItemsSpecification(basketId);
        var basket = await _basketRepository.FirstOrDefaultAsync(basketSpec);

        Guard.Against.Null(basket, nameof(basket));
        Guard.Against.EmptyBasketOnCheckout(basket.Items);

        var catalogItemsSpecification = new CatalogItemsSpecification(basket.Items.Select(item => item.CatalogItemId).ToArray());
        var catalogItems = await _itemRepository.ListAsync(catalogItemsSpecification);

        var items = basket.Items.Select(basketItem =>
        {
            var catalogItem = catalogItems.First(c => c.Id == basketItem.CatalogItemId);
            var itemOrdered = new CatalogItemOrdered(catalogItem.Id, catalogItem.Name, _uriComposer.ComposePicUri(catalogItem.PictureUri));
            var orderItem = new OrderItem(itemOrdered, basketItem.UnitPrice, basketItem.Quantity);
            return orderItem;
        }).ToList();

        var order = new Order(basket.BuyerId, shippingAddress, items);

        await _orderRepository.AddAsync(order);
       // await SendToDeliveryQueue(order);
    }

    async Task SendToDelivery(Order order)
    {
        var client = new HttpClient();
        var json = JsonSerializer.Serialize(order);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync("https://orderitemsreserver320250422090444.azurewebsites.net/api/Function2?", content);
        //"code=DyyPxQErUqF-nm7iYPbxWuPL6Bcs2EWecp63wq3P_BJZAzFu7NOGsQ==", content);

        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine("Data sent successfully!");
        }
        else
        {
            Console.WriteLine($"Error: {response.StatusCode}");
        }
    }

    private const string connectionString = "Endpoint=sb://shoporders7.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=zcJJ+xPjdnS5fAIv6vP3yqhvEptBazF4a+ASbPUe0BU=";
    private const string queueName = "eshop";

    async Task SendToDeliveryQueue(Order order)
    {
        // Create a Service Bus client
        await using var client = new ServiceBusClient(connectionString);

        // Create a sender for the queue
        ServiceBusSender sender = client.CreateSender(queueName);

        try
        {
            // Create a message to send
            string json = JsonSerializer.Serialize(order);
            ServiceBusMessage message = new ServiceBusMessage(json);
            message.ContentType = "application/json";

            // Send the message
            await sender.SendMessageAsync(message);

            Console.WriteLine("Message sent successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending message: {ex.Message}");
        }
        finally
        {
            // Dispose of the sender
            await sender.DisposeAsync();
        }
    }


}
