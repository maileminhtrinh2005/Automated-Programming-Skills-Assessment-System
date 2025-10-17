using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShareLibrary
{
    public interface IEventHandler<in TEvent> where TEvent : class
    {
        Task Handle(TEvent @event);
    }
}
