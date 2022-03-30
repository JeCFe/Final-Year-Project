namespace Forms_SSL_Client
{
    class HandShakeMessage //ID CODE 0
    {
        public string stage;
        public string test;
        public string RSAPublicKey;
        public byte[] EncryptedAESKey;
        public byte[] EncryptedAESIV;
        public bool Confirmation;
    }
}
