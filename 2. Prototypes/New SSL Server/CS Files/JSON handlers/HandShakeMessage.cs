using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace New_SSL_Server
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
