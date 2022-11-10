using System.Runtime.InteropServices;

public static class TwnUtl
{
    public const float TWN_DUR = 0.404508f; // ϕ/8 = golden ratio divided by eight
    public const float INV_TWN_DUR = 1.0f / TWN_DUR; // 8/ϕ = eight divided by the golden ratio

    // Basic tween structure for tweens that have a constant duration.
    [StructLayout(LayoutKind.Sequential)]
    public struct constDurTwn // 16 bytes in size
    {
        public float dt;
        public float strtVal;
        public float dist;
        public state state;
    }

    // Basic tween structure for tweens that have a variable duration.
    [StructLayout(LayoutKind.Sequential)]
    public struct twn // 24 bytes in size
    {
        public float dt;
        public float strtVal;
        public float dist;
        public float dur;
        public float invDur;
        public state state;
    }

    public enum TwnFunc : byte
    {
        // EO -> Ease Out
        // EI -> Ease In
        // EIO -> Ease In/Out
        QUAD_EO = 0,
        QUAD_EI,
        QUAD_EIO,
        CUBIC_EO,
        CUBIC_EI,
        CUBIC_EIO,
        QUART_EO,
        QUART_EI,
        QUART_EIO,
        QUINT_EO,
        QUINT_EI,
        QUINT_EIO,

        POW_EO,
        POW_EI,
        POW_EIO,
        CIRC_EO,
        CIRC_EI,
        CIRC_EIO,
        BACK_EO,
        BACK_EI,
        BACK_EIO,
        ELASTIC_EO,
        ELASTIC_EI,
        ELASTIC_EIO,
        BOUNCE_EO,
        BOUNCE_EI,
        BOUNCE_EIO,
        SIN_EO,
        SIN_EI,
        SIN_EIO,

        BEZIER,
        LINEAR = 255
    }

    public const byte FALSE = 0;
    public const byte TRUE = 1;

    [StructLayout(LayoutKind.Explicit)]
    public struct state // 4 bytes in size.
    {
        [FieldOffset(0)] public int full;
        [FieldOffset(0)] public byte RESERVED;
        [FieldOffset(1)] public byte RUNNING;
        [FieldOffset(2)] public TwnFunc TF;
        [FieldOffset(3)] public byte ID;
    }
}
