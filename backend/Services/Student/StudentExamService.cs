using backend.DTOs.AI;
using backend.DTOs.Student;
using backend.Helpers;
using backend.Models.Generated;
using backend.Repositories.Student;
using backend.Services.AI;

namespace backend.Services.Student;

public class StudentExamService : IStudentExamService
{
    private readonly IStudentExamRepository _repository;
    private readonly IAiInsightsService _ai;

    public StudentExamService(IStudentExamRepository repository, IAiInsightsService ai)
    {
        _repository = repository;
        _ai = ai;
    }

    public async Task<List<ExamHistoryDto>> GetExamHistoryAsync(Guid userId)
    {
        var results = await _repository.GetExamHistoryAsync(userId);

        return results.Select(result =>
        {
            var normalizedType = NormalizeExamType(result.ExamType);

            return new ExamHistoryDto
            {
                ExamResultId = result.ResultId,
                ExamId = result.ExamId ?? Guid.Empty,
                SubjectId = result.SubjectId ?? Guid.Empty,
                SubjectName = result.SubjectName ?? "",
                SubjectKey = MapSubjectKey(result.SubjectName ?? ""),
                LessonId = result.LessonId,
                LessonTitle = result.LessonTitle,
                Type = normalizedType,
                TypeName = normalizedType == "comprehensive" ? "شامل" : "قصير",
                Score = result.Score,
                QuestionsCount = result.QuestionsCount,
                Percentage = result.Score,
                Date = result.CreatedAt?.ToString("yyyy-MM-dd") ?? ""
            };
        }).ToList();
    }

    public async Task<List<ExamSubjectOptionDto>> GetExamSubjectsAsync(Guid userId)
    {
        var studentStream = await _repository.GetStudentStreamAsync(userId);

        if (string.IsNullOrWhiteSpace(studentStream))
        {
            return new List<ExamSubjectOptionDto>();
        }

        var subjects = await _repository.GetExamSubjectsAsync(studentStream);

        return subjects.Select(subject => new ExamSubjectOptionDto
        {
            SubjectId = subject.SubjectId,
            SubjectName = subject.SubjectName,
            IconKey = MapSubjectIcon(subject.SubjectName),
            Color = MapSubjectColor(subject.SubjectName)
        }).ToList();
    }

    public async Task<List<ExamLessonOptionDto>> GetSubjectLessonsForQuickExamAsync(Guid userId, Guid subjectId)
    {
        var studentStream = await _repository.GetStudentStreamAsync(userId);

        if (string.IsNullOrWhiteSpace(studentStream))
        {
            return new List<ExamLessonOptionDto>();
        }

        var subject = await _repository.GetSubjectByIdAndStreamAsync(subjectId, studentStream);

        if (subject == null)
        {
            return new List<ExamLessonOptionDto>();
        }

        var lessons = await _repository.GetQuickExamLessonsAsync(subjectId);

        return lessons.Select(lesson => new ExamLessonOptionDto
        {
            LessonId = lesson.LessonId,
            LessonTitle = lesson.LessonTitle,
            OrderNumber = lesson.OrderNumber,
            DisplayOrder = lesson.DisplayOrder,
            UnitTitle = lesson.UnitTitle
        }).ToList();
    }

    public async Task<StartedExamDto> StartExamAsync(Guid userId, StartExamRequestDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        if (dto.SubjectId == Guid.Empty)
        {
            throw new ArgumentException("يجب اختيار المادة.");
        }

        var studentStream = await _repository.GetStudentStreamAsync(userId);

        if (string.IsNullOrWhiteSpace(studentStream))
        {
            throw new InvalidOperationException("الطالب لم يكمل إعداد الفرع الدراسي.");
        }

        var subject = await _repository.GetSubjectByIdAndStreamAsync(dto.SubjectId, studentStream);

        if (subject == null)
        {
            throw new InvalidOperationException("المادة غير موجودة لهذا الطالب.");
        }

        var normalizedType = NormalizeExamType(dto.Type);

        if (normalizedType is not ("short" or "comprehensive"))
        {
            throw new ArgumentException("نوع الامتحان غير صالح.");
        }

        if (normalizedType == "short" && dto.LessonId == null)
        {
            throw new ArgumentException("يجب اختيار درس للاختبار القصير.");
        }

        var examEntity = await _repository.GetExamForStartAsync(
            dto.SubjectId,
            normalizedType,
            dto.LessonId
        );

        if (examEntity == null)
        {
            throw new InvalidOperationException("لا يوجد امتحان مطابق للمادة والنوع المختار.");
        }

        var questions = await _repository.GetQuestionsByExamIdAsync(examEntity.id);

        if (questions.Count == 0)
        {
            throw new InvalidOperationException("هذا الامتحان لا يحتوي على أسئلة.");
        }

        return new StartedExamDto
        {
            ExamId = examEntity.id,
            SubjectId = subject.id,
            SubjectName = subject.name ?? "",
            Type = normalizedType,
            TypeName = normalizedType == "comprehensive" ? "شامل" : "قصير",
            LessonId = examEntity.lesson_id,
            LessonTitle = examEntity.lesson?.title,
            Questions = questions.Select(q => new StartedExamQuestionDto
            {
                QuestionId = q.id,
                Text = q.question_text ?? "",
                Options = BuildQuestionOptions(q)
            }).ToList()
        };
    }

    public async Task<ExamResultDto> SubmitExamAsync(Guid userId, Guid examId, SubmitExamDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        dto.Answers ??= new List<QuestionAnswerDto>();

        var exam = await _repository.GetExamByIdAsync(examId);

        if (exam == null)
        {
            throw new InvalidOperationException("الامتحان غير موجود.");
        }

        var studentStream = await _repository.GetStudentStreamAsync(userId);

        if (string.IsNullOrWhiteSpace(studentStream))
        {
            throw new InvalidOperationException("الطالب لم يكمل إعداد الفرع الدراسي.");
        }

        if (exam.subject_id == null)
        {
            throw new InvalidOperationException("هذا الامتحان غير مربوط بمادة.");
        }

        var subject = await _repository.GetSubjectByIdAndStreamAsync(exam.subject_id.Value, studentStream);

        if (subject == null)
        {
            throw new InvalidOperationException("هذا الامتحان لا ينتمي لفرع الطالب.");
        }

        var questions = await _repository.GetQuestionsByExamIdAsync(examId);

        if (questions.Count == 0)
        {
            throw new InvalidOperationException("الامتحان لا يحتوي على أسئلة.");
        }

        if (dto.Answers.Count == 0)
        {
            throw new ArgumentException("يجب الإجابة على سؤال واحد على الأقل قبل إنهاء الامتحان.");
        }

        var examQuestionIds = questions.Select(q => q.id).ToHashSet();
        var submittedQuestionIds = dto.Answers.Select(a => a.QuestionId).ToList();

        if (submittedQuestionIds.Distinct().Count() != submittedQuestionIds.Count)
        {
            throw new ArgumentException("يوجد questionId مكرر في الإجابات.");
        }

        if (submittedQuestionIds.Any(id => !examQuestionIds.Contains(id)))
        {
            throw new ArgumentException("بعض questionId لا تنتمي لهذا الامتحان.");
        }

        if (examQuestionIds.Any(id => !submittedQuestionIds.Contains(id)))
        {
            throw new ArgumentException("يجب الإجابة على جميع الأسئلة قبل إنهاء الامتحان.");
        }

        if (dto.Answers.Any(a => string.IsNullOrWhiteSpace(a.SelectedAnswer)))
        {
            throw new ArgumentException("يجب اختيار إجابة لكل سؤال قبل إنهاء الامتحان.");
        }

        var invalidChoice = dto.Answers
            .Select(a => a.SelectedAnswer.Trim().ToUpper())
            .Any(x => x is not ("A" or "B" or "C" or "D"));

        if (invalidChoice)
        {
            throw new ArgumentException("الإجابة المختارة يجب أن تكون واحدة من A أو B أو C أو D.");
        }

        var answersDict = dto.Answers.ToDictionary(
            a => a.QuestionId,
            a => (string?)a.SelectedAnswer?.Trim().ToUpper()
        );

        var questionKeys = questions.Select(q => new QuestionKey
        {
            QuestionId = q.id,
            CorrectAnswer = q.correct_answer
        });

        var calc = ScoreCalculatorHelper.Calculate(questionKeys, answersDict);
        var score = calc.ScorePercent;
        var correctAnswers = calc.Details.Count(d => d.IsCorrect);
        var wrongAnswers = questions.Count - correctAnswers;

        var evalDict = calc.Details.ToDictionary(d => d.QuestionId, d => d);

        var aiQuestions = new List<AiQuestionResultDto>(questions.Count);

        foreach (var q in questions)
        {
            evalDict.TryGetValue(q.id, out var ev);

            aiQuestions.Add(new AiQuestionResultDto
            {
                QuestionText = q.question_text ?? "",
                SelectedAnswer = ev?.SelectedAnswer,
                CorrectAnswer = ev?.CorrectAnswer,
                IsCorrect = ev?.IsCorrect ?? false
            });
        }

        var aiReq = new AiExamAnalysisRequestDto
        {
            UserId = userId,
            ExamId = examId,
            ExamType = exam.type ?? "",
            SubjectName = subject.name ?? "",
            Score = score,
            Questions = aiQuestions
        };

        AiExamAnalysisResponseDto aiResp;

        try
        {
            aiResp = await _ai.AnalyzeExamAsync(aiReq);
        }
        catch
        {
            aiResp = new AiExamAnalysisResponseDto
            {
                StrengthAreas = new List<string>(),
                WeakAreas = new List<string>(),
                LevelMessage = "تعذر توليد تحليل من Gemini حالياً.",
                RecommendationText = string.Empty
            };
        }

        var createdAt = GetUnspecifiedNow();

        var examResult = new exam_result
        {
            id = Guid.NewGuid(),
            user_id = userId,
            exam_id = examId,
            score = score,
            strength_points = string.Join(", ", aiResp.StrengthAreas),
            weakness_points = string.Join(", ", aiResp.WeakAreas),
            level_message = aiResp.LevelMessage,
            created_at = createdAt
        };

        _repository.AddExamResult(examResult);

        var reviewItems = BuildReviewItems(questions, answersDict);
        await EnrichReviewItemsWithAiSolutionsAsync(subject.name ?? "", reviewItems);

        foreach (var item in reviewItems)
        {
            _repository.AddExamResultAnswer(new exam_result_answer
            {
                id = Guid.NewGuid(),
                exam_result_id = examResult.id,
                question_id = item.QuestionId,
                selected_answer = item.SelectedAnswer,
                is_correct = item.IsCorrect
            });
        }

        if (!string.IsNullOrWhiteSpace(aiResp.RecommendationText))
        {
            _repository.AddAiRecommendation(new ai_recommendation
            {
                id = Guid.NewGuid(),
                user_id = userId,
                recommendation_text = aiResp.RecommendationText,
                created_at = createdAt
            });
        }

        await _repository.SaveChangesAsync();

        return new ExamResultDto
        {
            ExamResultId = examResult.id,
            ExamId = examId,
            Score = score,
            TotalQuestions = questions.Count,
            CorrectAnswers = correctAnswers,
            WrongAnswers = wrongAnswers,
            Percentage = score,
            StrengthAreas = aiResp.StrengthAreas,
            WeakAreas = aiResp.WeakAreas,
            LevelMessage = aiResp.LevelMessage,
            RecommendationText = string.IsNullOrWhiteSpace(aiResp.RecommendationText)
                ? null
                : aiResp.RecommendationText,
            CreatedAt = createdAt,
            Review = reviewItems
        };
    }

    public async Task<ExamResultDto?> GetExamResultAsync(Guid userId, Guid resultId)
    {
        var result = await _repository.GetExamResultByUserAsync(userId, resultId);

        if (result == null || result.exam_id == null)
        {
            return null;
        }

        var questions = await _repository.GetQuestionsByExamIdAsync(result.exam_id.Value);
        var savedAnswers = await _repository.GetResultAnswersAsync(result.id);

        var answersDict = savedAnswers
            .Where(x => x.question_id != null)
            .ToDictionary(
                x => x.question_id!.Value,
                x => x.selected_answer
            );

        var exam = await _repository.GetExamByIdAsync(result.exam_id.Value);
        var review = BuildReviewItems(questions, answersDict);
        await EnrichReviewItemsWithAiSolutionsAsync(exam?.subject?.name ?? "", review);
        var correctCount = review.Count(x => x.IsCorrect);
        var wrongCount = review.Count - correctCount;

        return new ExamResultDto
        {
            ExamResultId = result.id,
            ExamId = result.exam_id.Value,
            Score = result.score ?? 0,
            TotalQuestions = questions.Count,
            CorrectAnswers = correctCount,
            WrongAnswers = wrongCount,
            Percentage = result.score ?? 0,
            StrengthAreas = SplitCsv(result.strength_points),
            WeakAreas = SplitCsv(result.weakness_points),
            LevelMessage = result.level_message,
            RecommendationText = null,
            CreatedAt = result.created_at,
            Review = review
        };
    }

    public Task<bool> DeleteExamResultAsync(Guid userId, Guid resultId)
    {
        return _repository.DeleteExamResultAsync(userId, resultId);
    }

    public Task<int> DeleteExamResultsAsync(Guid userId)
    {
        return _repository.DeleteExamResultsAsync(userId);
    }

    private static List<StartedExamOptionDto> BuildQuestionOptions(question q)
    {
        return new List<StartedExamOptionDto>
        {
            new() { Key = "A", Text = q.option_a ?? "" },
            new() { Key = "B", Text = q.option_b ?? "" },
            new() { Key = "C", Text = q.option_c ?? "" },
            new() { Key = "D", Text = q.option_d ?? "" }
        };
    }

    private static List<ExamReviewQuestionDto> BuildReviewItems(
        List<question> questions,
        Dictionary<Guid, string?> answersDict
    )
    {
        return questions.Select(q =>
        {
            answersDict.TryGetValue(q.id, out var selected);

            selected = selected?.Trim().ToUpper();

            var selectedAnswerText = GetAnswerText(q, selected);
            var correctAnswer = (q.correct_answer ?? "").Trim().ToUpper();
            var correctAnswerText = GetAnswerText(q, correctAnswer);

            return new ExamReviewQuestionDto
            {
                QuestionId = q.id,
                QuestionText = q.question_text ?? "",
                SelectedAnswer = selected,
                SelectedAnswerText = selectedAnswerText,
                CorrectAnswer = correctAnswer,
                CorrectAnswerText = correctAnswerText,
                IsCorrect = selected == correctAnswer,
                Solution = q.solution_text ?? ""
            };
        }).ToList();
    }

    private async Task EnrichReviewItemsWithAiSolutionsAsync(
        string subjectName,
        List<ExamReviewQuestionDto> reviewItems
    )
    {
        var itemsNeedingSolution = reviewItems
            .Where(item => string.IsNullOrWhiteSpace(item.Solution))
            .ToList();

        var explanationTasks = itemsNeedingSolution.Select(async item =>
        {
            var response = await _ai.ExplainQuestionAsync(new AiQuestionExplanationRequestDto
            {
                SubjectName = subjectName,
                QuestionText = item.QuestionText,
                SelectedAnswer = item.SelectedAnswer,
                SelectedAnswerText = item.SelectedAnswerText,
                CorrectAnswer = item.CorrectAnswer,
                CorrectAnswerText = item.CorrectAnswerText,
                IsCorrect = item.IsCorrect
            });

            if (!string.IsNullOrWhiteSpace(response.SolutionText))
            {
                item.Solution = response.SolutionText;
            }
        });

        await Task.WhenAll(explanationTasks);
    }

    private static string GetAnswerText(question q, string? key)
    {
        return (key ?? "").Trim().ToUpper() switch
        {
            "A" => q.option_a ?? "",
            "B" => q.option_b ?? "",
            "C" => q.option_c ?? "",
            "D" => q.option_d ?? "",
            _ => ""
        };
    }

    private static string NormalizeExamType(string? value)
    {
        var normalized = (value ?? "").Trim().ToLower();

        return normalized switch
        {
            "quick" => "short",
            "short" => "short",
            "قصير" => "short",
            "comprehensive" => "comprehensive",
            "full" => "comprehensive",
            "شامل" => "comprehensive",
            _ => normalized
        };
    }

    private static List<string> SplitCsv(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return new List<string>();
        }

        return value
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .Where(x => x.Length > 0)
            .ToList();
    }

    private static string MapSubjectKey(string name) => name switch
    {
        "الرياضيات" => "math",
        "الفيزياء" => "physics",
        "اللغة العربية" => "arabic",
        "اللغة الإنجليزية" => "english",
        "الكيمياء" => "chemistry",
        "الأحياء" => "biology",
        "التكنولوجيا" => "subject",
        "التربية الإسلامية" => "subject",
        _ => "subject"
    };

    private static string MapSubjectColor(string name) => name switch
    {
        "الرياضيات" => "blue",
        "الفيزياء" => "green",
        "اللغة العربية" => "amber",
        "الكيمياء" => "purple",
        "اللغة الإنجليزية" => "blue",
        "الأحياء" => "green",
        "التكنولوجيا" => "blue",
        "التربية الإسلامية" => "green",
        _ => "blue"
    };

    private static string MapSubjectIcon(string name) => name switch
    {
        "الرياضيات" => "BookMarked",
        "الفيزياء" => "Atom",
        "اللغة العربية" => "Languages",
        "الكيمياء" => "FlaskConical",
        "اللغة الإنجليزية" => "Languages",
        "الأحياء" => "Dna",
        "التكنولوجيا" => "BookOpen",
        "التربية الإسلامية" => "BookOpen",
        _ => "BookOpen"
    };

    private static DateTime GetUnspecifiedNow()
    {
        return DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
    }
}
