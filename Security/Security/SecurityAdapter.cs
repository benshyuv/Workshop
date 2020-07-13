using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Security
{
    class SecurityAdapter : ISecurityManager
    {
        public byte[] encrypt(string password)
        {
            SHA256 sha = SHA256.Create();
            return sha.ComputeHash(Encoding.UTF8.GetBytes(password));
        }
    }
}
