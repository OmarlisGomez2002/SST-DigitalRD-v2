using QuestPDF.Fluent;
using QuestPDF.Helpers;
using SSTDigitalRD.Shared.DTOs;

namespace SSTDigitalRD.Server.Services
{
    public interface IPdfService
    {
        byte[] GenerarInspeccionPdf(InspeccionDetalleDto insp);
        byte[] GenerarCharlaPdf(CharlaDetalleDto charla);
        byte[] GenerarEntregaEPPPdf(EntregaEPPDetalleDto entrega);
        byte[] GenerarIncidentePdf(IncidenteDetalleDto inc);
        byte[] GenerarDossierPdf(ReporteStatusDto reporte, string nombreEmpresa);
    }

    public class PdfService : IPdfService
    {
        public byte[] GenerarInspeccionPdf(InspeccionDetalleDto insp)
        {
            var documento = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.Letter);
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                    // ── Encabezado ───────────────────────────────
                    page.Header().Column(col =>
                    {
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("SST-Digital RD")
                                    .FontSize(16).Bold()
                                    .FontColor(Colors.Blue.Darken2);
                                c.Item().Text("Reporte de Inspección de Seguridad")
                                    .FontSize(11).FontColor(Colors.Grey.Darken1);
                            });
                            row.ConstantItem(120).AlignRight().Column(c =>
                            {
                                c.Item().Text($"ID: {insp.Id:D6}")
                                    .FontSize(9).FontColor(Colors.Grey.Darken1);
                                c.Item().Text(insp.FechaInspeccion.ToString("dd/MM/yyyy"))
                                    .FontSize(9).FontColor(Colors.Grey.Darken1);
                            });
                        });
                        col.Item().PaddingTop(8).LineHorizontal(1)
                            .LineColor(Colors.Grey.Lighten2);
                    });

                    // ── Contenido ────────────────────────────────
                    page.Content().PaddingVertical(15).Column(col =>
                    {
                        col.Spacing(12);

                        // Estado destacado
                        var (colorFondo, colorTexto, etiqueta) = insp.Estado switch
                        {
                            "Conforme" => (Colors.Green.Lighten4, Colors.Green.Darken2, "CONFORME"),
                            "Observación" => (Colors.Orange.Lighten4, Colors.Orange.Darken2, "OBSERVACIÓN"),
                            "No conforme" => (Colors.Red.Lighten4, Colors.Red.Darken2, "NO CONFORME"),
                            _ => (Colors.Grey.Lighten3, Colors.Grey.Darken2, insp.Estado.ToUpper())
                        };

                        col.Item().Background(colorFondo).Padding(10).Row(row =>
                        {
                            row.RelativeItem().Text(etiqueta)
                                .FontSize(13).Bold().FontColor(colorTexto);
                            row.AutoItem().Text($"{insp.Items.Count(i => i.Resultado == "ok")}/{insp.Items.Count(i => i.Resultado != "na")} ítems cumplidos")
                                .FontSize(10).FontColor(colorTexto);
                        });

                        // Información general
                        col.Item().Text("Información general")
                            .FontSize(12).Bold();

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(1);
                                c.RelativeColumn(2);
                            });

                            AddRow(table, "Área", insp.Area);
                            AddRow(table, "Obra", insp.Obra);
                            AddRow(table, "Tipo de inspección", insp.TipoInspeccion);
                            AddRow(table, "Inspector", insp.Inspector);
                            AddRow(table, "Responsable de área", insp.ResponsableArea);
                            AddRow(table, "Fecha y hora", insp.FechaInspeccion.ToString("dd/MM/yyyy hh:mm tt"));
                            AddRow(table, "Trabajadores presentes", insp.CantidadTrabajadores.ToString());
                        });

                        // Checklist
                        col.Item().PaddingTop(8).Text("Lista de verificación")
                            .FontSize(12).Bold();

                        foreach (var grupo in insp.Items.GroupBy(x => x.Categoria))
                        {
                            col.Item().Text(grupo.Key)
                                .FontSize(10).Bold().FontColor(Colors.Blue.Darken1);

                            foreach (var item in grupo)
                            {
                                var (icono, color) = item.Resultado switch
                                {
                                    "ok" => ("✓", Colors.Green.Darken1),
                                    "warn" => ("!", Colors.Orange.Darken1),
                                    "no" => ("✗", Colors.Red.Darken1),
                                    _ => ("–", Colors.Grey.Medium)
                                };

                                col.Item().Row(row =>
                                {
                                    row.ConstantItem(20).Text(icono)
                                        .FontColor(color).Bold();
                                    row.RelativeItem().Text(text =>
                                    {
                                        text.Span(item.Descripcion).FontSize(9.5f);
                                        if (!string.IsNullOrEmpty(item.Observacion))
                                            text.Span($"  ·  Obs: {item.Observacion}")
                                                .FontSize(9).Italic()
                                                .FontColor(Colors.Grey.Darken1);
                                    });
                                });
                            }
                        }

                        // GPS y evidencia
                        if (insp.GpsCapturado)
                        {
                            col.Item().PaddingTop(8).Background(Colors.Green.Lighten5)
                                .Padding(8).Row(row =>
                                {
                                    row.AutoItem().Text("📍").FontSize(11);
                                    row.RelativeItem().PaddingLeft(5).Text(
                                    $"Geolocalización capturada: " +
                                    $"{insp.Latitud:F6}, {insp.Longitud:F6} " +
                                    $"(±{insp.PrecisionGps:F0}m)")
                                    .FontSize(9).FontColor(Colors.Green.Darken2);
                                });
                        }

                        if (!string.IsNullOrEmpty(insp.PlanAccion))
                        {
                            col.Item().PaddingTop(4).Text("Plan de acción")
                                .FontSize(11).Bold();
                            col.Item().Background(Colors.Grey.Lighten4)
                                .Padding(8)
                                .Text(insp.PlanAccion).FontSize(9.5f);
                        }
                    });

                    // ── Pie de página ────────────────────────────
                    page.Footer().Column(col =>
                    {
                        col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                        col.Item().PaddingTop(5).Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Documento sellado digitalmente")
                                    .FontSize(8).Bold();
                                c.Item().Text($"SHA-256: {insp.HashSha256}")
                                    .FontSize(7).FontColor(Colors.Grey.Darken1)
                                    .FontFamily("Courier New");
                            });
                            row.ConstantItem(100).AlignRight().Text(
                                insp.Firmado ? $"Firmado {insp.HoraFirma}" : "Sin firma")
                                .FontSize(8)
                                .FontColor(insp.Firmado ? Colors.Green.Darken1 : Colors.Red.Darken1);
                        });
                        col.Item().AlignCenter().PaddingTop(3).Text(
                            "Generado por SST-Digital RD · Conforme al Reglamento 522-06")
                            .FontSize(7).FontColor(Colors.Grey.Medium);
                    });
                });
            });

            return documento.GeneratePdf();
        }

        public byte[] GenerarCharlaPdf(CharlaDetalleDto charla)
        {
            var documento = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.Letter);
                    page.Margin(40);
                    page.DefaultTextStyle(x =>
                        x.FontSize(10).FontFamily("Arial"));

                    // ── Encabezado ───────────────────────────────
                    page.Header().Column(col =>
                    {
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("SST-Digital RD")
                                    .FontSize(16).Bold()
                                    .FontColor(Colors.Blue.Darken2);
                                c.Item().Text("Registro de Charla de Seguridad")
                                    .FontSize(11)
                                    .FontColor(Colors.Grey.Darken1);
                            });
                            row.ConstantItem(120).AlignRight().Column(c =>
                            {
                                c.Item().Text($"ID: {charla.Id:D6}")
                                    .FontSize(9)
                                    .FontColor(Colors.Grey.Darken1);
                                c.Item().Text(charla.FechaCharla
                                    .ToString("dd/MM/yyyy"))
                                    .FontSize(9)
                                    .FontColor(Colors.Grey.Darken1);
                            });
                        });
                        col.Item().PaddingTop(8)
                            .LineHorizontal(1)
                            .LineColor(Colors.Grey.Lighten2);
                    });

                    // ── Contenido ────────────────────────────────
                    page.Content().PaddingVertical(15).Column(col =>
                    {
                        col.Spacing(12);

                        // Datos generales
                        col.Item().Text("Datos de la charla")
                            .FontSize(12).Bold();

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(1);
                                c.RelativeColumn(2);
                            });

                            AddRow(table, "Tema",
                                charla.Tema);
                            AddRow(table, "Instructor",
                                charla.Instructor);
                            AddRow(table, "Obra",
                                charla.Obra);
                            AddRow(table, "Cuadrilla",
                                charla.Cuadrilla);
                            AddRow(table, "Fecha y hora",
                                charla.FechaCharla
                                    .ToString("dd/MM/yyyy hh:mm tt"));
                            AddRow(table, "Duración",
                                $"{charla.DuracionMinutos} minutos");
                            AddRow(table, "Total asistentes",
                                $"{charla.TotalAsistentes} trabajadores");
                        });

                        // Lista de asistencia
                        col.Item().PaddingTop(4)
                            .Text("Lista de asistencia")
                            .FontSize(12).Bold();

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.ConstantColumn(30);
                                c.RelativeColumn(3);
                                c.RelativeColumn(2);
                                c.ConstantColumn(60);
                            });

                            // Encabezado tabla
                            table.Cell().Background(Colors.Grey.Lighten3)
                                .Padding(4).Text("#")
                                .FontSize(9).Bold();
                            table.Cell().Background(Colors.Grey.Lighten3)
                                .Padding(4).Text("Nombre")
                                .FontSize(9).Bold();
                            table.Cell().Background(Colors.Grey.Lighten3)
                                .Padding(4).Text("Cargo")
                                .FontSize(9).Bold();
                            table.Cell().Background(Colors.Grey.Lighten3)
                                .Padding(4).Text("Presente")
                                .FontSize(9).Bold();

                            var idx = 1;
                            foreach (var asistente in charla.Asistentes)
                            {
                                var bg = idx % 2 == 0
                                    ? Colors.Grey.Lighten5
                                    : Colors.White;

                                table.Cell().Background(bg)
                                    .Padding(4).Text(idx.ToString())
                                    .FontSize(9);
                                table.Cell().Background(bg)
                                    .Padding(4).Text(asistente.Nombre)
                                    .FontSize(9);
                                table.Cell().Background(bg)
                                    .Padding(4).Text(asistente.Cargo)
                                    .FontSize(9);
                                table.Cell().Background(bg)
                                    .Padding(4)
                                    .Text(asistente.Presente ? "✓" : "✗")
                                    .FontSize(9)
                                    .FontColor(asistente.Presente
                                        ? Colors.Green.Darken1
                                        : Colors.Red.Darken1);
                                idx++;
                            }
                        });

                        // Resumen de asistencia
                        var presentes = charla.Asistentes.Count(a => a.Presente);
                        var total = charla.Asistentes.Count;
                        var pct = total > 0
                            ? (int)Math.Round(presentes * 100.0 / total) : 0;

                        col.Item().Background(Colors.Green.Lighten5)
                            .Padding(8).Row(row =>
                            {
                                row.RelativeItem().Text(
                            $"Asistencia: {presentes}/{total} trabajadores " +
                            $"({pct}%)")
                            .FontSize(10).Bold()
                            .FontColor(Colors.Green.Darken2);
                            });

                        // GPS
                        if (charla.GpsCapturado)
                        {
                            col.Item().PaddingTop(4)
                                .Background(Colors.Green.Lighten5)
                                .Padding(8).Row(row =>
                                {
                                    row.AutoItem().Text("📍").FontSize(11);
                                    row.RelativeItem().PaddingLeft(5).Text(
                                $"Geolocalización capturada: " +
                                $"{charla.Latitud:F6}, " +
                                $"{charla.Longitud:F6} " +
                                $"(±{charla.PrecisionGps:F0}m)")
                                .FontSize(9)
                                .FontColor(Colors.Green.Darken2);
                                });
                        }

                        // Firma
                        col.Item().PaddingTop(8).Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Firma del instructor")
                                    .FontSize(10).Bold();
                                c.Item().PaddingTop(4)
                                    .Border(1).BorderColor(Colors.Grey.Lighten2)
                                    .Height(50).Background(
                                        charla.Firmado
                                            ? Colors.Green.Lighten5
                                            : Colors.Grey.Lighten4)
                                    .AlignCenter().AlignMiddle()
                                    .Text(charla.Firmado
                                        ? $"✓ Firmado digitalmente · {charla.HoraFirma}"
                                        : "Sin firma")
                                    .FontSize(9)
                                    .FontColor(charla.Firmado
                                        ? Colors.Green.Darken1
                                        : Colors.Grey.Darken1);
                            });
                            row.ConstantItem(20);
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Base legal")
                                    .FontSize(10).Bold();
                                c.Item().PaddingTop(4)
                                    .Background(Colors.Grey.Lighten4)
                                    .Padding(8)
                                    .Text("Art. 9.6, Reglamento 522-06 — " +
                                          "El empleador garantizará la " +
                                          "formación de los trabajadores " +
                                          "manteniendo registros firmados.")
                                    .FontSize(8)
                                    .FontColor(Colors.Grey.Darken1);
                            });
                        });
                    });

                    // ── Pie de página ────────────────────────────
                    page.Footer().Column(col =>
                    {
                        col.Item().LineHorizontal(1)
                            .LineColor(Colors.Grey.Lighten2);
                        col.Item().PaddingTop(5).Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Documento sellado digitalmente")
                                    .FontSize(8).Bold();
                                c.Item().Text($"SHA-256: {charla.HashSha256}")
                                    .FontSize(7)
                                    .FontColor(Colors.Grey.Darken1)
                                    .FontFamily("Courier New");
                            });
                            row.ConstantItem(120).AlignRight()
                                .Text(charla.Firmado
                                    ? $"Firmado {charla.HoraFirma}"
                                    : "Sin firma")
                                .FontSize(8)
                                .FontColor(charla.Firmado
                                    ? Colors.Green.Darken1
                                    : Colors.Red.Darken1);
                        });
                        col.Item().AlignCenter().PaddingTop(3)
                            .Text("Generado por SST-Digital RD · " +
                                  "Conforme al Reglamento 522-06")
                            .FontSize(7).FontColor(Colors.Grey.Medium);
                    });
                });
            });

            return documento.GeneratePdf();
        }

        public byte[] GenerarEntregaEPPPdf(EntregaEPPDetalleDto entrega)
        {
            var documento = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.Letter);
                    page.Margin(40);
                    page.DefaultTextStyle(x =>
                        x.FontSize(10).FontFamily("Arial"));

                    // ── Encabezado ───────────────────────────────
                    page.Header().Column(col =>
                    {
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("SST-Digital RD")
                                    .FontSize(16).Bold()
                                    .FontColor(Colors.Blue.Darken2);
                                c.Item().Text(
                                    "Constancia de Entrega de EPP")
                                    .FontSize(11)
                                    .FontColor(Colors.Grey.Darken1);
                            });
                            row.ConstantItem(120).AlignRight().Column(c =>
                            {
                                c.Item().Text($"ID: {entrega.Id:D6}")
                                    .FontSize(9)
                                    .FontColor(Colors.Grey.Darken1);
                                c.Item().Text(entrega.FechaEntrega
                                    .ToString("dd/MM/yyyy"))
                                    .FontSize(9)
                                    .FontColor(Colors.Grey.Darken1);
                            });
                        });
                        col.Item().PaddingTop(8)
                            .LineHorizontal(1)
                            .LineColor(Colors.Grey.Lighten2);
                    });

                    // ── Contenido ────────────────────────────────
                    page.Content().PaddingVertical(15).Column(col =>
                    {
                        col.Spacing(12);

                        // Datos del trabajador
                        col.Item().Text("Datos del trabajador")
                            .FontSize(12).Bold();

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(1);
                                c.RelativeColumn(2);
                            });

                            AddRow(table, "Nombre",
                                entrega.NombreTrabajador);
                            AddRow(table, "Cédula",
                                entrega.CedulaTrabajador);
                            AddRow(table, "Cargo",
                                entrega.Cargo);
                            AddRow(table, "Cuadrilla",
                                entrega.Cuadrilla);
                            AddRow(table, "Obra",
                                entrega.Obra);
                            AddRow(table, "Fecha de entrega",
                                entrega.FechaEntrega
                                    .ToString("dd/MM/yyyy hh:mm tt"));
                            AddRow(table, "Entregado por",
                                entrega.EntregadoPor);
                        });

                        // Artículos entregados
                        col.Item().PaddingTop(4)
                            .Text("Artículos de EPP entregados")
                            .FontSize(12).Bold();

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.ConstantColumn(30);
                                c.RelativeColumn(3);
                                c.RelativeColumn(2);
                                c.RelativeColumn(2);
                                c.ConstantColumn(70);
                            });

                            // Encabezado
                            foreach (var h in new[]
                            {
                        "#", "Tipo de EPP", "Marca",
                        "Vencimiento", "Estado"
                    })
                            {
                                table.Cell()
                                    .Background(Colors.Grey.Lighten3)
                                    .Padding(4).Text(h)
                                    .FontSize(9).Bold();
                            }

                            var idx = 1;
                            foreach (var art in entrega.Articulos)
                            {
                                var bg = idx % 2 == 0
                                    ? Colors.Grey.Lighten5
                                    : Colors.White;

                                var (colorEst, textoEst) = art.Estado switch
                                {
                                    "Vigente" => (Colors.Green.Darken1, "✓ Vigente"),
                                    "Vencido" => (Colors.Red.Darken1, "✗ Vencido"),
                                    _ => (Colors.Grey.Darken1, art.Estado)
                                };

                                table.Cell().Background(bg).Padding(4)
                                    .Text(idx.ToString()).FontSize(9);
                                table.Cell().Background(bg).Padding(4)
                                    .Text(art.TipoEPP).FontSize(9);
                                table.Cell().Background(bg).Padding(4)
                                    .Text(art.Marca).FontSize(9);
                                table.Cell().Background(bg).Padding(4)
                                    .Text(art.FechaVencimiento
                                        .ToString("dd/MM/yyyy"))
                                    .FontSize(9);
                                table.Cell().Background(bg).Padding(4)
                                    .Text(textoEst).FontSize(9)
                                    .FontColor(colorEst);
                                idx++;
                            }
                        });

                        // Resumen de estado
                        var vigentes = entrega.Articulos
                            .Count(a => a.Estado == "Vigente");
                        var vencidos = entrega.Articulos
                            .Count(a => a.Estado == "Vencido");

                        col.Item()
                            .Background(vigentes == entrega.Articulos.Count
                                ? Colors.Green.Lighten5
                                : Colors.Orange.Lighten5)
                            .Padding(8).Row(row =>
                            {
                                row.RelativeItem().Text(
                            $"Estado general: " +
                            $"{vigentes} vigentes, " +
                            $"{vencidos} vencidos de " +
                            $"{entrega.Articulos.Count} artículos")
                            .FontSize(10).Bold()
                            .FontColor(vigentes == entrega.Articulos.Count
                                ? Colors.Green.Darken2
                                : Colors.Orange.Darken2);
                            });

                        // Firma del trabajador
                        col.Item().PaddingTop(8).Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Firma de recepción del trabajador")
                                    .FontSize(10).Bold();
                                c.Item().PaddingTop(4)
                                    .Border(1)
                                    .BorderColor(Colors.Grey.Lighten2)
                                    .Height(60)
                                    .Background(entrega.Firmado
                                        ? Colors.Green.Lighten5
                                        : Colors.Grey.Lighten4)
                                    .AlignCenter().AlignMiddle()
                                    .Text(entrega.Firmado
                                        ? "✓ Firmado digitalmente"
                                        : "Sin firma del trabajador")
                                    .FontSize(9)
                                    .FontColor(entrega.Firmado
                                        ? Colors.Green.Darken1
                                        : Colors.Grey.Darken1);
                            });
                            row.ConstantItem(20);
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Base legal")
                                    .FontSize(10).Bold();
                                c.Item().PaddingTop(4)
                                    .Background(Colors.Grey.Lighten4)
                                    .Padding(8)
                                    .Text("Res. 04/2007 — El empleador " +
                                          "mantendrá un registro de entrega " +
                                          "de EPP por trabajador, con firma " +
                                          "del receptor y fecha de vencimiento.")
                                    .FontSize(8)
                                    .FontColor(Colors.Grey.Darken1);
                            });
                        });
                    });

                    // ── Pie de página ────────────────────────────
                    page.Footer().Column(col =>
                    {
                        col.Item().LineHorizontal(1)
                            .LineColor(Colors.Grey.Lighten2);
                        col.Item().PaddingTop(5).Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Documento sellado digitalmente")
                                    .FontSize(8).Bold();
                                c.Item().Text($"SHA-256: {entrega.HashSha256}")
                                    .FontSize(7)
                                    .FontColor(Colors.Grey.Darken1)
                                    .FontFamily("Courier New");
                            });
                            row.ConstantItem(120).AlignRight()
                                .Text(entrega.Firmado
                                    ? "Firmado digitalmente"
                                    : "Sin firma")
                                .FontSize(8)
                                .FontColor(entrega.Firmado
                                    ? Colors.Green.Darken1
                                    : Colors.Red.Darken1);
                        });
                        col.Item().AlignCenter().PaddingTop(3)
                            .Text("Generado por SST-Digital RD · " +
                                  "Conforme a Resolución 04/2007")
                            .FontSize(7).FontColor(Colors.Grey.Medium);
                    });
                });
            });

            return documento.GeneratePdf();
        }

        public byte[] GenerarIncidentePdf(IncidenteDetalleDto inc)
        {
            var documento = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.Letter);
                    page.Margin(40);
                    page.DefaultTextStyle(x =>
                        x.FontSize(10).FontFamily("Arial"));

                    page.Header().Column(col =>
                    {
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("SST-Digital RD")
                                    .FontSize(16).Bold()
                                    .FontColor(Colors.Blue.Darken2);
                                c.Item().Text("Reporte Oficial de Incidente")
                                    .FontSize(11)
                                    .FontColor(Colors.Grey.Darken1);
                            });
                            row.ConstantItem(120).AlignRight().Column(c =>
                            {
                                c.Item().Text($"ID: {inc.Id:D6}")
                                    .FontSize(9).FontColor(Colors.Grey.Darken1);
                                c.Item().Text(inc.Fecha.ToString("dd/MM/yyyy"))
                                    .FontSize(9).FontColor(Colors.Grey.Darken1);
                            });
                        });
                        col.Item().PaddingTop(8).LineHorizontal(1)
                            .LineColor(Colors.Grey.Lighten2);
                    });

                    page.Content().PaddingVertical(15).Column(col =>
                    {
                        col.Spacing(12);

                        // Banner de tipo
                        var (bg, fg, etiqueta) = inc.Tipo switch
                        {
                            "Accidente grave" => (Colors.Red.Lighten4, Colors.Red.Darken2, "ACCIDENTE GRAVE"),
                            "Accidente leve" => (Colors.Orange.Lighten4, Colors.Orange.Darken2, "ACCIDENTE LEVE"),
                            "Cuasi-accidente" => (Colors.Orange.Lighten4, Colors.Orange.Darken2, "CUASI-ACCIDENTE"),
                            "Condición insegura" => (Colors.Blue.Lighten4, Colors.Blue.Darken2, "CONDICIÓN INSEGURA"),
                            _ => (Colors.Grey.Lighten3, Colors.Grey.Darken2, inc.Tipo.ToUpper())
                        };

                        col.Item().Background(bg).Padding(10).Row(row =>
                        {
                            row.RelativeItem().Text(etiqueta)
                                .FontSize(13).Bold().FontColor(fg);
                            row.AutoItem().Text(inc.Estado)
                                .FontSize(10).FontColor(fg);
                        });

                        // Datos generales
                        col.Item().Text("Datos del incidente").FontSize(12).Bold();
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(1);
                                c.RelativeColumn(2);
                            });

                            AddRow(table, "Descripción", inc.Descripcion);
                            AddRow(table, "Tipo", inc.Tipo);
                            AddRow(table, "Área / Zona", inc.Area);
                            AddRow(table, "Obra", inc.Obra);
                            AddRow(table, "Inspector", inc.Inspector);
                            AddRow(table, "Trabajador afectado",
                                string.IsNullOrEmpty(inc.Afectado)
                                    ? "Sin afectado directo" : inc.Afectado);
                            AddRow(table, "Fecha y hora",
                                inc.Fecha.ToString("dd/MM/yyyy hh:mm tt"));
                            AddRow(table, "Días perdidos",
                                $"{inc.DiasPerdidos} días");
                            AddRow(table, "Atención médica",
                                inc.AtencionMedica ?? "—");
                            AddRow(table, "Testigos",
                                string.IsNullOrEmpty(inc.Testigos)
                                    ? "—" : inc.Testigos);
                            AddRow(table, "Notificado MTRAB",
                                inc.NotificarMTRAB ? "Sí" : "No");
                        });

                        // GPS
                        if (inc.GpsCapturado)
                        {
                            col.Item().PaddingTop(4)
                                .Background(Colors.Green.Lighten5)
                                .Padding(8).Row(row =>
                                {
                                    row.AutoItem().Text("📍").FontSize(11);
                                    row.RelativeItem().PaddingLeft(5)
                                .Text($"Geolocalización: " +
                                      $"{inc.Latitud:F6}, {inc.Longitud:F6}" +
                                      $" (±{inc.PrecisionGps:F0}m)")
                                .FontSize(9).FontColor(Colors.Green.Darken2);
                                });
                        }

                        // Fotografías
                        col.Item().PaddingTop(4).Text("Evidencia fotográfica")
                            .FontSize(11).Bold();
                        col.Item().Background(Colors.Grey.Lighten4).Padding(8)
                            .Text($"{inc.CantidadFotos} fotografía(s) capturada(s) " +
                                  $"y sellada(s) con GPS y timestamp en el dispositivo.")
                            .FontSize(9).FontColor(Colors.Grey.Darken1);

                        // Acciones correctivas
                        if (inc.AccionesCorrectivas.Any())
                        {
                            col.Item().PaddingTop(4)
                                .Text("Acciones correctivas").FontSize(12).Bold();

                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(c =>
                                {
                                    c.RelativeColumn(3);
                                    c.RelativeColumn(2);
                                    c.RelativeColumn(2);
                                    c.RelativeColumn(1);
                                });

                                foreach (var h in new[]
                                    { "Descripción", "Responsable",
                              "Fecha límite", "Estado" })
                                {
                                    table.Cell()
                                        .Background(Colors.Grey.Lighten3)
                                        .Padding(4).Text(h).FontSize(9).Bold();
                                }

                                var idx = 0;
                                foreach (var a in inc.AccionesCorrectivas)
                                {
                                    var rowBg = idx % 2 == 0
                                        ? Colors.White : Colors.Grey.Lighten5;
                                    table.Cell().Background(rowBg).Padding(4)
                                        .Text(a.Descripcion).FontSize(9);
                                    table.Cell().Background(rowBg).Padding(4)
                                        .Text(a.Responsable).FontSize(9);
                                    table.Cell().Background(rowBg).Padding(4)
                                        .Text(a.FechaLimite.ToString("dd/MM/yyyy"))
                                        .FontSize(9);
                                    table.Cell().Background(rowBg).Padding(4)
                                        .Text(a.Estado).FontSize(9);
                                    idx++;
                                }
                            });
                        }

                        // Firma
                        col.Item().PaddingTop(8).Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Firma del inspector")
                                    .FontSize(10).Bold();
                                c.Item().PaddingTop(4)
                                    .Border(1).BorderColor(Colors.Grey.Lighten2)
                                    .Height(50)
                                    .Background(inc.Firmado
                                        ? Colors.Green.Lighten5
                                        : Colors.Grey.Lighten4)
                                    .AlignCenter().AlignMiddle()
                                    .Text(inc.Firmado
                                        ? "✓ Firmado digitalmente"
                                        : "Sin firma")
                                    .FontSize(9)
                                    .FontColor(inc.Firmado
                                        ? Colors.Green.Darken1
                                        : Colors.Grey.Darken1);
                            });
                            row.ConstantItem(20);
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Base legal").FontSize(10).Bold();
                                c.Item().PaddingTop(4)
                                    .Background(Colors.Grey.Lighten4).Padding(8)
                                    .Text("Art. 10.2, Reglamento 522-06 — " +
                                          "Todo accidente de trabajo debe ser " +
                                          "investigado y notificado en un plazo " +
                                          "no mayor de 24-48 horas.")
                                    .FontSize(8).FontColor(Colors.Grey.Darken1);
                            });
                        });
                    });

                    page.Footer().Column(col =>
                    {
                        col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                        col.Item().PaddingTop(5).Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Documento sellado digitalmente")
                                    .FontSize(8).Bold();
                                c.Item().Text($"SHA-256: {inc.HashSha256}")
                                    .FontSize(7).FontColor(Colors.Grey.Darken1)
                                    .FontFamily("Courier New");
                            });
                            row.ConstantItem(120).AlignRight()
                                .Text(inc.Firmado
                                    ? "Firmado digitalmente"
                                    : "Sin firma")
                                .FontSize(8)
                                .FontColor(inc.Firmado
                                    ? Colors.Green.Darken1
                                    : Colors.Red.Darken1);
                        });
                        col.Item().AlignCenter().PaddingTop(3)
                            .Text("Generado por SST-Digital RD · " +
                                  "Conforme al Reglamento 522-06")
                            .FontSize(7).FontColor(Colors.Grey.Medium);
                    });
                });
            });

            return documento.GeneratePdf();
        }

        public byte[] GenerarDossierPdf(ReporteStatusDto reporte,
    string nombreEmpresa)
        {
            var documento = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.Letter);
                    page.Margin(40);
                    page.DefaultTextStyle(x =>
                        x.FontSize(10).FontFamily("Arial"));

                    page.Header().Column(col =>
                    {
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("SST-Digital RD")
                                    .FontSize(16).Bold()
                                    .FontColor(Colors.Blue.Darken2);
                                c.Item().Text(
                                    $"Reporte {reporte.Letra} — {reporte.Titulo}")
                                    .FontSize(11)
                                    .FontColor(Colors.Grey.Darken1);
                            });
                            row.ConstantItem(130).AlignRight().Column(c =>
                            {
                                c.Item().Text($"Período: {reporte.Periodo}")
                                    .FontSize(9)
                                    .FontColor(Colors.Grey.Darken1);
                                c.Item().Text(
                                    $"Generado: {reporte.FechaGeneracion}")
                                    .FontSize(9)
                                    .FontColor(Colors.Grey.Darken1);
                            });
                        });
                        col.Item().PaddingTop(8).LineHorizontal(1)
                            .LineColor(Colors.Grey.Lighten2);
                    });

                    page.Content().PaddingVertical(15).Column(col =>
                    {
                        col.Spacing(12);

                        // Banner de estado
                        var (bg, fg) = reporte.Estado switch
                        {
                            "generado" => (Colors.Green.Lighten4,
                                           Colors.Green.Darken2),
                            "urgente" => (Colors.Red.Lighten4,
                                           Colors.Red.Darken2),
                            _ => (Colors.Orange.Lighten4,
                                           Colors.Orange.Darken2)
                        };

                        col.Item().Background(bg).Padding(10).Row(row =>
                        {
                            row.RelativeItem()
                                .Text($"{reporte.Letra} — {reporte.Titulo}")
                                .FontSize(13).Bold().FontColor(fg);
                            row.AutoItem()
                                .Text($"{reporte.Completitud}% completo")
                                .FontSize(10).FontColor(fg);
                        });

                        // Datos del reporte
                        col.Item().Text("Identificación del reporte")
                            .FontSize(12).Bold();

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(1);
                                c.RelativeColumn(2);
                            });

                            AddRow(table, "Empresa", nombreEmpresa);
                            AddRow(table, "Reporte",
                                $"{reporte.Letra} — {reporte.Titulo}");
                            AddRow(table, "Base legal", reporte.BaseLegal);
                            AddRow(table, "Frecuencia", reporte.Frecuencia);
                            AddRow(table, "Período", reporte.Periodo);
                            AddRow(table, "Estado", reporte.Estado
                                .ToUpper());
                            AddRow(table, "Detalle", reporte.Detalle);
                            AddRow(table, "Registros incluidos",
                                reporte.RegistrosIncluidos);
                        });

                        // Resumen de contenido
                        col.Item().PaddingTop(4)
                            .Text("Contenido del reporte")
                            .FontSize(12).Bold();

                        col.Item().Background(Colors.Grey.Lighten4)
                            .Padding(12).Column(c =>
                            {
                                c.Item().Text(reporte.RegistrosIncluidos)
                            .FontSize(10);
                                if (!string.IsNullOrEmpty(reporte.MensajePendiente))
                                {
                                    c.Item().PaddingTop(6)
                                .Text(reporte.MensajePendiente)
                                .FontSize(9)
                                .FontColor(Colors.Orange.Darken2);
                                }
                            });

                        // Sección de firma y validación
                        col.Item().PaddingTop(8).Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Responsable SST")
                                    .FontSize(10).Bold();
                                c.Item().PaddingTop(4)
                                    .Border(1)
                                    .BorderColor(Colors.Grey.Lighten2)
                                    .Height(60)
                                    .Background(Colors.Grey.Lighten4)
                                    .AlignCenter().AlignMiddle()
                                    .Text("Firma del Responsable de SST")
                                    .FontSize(9)
                                    .FontColor(Colors.Grey.Darken1);
                            });
                            row.ConstantItem(20);
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Revisado por")
                                    .FontSize(10).Bold();
                                c.Item().PaddingTop(4)
                                    .Border(1)
                                    .BorderColor(Colors.Grey.Lighten2)
                                    .Height(60)
                                    .Background(Colors.Grey.Lighten4)
                                    .AlignCenter().AlignMiddle()
                                    .Text("Firma del Revisor")
                                    .FontSize(9)
                                    .FontColor(Colors.Grey.Darken1);
                            });
                        });
                    });

                    page.Footer().Column(col =>
                    {
                        col.Item().LineHorizontal(1)
                            .LineColor(Colors.Grey.Lighten2);
                        col.Item().PaddingTop(5).Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Documento sellado digitalmente")
                                    .FontSize(8).Bold();
                                c.Item().Text($"SHA-256: {reporte.HashSha256}")
                                    .FontSize(7)
                                    .FontColor(Colors.Grey.Darken1)
                                    .FontFamily("Courier New");
                            });
                            row.ConstantItem(120).AlignRight()
                                .Text($"Reg. 522-06 · {reporte.BaseLegal}")
                                .FontSize(7)
                                .FontColor(Colors.Grey.Medium);
                        });
                        col.Item().AlignCenter().PaddingTop(3)
                            .Text("Generado por SST-Digital RD · " +
                                  "Conforme al Reglamento 522-06")
                            .FontSize(7).FontColor(Colors.Grey.Medium);
                    });
                });
            });

            return documento.GeneratePdf();
        }

        private static void AddRow(QuestPDF.Fluent.TableDescriptor table, string etiqueta, string valor)
        {
            table.Cell().PaddingVertical(2).Text(etiqueta).FontSize(9).FontColor(Colors.Grey.Darken1);
            table.Cell().PaddingVertical(2).Text(valor).FontSize(9.5f).Bold();
        }
    }
}
