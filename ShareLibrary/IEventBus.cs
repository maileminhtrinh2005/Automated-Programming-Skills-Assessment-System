using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShareLibrary
{
    public interface IEventBus
    {

        void Publish<T>(T @event) where T : class;
        void Subscribe<T, TH>() where T : class where TH : IEventHandler<T>;

    }
}
