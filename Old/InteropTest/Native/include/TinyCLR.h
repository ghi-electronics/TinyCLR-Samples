// Copyright GHI Electronics, LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#pragma once

#include <cstdint>
#include <cstddef>

////////////////////////////////////////////////////////////////////////////////
//Results
////////////////////////////////////////////////////////////////////////////////
enum class TinyCLR_Result : uint32_t {
    Success = 0,
    ArgumentInvalid = 1,
    ArgumentNull = 2,
    ArgumentOutOfRange = 3,
    NotSupported = 4,
    NotFound = 5,
    NotAvailable = 6,
    NotImplemented = 7,
    InvalidOperation = 8,
    IndexOutOfRange = 9,
    NullReference = 10,
    AlreadyExists = 11,
    SharingViolation = 12,
    SynchronizationFailed = 13,
    Disposed = 14,
    WrongType = 15,
    DivideByZero = 16,
    SerializationFailed = 17,
    TimedOut = 18,
    WatchdogTimedOut = 19,
    Busy = 20,
    OutOfMemory = 21,
    WrongChecksum = 22,
    ShuttingDown = 23,
    DeviceFailure = 24,
    UnknownFailure = 25,
    NoDataAvailable = 26,
};

////////////////////////////////////////////////////////////////////////////////
//API
////////////////////////////////////////////////////////////////////////////////
enum class TinyCLR_Api_Type : uint32_t {
    ApiManager = 0,
    DebuggerManager = 1,
    InteropManager = 2,
    MemoryManager = 3,
    TaskManager = 4,
    SystemTimeManager = 5,
    InterruptController = 0 | 0x20000000,
    PowerController = 1 | 0x20000000,
    NativeTimeController = 2 | 0x20000000,
    AdcController = 0 | 0x40000000,
    CanController = 1 | 0x40000000,
    DacController = 2 | 0x40000000,
    DcmiController = 3 | 0x40000000,
    DisplayController = 4 | 0x40000000,
    EthernetMacController = 5 | 0x40000000,
    GpioController = 6 | 0x40000000,
    I2cController = 7 | 0x40000000,
    I2sController = 8 | 0x40000000,
    OneWireController = 9 | 0x40000000,
    PwmController = 10 | 0x40000000,
    RtcController = 11 | 0x40000000,
    SaiController = 12 | 0x40000000,
    SpiController = 13 | 0x40000000,
    StorageController = 14 | 0x40000000,
    TouchController = 15 | 0x40000000,
    UartController = 16 | 0x40000000,
    UsbClientController = 17 | 0x40000000,
    UsbHostController = 18 | 0x40000000,
    WatchdogController = 19 | 0x40000000,
    Custom = 0 | 0x80000000,
};

struct TinyCLR_Api_Info {
    const char* Author;
    const char* Name;
    TinyCLR_Api_Type Type;
    void* State;
    uint64_t Version;
    const void* Implementation;
};

struct TinyCLR_Api_Manager {
    const TinyCLR_Api_Info* ApiInfo;

    TinyCLR_Result(*Add)(const TinyCLR_Api_Manager* self, const TinyCLR_Api_Info* api);
    TinyCLR_Result(*Remove)(const TinyCLR_Api_Manager* self, const TinyCLR_Api_Info* api);
    const TinyCLR_Api_Info*(*Find)(const TinyCLR_Api_Manager* self, const char* name, TinyCLR_Api_Type type);
    const void*(*FindDefault)(const TinyCLR_Api_Manager* self, TinyCLR_Api_Type type);
    TinyCLR_Result(*SetDefaultName)(const TinyCLR_Api_Manager* self, TinyCLR_Api_Type type, const char* name);
    const char*(*GetDefaultName)(const TinyCLR_Api_Manager* self, TinyCLR_Api_Type type);
};

////////////////////////////////////////////////////////////////////////////////
//Debugger
////////////////////////////////////////////////////////////////////////////////
struct TinyCLR_Debugger_Manager {
    const TinyCLR_Api_Info* ApiInfo;

    TinyCLR_Result(*Log)(const TinyCLR_Debugger_Manager* self, const char* str, size_t length);
};

////////////////////////////////////////////////////////////////////////////////
//Interop
////////////////////////////////////////////////////////////////////////////////
struct TinyCLR_Interop_StackFrame;
struct TinyCLR_Interop_Manager;

struct TinyCLR_Interop_MethodData {
    const TinyCLR_Interop_Manager* InteropManager;
    const TinyCLR_Api_Manager* ApiManager;
    TinyCLR_Interop_StackFrame& Stack;
};

typedef TinyCLR_Result(*TinyCLR_Interop_MethodHandler)(const TinyCLR_Interop_MethodData md);

struct TinyCLR_Interop_Assembly {
    const char* Name;
    uint32_t Checksum;
    const TinyCLR_Interop_MethodHandler* Methods;
};

typedef uint32_t TinyCLR_Interop_ClrTypeId;

struct TinyCLR_Interop_ClrObjectReference {
    uint64_t a;
    uint64_t b;
};

struct TinyCLR_Interop_ClrObject {
    uint64_t a;
    uint64_t b;
};

struct TinyCLR_Interop_ClrValue {
    union NumericType {
        bool Boolean;
        int8_t I1;
        uint8_t U1;
        char16_t Char;
        int16_t I2;
        uint16_t U2;
        int32_t I4;
        uint32_t U4;
        float R4;
        intptr_t I;
        uintptr_t U;
        int64_t I8;
        uint64_t U8;
        double R8;
    };

    struct StringType {
        const char* Data;
        size_t Length;
    };

    struct SzArrayType {
        void* Data;
        size_t Length;
    };

    union DataType {
        NumericType* Numeric;
        StringType String;
        SzArrayType SzArray;
    };

    const TinyCLR_Interop_ClrObjectReference* Ref;
    const TinyCLR_Interop_ClrObject* Object;
    DataType Data;
};

struct TinyCLR_Interop_Manager {
    const TinyCLR_Api_Info* ApiInfo;

    TinyCLR_Result(*Add)(const TinyCLR_Interop_Manager* self, const TinyCLR_Interop_Assembly* interop);
    TinyCLR_Result(*Remove)(const TinyCLR_Interop_Manager* self, const TinyCLR_Interop_Assembly* interop);
    TinyCLR_Result(*RaiseEvent)(const TinyCLR_Interop_Manager* self, const char* eventDispatcherName, const char* data0, uint64_t data1, uint64_t data2, uint64_t data3, uintptr_t data4, uint64_t timestamp);
    TinyCLR_Result(*FindType)(const TinyCLR_Interop_Manager* self, const char* assemblyName, const char* namespaceName, const char* typeName, TinyCLR_Interop_ClrTypeId& type);
    TinyCLR_Result(*GetClrTypeId)(const TinyCLR_Interop_Manager* self, const TinyCLR_Interop_ClrObject* object, TinyCLR_Interop_ClrTypeId& type);
    TinyCLR_Result(*CreateArray)(const TinyCLR_Interop_Manager* self, size_t length, TinyCLR_Interop_ClrTypeId elementType, TinyCLR_Interop_ClrValue& value);
    TinyCLR_Result(*CreateString)(const TinyCLR_Interop_Manager* self, const char* data, size_t length, TinyCLR_Interop_ClrValue& value);
    TinyCLR_Result(*CreateObject)(const TinyCLR_Interop_Manager* self, TinyCLR_Interop_StackFrame& stack, TinyCLR_Interop_ClrTypeId type, TinyCLR_Interop_ClrValue& value);
    TinyCLR_Result(*CreateObjectReference)(const TinyCLR_Interop_Manager* self, const TinyCLR_Interop_ClrObject* object, TinyCLR_Interop_ClrValue& value);
    TinyCLR_Result(*AssignObjectReference)(const TinyCLR_Interop_Manager* self, TinyCLR_Interop_ClrValue& target, const TinyCLR_Interop_ClrObject* object);
    TinyCLR_Result(*ExtractObjectFromReference)(const TinyCLR_Interop_Manager* self, const TinyCLR_Interop_ClrObjectReference* reference, const TinyCLR_Interop_ClrObject*& object);
    TinyCLR_Result(*GetThisObject)(const TinyCLR_Interop_Manager* self, const TinyCLR_Interop_StackFrame& stack, const TinyCLR_Interop_ClrObject*& object);
    TinyCLR_Result(*GetField)(const TinyCLR_Interop_Manager* self, const TinyCLR_Interop_ClrObject* object, size_t index, TinyCLR_Interop_ClrValue& value);
    TinyCLR_Result(*GetStaticField)(const TinyCLR_Interop_Manager* self, const TinyCLR_Interop_Assembly& interop, size_t index, TinyCLR_Interop_ClrValue& value);
    TinyCLR_Result(*GetArgument)(const TinyCLR_Interop_Manager* self, const TinyCLR_Interop_StackFrame& stack, size_t index, TinyCLR_Interop_ClrValue& value);
    TinyCLR_Result(*GetReturn)(const TinyCLR_Interop_Manager* self, TinyCLR_Interop_StackFrame& stack, TinyCLR_Interop_ClrValue& value);
};

////////////////////////////////////////////////////////////////////////////////
//Memory
////////////////////////////////////////////////////////////////////////////////
struct TinyCLR_Memory_Manager {
    const TinyCLR_Api_Info* ApiInfo;

    void*(*Allocate)(const TinyCLR_Memory_Manager* self, size_t length);
    void(*Free)(const TinyCLR_Memory_Manager* self, void* ptr);
    TinyCLR_Result(*GetStats)(const TinyCLR_Memory_Manager* self, size_t& used, size_t& free);
};

////////////////////////////////////////////////////////////////////////////////
//Task
////////////////////////////////////////////////////////////////////////////////
struct TinyCLR_Task_Manager;

typedef const void* TinyCLR_Task_Reference;
typedef void(*TinyCLR_Task_Callback)(const TinyCLR_Task_Manager* self, const TinyCLR_Api_Manager* apiManager, TinyCLR_Task_Reference task, void* arg);

struct TinyCLR_Task_Manager {
    const TinyCLR_Api_Info* ApiInfo;

    TinyCLR_Result(*Create)(const TinyCLR_Task_Manager* self, TinyCLR_Task_Callback callback, void* arg, bool fireInIsr, TinyCLR_Task_Reference& task);
    TinyCLR_Result(*Free)(const TinyCLR_Task_Manager* self, TinyCLR_Task_Reference& task);
    TinyCLR_Result(*Enqueue)(const TinyCLR_Task_Manager* self, TinyCLR_Task_Reference task, uint64_t nativeTimeFromNow);
};

////////////////////////////////////////////////////////////////////////////////
//System Time
////////////////////////////////////////////////////////////////////////////////
struct TinyCLR_SystemTime_Manager {
    const TinyCLR_Api_Info* ApiInfo;

    TinyCLR_Result(*GetTime)(const TinyCLR_SystemTime_Manager* self, uint64_t& utcTime, int32_t& timeZoneOffset);
    TinyCLR_Result(*SetTime)(const TinyCLR_SystemTime_Manager* self, uint64_t utcTime, int32_t timeZoneOffset);
};

////////////////////////////////////////////////////////////////////////////////
//Interrupt
////////////////////////////////////////////////////////////////////////////////
typedef void(*TinyCLR_Interrupt_StartStopHandler)();

struct TinyCLR_Interrupt_Controller {
    const TinyCLR_Api_Info* ApiInfo;

    TinyCLR_Result(*Initialize)(const TinyCLR_Interrupt_Controller* self, TinyCLR_Interrupt_StartStopHandler onInterruptStart, TinyCLR_Interrupt_StartStopHandler onInterruptEnd);
    TinyCLR_Result(*Uninitialize)(const TinyCLR_Interrupt_Controller* self);
    void(*Enable)();
    void(*Disable)();
    void(*WaitForInterrupt)();
    bool(*IsDisabled)();
};

////////////////////////////////////////////////////////////////////////////////
//Power
////////////////////////////////////////////////////////////////////////////////
enum class TinyCLR_Power_Level : uint32_t {
    Active = 0,
    Idle = 1,
    Off = 2,
    Sleep1 = 3,
    Sleep2 = 4,
    Sleep3 = 5,
    Custom = 0 | 0x80000000
};

enum class TinyCLR_Power_WakeSource : uint64_t {
    Interrupt = 1,
    Gpio = 2,
    Rtc = 4,
    SystemTimer = 8,
    Timer = 16,
    Ethernet = 32,
    WiFi = 64,
    Can = 128,
    Uart = 256,
    UsbClient = 512,
    UsbHost = 1024,
    Charger = 2048,
    Custom = 0 | 0x80000000,
};

struct TinyCLR_Power_Controller {
    const TinyCLR_Api_Info* ApiInfo;

    TinyCLR_Result(*Initialize)(const TinyCLR_Power_Controller* self);
    TinyCLR_Result(*Uninitialize)(const TinyCLR_Power_Controller* self);
    TinyCLR_Result(*Reset)(const TinyCLR_Power_Controller* self, bool runCoreAfter);
    TinyCLR_Result(*SetLevel)(const TinyCLR_Power_Controller* self, TinyCLR_Power_Level level, TinyCLR_Power_WakeSource wakeSource, uint64_t data);
};

////////////////////////////////////////////////////////////////////////////////
//Native Time
////////////////////////////////////////////////////////////////////////////////
typedef void(*TinyCLR_NativeTime_Callback)();

struct TinyCLR_NativeTime_Controller {
    const TinyCLR_Api_Info* ApiInfo;

    TinyCLR_Result(*Initialize)(const TinyCLR_NativeTime_Controller* self);
    TinyCLR_Result(*Uninitialize)(const TinyCLR_NativeTime_Controller* self);
    uint64_t(*GetNativeTime)(const TinyCLR_NativeTime_Controller* self);
    uint64_t(*ConvertNativeTimeToSystemTime)(const TinyCLR_NativeTime_Controller* self, uint64_t nativeTime);
    uint64_t(*ConvertSystemTimeToNativeTime)(const TinyCLR_NativeTime_Controller* self, uint64_t systemTime);
    TinyCLR_Result(*SetCallback)(const TinyCLR_NativeTime_Controller* self, TinyCLR_NativeTime_Callback callback);
    TinyCLR_Result(*ScheduleCallback)(const TinyCLR_NativeTime_Controller* self, uint64_t nativeTime);
    void(*Wait)(const TinyCLR_NativeTime_Controller* self, uint64_t nativeTime);
};

////////////////////////////////////////////////////////////////////////////////
//Startup
////////////////////////////////////////////////////////////////////////////////
struct TinyCLR_Storage_Controller;

typedef void(*TinyCLR_Startup_SoftResetHandler)(const TinyCLR_Api_Manager* ApiManager);

struct TinyCLR_Startup_UsbDebuggerConfiguration {
    uint16_t VendorId;
    uint16_t ProductId;

    const wchar_t* Manufacturer;
    const wchar_t* Product;
    const wchar_t* SerialNumber;
};

struct TinyCLR_Startup_DeploymentConfiguration {
    bool RegionsContiguous;
    bool RegionsEqualSized;
    size_t RegionCount;
    const uint64_t* RegionAddresses;
    const size_t* RegionSizes;
};

TinyCLR_Result TinyCLR_Startup_AddHeapRegion(uint8_t* start, size_t length);
TinyCLR_Result TinyCLR_Startup_AddDeploymentRegion(const TinyCLR_Api_Info* api, const TinyCLR_Startup_DeploymentConfiguration* configuration);
TinyCLR_Result TinyCLR_Startup_SetMemoryProfile(size_t factor);
TinyCLR_Result TinyCLR_Startup_SetDeviceInformation(const char* deviceName, const char* manufacturerName, uint64_t version);
TinyCLR_Result TinyCLR_Startup_SetDebuggerTransportApi(const TinyCLR_Api_Info* api, const void* configuration);
TinyCLR_Result TinyCLR_Startup_SetRequiredApis(const TinyCLR_Api_Info* interruptApi, const TinyCLR_Api_Info* powerApi, const TinyCLR_Api_Info* nativeTimeApi);
TinyCLR_Result TinyCLR_Startup_Start(TinyCLR_Startup_SoftResetHandler handler, bool runManagedApplication);

////////////////////////////////////////////////////////////////////////////////
//ADC
////////////////////////////////////////////////////////////////////////////////
enum class TinyCLR_Adc_ChannelMode : uint32_t {
    SingleEnded = 0,
    Differential = 1
};

struct TinyCLR_Adc_Controller {
    const TinyCLR_Api_Info* ApiInfo;

    TinyCLR_Result(*Acquire)(const TinyCLR_Adc_Controller* self);
    TinyCLR_Result(*Release)(const TinyCLR_Adc_Controller* self);
    TinyCLR_Result(*OpenChannel)(const TinyCLR_Adc_Controller* self, uint32_t channel);
    TinyCLR_Result(*CloseChannel)(const TinyCLR_Adc_Controller* self, uint32_t channel);
    TinyCLR_Result(*ReadChannel)(const TinyCLR_Adc_Controller* self, uint32_t channel, int32_t& value);
    TinyCLR_Result(*SetChannelMode)(const TinyCLR_Adc_Controller* self, TinyCLR_Adc_ChannelMode mode);
    TinyCLR_Adc_ChannelMode(*GetChannelMode)(const TinyCLR_Adc_Controller* self);
    bool(*IsChannelModeSupported)(const TinyCLR_Adc_Controller* self, TinyCLR_Adc_ChannelMode mode);
    int32_t(*GetMinValue)(const TinyCLR_Adc_Controller* self);
    int32_t(*GetMaxValue)(const TinyCLR_Adc_Controller* self);
    uint32_t(*GetResolutionInBits)(const TinyCLR_Adc_Controller* self);
    uint32_t(*GetChannelCount)(const TinyCLR_Adc_Controller* self);
};

////////////////////////////////////////////////////////////////////////////////
//CAN
////////////////////////////////////////////////////////////////////////////////
struct TinyCLR_Can_Controller;

enum class TinyCLR_Can_Error : uint32_t {
    Overrun = 0,
    BufferFull = 1,
    BusOff = 2,
    Passive = 3
};

struct TinyCLR_Can_BitTiming {
    uint32_t Propagation;
    uint32_t Phase1;
    uint32_t Phase2;
    uint32_t BaudratePrescaler;
    uint32_t SynchronizationJumpWidth;
    bool UseMultiBitSampling;
};

struct TinyCLR_Can_Message {
    uint64_t Timestamp;
    uint32_t ArbitrationId;
    size_t Length;
    uint8_t Data[8];
    bool IsRemoteTransmissionRequest;
    bool IsExtendedId;
};

typedef void(*TinyCLR_Can_MessageReceivedHandler)(const TinyCLR_Can_Controller* self, size_t count, uint64_t timestamp);
typedef void(*TinyCLR_Can_ErrorReceivedHandler)(const TinyCLR_Can_Controller* self, TinyCLR_Can_Error error, uint64_t timestamp);

struct TinyCLR_Can_Controller {
    const TinyCLR_Api_Info* ApiInfo;

    TinyCLR_Result(*Acquire)(const TinyCLR_Can_Controller* self);
    TinyCLR_Result(*Release)(const TinyCLR_Can_Controller* self);
    TinyCLR_Result(*Enable)(const TinyCLR_Can_Controller* self);
    TinyCLR_Result(*Disable)(const TinyCLR_Can_Controller* self);
    TinyCLR_Result(*WriteMessage)(const TinyCLR_Can_Controller* self, const TinyCLR_Can_Message* messages, size_t& length);
    TinyCLR_Result(*ReadMessage)(const TinyCLR_Can_Controller* self, TinyCLR_Can_Message* messages, size_t& length);
    TinyCLR_Result(*SetBitTiming)(const TinyCLR_Can_Controller* self, const TinyCLR_Can_BitTiming* timing);
    TinyCLR_Result(*SetMessageReceivedHandler)(const TinyCLR_Can_Controller* self, TinyCLR_Can_MessageReceivedHandler handler);
    TinyCLR_Result(*SetErrorReceivedHandler)(const TinyCLR_Can_Controller* self, TinyCLR_Can_ErrorReceivedHandler handler);
    TinyCLR_Result(*SetExplicitFilters)(const TinyCLR_Can_Controller* self, const uint32_t* filters, size_t count);
    TinyCLR_Result(*SetGroupFilters)(const TinyCLR_Can_Controller* self, const uint32_t* lowerBounds, const uint32_t* upperBounds, size_t count);
    size_t(*GetMessagesToWrite)(const TinyCLR_Can_Controller* self);
    size_t(*GetMessagesToRead)(const TinyCLR_Can_Controller* self);
    TinyCLR_Result(*ClearWriteBuffer)(const TinyCLR_Can_Controller* self);
    TinyCLR_Result(*ClearReadBuffer)(const TinyCLR_Can_Controller* self);
    bool(*CanWriteMessage)(const TinyCLR_Can_Controller* self);
    bool(*CanReadMessage)(const TinyCLR_Can_Controller* self);
    size_t(*GetWriteErrorCount)(const TinyCLR_Can_Controller* self);
    size_t(*GetReadErrorCount)(const TinyCLR_Can_Controller* self);
    size_t(*GetWriteBufferSize)(const TinyCLR_Can_Controller* self);
    size_t(*GetReadBufferSize)(const TinyCLR_Can_Controller* self);
    TinyCLR_Result(*SetWriteBufferSize)(const TinyCLR_Can_Controller* self, size_t size);
    TinyCLR_Result(*SetReadBufferSize)(const TinyCLR_Can_Controller* self, size_t size);
    uint32_t(*GetSourceClock)(const TinyCLR_Can_Controller* self);
};

////////////////////////////////////////////////////////////////////////////////
//DAC
////////////////////////////////////////////////////////////////////////////////
struct TinyCLR_Dac_Controller {
    const TinyCLR_Api_Info* ApiInfo;

    TinyCLR_Result(*Acquire)(const TinyCLR_Dac_Controller* self);
    TinyCLR_Result(*Release)(const TinyCLR_Dac_Controller* self);
    TinyCLR_Result(*OpenChannel)(const TinyCLR_Dac_Controller* self, uint32_t channel);
    TinyCLR_Result(*CloseChannel)(const TinyCLR_Dac_Controller* self, uint32_t channel);
    TinyCLR_Result(*WriteValue)(const TinyCLR_Dac_Controller* self, uint32_t channel, int32_t value);
    int32_t(*GetMinValue)(const TinyCLR_Dac_Controller* self);
    int32_t(*GetMaxValue)(const TinyCLR_Dac_Controller* self);
    uint32_t(*GetResolutionInBits)(const TinyCLR_Dac_Controller* self);
    uint32_t(*GetChannelCount)(const TinyCLR_Dac_Controller* self);
};

////////////////////////////////////////////////////////////////////////////////
//Display
////////////////////////////////////////////////////////////////////////////////
enum class TinyCLR_Display_DataFormat : uint32_t {
    Rgb565 = 0,
    Rgb444 = 1,
    VerticalByteStrip1Bpp = 2,
};

enum class TinyCLR_Display_InterfaceType : uint32_t {
    Parallel = 0,
    Spi = 1,
    I2c = 2,
};

struct TinyCLR_Spi_Settings;
struct TinyCLR_I2c_Settings;

struct TinyCLR_Display_ParallelConfiguration {
    bool DataEnableIsFixed;
    bool DataEnablePolarity;
    bool PixelPolarity;
    uint32_t PixelClockRate;
    bool HorizontalSyncPolarity;
    uint32_t HorizontalSyncPulseWidth;
    uint32_t HorizontalFrontPorch;
    uint32_t HorizontalBackPorch;
    bool VerticalSyncPolarity;
    uint32_t VerticalSyncPulseWidth;
    uint32_t VerticalFrontPorch;
    uint32_t VerticalBackPorch;
};

struct TinyCLR_Display_SpiConfiguration {
    const char* ApiName;
    const TinyCLR_Spi_Settings* Settings;
};

struct TinyCLR_Display_I2cConfiguration {
    const char* ApiName;
    const TinyCLR_I2c_Settings* Settings;
};

struct TinyCLR_Display_Controller {
    const TinyCLR_Api_Info* ApiInfo;

    TinyCLR_Result(*Acquire)(const TinyCLR_Display_Controller* self);
    TinyCLR_Result(*Release)(const TinyCLR_Display_Controller* self);
    TinyCLR_Result(*Enable)(const TinyCLR_Display_Controller* self);
    TinyCLR_Result(*Disable)(const TinyCLR_Display_Controller* self);
    TinyCLR_Result(*GetCapabilities)(const TinyCLR_Display_Controller* self, TinyCLR_Display_InterfaceType& type, const TinyCLR_Display_DataFormat*& supportedDataFormats, size_t& supportedDataFormatCount);
    TinyCLR_Result(*GetConfiguration)(const TinyCLR_Display_Controller* self, TinyCLR_Display_DataFormat& dataFormat, uint32_t& width, uint32_t& height, void* configuration);
    TinyCLR_Result(*SetConfiguration)(const TinyCLR_Display_Controller* self, TinyCLR_Display_DataFormat dataFormat, uint32_t width, uint32_t height, const void* configuration);
    TinyCLR_Result(*DrawBuffer)(const TinyCLR_Display_Controller* self, uint32_t x, uint32_t y, uint32_t width, uint32_t height, const uint8_t* data);
    TinyCLR_Result(*DrawPixel)(const TinyCLR_Display_Controller* self, uint32_t x, uint32_t y, uint64_t color);
    TinyCLR_Result(*DrawString)(const TinyCLR_Display_Controller* self, const char* data, size_t length);
};

////////////////////////////////////////////////////////////////////////////////
//GPIO
////////////////////////////////////////////////////////////////////////////////
enum class TinyCLR_Gpio_PinDriveMode : uint32_t {
    Input = 0,
    Output = 1,
    InputPullUp = 2,
    InputPullDown = 3,
    OutputOpenDrain = 4,
    OutputOpenDrainPullUp = 5,
    OutputOpenSource = 6,
    OutputOpenSourcePullDown = 7,
};

enum class TinyCLR_Gpio_PinValue : uint32_t {
    Low = 0,
    High = 1,
};

enum class TinyCLR_Gpio_PinChangeEdge : uint32_t {
    FallingEdge = 1,
    RisingEdge = 2,
};

struct TinyCLR_Gpio_Controller;

typedef void(*TinyCLR_Gpio_PinChangedHandler)(const TinyCLR_Gpio_Controller* self, uint32_t pin, TinyCLR_Gpio_PinChangeEdge edge, uint64_t timestamp);

struct TinyCLR_Gpio_Controller {
    const TinyCLR_Api_Info* ApiInfo;

    TinyCLR_Result(*Acquire)(const TinyCLR_Gpio_Controller* self);
    TinyCLR_Result(*Release)(const TinyCLR_Gpio_Controller* self);
    TinyCLR_Result(*OpenPin)(const TinyCLR_Gpio_Controller* self, uint32_t pin);
    TinyCLR_Result(*ClosePin)(const TinyCLR_Gpio_Controller* self, uint32_t pin);
    TinyCLR_Result(*Read)(const TinyCLR_Gpio_Controller* self, uint32_t pin, TinyCLR_Gpio_PinValue& value);
    TinyCLR_Result(*Write)(const TinyCLR_Gpio_Controller* self, uint32_t pin, TinyCLR_Gpio_PinValue value);
    bool(*IsDriveModeSupported)(const TinyCLR_Gpio_Controller* self, uint32_t pin, TinyCLR_Gpio_PinDriveMode mode);
    TinyCLR_Gpio_PinDriveMode(*GetDriveMode)(const TinyCLR_Gpio_Controller* self, uint32_t pin);
    TinyCLR_Result(*SetDriveMode)(const TinyCLR_Gpio_Controller* self, uint32_t pin, TinyCLR_Gpio_PinDriveMode mode);
    uint64_t(*GetDebounceTimeout)(const TinyCLR_Gpio_Controller* self, uint32_t pin);
    TinyCLR_Result(*SetDebounceTimeout)(const TinyCLR_Gpio_Controller* self, uint32_t pin, uint64_t debounceTime);
    TinyCLR_Result(*SetPinChangedHandler)(const TinyCLR_Gpio_Controller* self, uint32_t pin, TinyCLR_Gpio_PinChangeEdge edge, TinyCLR_Gpio_PinChangedHandler handler);
    uint32_t(*GetPinCount)(const TinyCLR_Gpio_Controller* self);
};

////////////////////////////////////////////////////////////////////////////////
//I2C
////////////////////////////////////////////////////////////////////////////////
enum class TinyCLR_I2c_BusSpeed : uint32_t {
    StandardMode = 0,
    FastMode = 1,
};

enum class TinyCLR_I2c_AddressFormat : uint32_t {
    SevenBit = 0,
    TenBit = 1,
};

enum class TinyCLR_I2c_TransferStatus : uint32_t {
    FullTransfer = 0,
    PartialTransfer = 1,
    SlaveAddressNotAcknowledged = 2,
    ClockStretchTimeout = 3,
};

struct TinyCLR_I2c_Settings {
    uint32_t SlaveAddress;
    TinyCLR_I2c_AddressFormat AddressFormat;
    TinyCLR_I2c_BusSpeed BusSpeed;
};

struct TinyCLR_I2c_Controller {
    const TinyCLR_Api_Info* ApiInfo;

    TinyCLR_Result(*Acquire)(const TinyCLR_I2c_Controller* self);
    TinyCLR_Result(*Release)(const TinyCLR_I2c_Controller* self);
    TinyCLR_Result(*SetActiveSettings)(const TinyCLR_I2c_Controller* self, const TinyCLR_I2c_Settings* settings);
    TinyCLR_Result(*WriteRead)(const TinyCLR_I2c_Controller* self, const uint8_t* writeBuffer, size_t& writeLength, uint8_t* readBuffer, size_t& readLength, bool sendStartCondition, bool sendStopCondition, TinyCLR_I2c_TransferStatus& error);
};

////////////////////////////////////////////////////////////////////////////////
//PWM
////////////////////////////////////////////////////////////////////////////////
enum class TinyCLR_Pwm_PulsePolarity : uint32_t {
    ActiveHigh = 0,
    ActiveLow = 1,
};

struct TinyCLR_Pwm_Controller {
    const TinyCLR_Api_Info* ApiInfo;

    TinyCLR_Result(*Acquire)(const TinyCLR_Pwm_Controller* self);
    TinyCLR_Result(*Release)(const TinyCLR_Pwm_Controller* self);
    TinyCLR_Result(*OpenChannel)(const TinyCLR_Pwm_Controller* self, uint32_t channel);
    TinyCLR_Result(*CloseChannel)(const TinyCLR_Pwm_Controller* self, uint32_t channel);
    TinyCLR_Result(*EnableChannel)(const TinyCLR_Pwm_Controller* self, uint32_t channel);
    TinyCLR_Result(*DisableChannel)(const TinyCLR_Pwm_Controller* self, uint32_t channel);
    TinyCLR_Result(*SetPulseParameters)(const TinyCLR_Pwm_Controller* self, uint32_t channel, double dutyCycle, TinyCLR_Pwm_PulsePolarity polarity);
    TinyCLR_Result(*SetDesiredFrequency)(const TinyCLR_Pwm_Controller* self, double& frequency);
    double(*GetMinFrequency)(const TinyCLR_Pwm_Controller* self);
    double(*GetMaxFrequency)(const TinyCLR_Pwm_Controller* self);
    uint32_t(*GetChannelCount)(const TinyCLR_Pwm_Controller* self);
};

////////////////////////////////////////////////////////////////////////////////
//RTC
////////////////////////////////////////////////////////////////////////////////
struct TinyCLR_Rtc_DateTime {
    uint32_t Year;
    uint32_t Month;
    uint32_t Week;
    uint32_t DayOfYear;
    uint32_t DayOfMonth;
    uint32_t DayOfWeek;
    uint32_t Hour;
    uint32_t Minute;
    uint32_t Second;
    uint32_t Millisecond;
    uint32_t Microsecond;
    uint32_t Nanosecond;
};

struct TinyCLR_Rtc_Controller {
    const TinyCLR_Api_Info* ApiInfo;

    TinyCLR_Result(*Acquire)(const TinyCLR_Rtc_Controller* self);
    TinyCLR_Result(*Release)(const TinyCLR_Rtc_Controller* self);
    TinyCLR_Result(*IsValid)(const TinyCLR_Rtc_Controller* self, bool& value);
    TinyCLR_Result(*GetTime)(const TinyCLR_Rtc_Controller* self, TinyCLR_Rtc_DateTime& value);
    TinyCLR_Result(*SetTime)(const TinyCLR_Rtc_Controller* self, TinyCLR_Rtc_DateTime value);
};

////////////////////////////////////////////////////////////////////////////////
//SPI
////////////////////////////////////////////////////////////////////////////////
enum class TinyCLR_Spi_Mode : uint32_t {
    Mode0 = 0,
    Mode1 = 1,
    Mode2 = 2,
    Mode3 = 3,
};

enum class TinyCLR_Spi_ChipSelectType : uint32_t {
    None = 0,
    Controller = 1,
    Gpio = 2,
};

struct TinyCLR_Spi_Settings {
    TinyCLR_Spi_Mode Mode;
    uint32_t ClockFrequency;
    uint32_t DataBitLength;
    TinyCLR_Spi_ChipSelectType ChipSelectType;
    uint32_t ChipSelectLine;
    uint32_t ChipSelectSetupTime;
    uint32_t ChipSelectHoldTime;
    bool ChipSelectActiveState;
};

struct TinyCLR_Spi_Controller {
    const TinyCLR_Api_Info* ApiInfo;

    TinyCLR_Result(*Acquire)(const TinyCLR_Spi_Controller* self);
    TinyCLR_Result(*Release)(const TinyCLR_Spi_Controller* self);
    TinyCLR_Result(*SetActiveSettings)(const TinyCLR_Spi_Controller* self, const TinyCLR_Spi_Settings* settings);
    TinyCLR_Result(*WriteRead)(const TinyCLR_Spi_Controller* self, const uint8_t* writeBuffer, size_t& writeLength, uint8_t* readBuffer, size_t& readLength, bool deselectAfter);
    uint32_t(*GetChipSelectLineCount)(const TinyCLR_Spi_Controller* self);
    uint32_t(*GetMinClockFrequency)(const TinyCLR_Spi_Controller* self);
    uint32_t(*GetMaxClockFrequency)(const TinyCLR_Spi_Controller* self);
    TinyCLR_Result(*GetSupportedDataBitLengths)(const TinyCLR_Spi_Controller* self, uint32_t* dataBitLengths, size_t& dataBitLengthsCount);
};

////////////////////////////////////////////////////////////////////////////////
//Storage
////////////////////////////////////////////////////////////////////////////////
struct TinyCLR_Storage_Descriptor {
    bool CanReadDirect;
    bool CanWriteDirect;
    bool CanExecuteDirect;
    bool EraseBeforeWrite;
    bool Removable;
    bool RegionsContiguous;
    bool RegionsEqualSized;
    size_t RegionCount;
    const uint64_t* RegionAddresses;
    const size_t* RegionSizes;
};

struct TinyCLR_Storage_Controller {
    const TinyCLR_Api_Info* ApiInfo;

    TinyCLR_Result(*Acquire)(const TinyCLR_Storage_Controller* self);
    TinyCLR_Result(*Release)(const TinyCLR_Storage_Controller* self);
    TinyCLR_Result(*Open)(const TinyCLR_Storage_Controller* self);
    TinyCLR_Result(*Close)(const TinyCLR_Storage_Controller* self);
    TinyCLR_Result(*Read)(const TinyCLR_Storage_Controller* self, uint64_t address, size_t& count, uint8_t* data, uint64_t timeout);
    TinyCLR_Result(*Write)(const TinyCLR_Storage_Controller* self, uint64_t address, size_t& count, const uint8_t* data, uint64_t timeout);
    TinyCLR_Result(*Erase)(const TinyCLR_Storage_Controller* self, uint64_t address, size_t& count, uint64_t timeout);
    TinyCLR_Result(*IsErased)(const TinyCLR_Storage_Controller* self, uint64_t address, size_t count, bool& erased);
    TinyCLR_Result(*GetDescriptor)(const TinyCLR_Storage_Controller* self, const TinyCLR_Storage_Descriptor*& descriptor);
};

////////////////////////////////////////////////////////////////////////////////
//UART
////////////////////////////////////////////////////////////////////////////////
enum class TinyCLR_Uart_Parity : uint32_t {
    None = 0,
    Odd = 1,
    Even = 2,
    Mark = 3,
    Space = 4,
};

enum class TinyCLR_Uart_StopBitCount : uint32_t {
    One = 0,
    OnePointFive = 1,
    Two = 2,
};

enum class TinyCLR_Uart_Handshake : uint32_t {
    None = 0,
    RequestToSend = 1,
    XOnXOff = 2,
    RequestToSendXOnXOff = 3,
};

enum class TinyCLR_Uart_Error : uint32_t {
    Frame = 0,
    Overrun = 1,
    BufferFull = 2,
    ReceiveParity = 3,
};

struct TinyCLR_Uart_Settings {
    uint32_t BaudRate;
    uint32_t DataBits;
    TinyCLR_Uart_Parity Parity;
    TinyCLR_Uart_StopBitCount StopBits;
    TinyCLR_Uart_Handshake Handshaking;
};

struct TinyCLR_Uart_Controller;

typedef void(*TinyCLR_Uart_ErrorReceivedHandler)(const TinyCLR_Uart_Controller* self, TinyCLR_Uart_Error error, uint64_t timestamp);
typedef void(*TinyCLR_Uart_DataReceivedHandler)(const TinyCLR_Uart_Controller* self, size_t count, uint64_t timestamp);
typedef void(*TinyCLR_Uart_ClearToSendChangedHandler)(const TinyCLR_Uart_Controller* self, bool state, uint64_t timestamp);

struct TinyCLR_Uart_Controller {
    const TinyCLR_Api_Info* ApiInfo;

    TinyCLR_Result(*Acquire)(const TinyCLR_Uart_Controller* self);
    TinyCLR_Result(*Release)(const TinyCLR_Uart_Controller* self);
    TinyCLR_Result(*Enable)(const TinyCLR_Uart_Controller* self);
    TinyCLR_Result(*Disable)(const TinyCLR_Uart_Controller* self);
    TinyCLR_Result(*SetActiveSettings)(const TinyCLR_Uart_Controller* self, const TinyCLR_Uart_Settings* settings);
    TinyCLR_Result(*Flush)(const TinyCLR_Uart_Controller* self);
    TinyCLR_Result(*Read)(const TinyCLR_Uart_Controller* self, uint8_t* buffer, size_t& length);
    TinyCLR_Result(*Write)(const TinyCLR_Uart_Controller* self, const uint8_t* buffer, size_t& length);
    TinyCLR_Result(*SetErrorReceivedHandler)(const TinyCLR_Uart_Controller* self, TinyCLR_Uart_ErrorReceivedHandler handler);
    TinyCLR_Result(*SetDataReceivedHandler)(const TinyCLR_Uart_Controller* self, TinyCLR_Uart_DataReceivedHandler handler);
    TinyCLR_Result(*SetClearToSendChangedHandler)(const TinyCLR_Uart_Controller* self, TinyCLR_Uart_ClearToSendChangedHandler handler);
    TinyCLR_Result(*GetClearToSendState)(const TinyCLR_Uart_Controller* self, bool& state);
    TinyCLR_Result(*GetIsRequestToSendEnabled)(const TinyCLR_Uart_Controller* self, bool& state);
    TinyCLR_Result(*SetIsRequestToSendEnabled)(const TinyCLR_Uart_Controller* self, bool state);
    size_t(*GetBytesToWrite)(const TinyCLR_Uart_Controller* self);
    size_t(*GetBytesToRead)(const TinyCLR_Uart_Controller* self);
    TinyCLR_Result(*ClearWriteBuffer)(const TinyCLR_Uart_Controller* self);
    TinyCLR_Result(*ClearReadBuffer)(const TinyCLR_Uart_Controller* self);
    size_t(*GetWriteBufferSize)(const TinyCLR_Uart_Controller* self);
    size_t(*GetReadBufferSize)(const TinyCLR_Uart_Controller* self);
    TinyCLR_Result(*SetWriteBufferSize)(const TinyCLR_Uart_Controller* self, size_t size);
    TinyCLR_Result(*SetReadBufferSize)(const TinyCLR_Uart_Controller* self, size_t size);
};

////////////////////////////////////////////////////////////////////////////////
//USB Client
////////////////////////////////////////////////////////////////////////////////
struct TinyCLR_UsbClient_Controller;
struct TinyCLR_UsbClient_ConfigurationDescriptor;
struct TinyCLR_UsbClient_InterfaceDescriptor;
struct TinyCLR_UsbClient_EndpointDescriptor;
struct TinyCLR_UsbClient_StringDescriptor;

struct TinyCLR_UsbClient_VendorClassDescriptor {
    uint8_t Length;
    uint8_t Type;
    const uint8_t* Payload;
};

struct TinyCLR_UsbClient_DeviceDescriptor {
    uint16_t UsbVersion;
    uint8_t ClassCode;
    uint8_t SubClassCode;
    uint8_t ProtocolCode;
    uint8_t MaxPacketSizeEp0;
    uint16_t VendorId;
    uint16_t ProductId;
    uint16_t DeviceVersion;
    uint8_t ManufacturerNameIndex;
    uint8_t ProductNameIndex;
    uint8_t SerialNumberIndex;
    uint8_t ConfigurationCount;
    uint8_t StringCount;

    const TinyCLR_UsbClient_ConfigurationDescriptor* Configurations;
    const TinyCLR_UsbClient_StringDescriptor* Strings;
};

struct TinyCLR_UsbClient_ConfigurationDescriptor {
    uint16_t TotalLength;
    uint8_t InterfaceCount;
    uint8_t ConfigurationValue;
    uint8_t DescriptionIndex;
    uint8_t Attributes;
    uint8_t MaxPower;
    uint8_t VendorClassDescriptorCount;

    const TinyCLR_UsbClient_InterfaceDescriptor* Interfaces;
    const TinyCLR_UsbClient_VendorClassDescriptor* VendorClassDescriptors;
};

struct TinyCLR_UsbClient_InterfaceDescriptor {
    uint8_t InterfaceNumber;
    uint8_t AlternateSetting;
    uint8_t EndpointCount;
    uint8_t ClassCode;
    uint8_t SubClassCode;
    uint8_t ProtocolCode;
    uint8_t DescriptionIndex;
    uint8_t VendorClassDescriptorCount;

    const TinyCLR_UsbClient_EndpointDescriptor* Endpoints;
    const TinyCLR_UsbClient_VendorClassDescriptor* VendorClassDescriptors;
};

struct TinyCLR_UsbClient_EndpointDescriptor {
    uint8_t Address;
    uint8_t Attributes;
    uint16_t MaxPacketSize;
    uint8_t Interval;
    uint8_t VendorClassDescriptorCount;

    const TinyCLR_UsbClient_VendorClassDescriptor* VendorClassDescriptors;
};

struct TinyCLR_UsbClient_StringDescriptor {
    uint8_t Length;
    uint8_t Index;

    const wchar_t* Data;
};

struct TinyCLR_UsbClient_SetupPacket {
    uint8_t RequestType;
    uint8_t Request;
    uint16_t Value;
    uint16_t Index;
    uint16_t Length;
};

enum class TinyCLR_UsbClient_DeviceState {
    Detached = 0,
    Attached = 1,
    Powered = 2,
    Default = 3,
    Address = 4,
    Configured = 5,
    Suspended = 6
};

typedef void(*TinyCLR_UsbClient_DataReceivedHandler)(const TinyCLR_UsbClient_Controller* self, uint64_t timestamp);
typedef void(*TinyCLR_UsbClient_DeviceStateChangedHandler)(const TinyCLR_UsbClient_Controller* self, TinyCLR_UsbClient_DeviceState deviceState, uint64_t timestamp);
typedef TinyCLR_Result(*TinyCLR_UsbClient_RequestHandler)(const TinyCLR_UsbClient_Controller* self, const TinyCLR_UsbClient_SetupPacket* setupPacket, const uint8_t*& responsePayload, size_t& responsePayloadLength, uint64_t timestamp);

struct TinyCLR_UsbClient_Controller {
    const TinyCLR_Api_Info* ApiInfo;

    TinyCLR_Result(*Acquire)(const TinyCLR_UsbClient_Controller* self);
    TinyCLR_Result(*Release)(const TinyCLR_UsbClient_Controller* self);
    TinyCLR_Result(*OpenPipe)(const TinyCLR_UsbClient_Controller* self, uint8_t writeEndpoint, uint8_t readEndpoint, uint32_t& pipe);
    TinyCLR_Result(*ClosePipe)(const TinyCLR_UsbClient_Controller* self, uint32_t pipe);
    TinyCLR_Result(*WritePipe)(const TinyCLR_UsbClient_Controller* self, uint32_t pipe, const uint8_t* data, size_t& length);
    TinyCLR_Result(*ReadPipe)(const TinyCLR_UsbClient_Controller* self, uint32_t pipe, uint8_t* data, size_t& length);
    TinyCLR_Result(*FlushPipe)(const TinyCLR_UsbClient_Controller* self, uint32_t pipe);
    TinyCLR_Result(*SetDeviceDescriptor)(const TinyCLR_UsbClient_Controller* self, const TinyCLR_UsbClient_DeviceDescriptor* descriptor);
    TinyCLR_Result(*SetGetDescriptorHandler)(const TinyCLR_UsbClient_Controller* self, TinyCLR_UsbClient_RequestHandler handler);
    TinyCLR_Result(*SetVendorClassRequestHandler)(const TinyCLR_UsbClient_Controller* self, TinyCLR_UsbClient_RequestHandler handler);
    TinyCLR_Result(*SetDataReceivedHandler)(const TinyCLR_UsbClient_Controller* self, TinyCLR_UsbClient_DataReceivedHandler handler);
    TinyCLR_Result(*SetDeviceStateChangedHandler)(const TinyCLR_UsbClient_Controller* self, TinyCLR_UsbClient_DeviceStateChangedHandler handler);
    TinyCLR_Result(*GetDeviceState)(const TinyCLR_UsbClient_Controller* self, TinyCLR_UsbClient_DeviceState& deviceState);
    size_t(*GetBytesToWrite)(const TinyCLR_UsbClient_Controller* self, uint32_t pipe);
    size_t(*GetBytesToRead)(const TinyCLR_UsbClient_Controller* self, uint32_t pipe);
    TinyCLR_Result(*ClearWriteBuffer)(const TinyCLR_UsbClient_Controller* self, uint32_t pipe);
    TinyCLR_Result(*ClearReadBuffer)(const TinyCLR_UsbClient_Controller* self, uint32_t pipe);
    size_t(*GetWriteBufferSize)(const TinyCLR_UsbClient_Controller* self, uint32_t pipe);
    size_t(*GetReadBufferSize)(const TinyCLR_UsbClient_Controller* self, uint32_t pipe);
    TinyCLR_Result(*SetWriteBufferSize)(const TinyCLR_UsbClient_Controller* self, uint32_t pipe, size_t size);
    TinyCLR_Result(*SetReadBufferSize)(const TinyCLR_UsbClient_Controller* self, uint32_t pipe, size_t size);
};
