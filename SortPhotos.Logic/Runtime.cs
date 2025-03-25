using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LanguageExt;
using static MediaOrganiser.Core.Types;

namespace MediaOrganiser.Logic
{
    /// <summary>
    /// Dependency injection
    /// </summary>
    public static class Runtime
    {
        public static Func<MediaInfo, string, IO<Unit>> RotateImageAndSave = (_, _) => throw new NotImplementedException();
    }
}
