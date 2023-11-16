using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Horscht.Contracts.Messages;
public class ImportMessage
{
    public required string FileUri { get; set; }
}
