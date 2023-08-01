using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static TwnUtl;

public static class Twn
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Tween
    {
        public int id = -1;
        public float duration;
        public float start;
        public float end;
        public TwnFunc easeFunction;

        public Action onComplete;
        public Action<float> during;

        public Tween()
        {
            id = -1;
            duration = 0f;
            start = 0f;
            end = 0f;
            easeFunction = TwnFunc.NULL;
            onComplete = null;
            during = null;
        }
    }

    public const int MAX_TWN_CT = 32;
    public static int activeTwns = 0;

    public static twn[] pool = new twn[MAX_TWN_CT];

    [MethodImpl(INLINE)]
    private static void UpdateTwn(ref twn t, in float dt)
    {
        float v;

        t.dt += dt;
        if (t.dt < t.dur)
        {
            v = t.dt * t.invDur;

            switch (t.state.EaseFunction)
            {
                case TwnFunc.QUAD_EO: v = v * (2f - v); break;

                case TwnFunc.QUAD_EI: v = v * v; break;

                case TwnFunc.QUAD_EIO:
                    v = v < 0.5f ? 2f * v * v : v * (4f - 2f * v) - 1f;
                    break;

                case TwnFunc.CUBIC_EO: v = 1f + (--v) * v * v; break;

                case TwnFunc.CUBIC_EI: v = v * v * v; break;

                case TwnFunc.CUBIC_EIO:
                    v = v < 0.5f ? 4f * v * v * v : 1f + (--v) * (2f * (--v)) * (2f * v);
                    break;

                case TwnFunc.QUART_EO:
                    v = (--v) * v;
                    v = 1f - v * v;
                    break;

                case TwnFunc.QUART_EI:
                    v *= v;
                    v = v * v;
                    break;

                case TwnFunc.QUART_EIO:
                    if (v < 0.5f)
                    {
                        v *= v;
                        v = 8f * v * v;
                    }
                    else
                    {
                        v = (--v) * v;
                        v = 1f - 8f * v * v;
                    }
                    break;

                case TwnFunc.QUINT_EO:
                    {
                        float v2 = (--v) * v;
                        v = 1f + v * v2 * v2;
                    }
                    break;

                case TwnFunc.QUINT_EI:
                    {
                        float v2 = v * v;
                        v = v * v2 * v2;
                    }
                    break;

                case TwnFunc.QUINT_EIO:
                    {
                        float v2;
                        if (v < 0.5f)
                        {
                            v2 = v * v;
                            v = 16f * v * v2 * v2;
                        }
                        else
                        {
                            v2 = (--v) * v;
                            v = 1f + 16f * v * v2 * v2;
                        }
                    }
                    break;

                case TwnFunc.POW_EO: v = 1f - MathF.Pow(2f, -8f * v); break;

                case TwnFunc.POW_EI:
                    // 0.003921568 == 1.0 / 255
                    v = (MathF.Pow(2f, 8f * v) - 1f) * 0.003921568f;
                    break;

                case TwnFunc.POW_EIO:
                    if (v < 0.5f)
                    {
                        // 0.0019607 == 1.0 / 510
                        v = (MathF.Pow(2f, 16f * v) - 1f) * 0.0019607f;
                    }
                    else
                    {
                        v = 1f - 0.5f * MathF.Pow(2f, -16f * (v - 0.5f));
                    }
                    break;

                case TwnFunc.CIRC_EO: v = MathF.Sqrt(v); break;

                case TwnFunc.CIRC_EI: v = 1f - MathF.Sqrt(1f - v); break;

                case TwnFunc.CIRC_EIO:
                    if (v < 0.5f)
                    {
                        v = (1f - MathF.Sqrt(1f - 2f * v)) * 0.5f;
                    }
                    else
                    {
                        v = (1f + MathF.Sqrt(2f * v - 1f)) * 0.5f;
                    }
                    break;

                case TwnFunc.BACK_EO:
                    v = 1f + (--v) * v * (2.70158f * v + 1.70158f);
                    break;

                case TwnFunc.BACK_EI: v = v * v * (2.70158f * v - 1.70158f); break;

                case TwnFunc.BACK_EIO:
                    if (v < 0.5f)
                    {
                        v = v * v * (7f * v - 2.5f) * 2f;
                    }
                    else
                    {
                        v = 1f + (--v) * v * 2f * (7f * v + 2.5f);
                    }
                    break;

                case TwnFunc.ELASTIC_EO:
                    {
                        float v2 = (v - 1f) * (v - 1f);
                        v = 1f - v2 * v2 * MathF.Cos(v * MathF.PI * 4.5f);
                    }
                    break;

                case TwnFunc.ELASTIC_EI:
                    {
                        float v2 = v * v;
                        v = v2 * v2 * MathF.Sin(v * MathF.PI * 4.5f);
                    }
                    break;

                case TwnFunc.ELASTIC_EIO:
                    if (v < 0.45f)
                    {
                        float v2 = v * v;
                        v = 8f * v2 * v2 * MathF.Sin(v * MathF.PI * 9f);
                    }
                    else if (v < 0.55f)
                    {
                        v = 0.5f + (0.75f * MathF.Sin(v * MathF.PI * 4f));
                    }
                    else
                    {
                        float v2 = (v - 1f) * (v - 1f);
                        v = 1f - 8f * v2 * v2 * MathF.Sin(v * MathF.PI * 9f);
                    }
                    break;

                case TwnFunc.BOUNCE_EO:
                    v = 1f - (MathF.Pow(2f, -6f * v) * MathF.Abs(MathF.Cos(v * MathF.PI * 3.5f)));
                    break;

                case TwnFunc.BOUNCE_EI:
                    v = (MathF.Pow(2f, 6f * (v - 1f)) * MathF.Abs(MathF.Sin(v * MathF.PI * 3.5f)));
                    break;

                case TwnFunc.BOUNCE_EIO:
                    if (v < 0.5f)
                    {
                        v = 8f * (MathF.Pow(2f, 8f * (v - 1f)) * MathF.Abs(MathF.Sin(v * MathF.PI * 7f)));
                    }
                    else
                    {
                        v = 1f - 8f * (MathF.Pow(2f, -8f * v) * MathF.Abs(MathF.Sin(v * MathF.PI * 7f)));
                    }
                    break;

                case TwnFunc.SIN_EO: v = 1f + MathF.Sin(1.5707963f * (--v)); break;

                case TwnFunc.SIN_EI: v = MathF.Sin(1.5707963f * v); break;

                case TwnFunc.SIN_EIO:
                    v = 0.5f * (1f + MathF.Sin(MathF.PI * (v - 0.5f)));
                    break;

                case TwnFunc.BEZIER: v = v * v * (3f - 2f * v); break;

                case TwnFunc.PULSE:
                    v -= 0.5f;
                    v = -4f * v * v;
                    v += 1f;
                    break;

                case TwnFunc.ESLUP:
                    v -= 0.5f;
                    v = 4f * v * v;
                    break;

                default: break;
            }

            t.during(t.strtVal + (t.dist * v)); //LrpD
        }
        else
        {
            t.during(t.strtVal + t.dist);
            t.state.RUNNING = BOOL.FALSE;
            t.onComplete?.Invoke();
        }
    }

    // Start a tween that will execute once and be freed automatically.
    public static void Start(ref Tween t)
    {
        if (activeTwns >= MAX_TWN_CT) { return; }

        pool[activeTwns].strtVal = t.start;
        pool[activeTwns].dist = t.end - t.start;
        pool[activeTwns].dur = t.duration;
        pool[activeTwns].invDur = 1f / t.duration;
        pool[activeTwns].state.EaseFunction = t.easeFunction;

        pool[activeTwns].during = t.during;
        pool[activeTwns].onComplete = t.onComplete;

        pool[activeTwns].dt = 0f;
        pool[activeTwns].state.RUNNING = BOOL.TRUE;
        t.id = activeTwns++;
    }

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
