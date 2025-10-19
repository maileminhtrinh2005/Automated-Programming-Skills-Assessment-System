using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShareLibrary.Event
{
    public class CodeSubmittedEvents
    {
        // submission send it to assignment, 
        public string? SourceCode { get; set; }
        public int LanguageId { get; set; }
        public int AssignmentId { get; set; }
        public int SubmissionId { get; set; }

    }

    public class TestCaseEvent
    {
        public int TestCaseId { get; set; }
        public string? Input { get; set; }
        public string? ExpectedOutput { get; set; }
        public double Weight { get; set; }
    }


    public class TestCaseFetchEvent
    {
        public string? SourceCode { get; set; }
        public int LanguageId { get; set; }
        public int AssignmentId { get; set; }
        public int SubmissionId { get; set; }
        public List<TestCaseEvent> TestCaseList { get; set; } = new List<TestCaseEvent>();
    }
}
