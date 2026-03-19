using System.Collections.Generic;

namespace QuizApp
{
    public static class QuizData
    {
        public static List<Question> GetQuestions(int categoryId)
        {
            return DatabaseManager.GetQuestionsByCategory(categoryId);
        }
    }
}
