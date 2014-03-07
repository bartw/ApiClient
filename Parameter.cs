using System;
using System.Windows.Input;

namespace ApiClient
{
    public class Parameter
    {
        public string Key { get; private set; }
        public string Value { get; private set; }

        public Parameter(string key, object value)
        {
            if (key == null) throw new ArgumentNullException("key");
            if (value == null) throw new ArgumentNullException("value");

            Key = key;
            Value = value.ToString();
        }

        public override string ToString()
        {
            return Key.UrlEncode() + "=" + Value.UrlEncode();
        }
    }
}
