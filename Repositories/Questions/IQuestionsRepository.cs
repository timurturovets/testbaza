namespace TestBaza.Repositories
{
    public interface IQuestionsRepository
    {
        Question? GetQuestion(int id);
        Question? GetQuestionByTestAndNumber(Test test, int number);
        void AddQuestion(Question question);

        /// <summary>
        /// Creates a new empty answer associated with the given question.
        /// </summary>
        /// <param name="question">A question in which new answer should be created.</param>
        /// <returns>Tuple with answer ID as a first item and answer number in question as a second one.</returns>
        (int, int) AddAnswerToQuestion(Question question);
        void RemoveAnswerFromQuestion(Question question, Answer answer);
        void UpdateQuestion(Question question);
        void DeleteQuestion(Question question);
    }
}
