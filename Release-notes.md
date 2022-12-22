0.2.23
- Version bumps

0.2.21:
- fix bug: when multiple handlers were configured for a command or event they would all get the same scope. Now different handlers get different scopes and thus avoid problems when taking dependencies on objects such as EF Db contexts
- Use `Azure.Messaging.ServiceBus` instead of `Microsoft.Azure.ServiceBus`
- Allow for token based auth to the servicebus. `AzureCliCredential` and `AzureCliCredential` is now supported
- `MessageSubjectAttribute` is introduced to reflect naming in the new servicebus library. Replaces `MessageLableAttribute` which is now obsolete
0.2.20:
- Add support for loading message handlers from multiple assemblies
0.2.19: 
- added method for sending scheduled message

```public async Task SendScheduledAsync<T>(T message, DateTime time, string correlationId)```

- bumped versions on dependendencies to latest .net Core 3.1 versions

0.2.8:
- breaking change.
Interface IHandleMessage was changed again.
Handle should return HandlerResult to mark success, fail or abort
If abort is returned from a handler the message will immediately be moved to Dead letter queue,
and not retried.

0.2.7
- breaking change.
Interface IHandleMessage was changed.
Handle no longer takes a delegate argument and it should return true/false to mark success or not