using System;
using System.Collections.Generic;

namespace backend.Models.Generated;

public partial class site_content
{
    public Guid id { get; set; }

    public string content_key { get; set; } = null!;

    public string content_value { get; set; } = null!;
}
