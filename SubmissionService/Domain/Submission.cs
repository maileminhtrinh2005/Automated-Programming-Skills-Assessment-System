using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SubmissionService.Domain;

public partial class Submission
{
    [Key]
    public int SubmissionId { get; set; }

    public int AssignmentId { get; set; }

    public int StudentId { get; set; }

    public string Code { get; set; } = null!;

    public int LanguageId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime SubmittedAt { get; set; }

    [StringLength(20)]
    public string Status { get; set; } = null!;

    public double Score { get; set; }

    [ForeignKey("LanguageId")]
    [InverseProperty("Submissions")]
    public virtual Language Language { get; set; } = null!;

    [InverseProperty("Submission")]
    public virtual ICollection<Result> Results { get; set; } = new List<Result>();
}
