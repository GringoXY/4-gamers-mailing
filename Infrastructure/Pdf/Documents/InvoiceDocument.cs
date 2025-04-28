using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Contracts.Events.Order;
using Infrastructure.Pdf.Components;

namespace Infrastructure.Pdf.Documents;

public class InvoiceDocument(OrderCreatedEvent orderEvent) : IDocument
{
    public string Filename => $"4Gamers-order-{OrderEvent.Id}";
    private readonly OrderCreatedEvent OrderEvent = orderEvent;

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container
            .Page(page =>
            {
                page.Margin(50);

                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);

                page.Footer().AlignCenter().Text(x =>
                {
                    x.CurrentPageNumber();
                    x.Span(" / ");
                    x.TotalPages();
                });
            });
    }

    private void ComposeHeader(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item()
                    .Text($"Invoice #{OrderEvent.Id}")
                    .FontSize(20).SemiBold().FontColor(Colors.Blue.Medium);

                column.Item().Text(text =>
                {
                    text.Span("Issue date: ").SemiBold();
                    text.Span($"{OrderEvent.CreatedAt:d}");
                });
                // TODO: Add "Due to" date
            });

            row.ConstantItem(100).Height(50).Placeholder();
        });
    }

    private void ComposeTable(IContainer container)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(25);
                columns.RelativeColumn(3);
                columns.RelativeColumn();
                columns.RelativeColumn();
                columns.RelativeColumn();
            });

            table.Header(header =>
            {
                header.Cell().Element(CellStyle).Text("#");
                header.Cell().Element(CellStyle).Text("Product");
                header.Cell().Element(CellStyle).AlignRight().Text("Unit price");
                header.Cell().Element(CellStyle).AlignRight().Text("Quantity");
                header.Cell().Element(CellStyle).AlignRight().Text("Total");

                static IContainer CellStyle(IContainer container)
                {
                    return container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                }
            });

            foreach (var item in OrderEvent.Products)
            {
                var productActualPrice = item.ProductDiscountedPrice ?? item.ProductPrice;
                table.Cell().Element(CellStyle).Text(OrderEvent.Products.ToList().IndexOf(item) + 1);
                table.Cell().Element(CellStyle).Text($"{item.ProductCompanyName} {item.ProductModel}");
                table.Cell().Element(CellStyle).AlignRight().Text($"{productActualPrice:C}");
                table.Cell().Element(CellStyle).AlignRight().Text(item.ProductQuantity);
                table.Cell().Element(CellStyle).AlignRight().Text($"{(productActualPrice * item.ProductQuantity):C}");

                static IContainer CellStyle(IContainer container)
                {
                    return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
                }
            }
        });
    }

    private void ComposeRemarks(IContainer container)
    {
        container.Background(Colors.Grey.Lighten3).Padding(10).Column(column =>
        {
            column.Spacing(5);
            column.Item().Text("Remarks").FontSize(14);
            column.Item().Text(OrderEvent.Remarks);
        });
    }

    private void ComposeContent(IContainer container)
    {
        container.PaddingVertical(40).Column(column =>
        {
            column.Spacing(5);

            column.Item().Row(row =>
            {
                row.RelativeItem().Component(
                    new AddressComponent(
                        title: "From",
                        companyName: "4Gamers",
                        country: "Poland",
                        city: "Warsaw",
                        postalCode: "00-001",
                        stateOrProvince: "Warsaw",
                        email: "shop@4gamers.net",
                        phoneNumber: "123 123 123"
                    )
                );
                row.ConstantItem(50);
                row.RelativeItem().Component(
                    new AddressComponent(
                        title: "For",
                        companyName: OrderEvent.BillToName,
                        country: OrderEvent.BillToCountry,
                        city: OrderEvent.BillToCity,
                        postalCode: OrderEvent.BillToPostalCode,
                        stateOrProvince: OrderEvent.BillToStateOrProvince,
                        email: OrderEvent.BillToEmail,
                        phoneNumber: OrderEvent.BillToPhoneNumber
                    )
                );
            });

            column.Item().PaddingTop(25).Element(ComposeTable);

            var totalPrice = OrderEvent.Products.Sum(x => (x.ProductDiscountedPrice ?? x.ProductPrice) * x.ProductQuantity);
            column.Item().AlignRight().Text($"Total to pay: {OrderEvent.TotalPay:C}").FontSize(14);

            if (string.IsNullOrWhiteSpace(OrderEvent.Remarks) is false)
            {
                column.Item().PaddingTop(25).Element(ComposeRemarks);
            }
        });
    }
}