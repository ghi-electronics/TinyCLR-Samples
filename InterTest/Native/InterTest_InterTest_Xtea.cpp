#include "InterTest.h"
void encipher(unsigned int num_rounds, uint32_t v[2], uint32_t const key[4]) {
	unsigned int i;
	uint32_t v0 = v[0], v1 = v[1], sum = 0, delta = 0x9E3779B9;
	for (i = 0; i < num_rounds; i++) {
		v0 += (((v1 << 4) ^ (v1 >> 5)) + v1) ^ (sum + key[sum & 3]);
		sum += delta;
		v1 += (((v0 << 4) ^ (v0 >> 5)) + v0) ^ (sum + key[(sum >> 11) & 3]);
	}
	v[0] = v0; v[1] = v1;
}
TinyCLR_Result Interop_InterTest_Cipher_Xtea::EncipherFast___STATIC___VOID__U4__SZARRAY_U4__SZARRAY_U4(const TinyCLR_Interop_MethodData md) {

	auto ip = md.InteropManager;
	
	// Arg 0 is the cipher round count
	TinyCLR_Interop_ClrValue arg0;
	ip->GetArgument(ip, md.Stack, 0, arg0);
	int Rounds = arg0.Data.Numeric->U4;
	
	// Arg 1 is the Data
	TinyCLR_Interop_ClrValue arg1;
	ip->GetArgument(ip, md.Stack, 1, arg1);
	uint32_t* Data = reinterpret_cast<uint32_t*>(arg1.Data.SzArray.Data);
	size_t DataLen = arg1.Data.SzArray.Length;

	// Arg 2 is the Key
	TinyCLR_Interop_ClrValue arg2;
	ip->GetArgument(ip, md.Stack, 2, arg2);
	uint32_t* Key = reinterpret_cast<uint32_t*>(arg2.Data.SzArray.Data);
	size_t KeyLen = arg2.Data.SzArray.Length;
	
	
	encipher(Rounds, Data, Key);

	return TinyCLR_Result::Success;
}
void decipher(unsigned int num_rounds, uint32_t v[2], uint32_t const key[4]) {
	unsigned int i;
	uint32_t v0 = v[0], v1 = v[1], delta = 0x9E3779B9, sum = delta * num_rounds;
	for (i = 0; i < num_rounds; i++) {
		v1 -= (((v0 << 4) ^ (v0 >> 5)) + v0) ^ (sum + key[(sum >> 11) & 3]);
		sum -= delta;
		v0 -= (((v1 << 4) ^ (v1 >> 5)) + v1) ^ (sum + key[sum & 3]);
	}
	v[0] = v0; v[1] = v1;
}

TinyCLR_Result Interop_InterTest_Cipher_Xtea::DecipherFast___STATIC___VOID__U4__SZARRAY_U4__SZARRAY_U4(const TinyCLR_Interop_MethodData md) {

	auto ip = md.InteropManager;

	// Arg 0 is the cipher round count
	TinyCLR_Interop_ClrValue arg0;
	ip->GetArgument(ip, md.Stack, 0, arg0);
	int Rounds = arg0.Data.Numeric->U4;

	// Arg 1 is the Data
	TinyCLR_Interop_ClrValue arg1;
	ip->GetArgument(ip, md.Stack, 1, arg1);
	uint32_t* Data = reinterpret_cast<uint32_t*>(arg1.Data.SzArray.Data);
	size_t DataLen = arg1.Data.SzArray.Length;

	// Arg 2 is the Key
	TinyCLR_Interop_ClrValue arg2;
	ip->GetArgument(ip, md.Stack, 2, arg2);
	uint32_t* Key = reinterpret_cast<uint32_t*>(arg2.Data.SzArray.Data);
	size_t KeyLen = arg2.Data.SzArray.Length;


	decipher(Rounds, Data, Key);

	return TinyCLR_Result::Success;
}
