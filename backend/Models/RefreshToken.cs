using System;
using System.Collections.Generic;

namespace backend.Models;

public partial class RefreshToken
{
    public Guid Id { get; set; }

    public string TokenHash { get; set; } = null!;

    public DateTime ExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? RevokedAt { get; set; }

    public string? ReplacedByTokenHash { get; set; }

    public Guid UserId { get; set; }
}
