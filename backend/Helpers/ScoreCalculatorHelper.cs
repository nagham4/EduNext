namespace backend.Helpers;

public static class ScoreCalculatorHelper
{
    public static ScoreCalculationResult Calculate(
        IEnumerable<QuestionKey> questions,
        IDictionary<Guid, string?> answersByQuestionId)
    {
        var questionList = questions.ToList();
        var total = questionList.Count;

        var details = new List<QuestionEvaluation>();
        var correctCount = 0;

        foreach (var q in questionList)
        {
            answersByQuestionId.TryGetValue(q.QuestionId, out var selected);

            var selectedNorm = NormalizeChoice(selected);
            var correctNorm = NormalizeChoice(q.CorrectAnswer);

            var isCorrect = !string.IsNullOrWhiteSpace(selectedNorm)
                            && !string.IsNullOrWhiteSpace(correctNorm)
                            && selectedNorm == correctNorm;

            if (isCorrect) correctCount++;

            details.Add(new QuestionEvaluation
            {
                QuestionId = q.QuestionId,
                SelectedAnswer = selectedNorm,
                CorrectAnswer = correctNorm,
                IsCorrect = isCorrect
            });
        }

        var scorePercent = total == 0 ? 0 : (int)Math.Round(correctCount * 100.0 / total);

        return new ScoreCalculationResult
        {
            ScorePercent = scorePercent,
            CorrectCount = correctCount,
            TotalQuestions = total,
            Details = details
        };
    }

    private static string? NormalizeChoice(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;

        var v = value.Trim().ToUpper();

        // allow only A/B/C/D (optional strict)
        return v is "A" or "B" or "C" or "D" ? v : null;
    }
}

public class QuestionKey
{
    public Guid QuestionId { get; set; }
    public string? CorrectAnswer { get; set; }
}

public class ScoreCalculationResult
{
    public int ScorePercent { get; set; }
    public int CorrectCount { get; set; }
    public int TotalQuestions { get; set; }
    public List<QuestionEvaluation> Details { get; set; } = new();
}

public class QuestionEvaluation
{
    public Guid QuestionId { get; set; }
    public string? SelectedAnswer { get; set; }
    public string? CorrectAnswer { get; set; }
    public bool IsCorrect { get; set; }
}