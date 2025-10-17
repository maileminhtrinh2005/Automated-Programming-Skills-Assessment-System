using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShareLibrary.Event
{
    public class CodeSubmittedEvents
    {
        // assignment send to submission 
        public string? SourceCode { get; set; }
        public int LanguageId { get; set; }
        public int AssignmentId { get; set; }// luu de ti cap nhat sau



        // submission execute and send result to assginment (testcase)
        public string? Status { get; set; }
        public string? Output { get; set; }
        public string? ErrorMessage { get; set; }
        public double ExecutionTime { get; set; }
        public int MemoryUsed { get; set; }

    }
}
