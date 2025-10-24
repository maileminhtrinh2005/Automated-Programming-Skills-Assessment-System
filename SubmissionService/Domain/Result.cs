using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SubmissionService.Domain;

public partial class Result
{
    [Key]
    public int ResultId { get; set; }

    public int SubmissionId { get; set; }

    public int TestCaseId { get; set; }

    public bool Passed { get; set; }

    public double ExecutionTime { get; set; }

    public int MemoryUsed { get; set; }

    public string? Output { get; set; }

    public string? ErrorMessage { get; set; }

    [ForeignKey("SubmissionId")]
    [InverseProperty("Results")]
    public virtual Submission Submission { get; set; } = null!;
}
