namespace EasyContainer.Lib
{
    using System;
    using System.Reflection;
    using System.Text;

    public interface ISetting
    {
    }

    public abstract class Setting : ISetting
    {
        private readonly PropertyInfo[] _propertyInfos;

        protected Setting()
        {
            _propertyInfos = GetType().GetProperties();
        }

        public override string ToString()
        {
            return ToStringWithIndents(0);
        }

        private string ToStringWithIndents(int indents)
        {
            if (indents < 0)
            {
                throw new ArgumentException("Number of indents cannot be smaller than 0");
            }

            var sb = new StringBuilder();

            sb.Append(indents == 0 ? $"{GetType().Name}: \n" : "\n");

            foreach (var info in _propertyInfos)
            {
                for (int i = 0; i < indents + 2; i++)
                {
                    sb.Append(" ");
                }

                object value = info.GetValue(this) ?? "(null)";

                if (value is Setting s)
                {
                    sb.Append($"{info.Name}: {s.ToStringWithIndents(indents + 2)}\n");
                }
                else
                {
                    sb.Append($"{info.Name}: {value.ToString()}\n");
                }
            }

            return sb.ToString();
        }
    }
}