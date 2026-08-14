using System;
using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Admin;
//الأدمن قد يحتاج لتعديل "البرومبت" (Prompt) أو الإعدادات التي يعتمد عليها الـ AI لتوليد التوصيات.
public record AiConfigurationDto(
    [Required] string ModelName, // GPT-4, Gemini, etc.
    [Required] string SystemInstruction, // التوجيه الأساسي للذكاء الاصطناعي
    double Temperature // مدى إبداع الـ AI في التوصيات
);