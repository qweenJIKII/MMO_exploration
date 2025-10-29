#if !ENABLE_IL2CPP
namespace Newtonsoft.Json.Utilities
{
    public static class AotHelper
    {
        public static void EnsureType<T>() { }
        public static void EnsureList<T>() { }
        public static void EnsureDictionary<TKey, TValue>() { }
    }
}

public static class AotHelper
{
    public static void EnsureType<T>() { }
    public static void EnsureList<T>() { }
    public static void EnsureDictionary<TKey, TValue>() { }
}
#endif
