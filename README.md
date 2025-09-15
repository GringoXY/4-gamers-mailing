# 4Gamers - Mailing

Mailing is a .NET 8 Worker Service that consumes outbox messages from RabbitMQ, stores them as inbox messages in PostgreSQL, and sends transactional emails based on those messages. It implements the Outbox/Inbox pattern to achieve reliable, at-least-once message processing.

## How it works

- Producer services publish OutboxMessageDto JSONs to a RabbitMQ queue (durable).
- Background consumer (`OutboxMessagesConsumerBackgroundService`) connects to RabbitMQ and manually ACKs only after persisting the message as an `InboxMessage` in PostgreSQL.
- A scheduler (`SendEmailInboxMessagesBackgroundService`) periodically fetches unprocessed inbox messages and:
  - Maps them to domain events (OrderCreated, OrderUpdated, OrderStateUpdated).
  - Renders an HTML email body using Razor templates in `Infrastructure/Templates/Emails`.
  - Optionally calls a Docs API to generate a PDF document and attaches it to the email (for OrderCreated/OrderUpdated).
  - Sends the email using SMTP and marks the inbox message as processed.

Failure behavior:

- RabbitMQ message is ACKed only after DB insert succeeds; otherwise it stays in the queue for retry.
- Email sending failures do not delete the inbox message; it will be retried on the next interval until success.

## Supported events and email templates

- OrderCreatedEvent → subject "New order received: {Id}" → `OrderCreatedEvent.cshtml` (+ optional PDF)
- OrderUpdatedEvent → subject "Order data has changed: {Id}" → `OrderUpdatedEvent.cshtml` (+ optional PDF)
- OrderStateUpdatedEvent → subject "Order state updated: {Id}" → `OrderStateUpdatedEvent.cshtml`

Template rendering uses RazorLight from files copied to output. Subjects and recipients are derived from the event payload (ShipToEmail etc.).

## Storage model

- Table: `inbox_messages` (managed by EF Core, see `Shared/Entities/InboxMessage.cs`).
- Message is considered processed when `ProcessedAt` is set.
- Migrations are applied automatically at startup (`HostExtension.ApplyMigrations`).

EF Core usage and CLI examples are documented in `Infrastructure/EF/README.md`.

## Configuration

Configuration is loaded from:

- JSON: `appsettings/appsettings.{DOTNET_ENVIRONMENT}.json` (see `Infrastructure/appsettings*.json`).
- Environment variables with prefix `FORGAMERS__` (override JSON).

Important keys:

- BackgroundServices:OutboxMessagesConsumer:RabbitMQ
  - HostName, Port, UserName, Password, QueueName
- BackgroundServices:SendEmailInboxMessages
  - Interval (format dd.HH:mm:ss, e.g. 0.00:01:00 for 1 minute)
  - Smtp: Name, Host, Port, Username, Password, EnableSsl
- PostgreSQL: Database, Host, Port, Username, Password
- DocsApi: Scheme, Host, Port
- DOTNET_CULTURE (optional) sets culture used for formatting (e.g., currency in templates)

Example JSON (minimal):

```json
{
  "BackgroundServices": {
    "OutboxMessagesConsumer": {
      "RabbitMQ": {
        "HostName": "localhost",
        "Port": 5672,
        "UserName": "user",
        "Password": "password",
        "QueueName": "outbox-messages"
      }
    },
    "SendEmailInboxMessages": {
      "Interval": "0.00:01:00",
      "Smtp": {
        "Name": "4Gamers robot",
        "Host": "smtp.example.com",
        "Port": 587,
        "Username": "email@example.com",
        "Password": "secret",
        "EnableSsl": true
      }
    }
  },
  "PostgreSQL": {
    "Database": "4Gamers",
    "Host": "localhost",
    "Port": 5432,
    "Username": "postgres",
    "Password": "postgres"
  },
  "DocsApi": {
    "Scheme": "http",
    "Host": "localhost",
    "Port": 5287
  }
}
```

Environment variable overrides use the `FORGAMERS__` prefix and `__` as a separator, e.g.:

- FORGAMERS**BackgroundServices**OutboxMessagesConsumer**RabbitMQ**Password
- FORGAMERS**BackgroundServices**SendEmailInboxMessages**Smtp**Password
- FORGAMERS**PostgreSQL**Password

## RabbitMQ message contract

The consumer expects a JSON shaped like `Contracts.Dtos.OutboxMessage.OutboxMessageDto`:

```json
{
  "entityId": "{guid}",
  "entityType": 0,
  "eventType": "OrderCreated",
  "payload": "{serialized_event_json}"
}
```

`payload` is a serialized event (e.g., `OrderCreatedEvent`). The consumer deserializes it and stores the raw payload into `InboxMessage`.

## Running locally

Prerequisites:

- .NET SDK 8
- RabbitMQ
- PostgreSQL
- SMTP server credentials
- Docs API reachable at configured URI (optional for attachments)

Steps:

- Configure `Infrastructure/appsettings.Development.json` or use environment variables.
- From `Infrastructure/` run the worker:
  - `dotnet run`
- On first start the database schema is created via migrations.

## Docker

- Build: `docker build -t 4gamers-mailing .`
- Run (example):
  - `docker run --rm --name mailing ^
-e FORGAMERS__BackgroundServices__OutboxMessagesConsumer__RabbitMQ__HostName=host.docker.internal ^
-e FORGAMERS__BackgroundServices__OutboxMessagesConsumer__RabbitMQ__UserName=user ^
-e FORGAMERS__BackgroundServices__OutboxMessagesConsumer__RabbitMQ__Password=pass ^
-e FORGAMERS__BackgroundServices__SendEmailInboxMessages__Smtp__Host=smtp.example.com ^
-e FORGAMERS__BackgroundServices__SendEmailInboxMessages__Smtp__Username=email@example.com ^
-e FORGAMERS__BackgroundServices__SendEmailInboxMessages__Smtp__Password=secret ^
-e FORGAMERS__PostgreSQL__Host=host.docker.internal ^
-e FORGAMERS__PostgreSQL__Username=postgres ^
-e FORGAMERS__PostgreSQL__Password=postgres ^
4gamers-mailing`

Note: The service is a background worker and does not expose HTTP ports.

## Key files

- Worker entrypoint: `Infrastructure/Program.cs`
- Consumer: `Infrastructure/BackgroundServices/OutboxMessagesConsumerBackgroundService.cs`
- Email sender: `Infrastructure/BackgroundServices/SendEmailInboxMessagesBackgroundService.cs`
- EF Core DbContext: `Infrastructure/EF/PostgreSQL/ApplicationDbContext.cs`
- Inbox entity: `Shared/Entities/InboxMessage.cs`
- Options: `Infrastructure/Options/*.cs`
- Templates: `Infrastructure/Templates/Emails/*.cshtml`
- Docs API client: `Infrastructure/Apis/DocsApi.cs`

## Troubleshooting

- Missing emails: verify SMTP config and credentials; check logs for send errors.
- PDFs not attached: ensure Docs API is reachable and returns success for the selected event types.
- Messages stuck in queue: DB insert failed; fix DB/connectivity and the consumer will retry (message is unacked until persisted).
- Messages not processed in DB: verify `SendEmailInboxMessages:Interval` and logs; unprocessed messages will be retried each cycle.
