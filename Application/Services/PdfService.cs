using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using QuestPDF.Helpers;
using SignFlow.Domain.Entities;

namespace SignFlow.Application.Services;

public class PdfService
{
    public byte[] RenderProposal(Proposal proposal, IEnumerable<ProposalItem> items)
    {
        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Size(PageSizes.A4);
                page.DefaultTextStyle(x => x.FontSize(11));
                page.Header().Text(proposal.Title).FontSize(18).SemiBold();
                page.Content().Column(col =>
                {
                    col.Item().Text($"Status: {proposal.Status}");
                    col.Item().Text($"Totals: Subtotal {proposal.Subtotal:C} | Tax {proposal.TaxTotal:C} | Discount {proposal.DiscountTotal:C} | Grand {proposal.GrandTotal:C}");
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.ConstantColumn(6); // bullet
                            cols.RelativeColumn(5);
                            cols.RelativeColumn(1);
                            cols.RelativeColumn(2);
                            cols.RelativeColumn(2);
                        });
                        table.Header(h =>
                        {
                            h.Cell().Text(" ");
                            h.Cell().Text("Description").SemiBold();
                            h.Cell().AlignRight().Text("Qty").SemiBold();
                            h.Cell().AlignRight().Text("Unit").SemiBold();
                            h.Cell().AlignRight().Text("Line").SemiBold();
                        });
                        foreach (var i in items.OrderBy(x => x.SortOrder))
                        {
                            table.Cell().Text("•");
                            table.Cell().Text(i.Description);
                            table.Cell().AlignRight().Text(i.Quantity.ToString());
                            table.Cell().AlignRight().Text(i.UnitPrice.ToString("C"));
                            table.Cell().AlignRight().Text((i.Quantity * i.UnitPrice).ToString("C"));
                        }
                    });
                });
                page.Footer().AlignRight().Text($"Generated {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC");
            });
        });
        return doc.GeneratePdf();
    }
}
