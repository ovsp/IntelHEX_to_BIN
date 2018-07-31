﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

    public class IntelHEXfile
    {
        public IntelHEXfile(string filename)
        {
			if (filename.ToLower().EndsWith(".hex") != true) FileErrorMessages.Add("Файл имеет расширение отличное от hex");
			
			try 
			{
				StreamReader sr = new StreamReader(filename);
				bool eof = false;
				int lineNumber = 0;
				while (!eof)
				{
					lineNumber++;
					HEXline line = new HEXline(sr.ReadLine(), lineNumber);
					//if (ErrorMessage == "") ErrorMessage = line.ErrorMessages;
					FileErrorMessages.AddRange(line.LineErrorMessages);
					if (line.CriticalErrors == true) CriticalError = true;
					
					switch (line.recordtype)
					{
						case HEXline.RecordType.DataRecord:
								//data.AddRange(line.data);
								ulong ij=0;
								foreach( byte bt in line.data)
									{
											AddressByteSorted.Add((ulong)(line.address+ij), line.data[ij]);
											ij++;		
									}
								
							break;

						case HEXline.RecordType.EndOfFile:
							eof = true;
							break;
					}
					if (sr.EndOfStream)
					{
						eof = true;
					}
				}
				
				foreach( KeyValuePair<ulong, byte> dabs in AddressByteSorted )
				{
					Addresses.Add(dabs.Key);
					Bytes.Add(dabs.Value);
				}
				
				for (int i = 0; i < AddressByteSorted.Count; i++)
				{
					if (Addresses[i] < minAddress) minAddress = Addresses[i];
				}
				
				for (int i = 0; i < AddressByteSorted.Count; i++)
				{
					if (Addresses[i] > maxAddress) maxAddress = Addresses[i];
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
        public ulong[] GetAddresses() { return Addresses.ToArray(); }
        public ulong GetMinAddress() { return minAddress; }
        public ulong GetMaxAddress() { return maxAddress; }
        public byte[] GetBytes() { return Bytes.ToArray(); }
        public int GetCount() { return AddressByteSorted.Count; }
        public string[] GetErrorMessages() { return FileErrorMessages.ToArray(); }
		public SortedDictionary<ulong, byte> GetAddressByteSorted() { return AddressByteSorted; }
		
        List<ulong> Addresses = new List<ulong>();
        List<byte> Bytes = new List<byte>();
        List<byte> data = new List<byte>();
		SortedDictionary<ulong, byte> AddressByteSorted = new SortedDictionary<ulong, byte>();
		
        public class HEXline
        {
            public enum RecordType
            {
                DataRecord = 0,
                EndOfFile = 1,
                ExtendedSegmentAddress = 2,
                StartSegmentAddress = 3,
                ExtendedLinearAddress = 4,
                StartLinearAddress = 5
            }

            public HEXline(string s, int ln)
            {
				
                colon = s[0];
				//if (colon != ':') ErrorMessages = "Отсутствует двоеточие в начале одной из строк";
				if (colon != ':') LineErrorMessages.Add("В начале строки " + ln + " отсутствует двоеточие");
                s = s.Substring(1);
                				
                try{
					length = ulong.Parse(s.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
					s = s.Substring(2);
				}
				catch (Exception ex){
					//ErrorMessages = ex.Message;
					LineErrorMessages.Add("В строке " + ln + " " + ex.Message);
					CriticalErrors = true;
				}
                try{
					address = ulong.Parse(s.Substring(0, 4), System.Globalization.NumberStyles.HexNumber);
					s = s.Substring(4);
				}
				catch (Exception ex){
					//ErrorMessages = ex.Message;
					LineErrorMessages.Add("В строке " + ln + " " + ex.Message);
					CriticalErrors = true;
				}
                try{
					recordtypes = int.Parse(s.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
					recordtype = (RecordType)recordtypes;
					s = s.Substring(2);
				}
				catch (Exception ex){
					LineErrorMessages.Add("В строке " + ln + " " + ex.Message);
					CriticalErrors = true;
				}  
					data = new byte[length];
					for (ulong i = 0; i < length; i++)
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
				for (ulong i = 0; i < length; i++){
						bytes += data[i];
				}
				if ((byte)((byte)length + (byte)(address>>8) + (byte)address + (byte)recordtypes + (byte)bytes + (byte)checksum) != 0) 
						LineErrorMessages.Add("В строке " + ln + " не совпадает контрольная сумма");
            }


            char colon;
            ulong length;
            public ulong address;
            public RecordType recordtype;
			public int recordtypes;
            public  byte[] data;
			public SortedDictionary<ulong, byte> AddressByteSorted;
            byte checksum;
			byte bytes = 0;
			public List<string> LineErrorMessages = new List<string>();
			public bool CriticalErrors = false;

        }
	    //public string[] ErrorMessage;
		public List<string> FileErrorMessages = new List<string>();
		public string ErrorMessage ="";
		public bool CriticalError = false;
		public ulong minAddress = ulong.MaxValue;
		public ulong maxAddress = ulong.MinValue;
    }
