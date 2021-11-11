//-----------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------
// See https://aka.ms/new-console-template for more information

using System.Text;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Bogus;
using Newtonsoft.Json;

var items = new Faker<Item>()
    .RuleFor(item => item.Id, fake => Guid.NewGuid().ToString())
    .RuleFor(item => item.FirstName, fake => fake.Person.FirstName)
    .RuleFor(item => item.LastName, fake => fake.Person.LastName)
    .Generate(count: 10);

var connectionString = await HarnessUtility.GetEventHubConnectionStringAsync(connectionName: System.Environment.GetEnvironmentVariable("HARNESS_EVENTHUB_CONNECTIONSTRING_NAME"));
var producerClient = new EventHubProducerClient(connectionString: connectionString, eventHubName: System.Environment.GetEnvironmentVariable("HARNESS_EVENTHUB_NAME"));

using EventDataBatch eventBatch = await producerClient.CreateBatchAsync().ConfigureAwait(continueOnCapturedContext: false); ;

foreach(var item in items)
{
    var itemAsString = JsonConvert.SerializeObject(value: item);
    if (!eventBatch.TryAdd(eventData: new EventData(eventBody: Encoding.UTF8.GetBytes(s: itemAsString))))
    {
        throw new Exception(message: $"Event {itemAsString} is too large for the batch and cannot be sent.");
    }
    else
    {
        Console.WriteLine(value: itemAsString);
    }
}

await producerClient.SendAsync(eventBatch: eventBatch).ConfigureAwait(continueOnCapturedContext: false);
await producerClient.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);

Console.WriteLine("Done!");
