using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Depot.Common.Navigation
{
    internal interface Navigatable
    {
        public char KeyChar { get; set; }
        public Action? Action { get; set; }

    }
}
