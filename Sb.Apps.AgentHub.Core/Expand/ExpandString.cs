namespace Sb.Apps.AgentHub.Core.Expand
{
    /// <summary>
    /// 字符串扩展方法
    /// </summary>
    public static class ExpandString
    {
        /// <summary>
        /// 去除开头的指定字符串，方法不会改变字符串本身
        /// </summary>
        /// <param name="str">要操作的字符串</param>
        /// <param name="prefix">要去除的前缀</param>
        /// <returns>去除前缀后的字符串</returns>
        /// <exception cref="ArgumentNullException">str 或 prefix 为 null 时抛出</exception>
        public static string TrimStartsPrefix(this string str, string prefix)
        {
            ArgumentNullException.ThrowIfNull(str);
            ArgumentNullException.ThrowIfNull(prefix);

            if (str.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return str.Substring(prefix.Length);
            return str;
        }
    }
}
