using Godot;
using Matrix4x4 = System.Numerics.Matrix4x4;

public class Vector4
{
    public float x, y, z, w;

    public Vector4(float x, float y, float z, float w)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        this.w = w;
    }

    public static Vector4 Zero = new Vector4(0, 0, 0, 0);
    public static Vector4 Left = new Vector4(-1, 0, 0, 0);
    public static Vector4 Right = new Vector4(1, 0, 0, 0);
    public static Vector4 Up = new Vector4(0, -1, 0, 0);
    public static Vector4 Down = new Vector4(0, 1, 0, 0);
    public static Vector4 Forward = new Vector4(0, 0, -1, 0);
    public static Vector4 Back = new Vector4(0, 0, 1, 0);
    public static Vector4 Ana = new Vector4(0, 0, 0, -1);
    public static Vector4 Kata = new Vector4(0, 0, 0, 1);

    public static Vector4 operator +(Vector4 a, Vector4 b)
        => new Vector4(a.x + b.x, a.y + b.y, a.z + b.z, a.w + b.w);

    public static Vector4 operator -(Vector4 a, Vector4 b)
        => new Vector4(a.x - b.x, a.y - b.y, a.z - b.z, a.w - b.w);

    public static Vector4 operator *(float x, Vector4 a)
        => new Vector4(x * a.x, x * a.y, x * a.z, x * a.w);

    public static Vector4 operator *(Vector4 a, float x)
        => new Vector4(x * a.x, x * a.y, x * a.z, x * a.w);

    public static Vector4 operator /(Vector4 a, float x)
        => new Vector4(a.x / x, a.y / x, a.z / x, a.w / x);

    public static bool operator ==(Vector4 a, Vector4 b)
        => (a.x == b.x && a.y == b.y && a.z == b.z && a.w == b.w);

    public static bool operator !=(Vector4 a, Vector4 b)
        => (a.x != b.x || a.y != b.y || a.z != b.z || a.w != b.w);

    public static Vector4 operator *(Matrix4x4 M, Vector4 v)
        => new Vector4(
            M.M11 * v.x + M.M12 * v.y + M.M13 * v.z + M.M14 * v.w,
            M.M21 * v.x + M.M22 * v.y + M.M23 * v.z + M.M24 * v.w,
            M.M31 * v.x + M.M32 * v.y + M.M33 * v.z + M.M34 * v.w,
            M.M41 * v.x + M.M42 * v.y + M.M43 * v.z + M.M44 * v.w);

    public float Dot(Vector4 a)
        => x * a.x + y * a.y + z * a.z + w * a.w;

    public float LengthSquared()
        => x * x + y * y + z * z + w * w;

    public float Length()
        => Mathf.Sqrt(LengthSquared());

    public Vector4 Normalize()
        => this / Length();

    public Vector4 Negate()
        => new Vector4(-x, -y, -z, -w);

    public Vector4 RemoveComponent(Vector4 axis)
    {
        return this - ((this.Dot(axis) / axis.LengthSquared()) * axis);
    }

    public Plane ToPlane()
    {
        return new Plane(x, y, z, w);
    }

    public override string ToString()
    {
        return "(" + x + ", " + y + ", " + z + ", " + w + ")";
    }
}
