using System;

namespace Security
{
    public interface ISecurityManager
    {
        byte[] encrypt(string password);
    }
}
