namespace iLabPlus.Helpers
{
    public static class NumericUtility
    {
        public static bool IsNumeric(string value)
        {
            return value.All(char.IsDigit);
        }
    }
}
