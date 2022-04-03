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

        public async Task<Question?> GetQuestionAsync(int id)
        {
            return await _context.Questions
                .Where(q => q.QuestionId == id)
                .Include(q=>q.Test).ThenInclude(t=>t!.Creator)
                .Include(q=>q.MultipleAnswers)
                .SingleOrDefaultAsync();
        }

        public async Task<Question?> GetQuestionByTestAndNumberAsync(Test test, int number)
        {
            return await test.Questions
                .Where(q => q.Number == number).AsQueryable()
                .Include(q => q.Test).ThenInclude(t => t!.Creator)
                .Include(q => q.MultipleAnswers)
                .SingleOrDefaultAsync();
        }
        public async Task AddQuestionAsync(Question question)
        {
            await _context.Questions.AddAsync(question);
            await _context.SaveChangesAsync();
        }
        public async Task<AnswerInfo> AddAnswerToQuestionAsync(Question question)
        {
            int number = question.MultipleAnswers.Count() + 1;
            Answer answer = new()
            {
                Number = number,
                Question = question
            };
            await _context.Answers.AddAsync(answer);
            await _context.SaveChangesAsync();

            return new AnswerInfo(answer.AnswerId, number);
        }
        public async Task RemoveAnswerFromQuestionAsync(Question question, Answer answer)
        {
            int number = answer.Number;
            _context.Answers.Remove(answer);
            await _context.SaveChangesAsync();

            question = (await GetQuestionAsync(question.QuestionId))!;

            foreach(Answer a in question.MultipleAnswers)
            {
                if (a.Number < number) continue;
                a.Number--;
                _context.Answers.Update(a);
            }
            _context.Questions.Update(question);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateQuestionAsync(Question question)
        {
            _context.Questions.Update(question);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteQuestionAsync(Question question)
        {
            int number = question.Number;
            int testId = question.Test!.TestId;

            _context.Questions.Remove(question);
            await _context.SaveChangesAsync();

            Test test = (await _testsRepo.GetTestAsync(testId))!;

            foreach(Question q in test.Questions)
            {
                if (q.Number < number) continue;
                q.Number--;
                _context.Questions.Update(q);
            }

            _context.Tests.Update(test);
            await _context.SaveChangesAsync();
        }
    }
    public record class AnswerInfo(int Id, int Number);
}