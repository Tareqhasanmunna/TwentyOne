using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TwentyOne.Shared.DTOs.Responses;

namespace TwentyOne.Web.API
{
    public class OrderPdfService
    {
        public byte[] GenerateOrderPdf(OrderResponseDto order)
        {
            QuestPDF.Settings.License =
                LicenseType.Community;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(x =>
                        x.FontSize(11));

                    // Header
                    page.Header().Column(col =>
                    {
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("TwentyOne Diecast")
                                    .FontSize(22).Bold()
                                    .FontColor("#e94560");
                                c.Item().Text("Dhaka, Bangladesh")
                                    .FontSize(10)
                                    .FontColor("#6c757d");
                                c.Item().Text("01857997575")
                                    .FontSize(10)
                                    .FontColor("#6c757d");
                            });

                            row.RelativeItem().AlignRight().Column(c =>
                            {
                                c.Item().Text("ORDER RECEIPT")
                                    .FontSize(16).Bold()
                                    .FontColor("#1a1a2e");
                                c.Item().Text(order.OrderNumber)
                                    .FontSize(12).Bold()
                                    .FontColor("#e94560");
                                c.Item().Text(
                                    order.CreatedAt
                                        .ToString("MMM dd, yyyy"))
                                    .FontSize(10)
                                    .FontColor("#6c757d");
                            });
                        });

                        col.Item().PaddingTop(10)
                            .LineHorizontal(1)
                            .LineColor("#e94560");
                    });

                    // Content
                    page.Content().PaddingTop(20).Column(col =>
                    {
                        // Order & Customer Info
                        col.Item().Row(row =>
                        {
                            // Bill To - Name & Phone
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("BILL TO")
                                    .FontSize(9).Bold()
                                    .FontColor("#6c757d");
                                c.Item().Text(order.CustomerName)
                                    .FontSize(12).Bold();
                                if (!string.IsNullOrEmpty(order.CustomerPhone))
                                {
                                    c.Item().Text(order.CustomerPhone)
                                        .FontSize(10)
                                        .FontColor("#6c757d");
                                }
                                if (!string.IsNullOrEmpty(order.CustomerEmail))
                                {
                                    c.Item().Text(order.CustomerEmail)
                                        .FontSize(10)
                                        .FontColor("#6c757d");
                                }
                            });

                            // Deliver To - Address only
                            row.RelativeItem().AlignRight().Column(c =>
                            {
                                c.Item().Text("DELIVER TO")
                                    .FontSize(9).Bold()
                                    .FontColor("#6c757d");
                                c.Item().Text(order.DeliveryAddress)
                                    .FontSize(10);
                                c.Item().Text($"Status: {order.Status}")
                                    .FontSize(10).Bold()
                                    .FontColor("#198754");
                            });
                        });

                        col.Item().PaddingTop(20)
                            .PaddingBottom(10)
                            .Text("ORDER ITEMS")
                            .FontSize(10).Bold()
                            .FontColor("#6c757d");

                        // Items table
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(cols =>
                            {
                                cols.RelativeColumn(4);
                                cols.RelativeColumn(1);
                                cols.RelativeColumn(2);
                                cols.RelativeColumn(2);
                            });

                            // Header
                            table.Header(header =>
                            {
                                header.Cell().Background("#1a1a2e")
                                    .Padding(8)
                                    .Text("Product").Bold()
                                    .FontColor(Colors.White)
                                    .FontSize(10);
                                header.Cell().Background("#1a1a2e")
                                    .Padding(8)
                                    .Text("Qty").Bold()
                                    .FontColor(Colors.White)
                                    .FontSize(10);
                                header.Cell().Background("#1a1a2e")
                                    .Padding(8)
                                    .Text("Unit Price").Bold()
                                    .FontColor(Colors.White)
                                    .FontSize(10);
                                header.Cell().Background("#1a1a2e")
                                    .Padding(8)
                                    .Text("Subtotal").Bold()
                                    .FontColor(Colors.White)
                                    .FontSize(10);
                            });

                            // Rows
                            var isEven = false;
                            foreach (var item in order.Items ?? new())
                            {
                                var bg = isEven
                                    ? "#f8f9fa" : "#ffffff";
                                isEven = !isEven;

                                table.Cell()
                                    .Background(bg).Padding(8)
                                    .Text(item.ProductName)
                                    .FontSize(10);
                                table.Cell()
                                    .Background(bg).Padding(8)
                                    .Text(item.Quantity.ToString())
                                    .FontSize(10);
                                table.Cell()
                                    .Background(bg).Padding(8)
                                    .Text($"৳{item.UnitPrice:N0}")
                                    .FontSize(10);
                                table.Cell()
                                    .Background(bg).Padding(8)
                                    .Text($"৳{item.Subtotal:N0}")
                                    .FontSize(10).Bold();
                            }
                        });

                        // Totals
                        col.Item().PaddingTop(15)
                            .AlignRight().Column(c =>
                            {
                                c.Item().Row(r =>
                                {
                                    r.RelativeItem();
                                    r.ConstantItem(200).Column(tc =>
                                    {
                                        tc.Item()
                                            .BorderBottom(1)
                                            .BorderColor("#e9ecef")
                                            .PaddingBottom(5)
                                            .Row(row =>
                                            {
                                                row.RelativeItem()
                                                .Text("Subtotal")
                                                .FontSize(10)
                                                .FontColor("#6c757d");
                                                row.AutoItem()
                                                .Text($"৳{order.TotalAmount:N0}")
                                                .FontSize(10);
                                            });

                                        if (order.DeliveryCharge > 0)
                                        {
                                            tc.Item()
                                                .PaddingTop(5)
                                                .BorderBottom(1)
                                                .BorderColor("#e9ecef")
                                                .PaddingBottom(5)
                                                .Row(row =>
                                                {
                                                    row.RelativeItem()
                                                    .Text("Delivery")
                                                    .FontSize(10)
                                                    .FontColor("#6c757d");
                                                    row.AutoItem()
                                                    .Text($"৳{order.DeliveryCharge:N0}")
                                                    .FontSize(10);
                                                });
                                        }

                                        if (order.DiscountAmount > 0)
                                        {
                                            tc.Item()
                                                .PaddingTop(5)
                                                .BorderBottom(1)
                                                .BorderColor("#e9ecef")
                                                .PaddingBottom(5)
                                                .Row(row =>
                                                {
                                                    row.RelativeItem()
                                                    .Text("Discount")
                                                    .FontSize(10)
                                                    .FontColor("#198754");
                                                    row.AutoItem()
                                                    .Text($"-৳{order.DiscountAmount:N0}")
                                                    .FontSize(10)
                                                    .FontColor("#198754");
                                                });
                                        }

                                        tc.Item().PaddingTop(8)
                                            .Background("#e94560")
                                            .Padding(8).Row(row =>
                                            {
                                                row.RelativeItem()
                                                .Text("TOTAL")
                                                .FontSize(12).Bold()
                                                .FontColor(Colors.White);
                                                row.AutoItem()
                                                .Text($"৳{order.FinalAmount:N0}")
                                                .FontSize(12).Bold()
                                                .FontColor(Colors.White);
                                            });
                                    });
                                });
                            });

                        // Notes
                        if (!string.IsNullOrEmpty(order.Notes))
                        {
                            col.Item().PaddingTop(20)
                                .Background("#f8f9fa")
                                .Padding(12).Column(c =>
                                {
                                    c.Item().Text("Notes")
                                        .FontSize(9).Bold()
                                        .FontColor("#6c757d");
                                    c.Item().Text(order.Notes)
                                        .FontSize(10);
                                });
                        }

                        // Payment method
                        col.Item().PaddingTop(15)
                            .Background("#fff8f0")
                            .Padding(12).Row(row =>
                            {
                                row.AutoItem()
                                    .Text("Payment Method: ")
                                    .FontSize(10).Bold();
                                row.RelativeItem()
                                    .Text("Cash on Delivery (COD)")
                                    .FontSize(10);
                            });
                    });

                    // Footer
                    page.Footer().Column(col =>
                    {
                        col.Item().LineHorizontal(1)
                            .LineColor("#e9ecef");
                        col.Item().PaddingTop(8)
                            .Row(row =>
                            {
                                row.RelativeItem()
                                    .Text("Thank you for shopping " +
                                          "at TwentyOne Diecast!")
                                    .FontSize(9)
                                    .FontColor("#6c757d");
                                row.AutoItem()
                                    .Text($"Page 1")
                                    .FontSize(9)
                                    .FontColor("#6c757d");
                            });
                    });
                });
            });

            return document.GeneratePdf();
        }
    }
}