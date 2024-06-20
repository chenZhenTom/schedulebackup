namespace Common.Extensions
{
	public static class LongExtension
	{
        public static string GetFileSizeFormat(this long source)
        {
            string[] sizes = { "B", "KB", "MB", "GB"};
            int index = 0;
            double len = source;
            
            while (len >= 1024 && index < sizes.Length - 1)
            {
                index++;
                len = len / 1024;
            }

            return $"{len:0.#} {sizes[index]}";
        }
    }
}