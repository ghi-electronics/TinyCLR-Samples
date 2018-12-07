#include "InterTest.h"

static const TinyCLR_Interop_MethodHandler methods[] = {
    nullptr,
    nullptr,
    nullptr,
    Interop_InterTest_Cipher_Xtea::EncipherFast___STATIC___VOID__U4__SZARRAY_U4__SZARRAY_U4,
    nullptr,
    Interop_InterTest_Cipher_Xtea::DecipherFast___STATIC___VOID__U4__SZARRAY_U4__SZARRAY_U4,
    nullptr,
    nullptr,
    nullptr,
};

const TinyCLR_Interop_Assembly Interop_InterTest = {
    "InterTest",
    0x98B04F6D,
    methods
};
