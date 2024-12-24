namespace RentAutomation.Models
{
    public class UploadedFile
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public byte[] FileContent { get; set; }
        public string MimeType { get; set; } // MIME type of the file
        public long FileSize { get; set; } // Size in bytes
        public DateTime UploadDate { get; set; }
        public string? Description { get; set; } // Optional description
    }
}
