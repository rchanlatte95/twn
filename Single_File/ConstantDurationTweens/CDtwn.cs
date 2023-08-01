using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

public static class CDtwn // "CD" -> Constant Duration
{
    public const MethodImplOptions INLINE = MethodImplOptions.AggressiveInlining;

    [StructLayout(LayoutKind.Explicit)]
    public struct state // 4 bytes in size.
    {
        [FieldOffset(0)] public int full;
        [FieldOffset(0)] public BOOL RUNNING;
        [FieldOffset(1)] public TwnFunc EaseFunction;
        [FieldOffset(2)] public ushort id;
    }

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
        public Action? onComplete;
        public Action<float>? during;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Tween
    {
        public int id;
        public float start;
        public float end;
        public TwnFunc easeFunction;

        public Action onComplete;
        public Action<float> during;
    }

    public const float FLT_EPSILON = 0.0001f;
    public const float GOLDEN_RATIO = 1.6180339f;
    public const float TWN_DUR = 1f / (GOLDEN_RATIO * 2f); // ~0.309017011
    public const float INV_TWN_DUR = 1f / TWN_DUR;

    public const int MAX_TWN_CT = 32;
    public static int activeTwns = 0;

    public static constDurTwn[] pool = new constDurTwn[MAX_TWN_CT];

    // Start executing tween at id.
    public static void Start(ref Tween cdt)
    {
        if(activeTwns > MAX_TWN_CT) { return; }

        pool[activeTwns].dt = 0f;
        pool[activeTwns].strtVal = cdt.start;
        pool[activeTwns].dist = cdt.end - cdt.start;
        pool[activeTwns].during = cdt.during;
        pool[activeTwns].onComplete = cdt.onComplete;

        if (pool[activeTwns].state.RUNNING == BOOL.FALSE)
        {
            pool[activeTwns].state.RUNNING = BOOL.TRUE;
        }

        cdt.id = activeTwns;
    }

    // Stops Tween from executing.
    public static void Stop(int id, bool exeOnComplete = true)
    {
        bool inValidId = id < 0 || id >= MAX_TWN_CT;
        if (activeTwns < 1 || inValidId) { return; }

        if (pool[id].state.RUNNING == BOOL.TRUE)
        {
            pool[id].state.RUNNING = BOOL.FALSE;
            pool[id] = pool[--activeTwns];
        }

        if (exeOnComplete) { pool[id].onComplete?.Invoke(); }
    }

    private static void UpdateTwn(ref constDurTwn cdt, in float dt)
    {
        float t;
        cdt.dt += dt;
        if (cdt.dt < TWN_DUR)
        {
            t = cdt.dt * INV_TWN_DUR;

            switch (cdt.state.EaseFunction)
            {
                case TwnFunc.QUAD_EO: t = t * (2f - t); break;

                case TwnFunc.QUAD_EI: t = t * t; break;

                case TwnFunc.QUAD_EIO:
                    t = t < 0.5f ? 2f * t * t : t * (4f - 2f * t) - 1f;
                    break;

                case TwnFunc.CUBIC_EO: t = 1f + (--t) * t * t; break;

                case TwnFunc.CUBIC_EI: t = t * t * t; break;

                case TwnFunc.CUBIC_EIO:
                    t = t < 0.5f ? 4f * t * t * t : 1f + (--t) * (2f * (--t)) * (2f * t);
                    break;

                case TwnFunc.QUART_EO:
                    t = (--t) * t;
                    t = 1f - t * t;
                    break;

                case TwnFunc.QUART_EI:
                    t *= t;
                    t = t * t;
                    break;

                case TwnFunc.QUART_EIO:
                    if (t < 0.5f)
                    {
                        t *= t;
                        t = 8f * t * t;
                    }
                    else
                    {
                        t = (--t) * t;
                        t = 1f - 8f * t * t;
                    }
                    break;

                case TwnFunc.QUINT_EO:
                    {
                        float t2 = (--t) * t;
                        t = 1f + t * t2 * t2;
                    }
                    break;

                case TwnFunc.QUINT_EI:
                    {
                        float t2 = t * t;
                        t = t * t2 * t2;
                    }
                    break;

                case TwnFunc.QUINT_EIO:
                    {
                        float t2;
                        if (t < 0.5f)
                        {
                            t2 = t * t;
                            t = 16f * t * t2 * t2;
                        }
                        else
                        {
                            t2 = (--t) * t;
                            t = 1f + 16f * t * t2 * t2;
                        }
                    }
                    break;

                case TwnFunc.POW_EO:
                    t = 1f - MathF.Pow(2f, -8f * t);
                    break;

                case TwnFunc.POW_EI:
                    // 0.003921568 == 1.0 / 255
                    t = (MathF.Pow(2f, 8f * t) - 1f) * 0.003921568f;
                    break;

                case TwnFunc.POW_EIO:
                    if (t < 0.5f)
                    {
                        // 0.0019607 == 1.0 / 510
                        t = (MathF.Pow(2f, 16f * t) - 1f) * 0.0019607f;
                    }
                    else
                    {
                        t = 1f - 0.5f * MathF.Pow(2f, -16f * (t - 0.5f));
                    }
                    break;

                case TwnFunc.CIRC_EO: t = MathF.Sqrt(t); break;

                case TwnFunc.CIRC_EI: t = 1f - MathF.Sqrt(1f - t); break;

                case TwnFunc.CIRC_EIO:
                    if (t < 0.5f) { t = (1f - MathF.Sqrt(1f - 2f * t)) * 0.5f; }
                    else { t = (1f + MathF.Sqrt(2f * t - 1f)) * 0.5f; }
                    break;

                case TwnFunc.BACK_EO:
                    t = 1f + (--t) * t * (2.70158f * t + 1.70158f);
                    break;

                case TwnFunc.BACK_EI:
                    t = t * t * (2.70158f * t - 1.70158f);
                    break;

                case TwnFunc.BACK_EIO:
                    if (t < 0.5f) { t = t * t * (7f * t - 2.5f) * 2f; }
                    else { t = 1f + (--t) * t * 2f * (7f * t + 2.5f); }
                    break;

                case TwnFunc.ELASTIC_EO:
                    {
                        float t2 = (t - 1f) * (t - 1f);
                        t = 1f - t2 * t2 * MathF.Cos(t * MathF.PI * 4.5f);
                    }
                    break;

                case TwnFunc.ELASTIC_EI:
                    {
                        float t2 = t * t;
                        t = t2 * t2 * MathF.Sin(t * MathF.PI * 4.5f);
                    }
                    break;

                case TwnFunc.ELASTIC_EIO:
                    if (t < 0.45f)
                    {
                        float t2 = t * t;
                        t = 8f * t2 * t2 * MathF.Sin(t * MathF.PI * 9f);
                    }
                    else if (t < 0.55f)
                    {
                        t = 0.5f + (0.75f * MathF.Sin(t * MathF.PI * 4f));
                    }
                    else
                    {
                        float t2 = (t - 1f) * (t - 1f);
                        t = 1f - 8f * t2 * t2 * MathF.Sin(t * MathF.PI * 9f);
                    }
                    break;

                case TwnFunc.BOUNCE_EO:
                    t = 1f - (MathF.Pow(2f, -6f * t) * MathF.Abs(MathF.Cos(t * MathF.PI * 3.5f)));
                    break;

                case TwnFunc.BOUNCE_EI:
                    t = (MathF.Pow(2f, 6f * (t - 1f)) * MathF.Abs(MathF.Sin(t * MathF.PI * 3.5f)));
                    break;

                case TwnFunc.BOUNCE_EIO:
                    if (t < 0.5f)
                    {
                        t = 8f * (MathF.Pow(2f, 8f * (t - 1f)) * MathF.Abs(MathF.Sin(t * MathF.PI * 7f)));
                    }
                    else
                    {
                        t = 1f - 8f * (MathF.Pow(2F, -8F * t) * MathF.Abs(MathF.Sin(t * MathF.PI * 7f)));
                    }
                    break;

                case TwnFunc.SIN_EO: t = 1f + MathF.Sin(1.5707963f * (--t)); break;

                case TwnFunc.SIN_EI: t = MathF.Sin(1.5707963f * t); break;

                case TwnFunc.SIN_EIO:
                    t = 0.5f * (1f + MathF.Sin(MathF.PI * (t - 0.5f)));
                    break;

                case TwnFunc.BEZIER: t = t * t * (3f - 2f * t); break;

                case TwnFunc.PULSE:
                    t -= 0.5f;
                    t = -4f * t * t;
                    t += 1f;
                    break;

                case TwnFunc.ESLUP:
                    t -= 0.5f;
                    t = 4f * t * t;
                    break;

                default: break;

            }

            cdt.during(cdt.strtVal + (cdt.dist * t)); //LrpD
        }
        else
        {
            cdt.during(cdt.strtVal + cdt.dist);
            cdt.state.RUNNING = BOOL.FALSE;
            --activeTwns;

            cdt.onComplete?.Invoke();
        }
    }

    // Should be called every tick in your update loop.
    public static void Eval(float dt)
    {
        if (activeTwns < 1 || MathF.Abs(dt) < FLT_EPSILON) { return; }

        for (int i = 0; i < activeTwns; ++i)
        {
            UpdateTwn(ref pool[i], in dt);

            if (pool[i].state.RUNNING == BOOL.FALSE)
            {
                pool[i] = pool[--activeTwns];
                --i;
            }
        }
    }
}
