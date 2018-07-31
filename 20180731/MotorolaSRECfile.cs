using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

public class MotorolaSRECfile
{
    public MotorolaSRECfile(string filename)
    {
		// if (filename.ToLower().EndsWith(".s19") != true) FileErrorMessages.Add("Файл имеет расширение отличное от s19");
		if (filename.ToLower().EndsWith(".s19") | filename.ToLower().EndsWith(".s28") | filename.ToLower().EndsWith(".s37") |
		    filename.ToLower().EndsWith(".srec") | filename.ToLower().EndsWith(".sx") ) {}
			else FileErrorMessages.Add(" Файл имеет расширение отличное от srec (s19, s28, s37, sx)");
		try 
		{
			StreamReader sr = new StreamReader(filename);
			bool eof = false;
			int lineNumber = 0;
			while (!eof)
			{
				lineNumber++;
				SRECline line = new SRECline(sr.ReadLine(), lineNumber);
				//if (ErrorMessage == "") ErrorMessage = line.ErrorMessages;
				FileErrorMessages.AddRange(line.LineErrorMessages);
				if (line.CriticalErrors == true) CriticalError = true;
				
				switch (line.recordtype)
				{
					case SRECline.RecordType.DataRecord16:
					case SRECline.RecordType.DataRecord24:
					case SRECline.RecordType.DataRecord32:
							//data.AddRange(line.data);
							int ij=0;
							foreach( byte bt in line.data)
								{
										AddressByteSorted.Add((uint)(line.address+ij), line.data[ij]);
										ij++;		
								}
							
						break;
						

					case SRECline.RecordType.CountRecord16:
					case SRECline.RecordType.CountRecord24:
						//recordCount = line.address;
						
						break;
						

					case SRECline.RecordType.StartAddressRecord32:
					case SRECline.RecordType.StartAddressRecord24:
					case SRECline.RecordType.StartAddressRecord16:
						eof = true;
						break;
				}
				if (sr.EndOfStream)
				{
					eof = true;
				}
			}
			
			foreach( KeyValuePair<uint, byte> dabs in AddressByteSorted )
			{
				Addresses.Add(dabs.Key);
				Bytes.Add(dabs.Value);
			}
			
			for (int i = 0; i < AddressByteSorted.Count; i++)
			{
				if (Addresses[i] < minAddress) minAddress = Addresses[i];
				if (Addresses[i] > maxAddress) maxAddress = Addresses[i];
			}
			
			//for (int i = 0; i < AddressByteSorted.Count; i++)
			//{
			//	if (Addresses[i] > maxAddress) maxAddress = Addresses[i];
			//}
			
			sr.Close();
			sr.Dispose();
		}
		catch(Exception ex)
		{
			ErrorMessage = ex.Message;
			CriticalError = true;
		}
    }

   // public byte[] GetData()
   // {
    //    return data.ToArray();
    //}
    public uint[] GetAddresses() { return Addresses.ToArray(); }
    public uint GetMinAddress() { return minAddress; }
    public uint GetMaxAddress() { return maxAddress; }
    public byte[] GetBytes() { return Bytes.ToArray(); }
    public int GetCount() { return AddressByteSorted.Count; }
    public string[] GetErrorMessages() { return FileErrorMessages.ToArray(); }
	public SortedDictionary<uint, byte> GetAddressByteSorted() { return AddressByteSorted; }
	
    List<uint> Addresses = new List<uint>();
    List<byte> Bytes = new List<byte>();
    List<byte> data = new List<byte>();
	SortedDictionary<uint, byte> AddressByteSorted = new SortedDictionary<uint, byte>();
	
    public class SRECline
    {
        public enum RecordType
        {
            HeaderRecordS0       = 0,
        	DataRecord16         = 1,
        	DataRecord24         = 2,
        	DataRecord32         = 3,
        	ReservedRecord       = 4,
        	CountRecord16        = 5,
        	CountRecord24        = 6,
        	StartAddressRecord32 = 7,
        	StartAddressRecord24 = 8,
        	StartAddressRecord16 = 9,
            EndOfFile = 9,

        }

        public SRECline(string s, int ln)
        {
			
            startSymbol = s[0];

			if (startSymbol != 'S') LineErrorMessages.Add("В начале строки " + ln + " отсутствует символ 's'");
            s = s.Substring(1);
            
            try{
				recordtypes = int.Parse(s.Substring(0, 1), System.Globalization.NumberStyles.HexNumber);
				recordtype = (RecordType)recordtypes;
				s = s.Substring(1);
			}
			catch (Exception ex){
				LineErrorMessages.Add("В строке " + ln + " " + ex.Message);
				CriticalErrors = true;
			}  
            try{
				length = uint.Parse(s.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
				s = s.Substring(2);
			}
			catch (Exception ex){
				//ErrorMessages = ex.Message;
				LineErrorMessages.Add("В строке " + ln + " " + ex.Message);
				CriticalErrors = true;
			}
            try{
				address = uint.Parse(s.Substring(0, recordtypes*2+2), System.Globalization.NumberStyles.HexNumber);
				s = s.Substring(recordtypes*2+2);
			}
			catch (Exception ex){
				//ErrorMessages = ex.Message;
				LineErrorMessages.Add("В строке " + ln + " " + ex.Message);
				CriticalErrors = true;
			}

				data = new byte[length-(1+recordtypes*2)];
				for (int i = 0; i < length-(1+recordtypes*2); i++)
				{
					
            		try{
						data[i] = byte.Parse(s.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
						s = s.Substring(2);
					}
					catch (Exception ex){
						LineErrorMessages.Add("В строке " + ln + " " + ex.Message);
						CriticalErrors = true;
					}
				}
            try{
				checksum = byte.Parse(s.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
				s = s.Substring(2);
			}
			catch (Exception ex){
				//ErrorMessages = ex.Message;
				LineErrorMessages.Add("В строке " + ln + " " + ex.Message);
				CriticalErrors = true;
			}	
			for (int i = 0; i < length-(1+recordtypes*2); i++){
					bytes += data[i];
			}
			if(recordtype == RecordType.DataRecord16) {
				if ((byte)(length + (byte)(address>>8) + (byte)address + (byte)bytes + checksum) != 0xFF) 
					LineErrorMessages.Add("В строке " + ln + " не совпадает контрольная сумма");
			}
			if(recordtype == RecordType.DataRecord24) {
				if ((byte)(length + (byte)(address>>16) + (byte)(address>>8) + (byte)address + (byte)bytes + checksum) != 0xFF) 
					LineErrorMessages.Add("В строке " + ln + " не совпадает контрольная сумма");
			}
			if(recordtype == RecordType.DataRecord32) {
				if ((byte)(length + (byte)(address>>24) + (byte)(address>>16) + (byte)(address>>8) + (byte)address + (byte)bytes + checksum) != 0xFF) 
					LineErrorMessages.Add("В строке " + ln + " не совпадает контрольная сумма");
			}
        }


        char startSymbol;
        uint length;
        public uint address;
        public RecordType recordtype;
		public int recordtypes;
		public int recordCount;
        public  byte[] data;
		public SortedDictionary<uint, byte> AddressByteSorted;
        byte checksum;
		byte bytes = 0;
		public List<string> LineErrorMessages = new List<string>();
		public bool CriticalErrors = false;

    }
    //public string[] ErrorMessage;
	public List<string> FileErrorMessages = new List<string>();
	public string ErrorMessage ="";
	public bool CriticalError = false;
	public uint minAddress = uint.MinValue;
	public uint maxAddress = uint.MinValue;
} // class MotorolaSRECfile
// MotorolaSRECfile.cs
