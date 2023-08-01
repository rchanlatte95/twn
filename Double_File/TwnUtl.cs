using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

public enum TwnFunc : byte
{
    // EO   → Ease Out
    // EI   → Ease In
    // EIO  → Ease In/Out

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
    PULSE, // 1 → 0 → 1
    ESLUP, // 0 → 1 → 0
    LINEAR = 254,
    NULL = 255
}

public enum BOOL : byte { TRUE = 1, FALSE = 0 }

public static class TwnUtl
{
    public const MethodImplOptions INLINE = MethodImplOptions.AggressiveInlining;
    public const float FLT_EPSILON = 0.0001f;

    // Basic tween structure for tweens that have a constant duration.
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct constDurTwn // 32 bytes in size
    {
        // 16 bytes
        public float dt;
        public float strtVal;
        public float dist;
        public state state;

        // 16 bytes
        public Action onComplete;
        public Action<float> during;
    }

    // Basic tween structure for tweens that have a variable duration.
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct twn // 40 bytes in size
    {
        // 24 bytes
        public float dt;
        public float strtVal;
        public float dist;
        public float dur;
        public float invDur;
        public state state;

        // 16 bytes
        public Action onComplete;
        public Action<float> during;

        [MethodImpl(INLINE)]
        public static void Swap(ref twn a, ref twn b, bool preserveID = false)
        {
            twn tmp = a;
            a = b;
            b = tmp;

            if (preserveID)
            {
                // since we just swapped them, swap them back
                ushort a_ID = b.state.id;
                ushort b_ID = a.state.id;
                a.state.id = a_ID;
                b.state.id = b_ID;
            }
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct state // 4 bytes in size.
    {
        [FieldOffset(0)] public int full;
        [FieldOffset(0)] public BOOL RUNNING;
        [FieldOffset(1)] public TwnFunc EaseFunction;
        [FieldOffset(2)] public ushort id;
    }
}
