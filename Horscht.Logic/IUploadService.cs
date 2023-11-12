using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Horscht.Logic;
public interface IUploadService
{
    Task UploadFile(Stream fileStream, string filename);
}
