using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using backend.Models.Generated;
using backend.Models;

namespace backend.Data.Generated;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<achievement> achievements { get; set; }

    public virtual DbSet<admin_log> admin_logs { get; set; }

    public virtual DbSet<ai_recommendation> ai_recommendations { get; set; }

    public virtual DbSet<exam> exams { get; set; }

    public virtual DbSet<exam_result> exam_results { get; set; }

    public virtual DbSet<exam_result_answer> exam_result_answers { get; set; }

    public virtual DbSet<faq> faqs { get; set; }

    public virtual DbSet<lesson> lessons { get; set; }

    public virtual DbSet<lesson_progress> lesson_progresses { get; set; }

    public virtual DbSet<question> questions { get; set; }

    public virtual DbSet<site_content> site_contents { get; set; }

    public virtual DbSet<student_preference> student_preferences { get; set; }

    public virtual DbSet<student_preference_difficult_subject> student_preference_difficult_subjects { get; set; }

    public virtual DbSet<student_preference_learning_method> student_preference_learning_methods { get; set; }

    public virtual DbSet<student_profile> student_profiles { get; set; }

    public virtual DbSet<student_profile_subject> student_profile_subjects { get; set; }

    public virtual DbSet<study_plan> study_plans { get; set; }

    public virtual DbSet<study_plan_item> study_plan_items { get; set; }

    public virtual DbSet<study_session> study_sessions { get; set; }

    public virtual DbSet<subject> subjects { get; set; }

    public virtual DbSet<subject_analysis> subject_analyses { get; set; }

    public virtual DbSet<subject_unit> subject_units { get; set; }

    public virtual DbSet<user> users { get; set; }

    public virtual DbSet<user_achievement> user_achievements { get; set; }

    public virtual DbSet<user_stat> user_stats { get; set; }
    public virtual DbSet<ContactMessage> ContactMessages { get; set; }
    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("pgcrypto");
        modelBuilder.Entity<achievement>(entity =>
        {
            entity.HasKey(e => e.id).HasName("achievements_pkey");

            entity.Property(e => e.id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");

            entity.Property(e => e.title)
                .HasMaxLength(100)
                .HasColumnName("title");

            entity.Property(e => e.description)
                .HasColumnName("description");

            entity.Property(e => e.condition_type)
                .HasMaxLength(50)
                .HasColumnName("condition_type");

            entity.Property(e => e.condition_value)
                .HasColumnName("condition_value");

            entity.Property(e => e.title_ar)
                .HasMaxLength(200)
                .HasColumnName("title_ar");

            entity.Property(e => e.title_en)
                .HasMaxLength(200)
                .HasColumnName("title_en");

            entity.Property(e => e.description_ar)
                .HasColumnName("description_ar");

            entity.Property(e => e.description_en)
                .HasColumnName("description_en");

            entity.Property(e => e.reward_type)
                .HasMaxLength(50)
                .HasDefaultValueSql("'points'::character varying")
                .HasColumnName("reward_type");

            entity.Property(e => e.reward_value)
                .HasDefaultValue(0)
                .HasColumnName("reward_value");

            entity.Property(e => e.is_active)
                .HasDefaultValue(true)
                .HasColumnName("is_active");

            entity.Property(e => e.created_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");

            entity.Property(e => e.updated_at)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<admin_log>(entity =>
        {
            entity.HasKey(e => e.id).HasName("admin_logs_pkey");

            entity.Property(e => e.id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.action_type).HasMaxLength(100);
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.admin).WithMany(p => p.admin_logs)
                .HasForeignKey(d => d.admin_id)
                .HasConstraintName("admin_logs_admin_id_fkey");
        });

        modelBuilder.Entity<ai_recommendation>(entity =>
        {
            entity.HasKey(e => e.id).HasName("ai_recommendations_pkey");

            entity.HasIndex(e => new { e.user_id, e.created_at }, "idx_ai_recommendations_user_created").IsDescending(false, true);

            entity.Property(e => e.id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.user).WithMany(p => p.ai_recommendations)
                .HasForeignKey(d => d.user_id)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("ai_recommendations_user_id_fkey");
        });

        modelBuilder.Entity<exam>(entity =>
        {
            entity.HasKey(e => e.id).HasName("exams_pkey");

            entity.HasIndex(e => e.lesson_id, "idx_exams_lesson");

            entity.HasIndex(e => e.subject_id, "idx_exams_subject");

            entity.Property(e => e.id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.is_active).HasDefaultValue(true);
            entity.Property(e => e.title).HasMaxLength(200);
            entity.Property(e => e.type).HasMaxLength(20);

            entity.HasOne(d => d.lesson).WithMany(p => p.exams)
                .HasForeignKey(d => d.lesson_id)
                .HasConstraintName("exams_lesson_id_fkey");

            entity.HasOne(d => d.subject).WithMany(p => p.exams)
                .HasForeignKey(d => d.subject_id)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("exams_subject_id_fkey");
        });

        modelBuilder.Entity<exam_result>(entity =>
        {
            entity.HasKey(e => e.id).HasName("exam_results_pkey");

            entity.HasIndex(e => e.user_id, "idx_exam_results_user");

            entity.HasIndex(e => new { e.user_id, e.created_at }, "idx_exam_results_user_created").IsDescending(false, true);

            entity.Property(e => e.id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.exam).WithMany(p => p.exam_results)
                .HasForeignKey(d => d.exam_id)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("exam_results_exam_id_fkey");

            entity.HasOne(d => d.user).WithMany(p => p.exam_results)
                .HasForeignKey(d => d.user_id)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("exam_results_user_id_fkey");
        });

        modelBuilder.Entity<exam_result_answer>(entity =>
        {
            entity.HasKey(e => e.id).HasName("exam_result_answers_pkey");

            entity.Property(e => e.id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.selected_answer).HasMaxLength(1);

            entity.HasOne(d => d.exam_result).WithMany(p => p.exam_result_answers)
                .HasForeignKey(d => d.exam_result_id)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("exam_result_answers_exam_result_id_fkey");

            entity.HasOne(d => d.question).WithMany(p => p.exam_result_answers)
                .HasForeignKey(d => d.question_id)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("exam_result_answers_question_id_fkey");
        });

        modelBuilder.Entity<faq>(entity =>
        {
            entity.HasKey(e => e.id).HasName("faq_pkey");

            entity.ToTable("faq");

            entity.Property(e => e.id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<lesson>(entity =>
        {
            entity.HasKey(e => e.id).HasName("lessons_pkey");

            entity.HasIndex(e => e.subject_id, "idx_lessons_subject");

            entity.HasIndex(e => new { e.subject_id, e.order_number }, "idx_lessons_subject_order");

            entity.HasIndex(e => e.subject_unit_id, "idx_lessons_subject_unit_id");

            entity.HasIndex(e => new { e.subject_id, e.subject_unit_id, e.title }, "uq_lessons_subject_unit_title").IsUnique();

            entity.Property(e => e.id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.title).HasMaxLength(150);

            entity.HasOne(d => d.subject).WithMany(p => p.lessons)
                .HasForeignKey(d => d.subject_id)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("lessons_subject_id_fkey");

            entity.HasOne(d => d.subject_unit).WithMany(p => p.lessons)
                .HasForeignKey(d => d.subject_unit_id)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("lessons_subject_unit_id_fkey");
        });

        modelBuilder.Entity<lesson_progress>(entity =>
        {
            entity.HasKey(e => e.id).HasName("lesson_progress_pkey");

            entity.ToTable("lesson_progress");

            entity.HasIndex(e => new { e.user_id, e.completed }, "idx_lesson_progress_user_completed");

            entity.HasIndex(e => new { e.user_id, e.lesson_id }, "idx_lesson_progress_user_lesson");

            entity.HasIndex(e => new { e.user_id, e.lesson_id }, "uq_lesson_progress_user_lesson").IsUnique();

            entity.Property(e => e.id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.completed).HasDefaultValue(false);
            entity.Property(e => e.completed_at).HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.lesson).WithMany(p => p.lesson_progresses)
                .HasForeignKey(d => d.lesson_id)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("lesson_progress_lesson_id_fkey");

            entity.HasOne(d => d.user).WithMany(p => p.lesson_progresses)
                .HasForeignKey(d => d.user_id)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("lesson_progress_user_id_fkey");
        });

        modelBuilder.Entity<question>(entity =>
        {
            entity.HasKey(e => e.id).HasName("questions_pkey");

            entity.HasIndex(e => e.exam_id, "idx_questions_exam");

            entity.Property(e => e.id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.correct_answer).HasMaxLength(1);

            entity.HasOne(d => d.exam).WithMany(p => p.questions)
                .HasForeignKey(d => d.exam_id)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("questions_exam_id_fkey");
        });

        modelBuilder.Entity<site_content>(entity =>
        {
            entity.HasKey(e => e.id).HasName("site_content_pkey");

            entity.ToTable("site_content");

            entity.HasIndex(e => e.content_key, "site_content_content_key_key").IsUnique();

            entity.Property(e => e.id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.content_key).HasMaxLength(100);
        });

        modelBuilder.Entity<student_preference>(entity =>
        {
            entity.HasKey(e => e.user_id).HasName("student_preferences_pkey");

            entity.Property(e => e.user_id).ValueGeneratedNever();
            entity.Property(e => e.branch_code).HasMaxLength(20);
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.exam_experience_code).HasMaxLength(20);
            entity.Property(e => e.goal_code).HasMaxLength(30);
            entity.Property(e => e.level_code).HasMaxLength(20);
            entity.Property(e => e.study_hours_code).HasMaxLength(20);
            entity.Property(e => e.updated_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.user).WithOne(p => p.student_preference)
                .HasForeignKey<student_preference>(d => d.user_id)
                .HasConstraintName("student_preferences_user_id_fkey");
        });

        modelBuilder.Entity<student_preference_difficult_subject>(entity =>
        {
            entity.HasKey(e => new { e.user_id, e.subject_id }).HasName("student_preference_difficult_subjects_pkey");

            entity.HasIndex(e => e.subject_id, "idx_student_preference_difficult_subjects_subject");

            entity.HasIndex(e => e.user_id, "idx_student_preference_difficult_subjects_user");

            entity.Property(e => e.created_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.subject).WithMany(p => p.student_preference_difficult_subjects)
                .HasForeignKey(d => d.subject_id)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("student_preference_difficult_subjects_subject_id_fkey");

            entity.HasOne(d => d.user).WithMany(p => p.student_preference_difficult_subjects)
                .HasForeignKey(d => d.user_id)
                .HasConstraintName("student_preference_difficult_subjects_user_id_fkey");
        });

        modelBuilder.Entity<student_preference_learning_method>(entity =>
        {
            entity.HasKey(e => new { e.user_id, e.method_code }).HasName("student_preference_learning_methods_pkey");

            entity.HasIndex(e => e.user_id, "idx_student_preference_learning_methods_user");

            entity.Property(e => e.method_code).HasMaxLength(20);
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.user).WithMany(p => p.student_preference_learning_methods)
                .HasForeignKey(d => d.user_id)
                .HasConstraintName("student_preference_learning_methods_user_id_fkey");
        });

        modelBuilder.Entity<student_profile>(entity =>
        {
            entity.HasKey(e => e.id).HasName("student_profiles_pkey");

            entity.HasIndex(e => e.user_id, "idx_student_profiles_user");

            entity.HasIndex(e => e.user_id, "student_profiles_user_id_key").IsUnique();

            entity.Property(e => e.id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.created_at).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.current_grade).HasMaxLength(30);
            entity.Property(e => e.exam_experience).HasMaxLength(30);
            entity.Property(e => e.is_onboarding_completed).HasDefaultValue(false);
            entity.Property(e => e.learning_methods).HasDefaultValueSql("ARRAY[]::text[]");
            entity.Property(e => e.preferred_study_place).HasMaxLength(50);
            entity.Property(e => e.preferred_study_time).HasMaxLength(50);
            entity.Property(e => e.primary_goal).HasMaxLength(100);
            entity.Property(e => e.stream).HasMaxLength(50);
            entity.Property(e => e.updated_at).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.user).WithOne(p => p.student_profile)
                .HasForeignKey<student_profile>(d => d.user_id)
                .HasConstraintName("student_profiles_user_id_fkey");
        });

        modelBuilder.Entity<student_profile_subject>(entity =>
        {
            entity.HasKey(e => e.id).HasName("student_profile_subjects_pkey");

            entity.HasIndex(e => e.student_profile_id, "idx_student_profile_subjects_profile");

            entity.HasIndex(e => e.subject_id, "idx_student_profile_subjects_subject");

            entity.HasIndex(e => new { e.student_profile_id, e.subject_id }, "uq_student_profile_subject").IsUnique();

            entity.Property(e => e.id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.student_profile).WithMany(p => p.student_profile_subjects)
                .HasForeignKey(d => d.student_profile_id)
                .HasConstraintName("student_profile_subjects_student_profile_id_fkey");

            entity.HasOne(d => d.subject).WithMany(p => p.student_profile_subjects)
                .HasForeignKey(d => d.subject_id)
                .HasConstraintName("student_profile_subjects_subject_id_fkey");
        });

        modelBuilder.Entity<study_plan>(entity =>
        {
            entity.HasKey(e => e.id).HasName("study_plans_pkey");

            entity.HasIndex(e => e.subject_id, "idx_study_plans_subject");

            entity.HasIndex(e => e.user_id, "idx_study_plans_user");

            entity.Property(e => e.id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.is_ai_generated).HasDefaultValue(false);
            entity.Property(e => e.study_days).HasDefaultValueSql("ARRAY[]::text[]");
            entity.Property(e => e.title).HasMaxLength(150);
            entity.Property(e => e.updated_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.subject).WithMany(p => p.study_plans)
                .HasForeignKey(d => d.subject_id)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("study_plans_subject_id_fkey");

            entity.HasOne(d => d.user).WithMany(p => p.study_plans)
                .HasForeignKey(d => d.user_id)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("study_plans_user_id_fkey");
        });

        modelBuilder.Entity<study_plan_item>(entity =>
        {
            entity.HasKey(e => e.id).HasName("study_plan_items_pkey");

            entity.HasIndex(e => e.lesson_id, "idx_study_plan_items_lesson");

            entity.HasIndex(e => e.study_plan_id, "idx_study_plan_items_plan");

            entity.HasIndex(e => new { e.study_plan_id, e.order_number }, "idx_study_plan_items_plan_order");

            entity.Property(e => e.id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.is_completed).HasDefaultValue(false);
            entity.Property(e => e.order_number).HasDefaultValue(1);

            entity.HasOne(d => d.lesson).WithMany(p => p.study_plan_items)
                .HasForeignKey(d => d.lesson_id)
                .HasConstraintName("study_plan_items_lesson_id_fkey");

            entity.HasOne(d => d.study_plan).WithMany(p => p.study_plan_items)
                .HasForeignKey(d => d.study_plan_id)
                .HasConstraintName("study_plan_items_study_plan_id_fkey");
        });

        modelBuilder.Entity<study_session>(entity =>
        {
            entity.HasKey(e => e.id).HasName("study_sessions_pkey");

            entity.HasIndex(e => e.subject_id, "idx_study_sessions_subject");

            entity.HasIndex(e => new { e.user_id, e.created_at }, "idx_study_sessions_user_created").IsDescending(false, true);

            entity.HasIndex(e => new { e.user_id, e.started_at }, "idx_study_sessions_user_started").IsDescending(false, true);

            entity.Property(e => e.id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.ended_at).HasColumnType("timestamp without time zone");
            entity.Property(e => e.session_type)
                .HasMaxLength(20)
                .HasDefaultValueSql("'study'::character varying");
            entity.Property(e => e.started_at).HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.lesson).WithMany(p => p.study_sessions)
                .HasForeignKey(d => d.lesson_id)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("study_sessions_lesson_id_fkey");

            entity.HasOne(d => d.subject).WithMany(p => p.study_sessions)
                .HasForeignKey(d => d.subject_id)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("study_sessions_subject_id_fkey");

            entity.HasOne(d => d.user).WithMany(p => p.study_sessions)
                .HasForeignKey(d => d.user_id)
                .HasConstraintName("study_sessions_user_id_fkey");
        });

        modelBuilder.Entity<subject>(entity =>
        {
            entity.HasKey(e => e.id).HasName("subjects_pkey");

            entity.Property(e => e.id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.name).HasMaxLength(100);
            entity.Property(e => e.stream).HasMaxLength(50);
        });

        modelBuilder.Entity<subject_analysis>(entity =>
        {
            entity.HasKey(e => e.id).HasName("subject_analysis_pkey");

            entity.ToTable("subject_analysis");

            entity.Property(e => e.id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.strengths).HasColumnType("jsonb");
            entity.Property(e => e.weaknesses).HasColumnType("jsonb");

            entity.HasOne(d => d.subject).WithMany(p => p.subject_analyses)
                .HasForeignKey(d => d.subject_id)
                .HasConstraintName("subject_analysis_subject_id_fkey");

            entity.HasOne(d => d.user).WithMany(p => p.subject_analyses)
                .HasForeignKey(d => d.user_id)
                .HasConstraintName("subject_analysis_user_id_fkey");
        });

        modelBuilder.Entity<subject_unit>(entity =>
        {
            entity.HasKey(e => e.id).HasName("subject_units_pkey");

            entity.HasIndex(e => e.subject_id, "idx_subject_units_subject_id");

            entity.HasIndex(e => new { e.subject_id, e.title }, "uq_subject_units_subject_title").IsUnique();

            entity.Property(e => e.id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.title).HasMaxLength(200);

            entity.HasOne(d => d.subject).WithMany(p => p.subject_units)
                .HasForeignKey(d => d.subject_id)
                .HasConstraintName("subject_units_subject_id_fkey");
        });

        modelBuilder.Entity<user>(entity =>
        {
            entity.HasKey(e => e.id).HasName("users_pkey");

            entity.Property(e => e.id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.created_at).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.email).HasMaxLength(150);
            entity.Property(e => e.full_name).HasMaxLength(100);
            entity.Property(e => e.is_active).HasDefaultValue(true);
            entity.Property(e => e.phone).HasMaxLength(30);
            entity.Property(e => e.points).HasDefaultValue(0);
            entity.Property(e => e.role).HasMaxLength(20);
        });

        modelBuilder.Entity<user_achievement>(entity =>
        {
            entity.HasKey(e => e.id).HasName("user_achievements_pkey");

            entity.HasIndex(e => new { e.user_id, e.achievement_id }, "uq_user_achievement").IsUnique();

            entity.Property(e => e.id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.earned_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.achievement).WithMany(p => p.user_achievements)
                .HasForeignKey(d => d.achievement_id)
                .HasConstraintName("user_achievements_achievement_id_fkey");

            entity.HasOne(d => d.user).WithMany(p => p.user_achievements)
                .HasForeignKey(d => d.user_id)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("user_achievements_user_id_fkey");
        });

        modelBuilder.Entity<user_stat>(entity =>
        {
            entity.HasKey(e => e.user_id).HasName("user_stats_pkey");

            entity.Property(e => e.user_id).ValueGeneratedNever();
            entity.Property(e => e.level).HasDefaultValue(1);
            entity.Property(e => e.updated_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.user).WithOne(p => p.user_stat)
                .HasForeignKey<user_stat>(d => d.user_id)
                .HasConstraintName("user_stats_user_id_fkey");
        });
        modelBuilder.Entity<ContactMessage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_ContactMessages");

            entity.ToTable("ContactMessages", "public");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()");

            entity.Property(e => e.Name)
                .HasMaxLength(255);

            entity.Property(e => e.Email)
                .HasMaxLength(255);

            entity.Property(e => e.Subject)
                .HasMaxLength(150);

            entity.Property(e => e.Message);

            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
        modelBuilder.HasPostgresExtension("pgcrypto");

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("refresh_tokens_pkey");

            entity.ToTable("refresh_tokens");

            entity.HasIndex(e => e.UserId, "idx_refresh_tokens_user_id");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.ExpiresAt).HasColumnName("expires_at");
            entity.Property(e => e.ReplacedByTokenHash).HasColumnName("replaced_by_token_hash");
            entity.Property(e => e.RevokedAt).HasColumnName("revoked_at");
            entity.Property(e => e.TokenHash).HasColumnName("token_hash");
            entity.Property(e => e.UserId).HasColumnName("user_id");
        });


        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
