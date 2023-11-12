using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Horscht.Logic.Options;

internal class StorageOptions
{
    public required string BlobUri { get; set; }

    public required string UploadContainer { get; set; }
}
