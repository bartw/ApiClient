using System;

namespace ApiClient
{
    public class Header
    {
        public string Key { get; private set; }
        public string Value { get; private set; }

        public Header(string key, object value)
        {
            if (key == null) throw new ArgumentNullException("key");
            if (value == null) throw new ArgumentNullException("value");

            Key = key;
            Value = value.ToString();
        }
    }
}
