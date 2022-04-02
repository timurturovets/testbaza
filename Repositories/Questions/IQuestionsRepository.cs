namespace TestBaza.Repositories
{
    public interface IQuestionsRepository
    {
        Task<Question?> GetQuestionAsync(int id);
        Task<Question?> GetQuestionByTestAndNumberAsync(Test test, int number);
        Task AddQuestionAsync(Question question);
        Task<AnswerInfo> AddAnswerToQuestionAsync(Question question);
        Task RemoveAnswerFromQuestionAsync(Question question, Answer answer);
        Task UpdateQuestionAsync(Question question);
        Task DeleteQuestionAsync(Question question);
    }
}
