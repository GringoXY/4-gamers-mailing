using Infrastructure.Options;
using Microsoft.Extensions.Options;
using Shared.Repositories;
using System.Net.Mail;
using AutoMapper;
using Contracts.Dtos;
using Shared.Factories;

namespace Infrastructure.BackgroundServices;

public class SendEmailBackgroundService : BackgroundService
{
    private readonly ILogger<SendEmailBackgroundService> _logger;
    private readonly TimeSpan _runAt;
    private readonly SmtpClient _smtpClient;
    private readonly SendEmailOptions _sendEmailOptions;
    private readonly IInboxMessageRepository _inboxMessageRepository;
    private readonly IMapper _mapper;

    public SendEmailBackgroundService(
        ILogger<SendEmailBackgroundService> logger,
        IOptions<SendEmailOptions> sendEmailOptions,
        IServiceScopeFactory serviceScopeFactory,
        IMapper mapper)
    {
        _logger = logger;
        _smtpClient = sendEmailOptions.Value.Smtp.Client;
        _sendEmailOptions = sendEmailOptions.Value;
        _inboxMessageRepository = serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<IInboxMessageRepository>();
        _mapper = mapper;

        if (TimeSpan.TryParseExact(sendEmailOptions.Value.Interval, SendEmailOptions.IntervalFormat, null, out _runAt) is false)
        {
            _runAt = TimeSpan.Parse(SendEmailOptions.DefaultInterval);
            _logger.LogWarning($"Invalid Interval configuration. Falling back to {_runAt}.");
        }
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation($"{nameof(SendEmailBackgroundService)} started at: {DateTime.Now}");

        while (cancellationToken.IsCancellationRequested is false)
        {
            DateTime now = DateTime.Now;
            DateTime nextRun = now.Add(_runAt);

            TimeSpan delay = nextRun - now;
            _logger.LogInformation(
                $"{nameof(SendEmailBackgroundService)} will run in {delay} (at {nextRun}).");

            try
            {
                await Task.Delay(delay, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }

            await SendEmailsAsync();
        }
    }

    private async Task SendEmailsAsync()
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
                    From = new MailAddress(_sendEmailOptions.Smtp.Username),
                    Subject = template.Subject,
                    Body = template.Body,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(template.To);

                try
                {
                    _logger.LogInformation("Sending email to {recipient}", mailMessage.To);
                    await _smtpClient.SendMailAsync(mailMessage);
                    inboxMessage.MarkAsProcessed();
                    _logger.LogInformation("Email sent to {recipient}", mailMessage.To);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred during the sending email to {recipient}", mailMessage.To);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during the processing inbox message {message}", inboxMessage);
            }
        }

        try
        {
            await _inboxMessageRepository.UpdateRangeAsync(inboxMessages);
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
