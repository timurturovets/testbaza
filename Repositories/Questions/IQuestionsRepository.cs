namespace TestBaza.Repositories
{
    public interface IQuestionsRepository
    {
        Question? GetQuestion(int id);
        Question? GetQuestionByTestAndNumber(Test test, int number);
        void AddQuestion(Question question);
        AnswerInfo AddAnswerToQuestion(Question question);
        void RemoveAnswerFromQuestion(Question question, Answer answer);
        void UpdateQuestion(Question question);
        void DeleteQuestion(Question question);
    }
}
