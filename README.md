# fishbus

Tiny tiny library for receiving azure service bus messages

### Install

dotnet add package fishbus


### Declare a message type (an Event or a Command)

```c#
public class SomethingExcitingJustHappened
{
    public string Somedata { get; set; }
}
```

### Write a handler for the message

```c#
public class DeleteUserHandler : IHandleMessage<SomethingExcitingJustHappened>
{
    public async Task<HandlerResult> Handle(SomethingExcitingJustHappened message)
    {
        Log.Information("Received SomethingExcitingJustHappened");
        return HandlerResult.Success();
    }
}
```

### Configure the host for your handlers

Here we just have a console app, with a Program.cs, but this could just as well be a web app.
We just need a host that can host a Microsoft.Extensions.Hosting.IHostedService implementation.

```c#
public static async Task<int> Main(string[] args)
{
    try
    {
        await new HostBuilder()
            .ConfigureHostConfiguration(configHost =>
            {
                configHost.SetBasePath(Directory.GetCurrentDirectory());
                configHost.AddEnvironmentVariables(prefix: "ASPNETCORE_");
            })
            .ConfigureServices((hostContext, services) =>
            {
                services
                    .ConfigureMessaging() //will register messagehandlers from current assembly
                    .Configure<MessageSources>(configuration.GetSection("MessageSources")); //register the MessageSources
            })
            .RunConsoleAsync();

        return 0;
    }
    catch (Exception ex)
    {
        Log.Fatal(ex, "Host terminated unexpectedly");
        return 1;
    }
}
```

### Configuration - appsettings.json

The following appsettings.json file could be used to configure the messagsources that the application will listen to

```json
{
  "MessageSources": {
    "Subscriptions": [
      {
        "ConnectionString": "",
        "Name": "<name of topic>"
      }
    ],
    "Queues": [
      {
        "ConnectionString": ""
      }
    ]
  }
}
```
Both subscriptions and queues can specify the Queue/Topic name using the property `EntityName`. It is recommended to not use Connectionstrings with EntityPath if `EntityName` is set. 
If EntityPath is part of the connectionstring, this will override the `EntityName` property.

### Configuration - appsettings.json (Token credentials)

The following appsettings.json file shows how TokenCredentials can be used.
When using TokenCredentials Queue/Topics must have both `Namespace` and `EntityName` defined.
Namespace need to be a "[fully qualified namespace](https://docs.microsoft.com/en-us/dotnet/api/overview/azure/messaging.servicebus-readme-pre#authenticating-with-azureidentity)". 

```json
{
  "MessageSources": {
    "Subscriptions": [
      {
        "Namespace": "my-namespace.servicebus.windows.net",
        "EntityName": "<topic name>",
        "Name": "<subscription name>",
        "CredentialType": "AzureCliCredential"
      }
    ],
    "Queues": [
      {
        "Namespace": "my-namespace.servicebus.windows.net",
        "EntityName": "<queue name>",
        "CredentialType": "DefaultAzureCredential"
      }
    ]
  }
}
```

### Configuration - appsettings.json - setting concurrency
Optionally the message processors for each queue/subscription can be concurrent.
To set the number of thrads used for each Queue/Subscription use the `MaxConcurrentCalls` which will used to set the same property on the underlying `ServiceBusProcessor`

```json
{
  "MessageSources": {
    "Subscriptions": [
      {
        "Namespace": "my-namespace.servicebus.windows.net",
        "EntityName": "<topic name>",
        "Name": "<subscription name>",
        "CredentialType": "AzureCliCredential",
        "MaxConcurrentCalls": 2
      }
    ],
    "Queues": [
      {
        "Namespace": "my-namespace.servicebus.windows.net",
        "EntityName": "<queue name>",
        "CredentialType": "DefaultAzureCredential",
        "MaxConcurrentCalls": 3
      }
    ]
  }
}
```

## Sending Messages

```csharp
[MessageSubject("My.Message.Subject")]
public class MyMessage
{
    public string A { get; set; }
    public string B { get; set; }
}

...

var publisher = new MessagePublisher(connectionString);

var myMessage = new MyMessage
{
    A = "a",
    B = "b"
};

await publisher.SendAsync(myMessage);

```

## Sending Messages with azure credential

```csharp
[MessageSubject("My.Message.Subject")]
public class MyMessage
{
    public string A { get; set; }
    public string B { get; set; }
}

...

var publisher = new MessagePublisher("my-namespace.servicebus.windows.net", "subject.commands", new DefaultAzureCredential());

var myMessage = new MyMessage
{
    A = "a",
    B = "b"
};

await publisher.SendAsync(myMessage);

```

## Sending Message with custom Message Id

To leverage Azure Service Bus duplicate detection the MessageId should be set to an identifier based on your internal business logic. With Fishbus, Azure Service Bus MessageId logic can be overriden by adding the `[MessageId]` attribute to your custom message id property.

```csharp
[MessageSubject("My.Message.With.Custom.MessageId")]
public class MyMessage
{
    [MessageId]
    public string MyId { get; set; }
    public string A { get; set; }
    public string B { get; set; }
}

...

var publisher = new MessagePublisher(connectionString);

var myMessage = new MyMessage
{
    MyId = "id",
    A = "a",
    B = "b"
};

var duplicateMessage = new MyMessage
{
    MyId = "id",
    A = "a",
    B = "b"
};

await publisher.SendAsync(myMessage);
await publisher.SendAsync(duplicateMessage); // Will be discarded by Azure Service Bus if duplicate detection activated and message sent within the duplicate detection history window.
```

## Sending Message with TimeToLive Attribute

To override Azure Service Bus messages TimeToLive property you can, in your message, set a TimeToLive (the property name is not important) property of type `TimeSpan` which is tagged by a `[TimeToLive]` attribute. Only one property can be tagged as a `[TimeToLive]` attribute. The property will be used as the messages TimeToLive if the time span is shorter than the queues DefaultTimeToLive. Otherwise it will silently be adjusted to the default value.

```csharp
[MessageSubject("My.Message.With.Custom.TimeToLive.Attribute")]
public class MyMessage
{
    [TimeToLive]
    public TimeSpan TimeToLive { get; set; }

    public string A { get; set; }
}

...

var publisher = new MessagePublisher(connectionString);

var myMessage = new MyMessage
{
    TimeToLive = new TimeSpan(1, 0, 0)
    A = "a",
};

await publisher.SendAsync(myMessage); // TimeToLive for the message will now be set to 1 hour
```
