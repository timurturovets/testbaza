namespace TestBaza.Repositories
{
    public interface IQuestionsRepository
    {
        Question? GetQuestion(int id);
        void UpdateQuestion(Question question);
    }
}
