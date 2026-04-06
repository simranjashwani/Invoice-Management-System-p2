using InvoiceManagementSystem.DAL.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

public class InvoiceAnalyticsService
{
    private readonly ApplicationDbContext _context;
    private readonly IDistributedCache _cache;

    private const string OutstandingKey = "analytics:outstanding";
    private const string RevenueKey = "analytics:revenue";
    private const string DsoKey = "analytics:dso";
    private const string AgingKey = "analytics:aging";

    public InvoiceAnalyticsService(ApplicationDbContext context, IDistributedCache cache)
    {
        _context = context;
        _cache = cache;
    }

    private async Task<T> GetOrSetCache<T>(string key, Func<Task<T>> getData)
    {
        var cached = await _cache.GetStringAsync(key);

        if (!string.IsNullOrWhiteSpace(cached))
            return JsonSerializer.Deserialize<T>(cached)!;

        var data = await getData();

        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        };

        await _cache.SetStringAsync(key, JsonSerializer.Serialize(data), options);
        return data;
    }

    public async Task<object> GetOutstanding()
    {
        return await GetOrSetCache(OutstandingKey, async () =>
        {
            var totalOutstanding = await _context.Invoices
                .AsNoTracking()
                .SumAsync(i => i.OutstandingBalance);

            return new
            {
                totalOutstanding
            };
        });
    }

    public async Task<object> GetRevenue()
    {
        return await GetOrSetCache(RevenueKey, async () =>
        {
            var totalRevenue = await _context.Invoices
                .AsNoTracking()
                .SumAsync(i => i.GrandTotal);

            return new
            {
                totalRevenue
            };
        });
    }

    public async Task<object> GetDSO()
    {
        return await GetOrSetCache(DsoKey, async () =>
        {
            var totalOutstanding = await _context.Invoices
                .AsNoTracking()
                .SumAsync(i => i.OutstandingBalance);

            var totalSales = await _context.Invoices
                .AsNoTracking()
                .SumAsync(i => i.GrandTotal);

            if (totalSales == 0)
            {
                return new
                {
                    daysSalesOutstanding = 0d
                };
            }

            return new
            {
                daysSalesOutstanding = (double)(totalOutstanding / totalSales) * 30
            };
        });
    }

    public async Task<object> GetAging()
    {
        return await GetOrSetCache(AgingKey, async () =>
        {
            var today = DateTime.Today;

            var invoices = await _context.Invoices
                .AsNoTracking()
                .ToListAsync();

            var buckets = new[]
            {
                new { Bucket = "Current", Min = int.MinValue, Max = 0 },
                new { Bucket = "1 - 30 Days", Min = 1, Max = 30 },
                new { Bucket = "31 - 60 Days", Min = 31, Max = 60 },
                new { Bucket = "61+ Days", Min = 61, Max = int.MaxValue }
            };

            return buckets.Select(bucket =>
            {
                var bucketInvoices = invoices.Where(invoice =>
                {
                    var daysPastDue = (today - invoice.DueDate.Date).Days;
                    return daysPastDue >= bucket.Min && daysPastDue <= bucket.Max;
                }).ToList();

                return new
                {
                    bucket = bucket.Bucket,
                    invoiceCount = bucketInvoices.Count,
                    totalAmount = bucketInvoices.Sum(invoice => invoice.OutstandingBalance)
                };
            }).ToList();
        });
    }

    public async Task ClearCache()
    {
        await _cache.RemoveAsync(OutstandingKey);
        await _cache.RemoveAsync(RevenueKey);
        await _cache.RemoveAsync(DsoKey);
        await _cache.RemoveAsync(AgingKey);
    }
}
