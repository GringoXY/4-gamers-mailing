using Infrastructure.Options;
using Microsoft.Extensions.Options;
using Shared.Repositories;
using MimeKit;
using MailKit.Net.Smtp;
using AutoMapper;
using Contracts.Dtos;
using Shared.Factories;
using Infrastructure.Factories;
using Shared.Apis;
using System.Net.Mime;
using ContentType = MimeKit.ContentType;

namespace Infrastructure.BackgroundServices;

public class SendEmailInboxMessagesBackgroundService : BackgroundService
{
    private readonly ILogger<SendEmailInboxMessagesBackgroundService> _logger;
    private readonly TimeSpan _runAt;
    private readonly SendEmailInboxMessagesOptions _sendEmailInboxMessagesOptions;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IMapper _mapper;
    private readonly IDocsApi _docsApi;

    public SendEmailInboxMessagesBackgroundService(
        ILogger<SendEmailInboxMessagesBackgroundService> logger,
        IOptions<SendEmailInboxMessagesOptions> sendEmailInboxMessagesOptions,
        IServiceScopeFactory serviceScopeFactory,
        IMapper mapper,
        IDocsApi docsApi)
    {
        _logger = logger;
        _sendEmailInboxMessagesOptions = sendEmailInboxMessagesOptions.Value;
        _serviceScopeFactory = serviceScopeFactory;
        _mapper = mapper;
        _docsApi = docsApi;

        if (TimeSpan.TryParseExact(sendEmailInboxMessagesOptions.Value.Interval, SendEmailInboxMessagesOptions.IntervalFormat, null, out _runAt) is false)
        {
            _runAt = TimeSpan.Parse(SendEmailInboxMessagesOptions.DefaultInterval);
            _logger.LogWarning($"Invalid Interval configuration. Falling back to {_runAt}.");
        }
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation($"{nameof(SendEmailInboxMessagesBackgroundService)} started at: {DateTime.Now}");

        while (cancellationToken.IsCancellationRequested is false)
        {
            DateTime now = DateTime.Now;
            DateTime nextRun = now.Add(_runAt);

            TimeSpan delay = nextRun - now;
            _logger.LogInformation(
                $"{nameof(SendEmailInboxMessagesBackgroundService)} will run in {delay} (at {nextRun}).");

            try
            {
                await Task.Delay(delay, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }

            await SendEmailsAsync(cancellationToken);
        }
    }

    private async Task SendEmailsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var inboxMessageRepository = scope.ServiceProvider.GetRequiredService<IInboxMessageRepository>();
        try
        {
            var inboxMessages = await inboxMessageRepository.GetAsync(im => im.ProcessedAt == null);
            foreach (var inboxMessage in inboxMessages)
            {
                try
                {
                    var inboxMessageDto = _mapper.Map<InboxMessageDto>(inboxMessage);
                    var @event = inboxMessageDto.CreateEvent();
                    var template = await @event.GetTemplateAsync();

                    var message = new MimeMessage()
                    {
                        Subject = template.Subject
                    };
                    message.From.Add(
                        new MailboxAddress(
                            _sendEmailInboxMessagesOptions.Smtp.Name,
                            _sendEmailInboxMessagesOptions.Smtp.Username
                        ));
                    message.To.Add(MailboxAddress.Parse(template.To));

                    var builder = new BodyBuilder
                    {
                        HtmlBody = template.Body
                    };

                    var (filename, fileBytes) = await _docsApi.GeneratePdfAsync(@event);
                    if (string.IsNullOrWhiteSpace(filename) is false && fileBytes?.Length > 0)
                    {
                        builder.Attachments.Add(
                            filename,
                            fileBytes,
                            ContentType.Parse(MediaTypeNames.Application.Pdf));
                    }

                    message.Body = builder.ToMessageBody();

                    try
                    {
                        _logger.LogInformation("Sending email to {recipient}", template.To);

                        using var smtp = new SmtpClient();

                        await smtp.ConnectAsync(_sendEmailInboxMessagesOptions.Smtp.Host, _sendEmailInboxMessagesOptions.Smtp.Port, _sendEmailInboxMessagesOptions.Smtp.EnableSsl, cancellationToken);
                        await smtp.AuthenticateAsync(_sendEmailInboxMessagesOptions.Smtp.Username, _sendEmailInboxMessagesOptions.Smtp.Password, cancellationToken);
                        await smtp.SendAsync(message, cancellationToken);

                        await smtp.DisconnectAsync(true, cancellationToken);

                        _logger.LogInformation("Email sent to {recipient}", template.To);

                        inboxMessage.MarkAsProcessed();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "An error occurred during the sending email to {recipient}", template.To);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred during the processing inbox message {message}", inboxMessage);
                }
            }

            await inboxMessageRepository.UpdateRangeAsync(inboxMessages);

            var processedInboxMessages = inboxMessages.Where(om => om.IsProcessed);
            var notProcessedInboxMessages = inboxMessages.Except(processedInboxMessages).ToList();
            processedInboxMessages = processedInboxMessages.ToList();
            _logger.LogInformation($"Processed {processedInboxMessages.Count()} inbox message(s).");
            _logger.LogWarning($"Not processed {notProcessedInboxMessages.Count} inbox message(s).");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during the saving updated inbox messages");
        }

        _logger.LogInformation("Finished SendEmailsAsync cycle.");
    }

    public override void Dispose()
    {
        _logger.LogInformation($"{nameof(SendEmailInboxMessagesBackgroundService)} is disposing.");
        base.Dispose();
        GC.SuppressFinalize(this);
    }
}
