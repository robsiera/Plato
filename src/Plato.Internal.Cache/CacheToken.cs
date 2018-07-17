﻿using System;
using System.Text;

namespace Plato.Internal.Cache
{
    
    public class CacheToken : IEquatable<CacheToken>
    {

        private readonly Type _type;
        private readonly int _typeHashCode;
        private readonly string _varyByHashCode;

        public CacheToken(Type type, params object[] varyBy)
        {

            // This is not perfect but avoids the overhead of a real cryptographic hash
            // Get hashcodes for primative types as opposed to varyBy object array complex type
            var sb = new StringBuilder();
            if (varyBy != null)
            {
                foreach (var vary in varyBy)
                {
                    if (vary != null)
                    {
                        sb.Append(vary.GetHashCode());
                    }
                }
            }
          
            _type = type;
            _varyByHashCode = sb.ToString();
            _typeHashCode = _type.GetHashCode();
        }

        public static bool operator ==(CacheToken a, CacheToken b)
        {
            var areEqual = ReferenceEquals(a, b);
            if ((object) a != null && (object) b != null)
            {
                areEqual = a.Equals(b);
            }

            return areEqual;
        }

        public static bool operator !=(CacheToken a, CacheToken b) => !(a == b);

        public bool Equals(CacheToken other)
        {
            var areEqual = false;

            if (other != null)
            {
                areEqual = this.ToString() == other.ToString();
            }

            return areEqual;
        }

        public override bool Equals(object obj) => this.Equals(obj as CacheToken);

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (_type != null ? _type.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ _typeHashCode;
                hashCode = (hashCode * 397) ^ (_varyByHashCode != null ? _varyByHashCode.GetHashCode() : 0);
                return hashCode;
            }
        }
        
        public override string ToString()
        {
            return $"{_type}_{GetHashCode()}";
        }
    }

}