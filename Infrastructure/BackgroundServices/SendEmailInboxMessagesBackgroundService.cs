using Infrastructure.Options;
using Microsoft.Extensions.Options;
using Shared.Repositories;
using System.Net.Mail;
using AutoMapper;
using Contracts.Dtos;
using Shared.Factories;
using Infrastructure.Factories;
using Shared.Apis;
using System.Net.Mime;

namespace Infrastructure.BackgroundServices;

public class SendEmailInboxMessagesBackgroundService : BackgroundService
{
    private readonly ILogger<SendEmailInboxMessagesBackgroundService> _logger;
    private readonly TimeSpan _runAt;
    private readonly SmtpClient _smtpClient;
    private readonly SendEmailInboxMessagesOptions _sendEmailInboxMessagesOptions;
    private readonly IInboxMessageRepository _inboxMessageRepository;
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
        _smtpClient = sendEmailInboxMessagesOptions.Value.Smtp.Client;
        _sendEmailInboxMessagesOptions = sendEmailInboxMessagesOptions.Value;
        _inboxMessageRepository = serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<IInboxMessageRepository>();
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
        _logger.LogInformation(System.Threading.Thread.CurrentThread.CurrentCulture.ToString());
        try
        {
            var inboxMessages = await _inboxMessageRepository.GetAsync(im => im.ProcessedAt == null);
            foreach(var inboxMessage in inboxMessages)
            {
                try
                {
                    var inboxMessageDto = _mapper.Map<InboxMessageDto>(inboxMessage);
                    var @event = inboxMessageDto.CreateEvent();
                    var template = await @event.GetTemplateAsync();
                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(_sendEmailInboxMessagesOptions.Smtp.Username),
                        Subject = template.Subject,
                        Body = template.Body,
                        IsBodyHtml = true,
                    };

                    var (filename, fileBytes) = await _docsApi.GeneratePdfAsync(@event);
                    if (filename is not null)
                    {
                        var fileStream = new MemoryStream(fileBytes);
                        var attachment = new Attachment(fileStream, filename, MediaTypeNames.Application.Pdf);
                        mailMessage.Attachments.Add(attachment);
                    }

                    mailMessage.To.Add(template.To);

                    try
                    {
                        _logger.LogInformation("Sending email to {recipient}", mailMessage.To);
                        await _smtpClient.SendMailAsync(mailMessage, cancellationToken);
                        _logger.LogInformation("Email sent to {recipient}", mailMessage.To);

                        inboxMessage.MarkAsProcessed();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "An error occurred during the sending email to {recipient}", mailMessage.To);
                    }
                    finally
                    {
                        mailMessage?.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred during the processing inbox message {message}", inboxMessage);
                }
            }

            await _inboxMessageRepository.UpdateRangeAsync(inboxMessages);

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
    }

    public override void Dispose()
    {
        _smtpClient?.Dispose();

        base.Dispose();
    }
}
