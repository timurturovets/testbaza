using TestBaza.Data;
using TestBaza.Models;

namespace TestBaza.Repositories
{
    public class RatesRepository : IRatesRepository
    {
        private readonly AppDbContext _context;
        public RatesRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task AddRateAsync(Rate rate)
        {
            _context.Rates.Add(rate);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateRateAsync(Rate rate)
        {
            _context.Rates.Update(rate);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteRateAsync(Rate rate)
        {
            _context.Rates.Remove(rate);
            await _context.SaveChangesAsync();
        }
    }
}
