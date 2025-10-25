using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SubmissionService.Domain;

public partial class Language
{
    [Key]
    [Column("languageId")]
    public int LanguageId { get; set; }

    [Column("languageName")]
    [StringLength(100)]
    public string LanguageName { get; set; } = null!;

    [InverseProperty("Language")]
    public virtual ICollection<Submission> Submissions { get; set; } = new List<Submission>();
}
