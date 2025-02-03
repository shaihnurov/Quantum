using System;

namespace Quantum.Models
{
    public class UserDataJson
    {
        public string? Token { get; set; }
        public bool IsRememberMe { get; set; }
        public byte[]? Image { get; set; }
    }
}