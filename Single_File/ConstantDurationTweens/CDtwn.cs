using System;
using System.Runtime.InteropServices;

public static unsafe class CDtwn // "CD" -> Constant Duration
{
    #region Set Up

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
        LINEAR
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct state // 4 bytes in size.
    {
        [FieldOffset(0)] public int full;
        [FieldOffset(0)] public byte RESERVED;
        [FieldOffset(1)] public byte RUNNING;
        [FieldOffset(2)] public TwnFunc TF;
        [FieldOffset(3)] public byte ID;
    }

    // Basic tween structure for tweens that have a constant duration.
    [StructLayout(LayoutKind.Sequential)]
    public struct constDurTwn // 16 bytes in size
    {
        public float dt;
        public float strtVal;
        public float dist;
        public state state;
    }

    // For state structure comparisons
    public const byte FALSE = 0;
    public const byte TRUE = 1;

    #endregion

    public const float TWN_DUR = 0.404508f; // ϕ/8 = golden ratio divided by eight
    public const float INV_TWN_DUR = 1.0f / TWN_DUR; // 8/ϕ = eight divided by the golden ratio
    public const int MAX_TWN_CT = 16;
    public static int reservedTwns = 0;
    public static int currActiveTwns = 0;

    public static int[] executingTwnIDs = new int[MAX_TWN_CT];
    public static constDurTwn[] pool = new constDurTwn[MAX_TWN_CT];
    public static Action<float>[] duringFuncs = new Action<float>[MAX_TWN_CT];
    public static Action[] doneFuncs = new Action[MAX_TWN_CT];

    // Unreserve all tweens in the pool.
    // Will stop execution of a currently running tween.
    public static void FlushPool(bool stopExe = false)
    {
        if(stopExe == false)
        {
            for (int i = 0; i < MAX_TWN_CT; ++i)
            {
                if (pool[i].state.RESERVED == TRUE)
                {
                    pool[i].state.RESERVED = FALSE;
                    pool[i].state.RUNNING = FALSE;
                }
            }

            reservedTwns = 0;
            currActiveTwns = 0;
        }
        else
        {
            for (int i = 0; i < MAX_TWN_CT; ++i)
            {
                if (pool[i].state.RESERVED == TRUE)
                {
                    pool[i].state.RESERVED = FALSE;
                }
            }
            
            reservedTwns = 0;
        }
    }

    // Reserves a floating point tween in the pool for the caller. Returns an 
    // integer index that is used to access that same pool
    // 
    // A return value of -1 indicates that there were no more tweens to be reserved.
    public static int Reserve()
    {
        if (reservedTwns < MAX_TWN_CT)
        {
            for (int i = 0; i < MAX_TWN_CT; ++i)
            {
                if (pool[i].state.RESERVED == TRUE) { continue; }
                else
                {
                    pool[i].state.RESERVED = TRUE;
                    ++reservedTwns;
                    return i;
                }
            }
        }

        return -1;
    }

    // "Frees" a reserved tween to be reserved again.
    public static void Free(int id)
    {
        bool idInRange = id < MAX_TWN_CT & id > -1;
        if (idInRange && pool[id].state.RESERVED == TRUE)
        {
            pool[id].state.RESERVED = FALSE;
            --reservedTwns;
        }
    }

    // Reserves AND Initializes a UI tween in the pool for the caller. 
    // Returns the integer index that is used to access that same pool
    // 
    // A return value of -1 indicates that reservation failed.
    public static int ResInit(float start, float end, Action onComplete, Action<float> during, TwnFunc twnFunc = TwnFunc.BEZIER)
    {
        if (reservedTwns < MAX_TWN_CT)
        {
            for (int i = 0; i < MAX_TWN_CT; ++i)
            {
                if (pool[i].state.RESERVED == TRUE) { continue; }
                else
                {
                    pool[i].state.RESERVED = TRUE;
                    pool[i].strtVal = start;
                    pool[i].dist = end - start;
                    pool[i].state.TF = twnFunc;

                    duringFuncs[i] = during;
                    doneFuncs[i] = onComplete;

                    ++reservedTwns;
                    return i;
                }
            }
        }
        return -1;
    }

    // Initializes a reserved tween. Needs to be called before you use your tween for the first time.
    public static void Init(int id, float start, float end, Action onComplete, Action<float> during, TwnFunc twnFunc = TwnFunc.BEZIER)
    {
        pool[id].strtVal = start;
        pool[id].dist = end - start;
        pool[id].state.TF = twnFunc;

        duringFuncs[id] = during;
        doneFuncs[id] = onComplete;
    }

    // TODO(RYAN): Update this to set animated tween to the front of active/animating tween queue
    // 
    // Start executing tween at id.
    public static void Start(int id, float start, float end)
    {
        pool[id].dt = 0.0f;
        pool[id].strtVal = start;
        pool[id].dist = end - start;

        if (pool[id].state.RUNNING == FALSE)
        {
            pool[id].state.RUNNING = TRUE;
            ++currActiveTwns;
        }
    }

    // TODO(RYAN): Update this to set animated tween to the front of active/animating tween queue
    // 
    // Start executing tween at id.
    public static void Start(int id)
    {
        pool[id].dt = 0.0f;

        if (pool[id].state.RUNNING == FALSE)
        {
            pool[id].state.RUNNING = TRUE;
            ++currActiveTwns;
        }
    }

    public static void Start(int id, Action<float> during, Action onComplete)
    {
        pool[id].dt = 0.0f;

        duringFuncs[id] = during;
        doneFuncs[id] = onComplete;

        if (pool[id].state.RUNNING == FALSE)
        {
            pool[id].state.RUNNING = TRUE;
            ++currActiveTwns;
        }
    }

    // Restart a tween without modifying any other properties or values.
    public static void Restart (int id)
    {
        pool[id].dt = 0.0f;
        pool[id].state.RUNNING = TRUE;
        ++currActiveTwns;
    }

    // Flips the starting and ending values
    public static void Flip(int id)
    {
        float t = pool[id].strtVal;
        pool[id].strtVal += pool[id].dist;
        pool[id].dist = t - pool[id].strtVal;
    }

    // Flips the starting and ending point of the tween and then starts the tween. Useful for tweens that oscillate
    // between two different values.
    public static void StartFlip(int id)
    {
        float t = pool[id].strtVal;
        pool[id].strtVal += pool[id].dist;
        pool[id].dist = t - pool[id].strtVal;
        pool[id].dt = 0.0f;
        pool[id].state.RUNNING = TRUE;
        ++currActiveTwns;
    }

    // Stops Tween from executing.
    public static void StopTwn(int id, bool onComplete = true)
    {
        if (pool[id].state.RUNNING == TRUE)
        {
            pool[id].state.RUNNING = FALSE;
            --currActiveTwns;
        }

        if (doneFuncs[id] != null && onComplete) { doneFuncs[id](); }
    }

    // Should be called every tick in your update loop.
    public static void Eval(float dt)
    {
        if (currActiveTwns > -1)
        {
            int twns2Update = currActiveTwns;
            float t;
            for (int i = 0; twns2Update > 0; ++i)
            {
                if (pool[i].state.RUNNING == FALSE) { /*Debug.Log("Taken.");*/ continue; }
                else
                {
                    //Debug.Log("Not Taken");
                    --twns2Update;
                    pool[i].dt += dt;
                    if (pool[i].dt < TWN_DUR)
                    {
                        t = pool[i].dt * INV_TWN_DUR;

                        switch (pool[i].state.TF)
                        {
                            case TwnFunc.QUAD_EO:
                                t = t * (2.0f - t);
                                break;

                            case TwnFunc.QUAD_EI:
                                t = t * t;
                                break;

                            case TwnFunc.QUAD_EIO:
                                t = t < 0.5f ? 2.0f * t * t : t * (4.0f - 2.0f * t) - 1.0f;
                                break;

                            case TwnFunc.CUBIC_EO:
                                t = 1f + (--t) * t * t;
                                break;

                            case TwnFunc.CUBIC_EI:
                                t = t * t * t;
                                break;

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
                                t = 1f - (float)Math.Pow(2, -8 * t);
                                break;

                            case TwnFunc.POW_EI:
                                // 0.003921568 == 1.0 / 255
                                t = ((float)Math.Pow(2, 8 * t) - 1f) * 0.003921568f;
                                break;

                            case TwnFunc.POW_EIO:
                                if (t < 0.5f)
                                {
                                    // 0.0019607 == 1.0 / 510
                                    t = ((float)Math.Pow(2, 16 * t) - 1f) * 0.0019607f;
                                }
                                else
                                {
                                    t = 1f - 0.5f * (float)Math.Pow(2, -16 * (t - 0.5f));
                                }
                                break;

                            case TwnFunc.CIRC_EO:
                                t = (float)Math.Sqrt(t);
                                break;

                            case TwnFunc.CIRC_EI:
                                t = 1f - (float)Math.Sqrt(1 - t);
                                break;

                            case TwnFunc.CIRC_EIO:
                                if (t < 0.5f)
                                {
                                    t = (1f - (float)Math.Sqrt(1 - 2 * t)) * 0.5f;
                                }
                                else
                                {
                                    t = (1f + (float)Math.Sqrt(2 * t - 1)) * 0.5f;
                                }
                                break;

                            case TwnFunc.BACK_EO:
                                t = 1f + (--t) * t * (2.70158f * t + 1.70158f);
                                break;

                            case TwnFunc.BACK_EI:
                                t = t * t * (2.70158f * t - 1.70158f);
                                break;

                            case TwnFunc.BACK_EIO:
                                if (t < 0.5f)
                                {
                                    t = t * t * (7f * t - 2.5f) * 2f;
                                }
                                else
                                {
                                    t = 1f + (--t) * t * 2f * (7f * t + 2.5f);
                                }
                                break;

                            case TwnFunc.ELASTIC_EO:
                                {
                                    float t2 = (t - 1f) * (t - 1f);
                                    t = 1f - t2 * t2 * (float)Math.Cos(t * Math.PI * 4.5);
                                }
                                break;

                            case TwnFunc.ELASTIC_EI:
                                {
                                    float t2 = t * t;
                                    t = t2 * t2 * (float)Math.Sin(t * Math.PI * 4.5);
                                }
                                break;

                            case TwnFunc.ELASTIC_EIO:
                                if (t < 0.45f)
                                {
                                    float t2 = t * t;
                                    t = 8f * t2 * t2 * (float)Math.Sin(t * Math.PI * 9.0);
                                }
                                else if (t < 0.55f)
                                {
                                    t = 0.5f + (0.75f * (float)Math.Sin(t * Math.PI * 4.0));
                                }
                                else
                                {
                                    float t2 = (t - 1f) * (t - 1f);
                                    t = 1f - 8f * t2 * t2 * (float)Math.Sin(t * Math.PI * 9.0);
                                }
                                break;

                            case TwnFunc.BOUNCE_EO:
                                t = 1f - (float)(Math.Pow(2.0, -6.0 * t) * Math.Abs(Math.Cos(t * Math.PI * 3.5)));
                                break;

                            case TwnFunc.BOUNCE_EI:
                                t = (float)(Math.Pow(2, 6 * (t - 1f)) * Math.Abs(Math.Sin(t * Math.PI * 3.5)));
                                break;

                            case TwnFunc.BOUNCE_EIO:
                                if (t < 0.5f)
                                {
                                    t = 8f * (float)(Math.Pow(2.0, 8.0 * (t - 1f)) * Math.Abs(Math.Sin(t * Math.PI * 7.0)));
                                }
                                else
                                {
                                    t = 1f - 8f * (float)(Math.Pow(2.0, -8.0 * t) * Math.Abs(Math.Sin(t * Math.PI * 7.0)));
                                }
                                break;

                            case TwnFunc.SIN_EO:
                                t = 1.0f + (float)Math.Sin(1.5707963 * (--t));
                                break;

                            case TwnFunc.SIN_EI:
                                t = (float)Math.Sin(1.5707963 * (double)t);
                                break;

                            case TwnFunc.SIN_EIO:
                                t = 0.5f * (1.0f + (float)Math.Sin(3.1415926 * (t - 0.5f)));
                                break;

                            case TwnFunc.BEZIER:
                                t = t * t * (3.0f - 2.0f * t);
                                break;

                        }

                        //LrpD
                        duringFuncs[i](pool[i].strtVal + (pool[i].dist * t));
                    }
                    else
                    {
                        duringFuncs[i](pool[i].strtVal + pool[i].dist);
                        pool[i].state.RUNNING = FALSE;
                        --currActiveTwns;

                        if (doneFuncs[i] != null) { doneFuncs[i](); }
                    }
                }
            }
        }
    }
}
