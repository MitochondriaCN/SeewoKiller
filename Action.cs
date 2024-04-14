using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SeewoKiller
{
    internal abstract class Action
    {
        abstract internal bool Execute();
        abstract public override string ToString();
    }
}
