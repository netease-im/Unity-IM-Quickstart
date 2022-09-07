using System;

public static class Helper
{
    public static long toTicks(DateTime dt)
    {
        TimeSpan ts = dt.ToUniversalTime() - new DateTime(1970, 1, 1);
        long ticks = System.Convert.ToInt64(ts.TotalMilliseconds);
        return ticks;
    }
}
