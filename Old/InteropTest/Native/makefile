OUTPUT_NAME = bin/Interop
LINKERSCRIPT_NAME = scatterfile

MCU_FLAGS = -mcpu=cortex-m4 -mthumb
INC_DIRS = -I./include

CC = arm-none-eabi-g++.exe
LD = arm-none-eabi-g++.exe
OC = arm-none-eabi-objcopy.exe

CC_FLAGS = $(INC_DIRS) $(MCU_FLAGS) -g -std=c++11 -xc++ -Wall -Wabi -w -mabi=aapcs -fPIC -fno-exceptions -fno-rtti -fno-use-cxa-atexit -fno-threadsafe-statics

# Use this for optimize for size when not debugging
CC_FLAGS += -O2

LD_FLAGS = $(MCU_FLAGS) -nostartfiles -lc -lgcc -T $(LINKERSCRIPT_NAME) -Wl,-Map,$(OUTPUT_NAME).map -Wl,--oformat -Wl,elf32-littlearm
OC_FLAGS = -S -O binary

SRC_FILES = $(wildcard *.cpp)
OBJ_FILES = $(patsubst %.cpp, %.obj, $(SRC_FILES))

build: $(OBJ_FILES)
	if not exist bin mkdir bin
	$(LD) $(LD_FLAGS) -o $(OUTPUT_NAME).elf $^
	$(OC) $(OC_FLAGS) $(OUTPUT_NAME).elf $(OUTPUT_NAME).bin

clean:
	rm $(OBJ_FILES) $(OUTPUT_NAME).bin $(OUTPUT_NAME).elf $(OUTPUT_NAME).map

	

%.obj: %.cpp
	$(CC) -c $(CC_FLAGS) -o $@ $^