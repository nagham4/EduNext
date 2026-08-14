using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace backend.Models;

public partial class NeondbContext : DbContext
{
    public NeondbContext()
    {
    }

    public NeondbContext(DbContextOptions<NeondbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Achievement> Achievements { get; set; }

    public virtual DbSet<AdminLog> AdminLogs { get; set; }

    public virtual DbSet<AiRecommendation> AiRecommendations { get; set; }

    public virtual DbSet<ContactMessage> ContactMessages { get; set; }

    public virtual DbSet<Exam> Exams { get; set; }

    public virtual DbSet<ExamResult> ExamResults { get; set; }

    public virtual DbSet<ExamResultAnswer> ExamResultAnswers { get; set; }

    public virtual DbSet<Lesson> Lessons { get; set; }

    public virtual DbSet<LessonProgress> LessonProgresses { get; set; }

    public virtual DbSet<Question> Questions { get; set; }

    public virtual DbSet<StudentProfile> StudentProfiles { get; set; }

    public virtual DbSet<StudentProfileSubject> StudentProfileSubjects { get; set; }

    public virtual DbSet<StudyPlan> StudyPlans { get; set; }

    public virtual DbSet<StudyPlanItem> StudyPlanItems { get; set; }

    public virtual DbSet<StudySession> StudySessions { get; set; }

    public virtual DbSet<Subject> Subjects { get; set; }

    public virtual DbSet<SubjectAnalysis> SubjectAnalyses { get; set; }

    public virtual DbSet<SubjectUnit> SubjectUnits { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserAchievement> UserAchievements { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=ep-fancy-violet-am1sc1by-pooler.c-5.us-east-1.aws.neon.tech;Port=5432;Database=neondb;Username=neondb_owner;Password=npg_2RCaHhtv3eDd;SSL Mode=Require;Channel Binding=Require");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("pgcrypto");

        modelBuilder.Entity<Achievement>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("achievements_pkey");

            entity.ToTable("achievements");

            entity.HasIndex(e => e.TitleEn, "ux_achievements_title_en").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.ConditionType)
                .HasMaxLength(50)
                .HasColumnName("condition_type");
            entity.Property(e => e.ConditionValue).HasColumnName("condition_value");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.DescriptionAr).HasColumnName("description_ar");
            entity.Property(e => e.DescriptionEn).HasColumnName("description_en");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.RewardType)
                .HasMaxLength(50)
                .HasDefaultValueSql("'points'::character varying")
                .HasColumnName("reward_type");
            entity.Property(e => e.RewardValue)
                .HasDefaultValue(0)
                .HasColumnName("reward_value");
            entity.Property(e => e.Title)
                .HasMaxLength(100)
                .HasColumnName("title");
            entity.Property(e => e.TitleAr)
                .HasMaxLength(200)
                .HasColumnName("title_ar");
            entity.Property(e => e.TitleEn)
                .HasMaxLength(200)
                .HasColumnName("title_en");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<AdminLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("admin_logs_pkey");

            entity.ToTable("admin_logs");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.ActionType)
                .HasMaxLength(100)
                .HasColumnName("action_type");
            entity.Property(e => e.AdminId).HasColumnName("admin_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");

            entity.HasOne(d => d.Admin).WithMany(p => p.AdminLogs)
                .HasForeignKey(d => d.AdminId)
                .HasConstraintName("admin_logs_admin_id_fkey");
        });

        modelBuilder.Entity<AiRecommendation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("ai_recommendations_pkey");

            entity.ToTable("ai_recommendations");

            entity.HasIndex(e => new { e.UserId, e.CreatedAt }, "idx_ai_recommendations_user_created").IsDescending(false, true);

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.RecommendationText).HasColumnName("recommendation_text");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.AiRecommendations)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("ai_recommendations_user_id_fkey");
        });

        modelBuilder.Entity<ContactMessage>(entity =>
        {
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.Subject).HasMaxLength(150);
        });

        modelBuilder.Entity<Exam>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("exams_pkey");

            entity.ToTable("exams");

            entity.HasIndex(e => e.LessonId, "idx_exams_lesson");

            entity.HasIndex(e => e.SubjectId, "idx_exams_subject");

            entity.HasIndex(e => e.SubjectId, "idx_exams_subject_id");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.LessonId).HasColumnName("lesson_id");
            entity.Property(e => e.SubjectId).HasColumnName("subject_id");
            entity.Property(e => e.Title)
                .HasMaxLength(200)
                .HasColumnName("title");
            entity.Property(e => e.Type)
                .HasMaxLength(20)
                .HasColumnName("type");

            entity.HasOne(d => d.Lesson).WithMany(p => p.Exams)
                .HasForeignKey(d => d.LessonId)
                .HasConstraintName("exams_lesson_id_fkey");

            entity.HasOne(d => d.Subject).WithMany(p => p.Exams)
                .HasForeignKey(d => d.SubjectId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("exams_subject_id_fkey");
        });

        modelBuilder.Entity<ExamResult>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("exam_results_pkey");

            entity.ToTable("exam_results");

            entity.HasIndex(e => e.UserId, "idx_exam_results_user");

            entity.HasIndex(e => new { e.UserId, e.CreatedAt }, "idx_exam_results_user_created").IsDescending(false, true);

            entity.HasIndex(e => new { e.UserId, e.ExamId }, "idx_exam_results_user_exam");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.ExamId).HasColumnName("exam_id");
            entity.Property(e => e.LevelMessage).HasColumnName("level_message");
            entity.Property(e => e.Score).HasColumnName("score");
            entity.Property(e => e.StrengthPoints).HasColumnName("strength_points");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.WeaknessPoints).HasColumnName("weakness_points");

            entity.HasOne(d => d.Exam).WithMany(p => p.ExamResults)
                .HasForeignKey(d => d.ExamId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("exam_results_exam_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.ExamResults)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("exam_results_user_id_fkey");
        });

        modelBuilder.Entity<ExamResultAnswer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("exam_result_answers_pkey");

            entity.ToTable("exam_result_answers");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.ExamResultId).HasColumnName("exam_result_id");
            entity.Property(e => e.IsCorrect).HasColumnName("is_correct");
            entity.Property(e => e.QuestionId).HasColumnName("question_id");
            entity.Property(e => e.SelectedAnswer)
                .HasMaxLength(1)
                .HasColumnName("selected_answer");

            entity.HasOne(d => d.ExamResult).WithMany(p => p.ExamResultAnswers)
                .HasForeignKey(d => d.ExamResultId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("exam_result_answers_exam_result_id_fkey");

            entity.HasOne(d => d.Question).WithMany(p => p.ExamResultAnswers)
                .HasForeignKey(d => d.QuestionId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("exam_result_answers_question_id_fkey");
        });

        modelBuilder.Entity<Lesson>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("lessons_pkey");

            entity.ToTable("lessons");

            entity.HasIndex(e => e.SubjectId, "idx_lessons_subject");

            entity.HasIndex(e => e.SubjectId, "idx_lessons_subject_id");

            entity.HasIndex(e => new { e.SubjectId, e.OrderNumber }, "idx_lessons_subject_order");

            entity.HasIndex(e => e.SubjectUnitId, "idx_lessons_subject_unit_id");

            entity.HasIndex(e => new { e.SubjectId, e.SubjectUnitId, e.Title }, "uq_lessons_subject_unit_title").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.OrderNumber).HasColumnName("order_number");
            entity.Property(e => e.PdfUrl).HasColumnName("pdf_url");
            entity.Property(e => e.ResourcesUrl).HasColumnName("resources_url");
            entity.Property(e => e.SubjectId).HasColumnName("subject_id");
            entity.Property(e => e.SubjectUnitId).HasColumnName("subject_unit_id");
            entity.Property(e => e.Summary).HasColumnName("summary");
            entity.Property(e => e.Title)
                .HasMaxLength(150)
                .HasColumnName("title");
            entity.Property(e => e.VideoDurationSeconds).HasColumnName("video_duration_seconds");
            entity.Property(e => e.VideoUrl).HasColumnName("video_url");

            entity.HasOne(d => d.Subject).WithMany(p => p.Lessons)
                .HasForeignKey(d => d.SubjectId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("lessons_subject_id_fkey");

            entity.HasOne(d => d.SubjectUnit).WithMany(p => p.Lessons)
                .HasForeignKey(d => d.SubjectUnitId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("lessons_subject_unit_id_fkey");
        });

        modelBuilder.Entity<LessonProgress>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("lesson_progress_pkey");

            entity.ToTable("lesson_progress");

            entity.HasIndex(e => new { e.UserId, e.Completed }, "idx_lesson_progress_user_completed");

            entity.HasIndex(e => new { e.UserId, e.LessonId }, "idx_lesson_progress_user_lesson");

            entity.HasIndex(e => new { e.UserId, e.LessonId, e.Completed }, "idx_lesson_progress_user_lesson_completed");

            entity.HasIndex(e => new { e.UserId, e.LessonId }, "uq_lesson_progress_user_lesson").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Completed)
                .HasDefaultValue(false)
                .HasColumnName("completed");
            entity.Property(e => e.CompletedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("completed_at");
            entity.Property(e => e.LessonId).HasColumnName("lesson_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Lesson).WithMany(p => p.LessonProgresses)
                .HasForeignKey(d => d.LessonId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("lesson_progress_lesson_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.LessonProgresses)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("lesson_progress_user_id_fkey");
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("questions_pkey");

            entity.ToTable("questions");

            entity.HasIndex(e => e.ExamId, "idx_questions_exam");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CorrectAnswer)
                .HasMaxLength(1)
                .HasColumnName("correct_answer");
            entity.Property(e => e.ExamId).HasColumnName("exam_id");
            entity.Property(e => e.OptionA).HasColumnName("option_a");
            entity.Property(e => e.OptionB).HasColumnName("option_b");
            entity.Property(e => e.OptionC).HasColumnName("option_c");
            entity.Property(e => e.OptionD).HasColumnName("option_d");
            entity.Property(e => e.QuestionText).HasColumnName("question_text");
            entity.Property(e => e.SolutionText).HasColumnName("solution_text");

            entity.HasOne(d => d.Exam).WithMany(p => p.Questions)
                .HasForeignKey(d => d.ExamId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("questions_exam_id_fkey");
        });

        modelBuilder.Entity<StudentProfile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("student_profiles_pkey");

            entity.ToTable("student_profiles");

            entity.HasIndex(e => e.UserId, "idx_student_profiles_user");

            entity.HasIndex(e => e.UserId, "idx_student_profiles_user_id");

            entity.HasIndex(e => e.UserId, "student_profiles_user_id_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.CurrentGrade)
                .HasMaxLength(30)
                .HasColumnName("current_grade");
            entity.Property(e => e.ExamExperience)
                .HasMaxLength(30)
                .HasColumnName("exam_experience");
            entity.Property(e => e.ExamYear).HasColumnName("exam_year");
            entity.Property(e => e.IsOnboardingCompleted)
                .HasDefaultValue(false)
                .HasColumnName("is_onboarding_completed");
            entity.Property(e => e.LearningMethods)
                .HasDefaultValueSql("ARRAY[]::text[]")
                .HasColumnName("learning_methods");
            entity.Property(e => e.PreferredStudyPlace)
                .HasMaxLength(50)
                .HasColumnName("preferred_study_place");
            entity.Property(e => e.PreferredStudyTime)
                .HasMaxLength(50)
                .HasColumnName("preferred_study_time");
            entity.Property(e => e.PrimaryGoal)
                .HasMaxLength(100)
                .HasColumnName("primary_goal");
            entity.Property(e => e.Stream)
                .HasMaxLength(50)
                .HasColumnName("stream");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithOne(p => p.StudentProfile)
                .HasForeignKey<StudentProfile>(d => d.UserId)
                .HasConstraintName("student_profiles_user_id_fkey");
        });

        modelBuilder.Entity<StudentProfileSubject>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("student_profile_subjects_pkey");

            entity.ToTable("student_profile_subjects");

            entity.HasIndex(e => e.StudentProfileId, "idx_student_profile_subjects_profile");

            entity.HasIndex(e => e.SubjectId, "idx_student_profile_subjects_subject");

            entity.HasIndex(e => new { e.StudentProfileId, e.SubjectId }, "uq_student_profile_subject").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.StudentProfileId).HasColumnName("student_profile_id");
            entity.Property(e => e.SubjectId).HasColumnName("subject_id");

            entity.HasOne(d => d.StudentProfile).WithMany(p => p.StudentProfileSubjects)
                .HasForeignKey(d => d.StudentProfileId)
                .HasConstraintName("student_profile_subjects_student_profile_id_fkey");

            entity.HasOne(d => d.Subject).WithMany(p => p.StudentProfileSubjects)
                .HasForeignKey(d => d.SubjectId)
                .HasConstraintName("student_profile_subjects_subject_id_fkey");
        });

        modelBuilder.Entity<StudyPlan>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("study_plans_pkey");

            entity.ToTable("study_plans");

            entity.HasIndex(e => e.SubjectId, "idx_study_plans_subject");

            entity.HasIndex(e => e.UserId, "idx_study_plans_user");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.DailyDurationMinutes).HasColumnName("daily_duration_minutes");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IsAiGenerated)
                .HasDefaultValue(false)
                .HasColumnName("is_ai_generated");
            entity.Property(e => e.StudyDays)
                .HasDefaultValueSql("ARRAY[]::text[]")
                .HasColumnName("study_days");
            entity.Property(e => e.SubjectId).HasColumnName("subject_id");
            entity.Property(e => e.Title)
                .HasMaxLength(150)
                .HasColumnName("title");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Subject).WithMany(p => p.StudyPlans)
                .HasForeignKey(d => d.SubjectId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("study_plans_subject_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.StudyPlans)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("study_plans_user_id_fkey");
        });

        modelBuilder.Entity<StudyPlanItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("study_plan_items_pkey");

            entity.ToTable("study_plan_items");

            entity.HasIndex(e => e.LessonId, "idx_study_plan_items_lesson");

            entity.HasIndex(e => e.StudyPlanId, "idx_study_plan_items_plan");

            entity.HasIndex(e => new { e.StudyPlanId, e.OrderNumber }, "idx_study_plan_items_plan_order");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.IsCompleted)
                .HasDefaultValue(false)
                .HasColumnName("is_completed");
            entity.Property(e => e.LessonId).HasColumnName("lesson_id");
            entity.Property(e => e.OrderNumber)
                .HasDefaultValue(1)
                .HasColumnName("order_number");
            entity.Property(e => e.StudyPlanId).HasColumnName("study_plan_id");

            entity.HasOne(d => d.Lesson).WithMany(p => p.StudyPlanItems)
                .HasForeignKey(d => d.LessonId)
                .HasConstraintName("study_plan_items_lesson_id_fkey");

            entity.HasOne(d => d.StudyPlan).WithMany(p => p.StudyPlanItems)
                .HasForeignKey(d => d.StudyPlanId)
                .HasConstraintName("study_plan_items_study_plan_id_fkey");
        });

        modelBuilder.Entity<StudySession>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("study_sessions_pkey");

            entity.ToTable("study_sessions");

            entity.HasIndex(e => e.SubjectId, "idx_study_sessions_subject");

            entity.HasIndex(e => new { e.UserId, e.CreatedAt }, "idx_study_sessions_user_created").IsDescending(false, true);

            entity.HasIndex(e => new { e.UserId, e.StartedAt }, "idx_study_sessions_user_started").IsDescending(false, true);

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.DurationMinutes).HasColumnName("duration_minutes");
            entity.Property(e => e.EndedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("ended_at");
            entity.Property(e => e.LessonId).HasColumnName("lesson_id");
            entity.Property(e => e.SessionType)
                .HasMaxLength(20)
                .HasDefaultValueSql("'study'::character varying")
                .HasColumnName("session_type");
            entity.Property(e => e.StartedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("started_at");
            entity.Property(e => e.SubjectId).HasColumnName("subject_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Lesson).WithMany(p => p.StudySessions)
                .HasForeignKey(d => d.LessonId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("study_sessions_lesson_id_fkey");

            entity.HasOne(d => d.Subject).WithMany(p => p.StudySessions)
                .HasForeignKey(d => d.SubjectId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("study_sessions_subject_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.StudySessions)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("study_sessions_user_id_fkey");
        });

        modelBuilder.Entity<Subject>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("subjects_pkey");

            entity.ToTable("subjects");

            entity.HasIndex(e => e.Stream, "idx_subjects_stream");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.Stream)
                .HasMaxLength(50)
                .HasColumnName("stream");
        });

        modelBuilder.Entity<SubjectAnalysis>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("subject_analysis_pkey");

            entity.ToTable("subject_analysis");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.ImprovementTip).HasColumnName("improvement_tip");
            entity.Property(e => e.Strengths)
                .HasColumnType("jsonb")
                .HasColumnName("strengths");
            entity.Property(e => e.SubjectId).HasColumnName("subject_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Weaknesses)
                .HasColumnType("jsonb")
                .HasColumnName("weaknesses");

            entity.HasOne(d => d.Subject).WithMany(p => p.SubjectAnalyses)
                .HasForeignKey(d => d.SubjectId)
                .HasConstraintName("subject_analysis_subject_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.SubjectAnalyses)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("subject_analysis_user_id_fkey");
        });

        modelBuilder.Entity<SubjectUnit>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("subject_units_pkey");

            entity.ToTable("subject_units");

            entity.HasIndex(e => e.SubjectId, "idx_subject_units_subject_id");

            entity.HasIndex(e => new { e.SubjectId, e.Title }, "uq_subject_units_subject_title").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.OrderNumber).HasColumnName("order_number");
            entity.Property(e => e.SubjectId).HasColumnName("subject_id");
            entity.Property(e => e.Title)
                .HasMaxLength(200)
                .HasColumnName("title");

            entity.HasOne(d => d.Subject).WithMany(p => p.SubjectUnits)
                .HasForeignKey(d => d.SubjectId)
                .HasConstraintName("subject_units_subject_id_fkey");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("users_pkey");

            entity.ToTable("users");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .HasColumnName("email");
            entity.Property(e => e.FullName)
                .HasMaxLength(100)
                .HasColumnName("full_name");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.OnboardingCompleted).HasColumnName("onboarding_completed");
            entity.Property(e => e.OnboardingCompletedAt).HasColumnName("onboarding_completed_at");
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash");
            entity.Property(e => e.Phone)
                .HasMaxLength(30)
                .HasColumnName("phone");
            entity.Property(e => e.Points)
                .HasDefaultValue(0)
                .HasColumnName("points");
            entity.Property(e => e.Role)
                .HasMaxLength(20)
                .HasColumnName("role");
        });

        modelBuilder.Entity<UserAchievement>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("user_achievements_pkey");

            entity.ToTable("user_achievements");

            entity.HasIndex(e => new { e.UserId, e.AchievementId }, "uq_user_achievement").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.AchievementId).HasColumnName("achievement_id");
            entity.Property(e => e.EarnedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("earned_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Achievement).WithMany(p => p.UserAchievements)
                .HasForeignKey(d => d.AchievementId)
                .HasConstraintName("user_achievements_achievement_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.UserAchievements)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("user_achievements_user_id_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
