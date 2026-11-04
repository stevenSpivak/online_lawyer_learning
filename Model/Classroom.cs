using System;
using System.Collections.Generic;
using System.Text;

namespace Model;

public class Classroom
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public int TeacherId { get; set; }

    public int Students { get; set; }

    public DateTime CreatedAt { get; set; }
}