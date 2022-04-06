namespace TestBaza.Repositories
{
    public interface IQuestionsRepository
    {
        Task<Question?> GetQuestionAsync(int id);
        Question? GetQuestion(Test test, int questionNumber);
        Task AddQuestionAsync(Question question);
        Task<AnswerInfo> AddAnswerToQuestionAsync(Question question);
        Task RemoveAnswerFromQuestionAsync(Question question, Answer answer);
        Task UpdateQuestionAsync(Question question);
        Task DeleteQuestionAsync(Question question);
    }
}
