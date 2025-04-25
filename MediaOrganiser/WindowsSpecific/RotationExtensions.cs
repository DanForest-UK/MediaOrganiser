using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace MediaOrganiser.WindowsSpecific;

public static class RotationExtensions
{
    public static RotateFlipType ToFlipType(this Domain.Rotation rotation) =>
        rotation switch
        {
            Domain.Rotation.Rotate90 => RotateFlipType.Rotate90FlipNone,
            Domain.Rotation.Rotate180 => RotateFlipType.Rotate180FlipNone,
            Domain.Rotation.Rotate270 => RotateFlipType.Rotate270FlipNone,
            _ => RotateFlipType.RotateNoneFlipNone
        };
}

