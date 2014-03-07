namespace ApiClient
{
    internal class Token
    {
        public string Key { get; set; }
        public string Secret { get; set; }

        public Token(string key, string secret)
        {
            Key = key;
            Secret = secret;
        }

        public Token()
            : this("", "")
        {

        }
    }
}
