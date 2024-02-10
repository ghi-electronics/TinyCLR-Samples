using System.Diagnostics;
using System.Threading;

using GHIElectronics.TinyCLR.Devices.SecureStorage;
using GHIElectronics.TinyCLR.Devices.Rtc;
using GHIElectronics.TinyCLR.Drivers.MemoryManager.RtcMemory;
using GHIElectronics.TinyCLR.Drivers.MemoryManager.SecureMemory;

// ReSharper disable InconsistentNaming
// ReSharper disable ArrangeThisQualifier
#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable IDE0009 // Member access should be qualified.


namespace MemoryManager
{
	internal static class Program
	{
		private static void Main()
		{
			// to use the secure storage memory
			var _memoryManager = new GHIElectronics.TinyCLR.Drivers.MemoryManager.MemoryManager(new SecureStorageInterface(new SecureStorageController(SecureStorage.Configuration)));

			// to use rtc memory
			//_memoryManager = new GHIElectronics.TinyCLR.Drivers.MemoryManager.MemoryManager(new RtcMemoryInterface(RtcController.GetDefault()));


			// store a byte array
			if (!_memoryManager.Recall(0, out var @byteArray))
				_memoryManager.AddOrReplace(0, new byte[] { 88, 11, 0xbb });
			else
				foreach (var b in (byte[])@byteArray)
					Debug.WriteLine(b.ToString());

			// store a byte
			if (!_memoryManager.Recall(1, out var @byte))
				_memoryManager.AddOrReplace(1, (byte)22);
			else
				Debug.WriteLine(@byte.ToString());

			// store a signed byte
			if (!_memoryManager.Recall(2, out var @sbyte))
				_memoryManager.AddOrReplace(2, (sbyte)55);
			else
				Debug.WriteLine(@sbyte.ToString());

			// store a bool
			if (!_memoryManager.Recall(3, out var @bool))
				_memoryManager.AddOrReplace(3, true);
			else
				Debug.WriteLine(@bool.ToString());

			// store a short
			if (!_memoryManager.Recall(4, out var @short))
				_memoryManager.AddOrReplace(4, (short)15000);
			else
				Debug.WriteLine(@short.ToString());

			// store a ushort
			if (!_memoryManager.Recall(5, out var @ushort))
				_memoryManager.AddOrReplace(5, (ushort)49999);
			else
				Debug.WriteLine(@ushort.ToString());

			// store a int
			if (!_memoryManager.Recall(6, out var @int))
				_memoryManager.AddOrReplace(6, (int)270000);
			else
				Debug.WriteLine(@int.ToString());

			// store a uint
			if (!_memoryManager.Recall(7, out var @uint))
				_memoryManager.AddOrReplace(7, (uint)3949499);
			else
				Debug.WriteLine(@uint.ToString());

			// store a float
			if (!_memoryManager.Recall(8, out var @float))
				_memoryManager.AddOrReplace(8, (float)6.4);
			else
				Debug.WriteLine(@float.ToString());

			// store a long
			if (!_memoryManager.Recall(9, out var @long))
				_memoryManager.AddOrReplace(9, (long)987654321000000000);
			else
				Debug.WriteLine(@long.ToString());

			// store a ulong
			if (!_memoryManager.Recall(10, out var @ulong))
				_memoryManager.AddOrReplace(10, (ulong)9876543210000000000);
			else
				Debug.WriteLine(@ulong.ToString());

			// store a double
			if (!_memoryManager.Recall(11, out var @double))
				_memoryManager.AddOrReplace(11, (double)9876543210.987654321);
			else
				Debug.WriteLine(@double.ToString());

			// store a char
			if (!_memoryManager.Recall(13, out var @char))
				_memoryManager.AddOrReplace(13, (char)'h');
			else
				Debug.WriteLine(@char.ToString());

			// store a string
			if (!_memoryManager.Recall(12, out var @string))
				_memoryManager.AddOrReplace(12, (string)"xyz");
			else
				Debug.WriteLine(@string.ToString());

			Debug.WriteLine("");

			// display a memory dump in the output window
			_memoryManager.Dump();

			Thread.Sleep(Timeout.Infinite);
		}
	}
}
