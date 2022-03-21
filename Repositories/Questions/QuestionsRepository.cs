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
                .Include(q=>q.Test).ThenInclude(t=>t!.Creator)
                .Include(q=>q.MultipleAnswers)
                .SingleOrDefault();
        }

        public Question? GetQuestionByTestAndNumber(Test test, int number)
        {
            return test.Questions
                .Where(q => q.Number == number).AsQueryable()
                .Include(q => q.Test).ThenInclude(t => t!.Creator)
                .Include(q => q.MultipleAnswers)
                .SingleOrDefault();
        }
        public void AddQuestion(Question question)
        {
            _context.Questions.Add(question);
            _context.SaveChanges();
        }
        public AnswerInfo AddAnswerToQuestion(Question question)
        {
            int number = question.MultipleAnswers.Count() + 1;
            Answer answer = new()
            {
                Number = number,
                Question = question
            };
            _context.Answers.Add(answer);
            _context.SaveChanges();
            _logger.LogError($"Number: {number}");

            Answer createdAnswer = _context.Answers
                .Include(a => a.Question)
                .Where(a => a.Question!.Equals(question) && a.Number == number)
                .Single();
            int id = createdAnswer.AnswerId;

            return new AnswerInfo(id, number);
        }
        public void RemoveAnswerFromQuestion(Question question, Answer answer)
        {
            int number = answer.Number;
            _context.Answers.Remove(answer);
            _context.SaveChanges();

            question = GetQuestion(question.QuestionId)!;

            foreach(Answer a in question.MultipleAnswers)
            {
                if (a.Number < number) continue;
                a.Number--;
                _context.Answers.Update(a);
            }
            _context.Questions.Update(question);
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
    public record class AnswerInfo(int Id, int Number);
}