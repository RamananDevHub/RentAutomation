using System;

namespace RentAutomation.Services
{
    public interface IPdfService
    {
        byte[] CreatePdf(string html);
    }
}
