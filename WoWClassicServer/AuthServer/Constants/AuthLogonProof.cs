﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WoWClassicServer.AuthServer.Constants
{
    public struct AuthLogonProof
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public byte[] A;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public byte[] M1;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public byte[] CRC;
        public byte nKeys;
        public byte unk;

        public override string ToString()
        {
            return "A=" + string.Join(" ", A.Select(b => b.ToString("X2"))) + ", M1=" + string.Join(" ", M1.Select(b => b.ToString("X2")))
                + ", CRC=" + string.Join(" ", CRC.Select(b => b.ToString("X2"))) + ", nKeys=" + nKeys;
        }

        public static AuthLogonProof Read(BinaryReader br)
        {
            var alp = new AuthLogonProof();

            alp.A = br.ReadBytes(32);
            alp.M1 = br.ReadBytes(20);
            alp.CRC = br.ReadBytes(20);
            alp.nKeys = br.ReadByte();
            alp.unk = br.ReadByte();

            return alp;
        }
    }
}
