public static class InputBlocker
{
    private static bool locked = false;

    public static bool Locked => locked;

    public static void Lock(bool v)
    {
        locked = v;
    }
}
