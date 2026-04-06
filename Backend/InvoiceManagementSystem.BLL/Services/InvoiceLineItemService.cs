using InvoiceManagementSystem.BLL.Interfaces;
using InvoiceManagementSystem.BLL.Models;
using InvoiceManagementSystem.DAL.Entities;
using InvoiceManagementSystem.DAL.Enums;
using InvoiceManagementSystem.DAL.Repositories;

namespace InvoiceManagementSystem.BLL.Services
{
    public class InvoiceLineItemService : IInvoiceLineItemService
    {
        private readonly IInvoiceLineItemRepository _lineItemRepository;
        private readonly IInvoiceRepository _invoiceRepository;

        public InvoiceLineItemService(
            IInvoiceLineItemRepository lineItemRepository,
            IInvoiceRepository invoiceRepository)
        {
            _lineItemRepository = lineItemRepository;
            _invoiceRepository = invoiceRepository;
        }

        public async Task<IEnumerable<InvoiceLineItemDto>> GetByInvoiceIdAsync(int invoiceId)
        {
            var invoice = await _invoiceRepository.GetByIdAsync(invoiceId);
            if (invoice == null)
                throw new Exception("Invoice not found");

            var items = await _lineItemRepository.GetByInvoiceIdAsync(invoiceId);

            return items.Select(item => new InvoiceLineItemDto
            {
                LineItemId = item.LineItemId,
                InvoiceId = item.InvoiceId,
                Description = item.Description,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                Discount = item.Discount,
                Tax = item.Tax,
                LineTotal = item.LineTotal
            });
        }

        public async Task<InvoiceLineItemDto> AddLineItemAsync(int invoiceId, AddInvoiceLineItemDto dto)
        {
            var invoice = await _invoiceRepository.GetByIdAsync(invoiceId);
            if (invoice == null)
                throw new Exception("Invoice not found");

            if (invoice.Status == InvoiceStatus.Paid)
                throw new Exception("Paid invoice cannot be modified");

            if (dto.Quantity <= 0)
                throw new Exception("Quantity must be greater than zero");

            if (dto.UnitPrice < 0)
                throw new Exception("Unit price cannot be negative");

            var item = new InvoiceLineItem
            {
                InvoiceId = invoiceId,
                Description = dto.Description,
                Quantity = dto.Quantity,
                UnitPrice = dto.UnitPrice,
                Discount = dto.Discount,
                Tax = dto.Tax,
                LineTotal = CalculateLineTotal(dto.Quantity, dto.UnitPrice, dto.Discount, dto.Tax)
            };

            var createdItem = await _lineItemRepository.AddAsync(item);

            await RecalculateInvoiceTotals(invoiceId);

            return new InvoiceLineItemDto
            {
                LineItemId = createdItem.LineItemId,
                InvoiceId = createdItem.InvoiceId,
                Description = createdItem.Description,
                Quantity = createdItem.Quantity,
                UnitPrice = createdItem.UnitPrice,
                Discount = createdItem.Discount,
                Tax = createdItem.Tax,
                LineTotal = createdItem.LineTotal
            };
        }

        public async Task<InvoiceLineItemDto?> UpdateLineItemAsync(int invoiceId, int itemId, UpdateInvoiceLineItemDto dto)
        {
            var invoice = await _invoiceRepository.GetByIdAsync(invoiceId);
            if (invoice == null)
                throw new Exception("Invoice not found");

            if (invoice.Status == InvoiceStatus.Paid)
                throw new Exception("Paid invoice cannot be modified");

            var existingItem = await _lineItemRepository.GetByIdAsync(itemId);
            if (existingItem == null || existingItem.InvoiceId != invoiceId)
                return null;

            if (dto.Quantity <= 0)
                throw new Exception("Quantity must be greater than zero");

            if (dto.UnitPrice < 0)
                throw new Exception("Unit price cannot be negative");

            existingItem.Description = dto.Description;
            existingItem.Quantity = dto.Quantity;
            existingItem.UnitPrice = dto.UnitPrice;
            existingItem.Discount = dto.Discount;
            existingItem.Tax = dto.Tax;
            existingItem.LineTotal = CalculateLineTotal(dto.Quantity, dto.UnitPrice, dto.Discount, dto.Tax);

            var updatedItem = await _lineItemRepository.UpdateAsync(existingItem);

            await RecalculateInvoiceTotals(invoiceId);

            if (updatedItem == null)
                return null;

            return new InvoiceLineItemDto
            {
                LineItemId = updatedItem.LineItemId,
                InvoiceId = updatedItem.InvoiceId,
                Description = updatedItem.Description,
                Quantity = updatedItem.Quantity,
                UnitPrice = updatedItem.UnitPrice,
                Discount = updatedItem.Discount,
                Tax = updatedItem.Tax,
                LineTotal = updatedItem.LineTotal
            };
        }

        public async Task<bool> DeleteLineItemAsync(int invoiceId, int itemId)
        {
            var invoice = await _invoiceRepository.GetByIdAsync(invoiceId);
            if (invoice == null)
                throw new Exception("Invoice not found");

            if (invoice.Status == InvoiceStatus.Paid)
                throw new Exception("Paid invoice cannot be modified");

            var existingItem = await _lineItemRepository.GetByIdAsync(itemId);
            if (existingItem == null || existingItem.InvoiceId != invoiceId)
                return false;

            var deleted = await _lineItemRepository.DeleteAsync(itemId);

            if (deleted)
            {
                await RecalculateInvoiceTotals(invoiceId);
            }

            return deleted;
        }

        private decimal CalculateLineTotal(int quantity, decimal unitPrice, decimal discount, decimal tax)
        {
            return (quantity * unitPrice) - discount + tax;
        }

        private async Task RecalculateInvoiceTotals(int invoiceId)
        {
            var invoice = await _invoiceRepository.GetByIdAsync(invoiceId);
            if (invoice == null)
                throw new Exception("Invoice not found");

            invoice.SubTotal = invoice.LineItems.Sum(li => li.LineTotal);
            invoice.GrandTotal = invoice.SubTotal - invoice.Discount + invoice.Tax;
            invoice.OutstandingBalance = invoice.GrandTotal - invoice.Payments.Sum(p => p.PaymentAmount);

            if (invoice.OutstandingBalance <= 0 && invoice.GrandTotal > 0)
            {
                invoice.OutstandingBalance = 0;
                invoice.Status = InvoiceStatus.Paid;
            }
            else if (invoice.OutstandingBalance < invoice.GrandTotal && invoice.OutstandingBalance > 0)
            {
                invoice.Status = InvoiceStatus.PartiallyPaid;
            }
            else if (invoice.OutstandingBalance == invoice.GrandTotal)
            {
                invoice.Status = InvoiceStatus.Draft;
            }

            await _invoiceRepository.UpdateAsync(invoice);
        }
    }
}