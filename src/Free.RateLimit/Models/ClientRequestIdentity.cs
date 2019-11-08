namespace Free.RateLimit
{
    /// <summary>
    ///     客户端IP、ID、路径和HTTP动词
    /// </summary>
    public class ClientRequestIdentity
    {
        public ClientRequestIdentity(string clientIp,string clientId,string path,string httpVerb) {
            this.ClientId = clientId;
            this.ClientIp = clientIp;
            this.Path = path;
            this.HttpVerb = httpVerb;
        }

        /// <summary>
        ///     客户端IP
        /// </summary>
        public string ClientIp { get; set; }
        /// <summary>
        ///     客户端ID
        /// </summary>
        public string ClientId { get; set; }
        /// <summary>
        ///     路径
        /// </summary>
        public string Path { get; set; }
        /// <summary>
        ///     Http动词
        /// </summary>
        public string HttpVerb { get; set; }
    }
}
