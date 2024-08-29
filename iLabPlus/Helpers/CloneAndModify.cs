namespace iLabPlus.Helpers
{
    public static class Extensions
    {
        public static T CloneAndModify<T>(this T source, Action<T> modifier) where T : class,  new()
        {
            var clone = new T();
            var properties = typeof(T).GetProperties().Where(p => p.CanRead && p.CanWrite);

            foreach (var prop in properties)
            {
                prop.SetValue(clone, prop.GetValue(source));
            }
            modifier(clone);
            return clone;
        }
    }
}
