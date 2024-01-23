using System;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

[StructLayout(LayoutKind.Sequential)]
public struct NeoDeviceEx
{
	public IntPtr neoDevice;

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

	public byte[] MACAddress; //length 6
	public ushort hardwareRev;
	public ushort revReserved;
	public uint[] tcpIpAddress; //length 4
	public ushort tcpPort;
	public ushort Reserved0;
	public uint Reserved1;
}


internal class Program
{

    [DllImport(@"C:\Windows\SysWOW64\icsneo40.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int icsneoFindDevices(out IntPtr possibleDevices, ref int numDevices, int diviceTypes, int numDeviceTypes, IntPtr optionsFindeNeoEx, long reserved);

    [DllImport(@"C:\Windows\SysWOW64\icsneo40.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	private static extern int icsneoFindDevices(out NeoDeviceEx[] possibleDevices, ref int numDevices, int diviceTypes, int numDeviceTypes, IntPtr optionsFindeNeoEx, long reserved);
	[DllImport(@"C:\Windows\SysWOW64\icsneo40.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	private static extern int icsneoGenericAPISendCommand(out IntPtr handle, char apiIndex, char instanceIndex, char functionIndex, byte[] bData, int length, out char functionError);
	[DllImport(@"C:\Windows\SysWOW64\icsneo40.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	private static extern int icsneoOpenNeoDevice(ref IntPtr device, ref IntPtr handle, char networkIDs, int configRead, int options);

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
		Console.WriteLine("Hello, World!");

		IntPtr parameters = IntPtr.Zero;
        //char functionError;

		uint SensorBufferSize = 64;
        byte[] pParameters = new byte[512];
        pParameters[0] = 1;
        pParameters[1] = 1;
        pParameters[2] = (byte)((SensorBufferSize >> 8) & 0xFF);
        pParameters[3] = (byte)(SensorBufferSize & 0xFF);

        //var device = icsneoFindDevices(icsneoFindDevices, 1, null, 0, null, 0);
        Console.WriteLine(icsneoGenericAPISendCommand(out IntPtr handle, '1', '0', '2', pParameters, '4', out char functionError));

		//c array of neoDeviceEx, gets populated
		//need to access first item in array, and get neoDevice from it
		
		int numberOfDevices = 1;
		IntPtr arrayOfDevices;
		Console.WriteLine(numberOfDevices);
		// IntPtr as a handle for the array of devices works
		icsneoFindDevices(out arrayOfDevices, ref numberOfDevices, 0, 0, 0, 0);
        Console.WriteLine($"Number of devices {numberOfDevices}");

		//trying to pass in an array of devices does not work
		//can I marshall it into my struct?
        //NeoDeviceEx[] handle2 = new NeoDeviceEx[10];
		//Marshal.PtrToStructure(intHandle, typeof(NeoDeviceEx));
        //icsneoFindDevices(out handle2, ref numberOfDevices, 0, 0, 0, 0);

		var size = Marshal.SizeOf(typeof(NeoDeviceEx));
		NeoDeviceEx[] managedArray = new NeoDeviceEx[size];

		IntPtr ins = new nint(arrayOfDevices.ToInt64());
		Console.WriteLine("Will it marshal?");
		managedArray[0] = Marshal.PtrToStructure<NeoDeviceEx>(ins);
		
		IntPtr x = IntPtr.Zero;
		
		if (true)
		{
			//Marshall IntPtr to neoDeviceEx
			
			Console.WriteLine("not null");
            Console.WriteLine($"Opened device {icsneoOpenNeoDevice(ref managedArray[0].neoDevice, ref x, '0', 1, 0)}");
        } else
		{
            Console.WriteLine($"List is null");
        }
    }
}