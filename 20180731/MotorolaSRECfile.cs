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
				LinesOfFile.Add(line.crntLine);
				
				switch (line.recordtype)
				{
					case SRECline.RecordType.DataRecord16:
					case SRECline.RecordType.DataRecord24:
					case SRECline.RecordType.DataRecord32:
							//data.AddRange(line.data);
							AddressLineSorted.Add(line.address, line.data);
							int ij=0;
							foreach( byte bt in line.data)
								{
									AddressByteSorted.Add((long)(line.address+ij), line.data[ij]);
									Addresses.Add((long)(line.address+ij));
									Bytes.Add(line.data[ij]);
									if (minAddress > (long)(line.address+ij)) minAddress = (long)(line.address+ij);
									if (maxAddress < (long)(line.address+ij)) maxAddress = (long)(line.address+ij);
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
    public long[] GetAddresses() { return Addresses.ToArray(); }
    public long GetMinAddress() { return minAddress; }
    public long GetMaxAddress() { return maxAddress; }
    public byte[] GetBytes() { return Bytes.ToArray(); }
    public string[] GetLinesOfFile() { return LinesOfFile.ToArray(); }
    public int GetCount() { return AddressByteSorted.Count; }
    public string[] GetErrorMessages() { return FileErrorMessages.ToArray(); }
	public SortedDictionary<long, byte> GetAddressByteSorted() { return AddressByteSorted; }
	public SortedDictionary<long, byte[]> GetAddressLineSorted() { return AddressLineSorted; }
	
    List<long> Addresses = new List<long>();
    List<byte> Bytes = new List<byte>();
    List<byte> data = new List<byte>();
    List<string> LinesOfFile = new List<string>();
	SortedDictionary<long, byte> AddressByteSorted = new SortedDictionary<long, byte>();
	SortedDictionary<long, byte[]> AddressLineSorted = new SortedDictionary<long, byte[]>();

	
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
        	crntLine = s;
			
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
				length = long.Parse(s.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
				s = s.Substring(2);
			}
			catch (Exception ex){
				//ErrorMessages = ex.Message;
				LineErrorMessages.Add("В строке " + ln + " " + ex.Message);
				CriticalErrors = true;
			}
            try{
				address = long.Parse(s.Substring(0, recordtypes*2+2), System.Globalization.NumberStyles.HexNumber);
				s = s.Substring(recordtypes*2+2); // s1 2adr+1crc=3; s3 3adr+1crc4; s4 4adr+1crc=4;
			}
			catch (Exception ex){
				//ErrorMessages = ex.Message;
				LineErrorMessages.Add("В строке " + ln + " " + ex.Message);
				CriticalErrors = true;
			}

				data = new byte[length-(1+1+recordtypes)]; // s1 2adr+1crc=3; s3 3adr+1crc4; s4 4adr+1crc=4;
				for (int i = 0; i < length-(1+1+recordtypes); i++)
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
			for (int i = 0; i < length-(1+1+recordtypes); i++){
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

        public string crntLine;
        char startSymbol;
        long length;
        public long address;
        public RecordType recordtype;
		public int recordtypes;
		public int recordCount;
        public  byte[] data;
		public SortedDictionary<long, byte> AddressByteSorted;
		public SortedDictionary<long, byte[]> AddressLineSorted;
		public List<string> LinesOfFile;
        byte checksum;
		byte bytes = 0;
		public List<string> LineErrorMessages = new List<string>();
		public bool CriticalErrors = false;

    }
    //public string[] ErrorMessage;
	public List<string> FileErrorMessages = new List<string>();
	public string ErrorMessage ="";
	public bool CriticalError = false;
	public long minAddress = long.MaxValue;
	public long maxAddress = long.MinValue;
} // class MotorolaSRECfile
// MotorolaSRECfile.cs
