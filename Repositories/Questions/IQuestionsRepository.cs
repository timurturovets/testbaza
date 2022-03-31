namespace TestBaza.Repositories
{
    public interface IQuestionsRepository
    {
        Question? GetQuestion(int id);
        Question? GetQuestionByTestAndNumber(Test test, int number);
        Task AddQuestionAsync(Question question);
        Task<AnswerInfo> AddAnswerToQuestionAsync(Question question);
        Task RemoveAnswerFromQuestionAsync(Question question, Answer answer);
        Task UpdateQuestionAsync(Question question);
        Task DeleteQuestionAsync(Question question);
    }
}
