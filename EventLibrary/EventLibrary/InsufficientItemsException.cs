using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventLibrary
{
    public class InsufficientItemsException : Exception
    {
        public InsufficientItemsException(string message)
            : base(message) { }
    }
}
