using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProtocolBase
    {
        public virtual ProtocolBase Decode(byte[] readbuff, int start, int length)
        {
            return new ProtocolBase();
        }
        public virtual byte[] Encode()
        {
            return new byte[] { };
        }
        public virtual string GetName()
        {
            return "";
        }
        public virtual string GetDesc()
        {
            return "";

        }
    }
