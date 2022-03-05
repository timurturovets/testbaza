namespace TestBaza.Repositories
{
    public interface IQuestionsRepository
    {
        Question? GetQuestion(int id);
        Question? GetQuestionByTestAndNumber(Test test, int number);
        void AddQuestion(Question question);
        void UpdateQuestion(Question question);
        void DeleteQuestion(Question question);
    }
}
