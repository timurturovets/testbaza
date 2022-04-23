namespace TestBaza.Factories
{
    public interface IQuestionFactory
    {
        Question Create(Test test, int number);
    }
}