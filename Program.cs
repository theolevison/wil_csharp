using System;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

[StructLayout(LayoutKind.Sequential)]
public struct NeoDeviceEx
{
	public NeoDevice neoDevice;

	public uint FirmwareMajor;
	public uint FirmwareMinor;

	

	//#define CANNODE_STATUS_COREMINI_IS_RUNNING (0x1)
	//#define CANNODE_STATUS_IN_BOOTLOADER (0x2)
	public uint Status; // Bitfield, see defs above

	// Option bit flags
	/*
	#define MAIN_VNET (0x01)
	#define SLAVE_VNET_A (0x02)
	#define SLAVE_VNET_B (0x04)
	#define WIFI_CONNECTION (0x08)
	#define REGISTER_BY_SERIAL (0x10)
	#define TCP_SUPPORTED (0x20)
	#define DRIVER_MASK (0xC0)
	#define DRIVER_USB1 (0x40)
	#define DRIVER_USB2 (0x80)
	#define DRIVER_USB3 (0xC0)
    */
	public uint Options;

	public IntPtr pAvailWIFINetwork;
	public IntPtr pWIFIInterfaceInfo;

	public int isEthernetDevice;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
	public byte[] MACAddress; //length 6
	public ushort hardwareRev;
	public ushort revReserved;
	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
	public uint[] tcpIpAddress; //length 4
	public ushort tcpPort;
	public ushort Reserved0;
	public uint Reserved1;
}

[StructLayout(LayoutKind.Sequential)]
public struct NeoDevice
{
	public uint DeviceType;
	public int Handle;
	public int NumberOfClients;
	public int SerialNumber;
	public int MaxAllowedClients;
}

internal class Program
{

    [DllImport(@"C:\Windows\SysWOW64\icsneo40.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int icsneoFindDevices(IntPtr possibleDevices, ref int numDevices, IntPtr deviceTypes, uint numDeviceTypes, IntPtr optionsFindeNeoEx, uint reserved);
	[DllImport(@"C:\Windows\SysWOW64\icsneo40.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	private static extern int icsneoOpenNeoDevice(out IntPtr device, IntPtr handle, IntPtr networkIDs, int configRead, int options);
    [DllImport(@"C:\Windows\SysWOW64\icsneo40.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int icsneoSerialNumberToString(int serialNumber, [MarshalAs(UnmanagedType.LPStr)] StringBuilder data, int lengthOfBuffer);

    /*
     * typedef struct
        {
	        uint32_t DeviceType;
	        int32_t Handle;
	        int32_t NumberOfClients;
	        int32_t SerialNumber;
	        int32_t MaxAllowedClients;

        } NeoDevice;
     */

    /* 
     * typedef struct _NeoDeviceEx
		{
			NeoDevice neoDevice;

			uint32_t FirmwareMajor;
			uint32_t FirmwareMinor;

		#define CANNODE_STATUS_COREMINI_IS_RUNNING (0x1)
		#define CANNODE_STATUS_IN_BOOTLOADER (0x2)
			uint32_t Status; // Bitfield, see defs above

		// Option bit flags
		#define MAIN_VNET (0x01)
		#define SLAVE_VNET_A (0x02)
		#define SLAVE_VNET_B (0x04)
		#define WIFI_CONNECTION (0x08)
		#define REGISTER_BY_SERIAL (0x10)
		#define TCP_SUPPORTED (0x20)
		#define DRIVER_MASK (0xC0)
		#define DRIVER_USB1 (0x40)
		#define DRIVER_USB2 (0x80)
		#define DRIVER_USB3 (0xC0)
			uint32_t Options;

			void* pAvailWIFINetwork;
			void* pWIFIInterfaceInfo;

			int isEthernetDevice;

			uint8_t MACAddress[6];
			uint16_t hardwareRev;
			uint16_t revReserved;
			uint32_t tcpIpAddress[4];
			uint16_t tcpPort;
			uint16_t Reserved0;
			uint32_t Reserved1;

		} NeoDeviceEx;
     */

    /*
	 
	eWILFunction_Init = 0, //0
	eWILFunction_Terminate, //1
	eWILFunction_Connect, //2
	eWILFunction_Disconnect, //3
	eWILFunction_SetMode, //4
	eWILFunction_GetMode, //5
	eWILFunction_SetACL, //6
	eWILFunction_GetACL, //7
	eWILFunction_QueryDevice, //8
	eWILFunction_GetNetworkStatus, //9
	eWILFunction_LoadFile, //10
	eWILFunction_EraseFile, //11
	eWILFunction_GetContainerDetails, //12
	eWILFunction_SetGPIO, //13
	eWILFunction_GetGPIO, //14
	eWILFunction_SelectScript, //15
	eWILFunction_ModifyScript, //16
	eWILFunction_GetDeviceVersion, //17
	eWILFunction_GetWilSoftwareVersion, //18
	eWILFunction_EnterInventoryState, //19
	eWILFunction_GetFile, //20
	eWILFunction_SetContextualData, //21
	eWILFunction_GetContextualData, //22
	eWILFunction_SetStateOfHealth, //23
	eWILFunction_GetStateOfHealth, //24
	eWILFunction_EnableFaultServicing, //25
	eWILFunction_ResetDevice, //26
	eWILFunction_RotateKey, //27
	eWILFunction_EnableNetworkDataCapture, //28
	eWILFunction_SetCustomerIdentifierData, //29
	eWILFunction_UpdateMonitorParameters, //30
	eWILFunction_GetMonitorParametersCRC, //31
	 */
    private static void Main(string[] args)
	{

		//c array of neoDeviceEx, gets populated
		//need to access first item in array, and get neoDevice from it
		int numberOfDevices = 1;

        //allocate memory for array of deviceExs
        NeoDeviceEx[] arrayOfDevices = new NeoDeviceEx[10];
		IntPtr pointerToArray = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(NeoDeviceEx)) * arrayOfDevices.Length);
		long longPtr = pointerToArray.ToInt64();
		for (int i = 0; i < arrayOfDevices.Length; i++)
		{
			IntPtr tempPtr = new IntPtr(longPtr);
			Marshal.StructureToPtr(arrayOfDevices[i], tempPtr, false);
			longPtr += Marshal.SizeOf(typeof(NeoDeviceEx));
		}

		// IntPtr as a handle for the array of devices works
		icsneoFindDevices(pointerToArray, ref numberOfDevices, IntPtr.Zero, 0, IntPtr.Zero, 0);
        Console.WriteLine($"Number of devices found: {numberOfDevices}");

		//take first chunk of memory at IntPtr location, try to turn it into a NeoDeviceEx
		NeoDeviceEx deviceEx = Marshal.PtrToStructure<NeoDeviceEx>(pointerToArray);// (NeoDeviceEx)Marshal.PtrToStructure(new IntPtr(pointerToArray.ToInt64()), typeof(NeoDeviceEx));	
		
		//find serial number of found device
		StringBuilder stringBuilder = new StringBuilder();
		icsneoSerialNumberToString(deviceEx.neoDevice.SerialNumber, stringBuilder, 100);
        Console.WriteLine($"Serial number of first device: {stringBuilder}");

		//allocate memory for handle, idk how big it should be, so go big
		IntPtr handlePointer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(NeoDeviceEx)));

        //allocate memory for neoDevice, then convert managed struct to unmanaged
        IntPtr pointerToDevice = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(NeoDevice)));
        Marshal.StructureToPtr(deviceEx.neoDevice, pointerToDevice, false);

		if (icsneoOpenNeoDevice(out pointerToDevice, handlePointer, IntPtr.Zero, 1, 0) == 1) {
            Console.WriteLine($"Opened device");
        } else
		{
            Console.WriteLine($"Cannot open device");
        }

		//TODO: after allocating memory manually, it needs cleaning up otherwise memory leak
    }
}