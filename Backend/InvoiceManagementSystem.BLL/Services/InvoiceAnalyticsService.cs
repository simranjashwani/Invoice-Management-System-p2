using InvoiceManagementSystem.DAL.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

public class InvoiceAnalyticsService
{
    private readonly ApplicationDbContext _context;
    private readonly IDistributedCache _cache;

    public InvoiceAnalyticsService(ApplicationDbContext context, IDistributedCache cache)
    {
        _context = context;
        _cache = cache;
    }

    // 🔥 CACHE HELPER
    private async Task<T> GetOrSetCache<T>(string key, Func<Task<T>> getData)
    {
        var cached = await _cache.GetStringAsync(key);

        if (cached != null)
            return JsonSerializer.Deserialize<T>(cached)!;

        var data = await getData();

        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        };

        await _cache.SetStringAsync(key, JsonSerializer.Serialize(data), options);

        return data;
    }

    // ✅ 1. Outstanding Total
    public async Task<decimal> GetOutstanding()
    {
        return await GetOrSetCache("outstanding", async () =>
        {
            return await _context.Invoices.SumAsync(i => i.OutstandingBalance);
        });
    }

    // ✅ 2. Revenue Summary
    public async Task<decimal> GetRevenue()
    {
        return await GetOrSetCache("revenue", async () =>
        {
            return await _context.Invoices.SumAsync(i => i.GrandTotal);
        });
    }

    // ✅ 3. DSO
    public async Task<double> GetDSO()
    {
        return await GetOrSetCache("dso", async () =>
        {
            var totalOutstanding = await _context.Invoices.SumAsync(i => i.OutstandingBalance);
            var totalSales = await _context.Invoices.SumAsync(i => i.GrandTotal);

            if (totalSales == 0) return 0;

            return (double)(totalOutstanding / totalSales) * 30; // 30 days
        });
    }

    // ✅ 4. Aging
    public async Task<object> GetAging()
    {
        return await GetOrSetCache("aging", async () =>
        {
            var today = DateTime.Now;

            var invoices = await _context.Invoices.ToListAsync();

            return new
            {
                Current = invoices.Count(i => (today - i.DueDate).Days <= 0),
                Days1To30 = invoices.Count(i => (today - i.DueDate).Days > 0 && (today - i.DueDate).Days <= 30),
                Days31To60 = invoices.Count(i => (today - i.DueDate).Days > 30 && (today - i.DueDate).Days <= 60),
                Days60Plus = invoices.Count(i => (today - i.DueDate).Days > 60)
            };
        });
    }

    // 🔥 CLEAR CACHE
    public async Task ClearCache()
    {
        await _cache.RemoveAsync("outstanding");
        await _cache.RemoveAsync("revenue");
        await _cache.RemoveAsync("dso");
        await _cache.RemoveAsync("aging");
    }
}