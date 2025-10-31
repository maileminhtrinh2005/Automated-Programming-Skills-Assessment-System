using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShareLibrary.Event
{
    public class DeadlineNotification
    {
        public string Message { get; set; } = string.Empty;
        public DateTime Deadline {  get; set; }

    }
}
