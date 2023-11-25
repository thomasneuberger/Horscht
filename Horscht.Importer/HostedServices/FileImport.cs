using Azure.Storage.Queues;
using Horscht.Contracts.Messages;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using Horscht.Contracts.Services;

namespace Horscht.Importer.HostedServices;

internal class FileImport : IObservableHostedService, IDisposable
{
    private readonly IImportService _importService;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    private readonly QueueClient _queueClient;

    private readonly CancellationTokenSource _cancellationTokenSource = new();

    private State _state = State.New;

    public FileImport(IOptions<ImporterStorageOptions> storageOptions, IImportService importService, JsonSerializerOptions jsonSerializerOptions)
    {
        Console.WriteLine($"FileImport: {InstanceId}");

        _importService = importService;
        _jsonSerializerOptions = jsonSerializerOptions;
        _queueClient = new QueueClient(storageOptions.Value.ConnectionString, storageOptions.Value.ImportQueue);
    }

    public Guid InstanceId { get; } = Guid.NewGuid();

    public string Name => nameof(FileImport);

    public string GetServiceInfo()
    {
        var builder = new StringBuilder();

        builder.AppendLine(nameof(FileImport));
        builder.AppendLine(InstanceId.ToString());

        builder.AppendLine(_state.ToString());

        return builder.ToString();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        ListenToQueueMessagesAsync();
        _state = State.Started;
        Console.WriteLine("FileImport Started.");
        return Task.CompletedTask;
    }

    private async void ListenToQueueMessagesAsync()
    {
        try
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                var response = await _queueClient.ReceiveMessageAsync(cancellationToken: _cancellationTokenSource.Token);
                if (response.HasValue && response.Value is not null)
                {
                    var queueMessage = response.Value;

                    Console.WriteLine($"Message received: {queueMessage.MessageText}");

                    var importMessage = JsonSerializer.Deserialize<ImportMessage>(queueMessage.MessageText, _jsonSerializerOptions);
                    if (importMessage is not null)
                    {
                        await _importService.ImportFile(importMessage.FileName, _cancellationTokenSource.Token);
                    }

                    await _queueClient.DeleteMessageAsync(queueMessage.MessageId, queueMessage.PopReceipt);
                }
                else
                {
                    Console.WriteLine("No message received. Wait...");
                    await Task.Delay(5000);
                }
            }
        }
        catch (TaskCanceledException)
        {
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _cancellationTokenSource.CancelAsync();
        Console.WriteLine("FileImport stopped.");
        _state = State.Stopped;
    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _state = State.Disposed;
        Console.WriteLine("FileImport disposed.");
    }

    private enum State
    {
        New,
        Started,
        Stopped,
        Disposed
    }
}
