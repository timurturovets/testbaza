namespace TestBaza.Factories
{
    public class QuestionFactory : IQuestionFactory
    {
        public Question Create(Test test, int number)
        {
            return new Question
            {
                Test = test,
                Number = number
            };
        }
    }
}
