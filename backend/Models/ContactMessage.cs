using System;
using System.Collections.Generic;

namespace backend.Models;

public partial class ContactMessage
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Subject { get; set; }

    public string Message { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}
