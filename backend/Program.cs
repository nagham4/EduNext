using System.Text;
using backend.Data.Generated;
using backend.Repositories.Admin;
using backend.Repositories.Student;
using backend.Services.Admin;
using backend.Services.AI;
using backend.Services.Auth;
using backend.Services.Student;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using backend.Services.Guest;


var builder = WebApplication.CreateBuilder(args);

// OpenAPI
builder.Services.AddOpenApi();

// Controllers
builder.Services.AddControllers();
builder.Services.AddMemoryCache();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy
            .WithOrigins(

                "http://localhost:8080",
                "http://127.0.0.1:8080"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddEndpointsApiExplorer();

// DbContext (PostgreSQL)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("Email"));

// ====================== ADMIN SERVICES ======================
builder.Services.AddScoped<IAdminDashboardRepository, AdminDashboardRepository>();
builder.Services.AddScoped<IAdminDashboardService, AdminDashboardService>();

builder.Services.AddScoped<IAdminSubjectsRepository, AdminSubjectsRepository>();
builder.Services.AddScoped<IAdminSubjectsService, AdminSubjectsService>();

builder.Services.AddScoped<IAdminLessonsRepository, AdminLessonsRepository>();
builder.Services.AddScoped<IAdminLessonsService, AdminLessonsService>();

builder.Services.AddScoped<IAdminExamsRepository, AdminExamsRepository>();
builder.Services.AddScoped<IAdminExamsService, AdminExamsService>();

builder.Services.AddScoped<IAdminAnalyticsRepository, AdminAnalyticsRepository>();
builder.Services.AddScoped<IAdminAnalyticsService, AdminAnalyticsService>();

builder.Services.AddScoped<IAdminUsersRepository, AdminUsersRepository>();
builder.Services.AddScoped<IAdminUsersService, AdminUsersService>();

builder.Services.AddScoped<IAdminAchievementsRepository, AdminAchievementsRepository>();
builder.Services.AddScoped<IAdminAchievementsService, AdminAchievementsService>();
// ====================== AUTH SERVICES ======================
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IPasswordValidator, PasswordValidator>();
builder.Services.AddScoped<IGoogleTokenValidator, GoogleTokenValidator>();

// ====================== STUDENT SERVICES ======================
builder.Services.AddScoped<IStudentSetupService, StudentSetupService>();
builder.Services.AddScoped<IStudentSetupRepository, StudentSetupRepository>();
builder.Services.AddScoped<IStudentDashboardRepository, StudentDashboardRepository>();
builder.Services.AddScoped<IStudentDashboardService, StudentDashboardService>();
builder.Services.AddScoped<ProgressService>();
builder.Services.AddHttpClient<IAiInsightsService, FlaskAiInsightsService>();
builder.Services.AddHttpClient<ISubjectChatbotService, FlaskSubjectChatbotService>();
builder.Services.AddScoped<IStudentSubjectRepository, StudentSubjectRepository>();
builder.Services.AddScoped<IStudentSubjectService, StudentSubjectService>();
builder.Services.AddScoped<IStudentExamRepository, StudentExamRepository>();
builder.Services.AddScoped<IStudentExamService, StudentExamService>();
builder.Services.AddScoped<IStudentAnalyticsRepository, StudentAnalyticsRepository>();
builder.Services.AddScoped<IStudentAnalyticsService, StudentAnalyticsService>();
builder.Services.AddScoped<IStudentAchievementRepository, StudentAchievementRepository>();
builder.Services.AddScoped<IStudentAchievementService, StudentAchievementService>();
builder.Services.AddScoped<IStudentProfileRepository, StudentProfileRepository>();
builder.Services.AddScoped<IStudentProfileService, StudentProfileService>();
builder.Services.AddScoped<IStudentStudyPlanRepository, StudentStudyPlanRepository>();
builder.Services.AddScoped<IStudentStudyPlanService, StudentStudyPlanService>();
builder.Services.AddScoped<IAdminProfileRepository, AdminProfileRepository>();
builder.Services.AddScoped<IAdminProfileService, AdminProfileService>();
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();
builder.Services.AddScoped<IContactMessageService, ContactMessageService>();
// JWT Configuration
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey))
    throw new InvalidOperationException("Jwt:Key is missing. Add it using user-secrets.");
if (Encoding.UTF8.GetByteCount(jwtKey) < 32)
    throw new InvalidOperationException("Jwt:Key must be at least 32 bytes for HmacSha256.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],

            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),

            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// app.UseHttpsRedirection();

app.UseCors("FrontendPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.UseSwagger();
app.UseSwaggerUI();

app.Run();
