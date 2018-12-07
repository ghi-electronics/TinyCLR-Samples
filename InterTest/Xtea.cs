using System;
using System.Runtime.CompilerServices;

namespace Cipher {
    static class Xtea {
        [MethodImpl(MethodImplOptions.InternalCall)]
        static public extern void EncipherFast(UInt32 Rounds, UInt32[] Data, UInt32[] Key);

        static public void EncipherSlow(UInt32 Rounds, UInt32[] Data, UInt32[] Key) {
            UInt32 i;
            UInt32 v0 = Data[0], v1 = Data[1], sum = 0, delta = 0x9E3779B9;
            for (i = 0; i < Rounds; i++) {
                v0 += (((v1 << 4) ^ (v1 >> 5)) + v1) ^ (sum + Key[sum & 3]);
                sum += delta;
                v1 += (((v0 << 4) ^ (v0 >> 5)) + v0) ^ (sum + Key[(sum >> 11) & 3]);
            }
            Data[0] = v0; Data[1] = v1;
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        static public extern void DecipherFast(UInt32 Rounds, UInt32[] Data, UInt32[] Key);

        static public void DecipherSlow(UInt32 Rounds, UInt32[] Data, UInt32[] Key) {
            UInt32 i;
            UInt32 v0 = Data[0], v1 = Data[1], delta = 0x9E3779B9, sum = delta * Rounds;
            for (i = 0; i < Rounds; i++) {
                v1 -= (((v0 << 4) ^ (v0 >> 5)) + v0) ^ (sum + Key[(sum >> 11) & 3]);
                sum -= delta;
                v0 -= (((v1 << 4) ^ (v1 >> 5)) + v1) ^ (sum + Key[sum & 3]);
            }
            Data[0] = v0; Data[1] = v1;
        }
    }
}


/*
// The original public domain code from https://en.wikipedia.org/wiki/XTEA
void encipher(unsigned int num_rounds, uint32_t v[2], uint32_t const key[4]) {
    unsigned int i;
    uint32_t v0=v[0], v1=v[1], sum=0, delta=0x9E3779B9;
    for (i=0; i < num_rounds; i++) {
        v0 += (((v1 << 4) ^ (v1 >> 5)) + v1) ^ (sum + key[sum & 3]);
        sum += delta;
        v1 += (((v0 << 4) ^ (v0 >> 5)) + v0) ^ (sum + key[(sum>>11) & 3]);
    }
    v[0]=v0; v[1]=v1;
}

void decipher(unsigned int num_rounds, uint32_t v[2], uint32_t const key[4]) {
    unsigned int i;
    uint32_t v0=v[0], v1=v[1], delta=0x9E3779B9, sum=delta*num_rounds;
    for (i=0; i < num_rounds; i++) {
        v1 -= (((v0 << 4) ^ (v0 >> 5)) + v0) ^ (sum + key[(sum>>11) & 3]);
        sum -= delta;
        v0 -= (((v1 << 4) ^ (v1 >> 5)) + v1) ^ (sum + key[sum & 3]);
    }
    v[0]=v0; v[1]=v1;
}

    */
