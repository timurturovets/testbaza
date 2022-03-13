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
        public void AddRate(Rate rate)
        {
            _context.Rates.Add(rate);
            _context.SaveChanges();
        }
        public void UpdateRate(Rate rate)
        {
            _context.Rates.Update(rate);
            _context.SaveChanges();
        }
        public void DeleteRate(Rate rate)
        {
            _context.Rates.Remove(rate);
            _context.SaveChanges();
        }
    }
}
