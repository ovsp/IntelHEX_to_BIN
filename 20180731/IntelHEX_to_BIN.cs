// IntelHEX_to_BIN.cs
using System;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Collections.Generic;

public class IntelHEX_to_BIN
{
	
	public static void Main(string[] arg)   { 
		Console.WriteLine(" IntelHEX_to_BIN - преобразует содержимое файла прошивки в бинарный файл."); 
		if (arg.Length <= 1) {	Console.WriteLine(" Не достаточно аргументов командной строки.");
								Console.WriteLine(" Первый аргумент (обязательный) - название файла прошивки.");
								Console.WriteLine(" Второй аргумент (обязательный) - две шестнадцатеричные цифры слитно - байт для заполнения недостающих адресов.");
								Console.WriteLine(" Остальные аргументы игнорируются.");	
								Console.ReadKey(); return;}
		fileNameExt = arg[0];

		Console.WriteLine(" Загружен файл " + fileNameExt);
		
		if(fileNameExt.ToLower().EndsWith(".hex")) { fileName = fileNameExt.Replace(".hex", ""); fileExt = ".hex";}
		if(fileNameExt.ToLower().EndsWith(".srec")) { fileName = fileNameExt.Replace(".srec", ""); fileExt = ".srec";}
		if(fileNameExt.ToLower().EndsWith(".sx")) { fileName = fileNameExt.Replace(".sx", ""); fileExt = ".sx";}
		if(fileNameExt.ToLower().EndsWith(".s19")) { fileName = fileNameExt.Replace(".s19", ""); fileExt = ".s19";}
		if(fileNameExt.ToLower().EndsWith(".s28")) { fileName = fileNameExt.Replace(".s28", ""); fileExt = ".s28";}
		if(fileNameExt.ToLower().EndsWith(".s37")) { fileName = fileNameExt.Replace(".s37", ""); fileExt = ".s37";}
		
        try{
			fillByte = byte.Parse(arg[1], System.Globalization.NumberStyles.HexNumber);
		}
			catch (Exception ex){
			Console.WriteLine(" Второй аргумент командной строки содержит символ(ы) не из диапазона 0...9, A...F");
			return;
			}  
		
		Console.WriteLine(" Байт для заполнения 0x{0:X2}", fillByte);
		
		
		
		
    } // Main();
	

	public static bool unready = true;
    
	public static byte fillByte = 0x00;
	public static byte rvsn_byte;
	public static byte[] btBytes = new byte[1];
	public static byte[] wrd4Bytes = new byte[4];
	public static byte[] blck64Bytes = new byte[64];
	public static byte[] blck192Bytes = new byte[192];
	public static byte[] txBytes = {0, 0};
	public static byte[] go_bytes = { 0xF5, 0x00, 0x00, 0x00, 0x17, 0x00}; // { GO cmd, YH, YL, go_adrH, go_adrL, cntr }
	
	public static int goAdrCmdStr;
	public static int goAdrFrmSTM8;
	public static int blckSize;
	
	public static SortedDictionary<int, byte> srtDic;

	public static string readLine = "";	
	public static string fileName = "";	
	public static string fileExt = "";	
	public static string fileNameExt = "";	
	public static string srtGoAddress = "";	
	public static string stringMemoryMap = "";
	
    public static Thread readThreadBytes;
    public static Thread readThreadKeys;

} // class IntelHEX_to_BIN
// IntelHEX_to_BIN.cs