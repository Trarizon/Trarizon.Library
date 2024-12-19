using System.Numerics;

namespace Trarizon.Library.Numerics;
public static class TraGeometry
{
    public static Vector2 ToNormalized(this Vector2 v)
        => v / v.Length();

    public static Vector3 ToNormalized(this Vector3 v)
        => v / v.Length();

    public static Quaternion ToNormalized(this Quaternion q)
    {
        var len = q.Length();
        if (len < float.Epsilon)
            return Quaternion.Identity;
        else
            return new(q.X / len, q.Y / len, q.Z / len, q.W / len);
    }

#if !NETSTANDARD2_0 // 反正这东西纯粹写着玩的

    public static Vector3 ToEulerAngles(this Quaternion q)
    {
        const float RadToAngle = 180 / float.Pi;

        float sinr_cosp = 2f * (q.W * q.X + q.Y * q.Z);
        float cosr_cosp = 1f - 2f * (q.X * q.X + q.Y * q.Y);
        float roll = float.Atan2(sinr_cosp, cosr_cosp) * RadToAngle;

        float sinp = 2f * (q.W * q.Y - q.Z * q.X);
        float pitch = sinp switch
        {
            <= -1f => -90f,
            >= 1f => -90f,
            _ => float.Asin(sinp) * RadToAngle,
        };

        float siny_cosy = 2f * (q.W * q.Z + q.X * q.Y);
        float cosy_cosy = 1f - 2f * (q.Y * q.Y + q.Z * q.Z);
        float yaw = float.Atan2(siny_cosy, cosy_cosy) * RadToAngle;

        return new(roll, pitch, yaw);
    }

#endif
}
