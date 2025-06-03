using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trarizon.Library.Experimental.Interpreting.Brainfuck;
internal class BrainfuckException(string? message = null) : Exception(message)
{
}
