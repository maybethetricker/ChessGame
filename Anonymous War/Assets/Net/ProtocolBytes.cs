﻿using System.Collections;
using System;
using System.Linq;
public class ProtocolBytes : ProtocolBase
{
    public byte[] bytes;
    public override ProtocolBase Decode(byte[] readbuff, int start, int length)
    {
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.bytes = new byte[length];
        Array.Copy(readbuff, start, protocol.bytes, 0, length);
        return protocol;
    }
    public override byte[] Encode()
    {
        return bytes;
    }
    public override string GetName()
    {
        return GetString(0);
    }
    public override string GetDesc()
    {
        string str = "";
        if (bytes == null)
            return str;
        for (int i = 0; i < bytes.Length; i++)
        {
            int b = (int)bytes[i];
            str += b.ToString() + " ";
        }
        return str;
    }
    public void AddString(string str)
    {
        Int32 len = str.Length;
        byte[] lenBytes = BitConverter.GetBytes(len);
        byte[] strBytes = System.Text.Encoding.UTF8.GetBytes(str);
        if (bytes == null)
            bytes = lenBytes.Concat(strBytes).ToArray();
        else
            bytes = bytes.Concat(lenBytes).Concat(strBytes).ToArray();
    }
    public string GetString(int start, ref int end)
    {
        if (bytes == null)
            return "";
        if (bytes.Length < start + sizeof(Int32))
            return "";
        Int32 strLen = BitConverter.ToInt32(bytes, start);
        if (bytes.Length < start + sizeof(Int32) + strLen)
            return "";
        string str = System.Text.Encoding.UTF8.GetString(bytes, start + sizeof(Int32), strLen);
        end = start + sizeof(Int32) + strLen;
        return str;
    }
    public string GetString(int start)
    {
        int end = 0;
        return GetString(start, ref end);
    }
    public void AddInt(int num)
    {
        byte[] numBytes = BitConverter.GetBytes(num);
        if (bytes == null)
            bytes = numBytes;
        else
            bytes = bytes.Concat(numBytes).ToArray();
    }
    public int GetInt(int start, ref int end)
    {
        if (bytes == null)
            return 0;
        if (bytes.Length < start + sizeof(Int32))
            return 0;
        end = start + sizeof(Int32);
        return BitConverter.ToInt32(bytes, start);
    }
    public int GetInt(int start)
    {
        int end = 0;
        return GetInt(start, ref end);
    }
    public void AddFloat(float num)
    {
        byte[] numBytes = BitConverter.GetBytes(num);
        if (bytes == null)
            bytes = numBytes;
        else
            bytes = bytes.Concat(numBytes).ToArray();
    }
    public float GetFloat(int start, ref int end)
    {
        if (bytes == null)
            return 0;
        if (bytes.Length < start + sizeof(Int32))
            return 0;
        end = start + sizeof(Int32);
        return BitConverter.ToSingle(bytes, start);
    }
    public float GetFloat(int start)
    {
        int end = 0;
        return GetFloat(start, ref end);
    }

}
