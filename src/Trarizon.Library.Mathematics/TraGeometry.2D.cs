using System.Numerics;
using Trarizon.Library.Mathematics.Helpers;

namespace Trarizon.Library.Mathematics;
public static partial class TraGeometry
{
    public static Vector2 RotateBy90Degree(Vector2 vector, int multiplier)
    {
        multiplier %= 4;
        return multiplier switch
        {
            -3 or 1 => new Vector2(vector.Y, -vector.X),
            -2 or 2 => new Vector2(-vector.X, -vector.Y),
            -1 or 3 => new Vector2(-vector.Y, vector.X),
            _ => Throws.ThrowUnreachable<Vector2>(),
        };
    }
}
