using System.Text.Json;
using Microsoft.Extensions.Configuration;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Models;
using RabbitMQ.Client;
using OrchardCore.SongServices.Models;
using System.Collections.Generic;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;
using System;

namespace OrchardCore.SongServices
{
    public class RabbitMQProducer : IRabbitMQProducer
    {
        private readonly IConfiguration _config;
        public RabbitMQProducer(IConfiguration config)
        {
            _config = config;
        }
        public bool GetUserConversation(string userName)
        {
            try
            {
                var factory = new ConnectionFactory() { Uri = new Uri(_config["RabbitMQConnectionString"]) };

                using (var connection = factory.CreateConnection())
                {
                    using var channel = connection.CreateModel();
                    channel.QueueDeclare(queue: "GetUserConversation",
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);


                    var messageBody = JsonSerializer.SerializeToUtf8Bytes(userName);
                    channel.BasicPublish(exchange: "", routingKey: "GetUserConversation", basicProperties: null, body: messageBody);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message} | {ex.StackTrace}");
                return false;
            }
        }
        public bool CreateConversation(CreateConversationModel model)
        {
            try
            {
                var factory = new ConnectionFactory() { Uri = new Uri(_config["RabbitMQConnectionString"]) };

                using (var connection = factory.CreateConnection())
                {
                    using var channel = connection.CreateModel();
                    channel.QueueDeclare(queue: "CreateConversation",
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);


                    var messageBody = JsonSerializer.SerializeToUtf8Bytes(model);
                    channel.BasicPublish(exchange: "", routingKey: "CreateConversation", basicProperties: null, body: messageBody);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message} | {ex.StackTrace}");
                return false;
            }
        }
        public bool UserReadMessage(string userName, long conversationId)
        {
            try
            {
                var factory = new ConnectionFactory() { Uri = new Uri(_config["RabbitMQConnectionString"]) };

                using (var connection = factory.CreateConnection())
                {
                    using var channel = connection.CreateModel();
                    channel.QueueDeclare(queue: "UserReadMessage",
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

                    var body = String.Join(',', userName, conversationId);
                    var messageBody = JsonSerializer.SerializeToUtf8Bytes(body);
                    channel.BasicPublish(exchange: "", routingKey: "UserReadMessage", basicProperties: null, body: messageBody);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message} | {ex.StackTrace}");
                return false;
            }
        }
        public bool CreateMessage(CreateMessageModel model)
        {
            try
            {
                var factory = new ConnectionFactory() { Uri = new Uri(_config["RabbitMQConnectionString"]) };

                using (var connection = factory.CreateConnection())
                {
                    using var channel = connection.CreateModel();
                    channel.QueueDeclare(queue: "CreateMessage",
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);


                    var messageBody = JsonSerializer.SerializeToUtf8Bytes(model);
                    channel.BasicPublish(exchange: "", routingKey: "CreateMessage", basicProperties: null, body: messageBody);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message} | {ex.StackTrace}");
                return false;
            }
        }
        public bool GetUserNotify(string userName)
        {
            try
            {
                var factory = new ConnectionFactory() { Uri = new Uri(_config["RabbitMQConnectionString"]) };

                using (var connection = factory.CreateConnection())
                {
                    using var channel = connection.CreateModel();
                    channel.QueueDeclare(queue: "GetUserNotify",
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);


                    var messageBody = JsonSerializer.SerializeToUtf8Bytes(userName);
                    channel.BasicPublish(exchange: "", routingKey: "GetUserNotify", basicProperties: null, body: messageBody);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message} | {ex.StackTrace}");
                return false;
            }
        }
        public bool CreateUserNotify(CreateNotifyModel model)
        {
            try
            {
                var factory = new ConnectionFactory() { Uri = new Uri(_config["RabbitMQConnectionString"]) };

                using (var connection = factory.CreateConnection())
                {
                    using var channel = connection.CreateModel();
                    channel.QueueDeclare(queue: "CreateUserNotify",
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);


                    var messageBody = JsonSerializer.SerializeToUtf8Bytes(model);
                    channel.BasicPublish(exchange: "", routingKey: "CreateUserNotify", basicProperties: null, body: messageBody);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message} | {ex.StackTrace}");
                return false;
            }
        }
        public bool ReadUserNotify(string userName, long notifyId)
        {
            try
            {
                var factory = new ConnectionFactory() { Uri = new Uri(_config["RabbitMQConnectionString"]) };

                using (var connection = factory.CreateConnection())
                {
                    using var channel = connection.CreateModel();
                    channel.QueueDeclare(queue: "ReadUserNotify",
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);


                    var messageBody = JsonSerializer.SerializeToUtf8Bytes(userName + "-" + notifyId);
                    channel.BasicPublish(exchange: "", routingKey: "ReadUserNotify", basicProperties: null, body: messageBody);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message} | {ex.StackTrace}");
                return false;
            }
        }
        public bool JoinToGroup(string conversationId, string userName, string sender)
        {
            try
            {
                var factory = new ConnectionFactory() { Uri = new Uri(_config["RabbitMQConnectionString"]) };

                using (var connection = factory.CreateConnection())
                {
                    using var channel = connection.CreateModel();
                    channel.QueueDeclare(queue: "JoinToGroup",
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);


                    var messageBody = JsonSerializer.SerializeToUtf8Bytes(conversationId + "-" + userName + "-" + sender);
                    channel.BasicPublish(exchange: "", routingKey: "JoinToGroup", basicProperties: null, body: messageBody);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message} | {ex.StackTrace}");
                return false;
            }
        }
        public bool ConnectedSignalR(string userName)
        {
            try
            {
                var factory = new ConnectionFactory() { Uri = new Uri(_config["RabbitMQConnectionString"]) };

                using (var connection = factory.CreateConnection())
                {
                    using var channel = connection.CreateModel();
                    channel.QueueDeclare(queue: "ConnectedSignalR",
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);


                    var messageBody = JsonSerializer.SerializeToUtf8Bytes(userName);
                    channel.BasicPublish(exchange: "", routingKey: "ConnectedSignalR", basicProperties: null, body: messageBody);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message} | {ex.StackTrace}");
                return false;
            }
        }
    }
}
