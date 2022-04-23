namespace New_SSL_Server
{
    // This class handles data requires for serialisation and deserialsation for handshake message
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
