using System;

// EO -> Ease Out
// EI -> Ease In
// EIO -> Ease In/Out
public static class TwnFunctions
{
    public static float Linear(float s, float e, float t)
    {
        return s + (t * (e - s));
    }

    public static float SinEI(float t)
    {
        return (float)Math.Sin(1.5707963 * (double)t);
    }

    public static float SineEO(float t)
    {
        return 1.0f + (float)Math.Sin(1.5707963 * (--t));
    }

    public static float SineEIO(float t)
    {
        return 0.5f * (1.0f + (float)Math.Sin(3.1415926 * (t - 0.5f)));
    }

    public static float QuadEI(float t)
    {
        return t * t;
    }

    public static float QuadEO(float t)
    {
        return t * (2.0f - t);
    }

    public static float QuadEIO(float t)
    {
        return t < 0.5f ? 2.0f * t * t : t * (4.0f - 2.0f * t) - 1.0f;
    }

    public static float CubicEI(float t)
    {
        return t * t * t;
    }

    public static float CubicEO(float t)
    {
        return 1f + (--t) * t * t;
    }

    public static float CubicEIO(float t)
    {
        return t < 0.5f ? 4f * t * t * t : 1f + (--t) * (2f * (--t)) * (2f * t);
    }

    public static float QuartEI(float t)
    {
        t *= t;
        return t * t;
    }

    public static float QuartEO(float t)
    {
        t = (--t) * t;
        return 1f - t * t;
    }

    public static float QuartEIO(float t)
    {
        if (t < 0.5f)
        {
            t *= t;
            return 8f * t * t;
        }
        else
        {
            t = (--t) * t;
            return 1f - 8f * t * t;
        }
    }

    public static float QuintEI(float t)
    {
        float t2 = t * t;
        return t * t2 * t2;
    }

    public static float QuintEO(float t)
    {
        float t2 = (--t) * t;
        return 1f + t * t2 * t2;
    }

    public static float QuintEIO(float t)
    {
        float t2;
        if (t < 0.5f)
        {
            t2 = t * t;
            return 16f * t * t2 * t2;
        }
        else
        {
            t2 = (--t) * t;
            return 1f + 16f * t * t2 * t2;
        }
    }

    public static float PowEI(float t)
    {
        return ((float)Math.Pow(2, 8 * t) - 1f) * 0.003921568f; // 0.003921568 == 1.0 / 255
    }

    public static float PowEO(float t)
    {
        return 1f - (float)Math.Pow(2, -8 * t);
    }

    public static float PowEIO(float t)
    {
        if (t < 0.5f)
        {
            return ((float)Math.Pow(2, 16 * t) - 1f) * 0.0019607f; // 0.0019607 == 1.0 / 510
        }
        else
        {
            return 1f - 0.5f * (float)Math.Pow(2, -16 * (t - 0.5f));
        }
    }

    public static float CircEI(float t)
    {
        return 1f - (float)Math.Sqrt(1 - t);
    }

    public static float CircEO(float t)
    {
        return (float)Math.Sqrt(t);
    }

    public static float CircEIO(float t)
    {
        if (t < 0.5f)
        {
            return (1f - (float)Math.Sqrt(1 - 2 * t)) * 0.5f;
        }
        else
        {
            return (1f + (float)Math.Sqrt(2 * t - 1)) * 0.5f;
        }
    }

    public static float BackEI(float t)
    {
        return t * t * (2.70158f * t - 1.70158f);
    }

    public static float BackEO(float t)
    {
        return 1f + (--t) * t * (2.70158f * t + 1.70158f);
    }

    public static float BackEIO(float t)
    {
        if (t < 0.5f)
        {
            return t * t * (7f * t - 2.5f) * 2f;
        }
        else
        {
            return 1f + (--t) * t * 2f * (7f * t + 2.5f);
        }
    }

    public static float ElasticEI(float t)
    {
        float t2 = t * t;
        return t2 * t2 * (float)Math.Sin(t * Math.PI * 4.5);
    }

    public static float ElasticEO(float t)
    {
        float t2 = (t - 1f) * (t - 1f);
        return 1f - t2 * t2 * (float)Math.Cos(t * Math.PI * 4.5);
    }

    public static float ElasticEIO(float t)
    {
        if (t < 0.45f)
        {
            float t2 = t * t;
            return 8f * t2 * t2 * (float)Math.Sin(t * Math.PI * 9.0);
        }
        else if (t < 0.55f)
        {
            return 0.5f + (0.75f * (float)Math.Sin(t * Math.PI * 4.0));
        }
        else
        {
            float t2 = (t - 1f) * (t - 1f);
            return 1f - 8f * t2 * t2 * (float)Math.Sin(t * Math.PI * 9.0);
        }
    }

    public static float BounceEI(float t)
    {
        return (float)(Math.Pow(2, 6 * (t - 1f)) * Math.Abs(Math.Sin(t * Math.PI * 3.5)));
    }

    public static float BounceEO(float t)
    {
        return 1f - (float)(Math.Pow(2.0, -6.0 * t) * Math.Abs(Math.Cos(t * Math.PI * 3.5)));
    }

    public static float BounceEIO(float t)
    {
        if (t < 0.5f)
        {
            return 8f * (float)(Math.Pow(2.0, 8.0 * (t - 1f)) * Math.Abs(Math.Sin(t * Math.PI * 7.0)));
        }
        else
        {
            return 1f - 8f * (float)(Math.Pow(2.0, -8.0 * t) * Math.Abs(Math.Sin(t * Math.PI * 7.0)));
        }
    }

    public static float Bezier(float t)
    {
        return t * t * (3.0f - 2.0f * t);
    }
}
