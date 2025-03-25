using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MediaOrganiser.Core.Types;

namespace MediaOrganiser.Core
{
    public static class Logic
    {
        public static int CountFilesForDeletion(this AppModel model) =>
           model.Files.Values.Count(f => f.State == FileState.Bin);
    }
}
