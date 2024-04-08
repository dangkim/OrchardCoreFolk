using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OrchardCore.BackgroundTasks;
using OrchardCore.SongServices.Models;

namespace OrchardCore.SongServices.Services;

[BackgroundTask(Schedule = "* * * * * /20", Description = "lobby", IsIncludedSeconds = true)]
public class LobbyBackGroundTask : IBackgroundTask
{
    private readonly ILogger _logger;
    private readonly IRabbitMQProducer _rabbitMQProducer;

    public LobbyBackGroundTask(ILogger<LobbyBackGroundTask> logger, IRabbitMQProducer rabbitMQProducer)
    {
        _logger = logger;
        _rabbitMQProducer = rabbitMQProducer;
    }

    public Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        // Create user notify
        _rabbitMQProducer.CreateUserNotify(new CreateNotifyModel()
        {
            IsAll = true,
            UserNames = new List<string>(),
            Content = "",
        });

        return Task.CompletedTask;
    }
}
