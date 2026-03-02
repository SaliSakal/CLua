namespace CLua
{

    public static class Extensions
    {
        public static bool In<T>(this T item, params T[] values)
        {
            return values.Contains(item);
        }
    }

    public partial class LuaManager
    {
        /// GUI metody

 

    }
}
