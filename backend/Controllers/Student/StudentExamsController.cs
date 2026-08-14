using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using backend.DTOs.Student;
using backend.Services.Student;

namespace backend.Controllers.Student;

[ApiController]
[Route("api/student/exams")]
[Authorize(Roles = "student")]
public class StudentExamsController : ControllerBase
{
    private readonly IStudentExamService _service;

    public StudentExamsController(IStudentExamService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<ExamHistoryDto>>> GetExamHistory()
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized(new
            {
                message = "تعذر التحقق من المستخدم الحالي."
            });
        }

        var exams = await _service.GetExamHistoryAsync(userId.Value);

        return Ok(exams);
    }

    [HttpGet("subjects")]
    public async Task<ActionResult<List<ExamSubjectOptionDto>>> GetSubjects()
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized(new
            {
                message = "تعذر التحقق من المستخدم الحالي."
            });
        }

        var subjects = await _service.GetExamSubjectsAsync(userId.Value);

        return Ok(subjects);
    }

    [HttpGet("subjects/{subjectId:guid}/lessons")]
    public async Task<ActionResult<List<ExamLessonOptionDto>>> GetLessons(Guid subjectId)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized(new
            {
                message = "تعذر التحقق من المستخدم الحالي."
            });
        }

        var lessons = await _service.GetSubjectLessonsForQuickExamAsync(userId.Value, subjectId);

        return Ok(lessons);
    }

    [HttpPost("start")]
    public async Task<ActionResult<StartedExamDto>> Start([FromBody] StartExamRequestDto dto)
    {
        if (dto == null || dto.SubjectId == Guid.Empty || string.IsNullOrWhiteSpace(dto.Type))
        {
            return BadRequest(new
            {
                message = "بيانات بدء الامتحان غير مكتملة."
            });
        }

        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized(new
            {
                message = "تعذر التحقق من المستخدم الحالي."
            });
        }

        try
        {
            var startedExam = await _service.StartExamAsync(userId.Value, dto);

            return Ok(startedExam);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
    }

    [HttpPost("{examId:guid}/submit")]
    public async Task<ActionResult<ExamResultDto>> Submit(Guid examId, [FromBody] SubmitExamDto dto)
    {
        if (dto?.Answers == null)
        {
            return BadRequest(new
            {
                message = "بيانات الإجابات غير صالحة."
            });
        }

        if (dto.Answers.Any(a => a.QuestionId == Guid.Empty))
        {
            return BadRequest(new
            {
                message = "يجب أن يحتوي كل جواب على questionId صالح."
            });
        }

        if (dto.Answers.Any(a => string.IsNullOrWhiteSpace(a.SelectedAnswer)))
        {
            return BadRequest(new
            {
                message = "يجب أن يحتوي كل جواب على selectedAnswer."
            });
        }

        var invalidChoice = dto.Answers
            .Select(a => a.SelectedAnswer.Trim().ToUpper())
            .Any(x => x is not ("A" or "B" or "C" or "D"));

        if (invalidChoice)
        {
            return BadRequest(new
            {
                message = "selectedAnswer يجب أن تكون واحدة من A أو B أو C أو D."
            });
        }

        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized(new
            {
                message = "تعذر التحقق من المستخدم الحالي."
            });
        }

        try
        {
            var result = await _service.SubmitExamAsync(userId.Value, examId, dto);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
    }

    [HttpGet("results/{resultId:guid}")]
    public async Task<ActionResult<ExamResultDto>> GetResult(Guid resultId)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized(new
            {
                message = "تعذر التحقق من المستخدم الحالي."
            });
        }

        var result = await _service.GetExamResultAsync(userId.Value, resultId);

        if (result == null)
        {
            return NotFound(new
            {
                message = "نتيجة الامتحان غير موجودة."
            });
        }

        return Ok(result);
    }

    [HttpDelete("results/{resultId:guid}")]
    public async Task<IActionResult> DeleteResult(Guid resultId)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized(new
            {
                message = "تعذر التحقق من المستخدم الحالي."
            });
        }

        var deleted = await _service.DeleteExamResultAsync(userId.Value, resultId);

        if (!deleted)
        {
            return NotFound(new
            {
                message = "نتيجة الامتحان غير موجودة أو غير مسموح بحذفها."
            });
        }

        return NoContent();
    }

    private Guid? GetCurrentUserId()
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userIdValue))
        {
            return null;
        }

        return Guid.TryParse(userIdValue, out var userId)
            ? userId
            : null;
    }
}