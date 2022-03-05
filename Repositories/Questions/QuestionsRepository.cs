using Microsoft.EntityFrameworkCore;

using TestBaza.Data;

namespace TestBaza.Repositories
{
    public class QuestionsRepository : IQuestionsRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<QuestionsRepository> _logger;
        private readonly ITestsRepository _testsRepo;
        public QuestionsRepository(AppDbContext context, ILogger<QuestionsRepository> logger, ITestsRepository testsRepo)
        {
            _context = context;
            _logger = logger;
            _testsRepo = testsRepo;
        }

        public Question? GetQuestion(int id)
        {
            return _context.Questions
                .Where(q => q.QuestionId == id)
                .Include(q => q.Test)
                .SingleOrDefault();
        }

        public Question? GetQuestionByTestAndNumber(Test test, int number)
        {
            return test.Questions.SingleOrDefault(q => q.Number == number);
        }
        public void AddQuestion(Question question)
        {
            _context.Questions.Add(question);
            _context.SaveChanges();
        }
        public void UpdateQuestion(Question question)
        {
            _context.Questions.Update(question);
            _context.SaveChanges();
        }

        public void DeleteQuestion(Question question)
        {
            int number = question.Number;
            int testId = question.Test!.TestId;

            _context.Questions.Remove(question);
            _context.SaveChanges();

            Test test = _testsRepo.GetTest(testId)!;

            foreach(Question q in test.Questions)
            {
                if (q.Number < number) continue;
                q.Number--;
                _context.Questions.Update(q);
            }

            _context.Tests.Update(test);
            _context.SaveChanges();
        }
    }
}