﻿using DinkToPdf;
using DinkToPdf.Contracts;

namespace RentAutomation.Services
{
    public class PdfService : IPdfService
    {
        private readonly IConverter _converter;

        public PdfService(IConverter converter)
        {
            _converter = converter;
        }

        public byte[] CreatePdf(string html)
        {
            var doc = new HtmlToPdfDocument()
            {
                GlobalSettings = {
                    ColorMode = ColorMode.Color,
                    Orientation = Orientation.Portrait,
                    PaperSize = PaperKind.A4,
                },
                Objects = {
                    new ObjectSettings() {
                        HtmlContent = html,
                        WebSettings = { DefaultEncoding = "utf-8" }
                    }
                }
            };

            return _converter.Convert(doc);
        }
    }
}