using Microsoft.EntityFrameworkCore;

using TestBaza.Data;

namespace TestBaza.Repositories
{
    public class QuestionsRepository : IQuestionsRepository
    {
        private readonly AppDbContext _context;
        public QuestionsRepository(AppDbContext context)
        {
            _context = context;
        }

        public Question? GetQuestion(int id)
        {
            return _context.Questions
                .Where(q => q.QuestionId == id)
                .Include(q => q.Test)
                .SingleOrDefault();
        }

        public void UpdateQuestion(Question question)
        {
            _context.Questions.Update(question);
            _context.SaveChanges();
        }
    }
}
