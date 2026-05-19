using System.Collections.Generic;
using System.Linq;
using hikingService.Dtos;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace hikingService.Services;

public class PdfExportService
{
    public byte[] Generate(PostDetailDto detail, bool includeGears)
    {
        var post  = detail.Post;
        var gears = includeGears ? detail.Gears : [];

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.MarginHorizontal(48);
                page.MarginVertical(48);
                page.DefaultTextStyle(x => x.FontSize(11).FontColor(Colors.Grey.Darken3));

                page.Content().Column(col =>
                {
                    // ── Title block ──────────────────────────────────────
                    col.Item()
                        .BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                        .PaddingBottom(14)
                        .Column(header =>
                        {
                            header.Item()
                                .Text("EXPEDITION RECORD")
                                .FontSize(8).Bold()
                                .FontColor(Colors.Grey.Lighten1);

                            header.Item().PaddingTop(6)
                                .Text(post.Title)
                                .FontSize(22).Bold();
                        });

                    // ── Meta ─────────────────────────────────────────────
                    var meta = BuildMetaRows(post);
                    if (meta.Count > 0)
                    {
                        col.Item().PaddingTop(16).Column(m =>
                        {
                            foreach (var (label, value) in meta)
                            {
                                m.Item().Row(row =>
                                {
                                    row.ConstantItem(52)
                                        .Text(label).FontSize(9).Bold()
                                        .FontColor(Colors.Grey.Medium);
                                    row.RelativeItem()
                                        .Text(value).FontSize(11);
                                });
                            }
                        });
                    }

                    // ── Description ──────────────────────────────────────
                    if (!string.IsNullOrWhiteSpace(post.Description))
                    {
                        col.Item()
                            .PaddingTop(20)
                            .BorderTop(1).BorderColor(Colors.Grey.Lighten2)
                            .PaddingTop(16)
                            .Column(desc =>
                            {
                                desc.Item()
                                    .Text("描述")
                                    .FontSize(8).Bold()
                                    .FontColor(Colors.Grey.Medium);

                                desc.Item().PaddingTop(6)
                                    .Text(post.Description)
                                    .FontSize(11)
                                    .FontColor(Colors.Grey.Darken1)
                                    .LineHeight(1.65f);
                            });
                    }

                    // ── Gear table ───────────────────────────────────────
                    if (gears.Count > 0)
                    {
                        var totalWeight = gears.Sum(g => g.Weight * g.Quantity);

                        col.Item()
                            .PaddingTop(20)
                            .BorderTop(1).BorderColor(Colors.Grey.Lighten2)
                            .PaddingTop(16)
                            .Column(gs =>
                            {
                                gs.Item().Row(row =>
                                {
                                    row.RelativeItem()
                                        .Text("裝備清單")
                                        .FontSize(8).Bold()
                                        .FontColor(Colors.Grey.Medium);
                                    row.AutoItem()
                                        .Text($"{gears.Count} 件")
                                        .FontSize(8)
                                        .FontColor(Colors.Grey.Medium);
                                });

                                gs.Item().PaddingTop(8).Table(table =>
                                {
                                    table.ColumnsDefinition(cols =>
                                    {
                                        cols.RelativeColumn(3);   // 名稱
                                        cols.RelativeColumn(2);   // 品牌
                                        cols.RelativeColumn(2);   // 分類
                                        cols.ConstantColumn(52);  // 重量
                                        cols.ConstantColumn(36);  // 數量
                                        cols.RelativeColumn(3);   // 備註
                                    });

                                    table.Header(h =>
                                    {
                                        foreach (var lbl in new[] { "名稱", "品牌", "分類", "重量(g)", "數量", "備註" })
                                            h.Cell()
                                                .BorderBottom(1).BorderColor(Colors.Grey.Lighten1)
                                                .PaddingVertical(4).PaddingHorizontal(2)
                                                .Text(lbl).FontSize(8).Bold()
                                                .FontColor(Colors.Grey.Medium);
                                    });

                                    foreach (var g in gears)
                                    {
                                        void DataCell(IContainer cell, string text, bool muted = false)
                                        {
                                            cell.BorderBottom(1).BorderColor(Colors.Grey.Lighten3)
                                                .PaddingVertical(5).PaddingHorizontal(2)
                                                .Text(string.IsNullOrEmpty(text) ? "—" : text)
                                                .FontSize(10)
                                                .FontColor(muted ? Colors.Grey.Medium : Colors.Grey.Darken3);
                                        }

                                        DataCell(table.Cell(), g.Name);
                                        DataCell(table.Cell(), g.Brand ?? "", muted: true);
                                        DataCell(table.Cell(), g.Category, muted: true);
                                        DataCell(table.Cell(), g.Weight.ToString());
                                        DataCell(table.Cell(), g.Quantity.ToString());
                                        DataCell(table.Cell(), g.Note, muted: true);
                                    }
                                });

                                gs.Item().AlignRight().PaddingTop(8)
                                    .Text($"總重量：{totalWeight:N0} g")
                                    .FontSize(10).FontColor(Colors.Grey.Medium);
                            });
                    }
                });

                page.Footer()
                    .AlignRight()
                    .Text(t =>
                    {
                        t.Span("Hiking Journal  ·  ").FontSize(8).FontColor(Colors.Grey.Lighten1);
                        t.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Lighten1);
                        t.Span(" / ").FontSize(8).FontColor(Colors.Grey.Lighten1);
                        t.TotalPages().FontSize(8).FontColor(Colors.Grey.Lighten1);
                    });
            });
        })
        .GeneratePdf();
    }

    private static List<(string Label, string Value)> BuildMetaRows(hikingRepository.Model.PostModel post)
    {
        var rows = new List<(string, string)>();

        if (!string.IsNullOrEmpty(post.DateStart))
        {
            var range = FmtDate(post.DateStart);
            if (!string.IsNullOrEmpty(post.DateEnd) && post.DateEnd != post.DateStart)
                range += $" – {FmtDate(post.DateEnd)}";
            rows.Add(("日期", range));
        }

        if (!string.IsNullOrEmpty(post.Weather))
            rows.Add(("天氣", post.Weather));

        if (post.PeopleCount.HasValue)
            rows.Add(("人數", $"{post.PeopleCount} 人"));

        return rows;
    }

    private static string FmtDate(string iso)
    {
        if (iso.Length < 10) return iso;
        return $"{iso[..4]}/{iso[5..7]}/{iso[8..10]}";
    }
}
