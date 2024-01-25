using System;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

/*
 * [StructLayout(LayoutKind.Sequential)]
    public struct icsSpyMessage   //reff
    {
        public Int32 StatusBitField; //4
        public Int32 StatusBitField2; //new '4
        public Int32 TimeHardware; // 4
        public Int32 TimeHardware2; //new ' 4
        public Int32 TimeSystem; // 4
        public Int32 TimeSystem2;
        public byte TimeStampHardwareID; //new ' 1
        public byte TimeStampSystemID;
        public byte NetworkID; //new ' 1
        public byte NodeID;
        public byte Protocol;
        public byte MessagePieceID; // 1
        public byte ExtraDataPtrEnabled; //1
        public byte NumberBytesHeader; // 1
        public byte NumberBytesData; // 1
        public byte NetworkID2;//1
        public Int16 DescriptionID; // 2
        public Int32 ArbIDOrHeader; // Holds (up to 3 byte 1850 header or 29 bit CAN header) '4
        //public byte[] Data = new byte[8]; //(1 To 8); //8
        public byte Data1;
        public byte Data2;
        public byte Data3;
        public byte Data4;
        public byte Data5;
        public byte Data6;
        public byte Data7;
        public byte Data8;
        public Int32 StatusBitField3;
        public Int32 StatusBitField4;
        //public byte[] AckBytes = new byte[8]; //(1 To 8); //new '8
        public IntPtr iExtraDataPtr; // As Single ' 4 or 8 depending on system
        public byte MiscData;
        public byte Reserved1;
        public byte Reserved2;
        public byte Reserved3;
    }
 */

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

[StructLayout(LayoutKind.Sequential)]
public struct IcsSpyMessage
{
    public uint StatusBitField;
    public uint StatusBitField2;
    public uint TimeHardware;
    public uint TimeHardware2;
    public uint TimeSystem;
    public uint TimeSystem2;
    public byte TimeStampHardwareID;
    public byte TimeStampSystemID;
    public byte NetworkID;
    public byte NodeID;
    public byte Protocol;
    public byte MessagePieceID;
    public byte ExtraDataPtrEnabled;
    public byte NumberBytesHeader;
    public byte NumberBytesData;
    public byte NetworkID2;
    public short DescriptionID; //int16_t or uint32_t depending on vspy version, as far as I could tell
    public uint ArbIDOrHeader;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
    public byte[] Data;

    
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
    public byte[] AckBytes; //replaced the union with just this pointer
  

    public IntPtr ExtraDataPtr;
    public byte MiscData;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
    public byte[] Reserved;
}

internal class Program
{
	//TODO: If these dlls are in a different location on your computer, change the code to match!
	[DllImport(@"C:\Windows\SysWOW64\icsneo40.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	private static extern int icsneoFindDevices(IntPtr possibleDevices, ref int numDevices, IntPtr deviceTypes, uint numDeviceTypes, IntPtr optionsFindeNeoEx, uint reserved);
	[DllImport(@"C:\Windows\SysWOW64\icsneo40.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	private static extern int icsneoOpenNeoDevice(IntPtr device, IntPtr handle, IntPtr networkIDs, int configRead, int options);
	[DllImport(@"C:\Windows\SysWOW64\icsneo40.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	private static extern int icsneoSerialNumberToString(int serialNumber, [MarshalAs(UnmanagedType.LPStr)] StringBuilder data, int lengthOfBuffer);
	[DllImport(@"C:\Windows\SysWOW64\icsneo40.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	private static extern int icsneoGetMessages(IntPtr handle, ref IcsSpyMessage icsSpyMessage, ref int numberOfMessages, ref int numberOfErrors);
    private static void Main(string[] args)
	{		
		//create array of neoDeviceEx, gets populated by c function
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

		//populate reserved memory with NeoDeviceEx's
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

		//if the firmware is not correct, may not work?
		//can be overloaded in c, but overload in c# is not implemented yet
		if (icsneoOpenNeoDevice(pointerToDevice, handlePointer, IntPtr.Zero, 1, 0) == 1) {
			//--> at this point, global var `hObject` is a reference to the open wBMS device;
			Console.WriteLine($"Opened device");


			/*
			 * //Query both ports first
				{"test_wbms_QueryDeviceOnPort1", test_wbms_QueryDeviceOnPort1}, // Query ports
				{"test_wbms_QueryDeviceOnPort2", test_wbms_QueryDeviceOnPort2}, // Query ports

				{"test_wbms_Connect", test_wbms_Connect}, // ALL MODE
			*/


			//allocate memory for array of deviceExs
			//TODO: test if this is necessary, might be allocated by C function
			IcsSpyMessage[] arrayOfMessages = new IcsSpyMessage[2000];
			/*
			IntPtr pointerToMsgArray = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(IcsSpyMessage)) * arrayOfMessages.Length);
			long longPtr2 = pointerToMsgArray.ToInt64();
			for (int i = 0; i < arrayOfMessages.Length; i++)
			{
				IntPtr tempPtr2 = new IntPtr(longPtr2);
				Marshal.StructureToPtr(arrayOfMessages[i], tempPtr2, false);
				longPtr2 += Marshal.SizeOf(typeof(IcsSpyMessage));
			}
			*/

			int numberMessages = 0;
			int numberErrors = 0;
			
			if (icsneoGetMessages(handlePointer, ref arrayOfMessages[0], ref numberMessages, ref numberErrors) == 1)
			{
				Console.WriteLine($"Number of messages recieved: {numberMessages}");
				Console.WriteLine($"Number of errors recieved: {numberErrors}");
			}
			else
			{
				Console.WriteLine("Error getting messages");
			}			
		
        } else
		{
            Console.WriteLine($"Cannot open device");
        }

		//TODO: after allocating memory manually, it needs cleaning up otherwise memory leak
		
    }
}