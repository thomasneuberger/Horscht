var builder = DistributedApplication.CreateBuilder(args);

// Add Azurite for local Azure Storage emulation (Blobs, Queues, and Tables)
var storage = builder.AddAzureStorage("storage")
    .RunAsEmulator(emulator =>
    {
        emulator.WithBlobPort(10000)
                .WithQueuePort(10001)
                .WithTablePort(10002);
    });

var blobs = storage.AddBlobs("blobs");
var queues = storage.AddQueues("queues");
var tables = storage.AddTables("tables");

// Add Horscht.Importer service
var importer = builder.AddProject<Projects.Horscht_Importer>("importer")
    .WithReference(blobs)
    .WithReference(queues)
    .WithReference(tables);

// Add Horscht.Web Blazor WebAssembly application
var web = builder.AddProject<Projects.Horscht_Web>("web")
    .WithReference(blobs)
    .WithReference(queues)
    .WithReference(tables)
    .WithReference(importer);

builder.Build().Run();
