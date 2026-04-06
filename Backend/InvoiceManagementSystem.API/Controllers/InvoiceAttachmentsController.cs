using InvoiceManagementSystem.DAL.Data;
using InvoiceManagementSystem.DAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InvoiceManagementSystem.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/invoices/{invoiceId}/attachments")]
    public class InvoiceAttachmentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public InvoiceAttachmentsController(
            ApplicationDbContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [Authorize(Roles = "FinanceUser,FinanceManager,Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAttachments(int invoiceId)
        {
            var attachments = await _context.InvoiceAttachments
                .AsNoTracking()
                .Where(attachment => attachment.InvoiceId == invoiceId)
                .OrderByDescending(attachment => attachment.UploadedAt)
                .Select(attachment => new
                {
                    attachment.InvoiceAttachmentId,
                    attachment.InvoiceId,
                    attachment.FileName,
                    attachment.ContentType,
                    attachment.FileSize,
                    attachment.UploadedAt,
                    downloadUrl = $"/api/invoices/{invoiceId}/attachments/{attachment.InvoiceAttachmentId}/download"
                })
                .ToListAsync();

            return Ok(attachments);
        }

        [Authorize(Roles = "FinanceUser,Admin")]
        [HttpPost]
        [RequestSizeLimit(10 * 1024 * 1024)]
        public async Task<IActionResult> UploadAttachment(int invoiceId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "A file is required." });

            var invoiceExists = await _context.Invoices.AnyAsync(invoice => invoice.InvoiceId == invoiceId);

            if (!invoiceExists)
                return NotFound(new { message = "Invoice not found." });

            var uploadsRoot = Path.Combine(_environment.ContentRootPath, "UploadedFiles", "Invoices", invoiceId.ToString());
            Directory.CreateDirectory(uploadsRoot);

            var storedFileName = $"{Guid.NewGuid():N}{Path.GetExtension(file.FileName)}";
            var fullPath = Path.Combine(uploadsRoot, storedFileName);

            await using (var stream = System.IO.File.Create(fullPath))
            {
                await file.CopyToAsync(stream);
            }

            var attachment = new InvoiceAttachment
            {
                InvoiceId = invoiceId,
                FileName = file.FileName,
                StoredFileName = storedFileName,
                ContentType = file.ContentType ?? "application/octet-stream",
                FileSize = file.Length,
                RelativePath = Path.Combine("UploadedFiles", "Invoices", invoiceId.ToString(), storedFileName),
                UploadedAt = DateTime.UtcNow
            };

            _context.InvoiceAttachments.Add(attachment);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                attachment.InvoiceAttachmentId,
                attachment.InvoiceId,
                attachment.FileName,
                attachment.ContentType,
                attachment.FileSize,
                attachment.UploadedAt,
                downloadUrl = $"/api/invoices/{invoiceId}/attachments/{attachment.InvoiceAttachmentId}/download"
            });
        }

        [Authorize(Roles = "FinanceUser,FinanceManager,Admin")]
        [HttpGet("{attachmentId}/download")]
        public async Task<IActionResult> DownloadAttachment(int invoiceId, int attachmentId)
        {
            var attachment = await _context.InvoiceAttachments
                .AsNoTracking()
                .FirstOrDefaultAsync(item =>
                    item.InvoiceId == invoiceId &&
                    item.InvoiceAttachmentId == attachmentId);

            if (attachment == null)
                return NotFound(new { message = "Attachment not found." });

            var fullPath = Path.Combine(_environment.ContentRootPath, attachment.RelativePath);

            if (!System.IO.File.Exists(fullPath))
                return NotFound(new { message = "Stored file not found." });

            var bytes = await System.IO.File.ReadAllBytesAsync(fullPath);
            return File(bytes, attachment.ContentType, attachment.FileName);
        }

        [Authorize(Roles = "Admin,FinanceUser")]
        [HttpDelete("{attachmentId}")]
        public async Task<IActionResult> DeleteAttachment(int invoiceId, int attachmentId)
        {
            var attachment = await _context.InvoiceAttachments
                .FirstOrDefaultAsync(item =>
                    item.InvoiceId == invoiceId &&
                    item.InvoiceAttachmentId == attachmentId);

            if (attachment == null)
                return NotFound(new { message = "Attachment not found." });

            var fullPath = Path.Combine(_environment.ContentRootPath, attachment.RelativePath);

            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }

            _context.InvoiceAttachments.Remove(attachment);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Attachment deleted successfully." });
        }
    }
}
