using System.ComponentModel;
using System.Reflection;

namespace Common.Extensions
{
	public static class EnumExtension
	{
        public static string GetDescription(this Enum source)
        {
            FieldInfo fieldInfo = source.GetType().GetField(source.ToString());
            DescriptionAttribute[] attributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (attributes.Length <= 0)
                return source.ToString();

            return attributes[0].Description;
        }
    }
}

