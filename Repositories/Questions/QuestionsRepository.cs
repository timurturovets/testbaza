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

        public Question? GetQuestion(Test test, int questionNumber)
        {
            return test.Questions
                .Where(q => q.Number == questionNumber).AsQueryable()
                .Include(q => q.Test).ThenInclude(t => t!.Creator)
                .Include(q => q.MultipleAnswers)
                .SingleOrDefault();
        }
        public async Task AddQuestionAsync(Question question)
        {
            await _context.Questions.AddAsync(question);
            await _context.SaveChangesAsync();
        }
        public async Task<AnswerInfo> AddAnswerToQuestionAsync(Question question)
        {
            var number = question.MultipleAnswers.Count() + 1;
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
            var number = answer.Number;
            _context.Answers.Remove(answer);
            await _context.SaveChangesAsync();

            question = (await GetQuestionAsync(question.QuestionId))!;

            foreach(var a in question.MultipleAnswers)
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
            var number = question.Number;
            var testId = question.Test!.TestId;

            if (question.HasImage)
            {
                var path = question.ImagePhysicalPath;
                if(File.Exists(path)) File.Delete(path);
            }
            
            _context.Questions.Remove(question);
            await _context.SaveChangesAsync();

            var test = (await _testsRepo.GetTestAsync(testId))!;

            foreach(var q in test.Questions)
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