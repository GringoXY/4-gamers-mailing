using Contracts.Events;
using Contracts.Events.Order;
using Infrastructure.Pdf.Documents;
using QuestPDF.Fluent;
using System.Net.Mail;
using System.Net.Mime;

namespace Shared.Factories;

public static class EmailAttachmentsFactory
{
    public static Attachment GetAttachment(this IEvent @event)
    {
        var document = @event switch
        {
            OrderCreatedEvent createdEvent => new InvoiceDocument(createdEvent),
            _ => throw new NotSupportedException($"No document equivalent to event type {@event.GetType().Name} found")
        };

        var pdfStream = new MemoryStream();
        document.GeneratePdf(pdfStream);
        pdfStream.Position = 0; // Reset stream position for reading

        return new Attachment(pdfStream, $"{document.Filename}.pdf", MediaTypeNames.Application.Pdf);
    }
}
