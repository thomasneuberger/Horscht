using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Forms;

namespace Horscht.App.ViewModels;
internal class UploadFile
{
    public required IBrowserFile File { get; set; }

    public bool IsUploading { get; set; }

    public bool IsUploaded { get; set; }
}
