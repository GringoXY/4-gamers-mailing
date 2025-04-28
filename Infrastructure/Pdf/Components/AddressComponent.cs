using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace Infrastructure.Pdf.Components;

public class AddressComponent(
    string title,
    string companyName,
    string country,
    string city,
    string postalCode,
    string stateOrProvince,
    string email,
    string phoneNumber)
    : IComponent
{
    private string Title => title;
    private string CompanyName => companyName;
    private string Country => country;
    private string City => city;
    private string PostalCode => postalCode;
    private string StateOrProvince => stateOrProvince;
    private string Email => email;
    private string PhoneNumber => phoneNumber;

    public void Compose(IContainer container)
    {
        container.Column(column =>
        {
            column.Spacing(2);

            column.Item().BorderBottom(1).PaddingBottom(5).Text(Title).SemiBold();

            column.Item().Text(CompanyName);
            column.Item().Text(Country);
            column.Item().Text($"{City}, {PostalCode} {StateOrProvince}");
            column.Item().Text(Email);
            column.Item().Text(PhoneNumber);
        });
    }
}
