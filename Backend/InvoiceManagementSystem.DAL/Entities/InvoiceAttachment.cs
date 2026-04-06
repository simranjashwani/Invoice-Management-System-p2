using System;

namespace InvoiceManagementSystem.DAL.Entities
{
    public class InvoiceAttachment
    {
        public int InvoiceAttachmentId { get; set; }

        public int InvoiceId { get; set; }

        public string FileName { get; set; } = string.Empty;

        public string StoredFileName { get; set; } = string.Empty;

        public string ContentType { get; set; } = string.Empty;

        public long FileSize { get; set; }

        public string RelativePath { get; set; } = string.Empty;

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public Invoice? Invoice { get; set; }
    }
}
