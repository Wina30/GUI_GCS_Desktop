using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.IO.Ports;
using System.IO;
using System.Windows.Forms;
using System.Runtime.InteropServices;

//*********** Aeromodelling and Payload Telemetry Research Group (APTRG) ********************
//
//  This program has been edit by Muhamad Fadhil Abdullah (Telkom University) for 
//  Kompetisi Muatan Roket Indonesia (KOMURIDNO), a payload of sattelite competition in Indonesia (Can Sattelite)  
//  and Kompetisi Balon Atmosfer (KOMBAT) in 2014
//
//*****************************************************************************************
//                           LICENSE INFORMATION
//*****************************************************************************************
//   PCCom.SerialCommunication Version 1.0.0.0
//   Class file for managing serial port communication
//
//   Copyright (C) 2007  
//   Richard L. McCutchen  
//   Email: richard@psychocoder.net
//   Created: 20OCT07
//
//   This program is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General Public License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
//
//   This program is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//   GNU General Public License for more details.
//
//   You should have received a copy of the GNU General Public License
//   along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
//
//*****************************************************************************************
namespace PCComm
{
	class CommunicationManager
	{
		#region Manager Enums
		/// <summary>
		/// enumeration to hold our transmission types
		/// </summary>
		public enum TransmissionType { Text, Hex }

		/// <summary>
		/// enumeration to hold our message types
		/// </summary>
		public enum MessageType { Incoming, Outgoing, Normal, Warning, Error };
		#endregion

		#region Manager Variables
		//property variables
		private string _baudRate = string.Empty;
		private string _parity = string.Empty;
		private string _stopBits = string.Empty;
		private string _dataBits = string.Empty;
		private string _portName = string.Empty;
		private int _gambar = 0;
		private Image kamera;
		private PictureBox hasilgambar;
		private TransmissionType _transType;
		private RichTextBox _displayWindow;
		private RichTextBox _displayWindowforGambar;
		private ListView _displayCopyGambar;
		private TextBox _datagambar;
		private Timer _timergbr;
		private ProgressBar _progBar;
		//global manager variables
		private Color[] MessageColor = { Color.Blue, Color.Green, Color.Black, Color.Orange, Color.Red };
		private SerialPort comPort = new SerialPort();
		#endregion

		#region lainlain
		public static int imageSize;
		public static String fileName;
		public static int pixelSize = 1;
		public static String outputFileName;
		public static Boolean verbrose = true;
		#endregion

		#region Manager Properties
		/// <summary>
		/// Property to hold the BaudRate
		/// of our manager class
		/// </summary>
		public string BaudRate
		{
			get { return _baudRate; }
			set { _baudRate = value; }
		}

		/// <summary>
		/// property to hold the Parity
		/// of our manager class
		/// </summary>
		public string Parity
		{
			get { return _parity; }
			set { _parity = value; }
		}

		/// <summary>
		/// property to hold the StopBits
		/// of our manager class
		/// </summary>
		public string StopBits
		{
			get { return _stopBits; }
			set { _stopBits = value; }
		}

		/// <summary>
		/// property to hold the DataBits
		/// of our manager class
		/// </summary>
		public string DataBits
		{
			get { return _dataBits; }
			set { _dataBits = value; }
		}

		/// <summary>
		/// property to hold the PortName
		/// of our manager class
		/// </summary>
		public string PortName
		{
			get { return _portName; }
			set { _portName = value; }
		}

		/// <summary>
		/// property to hold our TransmissionType
		/// of our manager class
		/// </summary>
		public TransmissionType CurrentTransmissionType
		{
			get { return _transType; }
			set { _transType = value; }
		}

		/// <summary>
		/// property to hold our display window
		/// value
		/// </summary>
		public RichTextBox DisplayWindow
		{
			get { return _displayWindow; }
			set { _displayWindow = value; }
		}
		#endregion

		#region copydatagambar
		public ListView DisplayCopyGambar
		{
			get { return _displayCopyGambar; }
			set { _displayCopyGambar = value; }
		}
		#endregion

		#region DisplayWindowforGambar
		public RichTextBox DisplayWindowforGambar
		{
			get { return _displayWindowforGambar; }
			set { _displayWindowforGambar = value; }
		}
		#endregion

		#region DisplayGambar
		public PictureBox DisplayGambar
		{
			get { return hasilgambar; }
			set { hasilgambar = value; }
		}
		#endregion

		#region gambar
		public int gambar
		{
			get { return _gambar; }
			set { _gambar = value; }
		}
		#endregion

		#region Manager Constructors
		/// <summary>
		/// Constructor to set the properties of our Manager Class
		/// </summary>
		/// <param name="baud">Desired BaudRate</param>
		/// <param name="par">Desired Parity</param>
		/// <param name="sBits">Desired StopBits</param>
		/// <param name="dBits">Desired DataBits</param>
		/// <param name="name">Desired PortName</param>
		public CommunicationManager(string baud, string par, string sBits, string dBits, string name, RichTextBox rtb)
		{
			_baudRate = baud;
			_parity = par;
			_stopBits = sBits;
			_dataBits = dBits;
			_portName = name;
			_displayWindow = rtb;
			//now add an event handler
			comPort.DataReceived += new SerialDataReceivedEventHandler(comPort_DataReceived);
		}

		/// <summary>
		/// Comstructor to set the properties of our
		/// serial port communicator to nothing
		/// </summary>
		public CommunicationManager()
		{
			_baudRate = string.Empty;
			_parity = string.Empty;
			_stopBits = string.Empty;
			_dataBits = string.Empty;
			_portName = "COM3";
			_displayWindow = null;
			//add event handler
			comPort.DataReceived += new SerialDataReceivedEventHandler(comPort_DataReceived);
		}
		#endregion

		#region WriteData
		public void WriteData(string msg)
		{
			switch (CurrentTransmissionType)
			{
				case TransmissionType.Text:
					//first make sure the port is open
					//if its not open then open it
					if (!(comPort.IsOpen == true)) comPort.Open();
					//send the message to the port
					comPort.Write(msg);
					comPort.DiscardInBuffer();

					//display the message
					//DisplayData(MessageType.Outgoing, msg + "\n");
					break;
				case TransmissionType.Hex:
					try
					{
						//convert the message to byte array
						byte[] newMsg = HexToByte(msg);
						//send the message to the port
						comPort.Write(newMsg, 0, newMsg.Length);
						//convert back to hex and display
						//DisplayData(MessageType.Outgoing, ByteToHex(newMsg) + "\n");
					}
					catch (FormatException ex)
					{
						//display error message
						DisplayData(MessageType.Error, ex.Message);
					}
					finally
					{
						_displayWindow.SelectAll();
					}
					break;
				default:
					//first make sure the port is open
					//if its not open then open it
					if (!(comPort.IsOpen == true)) comPort.Open();
					//send the message to the port
					comPort.Write(msg);
					comPort.DiscardInBuffer();
					//display the message
					//DisplayData(MessageType.Outgoing, msg + "\n");
					break;
			}
		}
		#endregion

		#region HexToByte
		/// <summary>
		/// method to convert hex string into a byte array
		/// </summary>
		/// <param name="msg">string to convert</param>
		/// <returns>a byte array</returns>
		private byte[] HexToByte(string msg)
		{
			//remove any spaces from the string
			msg = msg.Replace(" ", "");
			//create a byte array the length of the
			//divided by 2 (Hex is 2 characters in length)
			byte[] comBuffer = new byte[msg.Length / 2];
			//loop through the length of the provided string
			for (int i = 0; i < msg.Length; i += 2)
				//convert each set of 2 characters to a byte
				//and add to the array
				comBuffer[i / 2] = (byte)Convert.ToByte(msg.Substring(i, 2), 16);
			//return the array
			return comBuffer;
		}
		#endregion

		#region ByteToHex
		/// <summary>
		/// method to convert a byte array into a hex string
		/// </summary>
		/// <param name="comByte">byte array to convert</param>
		/// <returns>a hex string</returns>
		private string ByteToHex(byte[] comByte)
		{
			//create a new StringBuilder object
			StringBuilder builder = new StringBuilder(comByte.Length * 3);
			//loop through each byte in the array
			foreach (byte data in comByte)
				//convert the byte to a string and add to the stringbuilder
				builder.Append(Convert.ToString(data, 16).PadLeft(2, '0').PadRight(3, ' '));
			//return the converted value
			return builder.ToString().ToUpper();
		}
		#endregion

		#region DisplayData
		/// <summary>
		/// method to display the data to & from the port
		/// on the screen
		/// </summary>
		/// <param name="type">MessageType of the message</param>
		/// <param name="msg">Message to display</param>
		[STAThread]
		private void DisplayData(MessageType type, string msg)
		{
			_displayWindow.Invoke(new EventHandler(delegate
			{
				_displayWindow.SelectedText = string.Empty;
				//_displayWindow.SelectionFont = new Font(_displayWindow.SelectionFont, FontStyle.Bold);
				//_displayWindow.SelectionColor = MessageColor[(int)type];          
				_displayWindow.AppendText(msg);
				_displayWindow.ScrollToCaret();
			}));
		}
		#endregion

		#region ProgressBar
		public ProgressBar Progressbar
		{
			get { return _progBar; }
			set { _progBar = value; }
		}
		#endregion

		#region DisplayDataGambar
		string[] data;
		string head, gbr, ff;

		private void DisplayDataGambar(MessageType type, string msg)
		{
			//_progBar.Minimum = 0;
			//_progBar.Maximum = 100000;
			int i;
			_displayWindowforGambar.Invoke(new EventHandler(delegate
			{
				_displayWindowforGambar.SelectedText = string.Empty;
				//_displayWindow.SelectionFont = new Font(_displayWindow.SelectionFont, FontStyle.Bold);
				//_displayWindow.SelectionColor = MessageColor[(int)type];          
				_displayWindowforGambar.AppendText(msg);
				_displayWindowforGambar.ScrollToCaret();


				if (_displayWindowforGambar.Text.IndexOf("FFD9") != -1)
				{
					//string tes = Regex.Replace(msg, @"\t|\n|\r|\s", "");
					//byte[] v = stringtobyte(tes);
					//hasilgambar.Image = arraytoimage(v);

					/*string[] aray = _displayWindowforGambar.Lines;
                    string line = aray[aray.Length-2];
                    data = Regex.Split(line, " ");*/
					string replacement = Regex.Replace(_displayWindowforGambar.Text, @"\t|\n|\r|\s", "");

					//head = data[0];
					//ff = data[1];
					//gbr = data[2];
					//_datagambar = ff;

					byte[] datarawgambar = stringtobyte(replacement);
					hasilgambar.Image = byteArrayToImage(datarawgambar);
					MessageBox.Show("Image Ready");
					//_progBar.Value = 100000;
					//_timergbr.Stop();
					//hasilgambar.Image = ConvertToBitmap(datarawgambar);


					//ListViewItem item2 = new ListViewItem(replacement);
					//_displayCopyGambar.Items.Add(item2);
				}

			}));
		}

		/*private Image arraytoimage(byte[] datarawgambar)
        {
            throw new NotImplementedException();
        }*/
		#endregion

		#region comPort_DataReceived
		/// <summary>
		/// method that will be called when theres data waiting in the buffer
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void comPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
		{
			//determine the mode the user selected (binary/string)
			switch (CurrentTransmissionType)
			{
				//user chose string
				case TransmissionType.Text:
					if (_gambar == 0)
					{
						//read data waiting in the buffer
						string msg = comPort.ReadExisting();
						//display the data to the user
						DisplayData(MessageType.Incoming, msg);
					}
					else if (_gambar == 2)
					{
						string msggambar = comPort.ReadExisting();
						//display the data to the user
						DisplayDataGambar(MessageType.Incoming, msggambar);
					}
					break;
				//user chose binary
				case TransmissionType.Hex:
					//retrieve number of bytes in the buffer
					int bytes = comPort.BytesToRead;
					//create a byte array to hold the awaiting data
					byte[] comBuffer = new byte[bytes];
					//read the data and store it
					comPort.Read(comBuffer, 0, bytes);
					//display the data to the user
					DisplayData(MessageType.Incoming, ByteToHex(comBuffer) + "\n");
					break;
				default:
					if (_gambar == 0)
					{
						//read data waiting in the buffer
						string str = comPort.ReadExisting();
						//display the data to the user
						DisplayData(MessageType.Incoming, str + "\n");
						//System.Threading.Thread.Sleep(1000);
					}
					else if (_gambar == 2)
					{
						//System.Threading.Thread.Sleep(10);
						string msggambar = comPort.ReadExisting();
						//display the data to the user
						DisplayDataGambar(MessageType.Incoming, msggambar);
					}
					break;
			}
		}
		#endregion

		#region OpenPort
		public bool OpenPort()
		{
			try
			{
				//first check if the port is already open
				//if its open then close it
				if (comPort.IsOpen == true) comPort.Close();

				//set the properties of our SerialPort Object
				comPort.BaudRate = int.Parse(_baudRate);    //BaudRate
				comPort.DataBits = int.Parse(_dataBits);    //DataBits
				comPort.StopBits = (StopBits)Enum.Parse(typeof(StopBits), _stopBits);    //StopBits
				comPort.Parity = (Parity)Enum.Parse(typeof(Parity), _parity);    //Parity
				comPort.PortName = _portName;   //PortName
												//now open the port
				comPort.Open();
				comPort.DiscardInBuffer();
				comPort.DiscardOutBuffer();
				//comPort = null;
				//display message
				DisplayData(MessageType.Normal, "\n");




				//DisplayData(MessageType.Normal, "Port opened at " + DateTime.Now + "\n");
				//DisplayData(MessageType.Normal, DisplayWindow.Text);
				//return true
				return true;
			}
			catch (Exception ex)
			{
				DisplayData(MessageType.Error, ex.Message);
				return false;
			}
		}
		#endregion

		#region ClosePort
		public bool ClosePort()
		{
			try
			{
				comPort.DiscardOutBuffer();
				comPort.Close();
				//MessageBox.Show("Connection successful");
				return true;
			}
			catch
			{
				MessageBox.Show("Failed. Please reconnect the port and try again.");
				return false;
			}
		}
		#endregion

		#region SetParityValues
		public void SetParityValues(object obj)
		{
			foreach (string str in Enum.GetNames(typeof(Parity)))
			{
				((ComboBox)obj).Items.Add(str);
			}
		}
		#endregion

		#region SetStopBitValues
		public void SetStopBitValues(object obj)
		{
			foreach (string str in Enum.GetNames(typeof(StopBits)))
			{
				((ComboBox)obj).Items.Add(str);
			}

		}
		#endregion

		#region SetPortNameValues
		public void SetPortNameValues(object obj)
		{

			foreach (string str in SerialPort.GetPortNames())
			{
				((ComboBox)obj).Items.Add(str);
			}
		}
		#endregion

		#region isOpen
		public bool isOpen()
		{
			if (comPort.IsOpen == true)
			{
				return true;
				comPort.DiscardInBuffer();
			}
			else
			{
				return false;
			}
		}
		#endregion

		#region StringtoByte
		public byte[] stringtobyte(string strinput)
		{
			int i = 0;
			int x = 0;
			byte[] bytes = new byte[(strinput.Length) / 2];
			while (strinput.Length > i + 1)
			{
				long lgndecimal = Convert.ToInt32(strinput.Substring(i, 2), 16);
				bytes[x] = Convert.ToByte(lgndecimal);
				i = i + 2;
				++x;
			}
			return bytes;
		}
		#endregion

		#region byteArrayToBitmapImage
		private static Bitmap ConvertToBitmap(byte[] imagesSource)
		{
			var imageConverter = new ImageConverter();
			var image = (Image)imageConverter.ConvertFrom(imagesSource);
			return new Bitmap(image);

		}
		#endregion

		#region byteArrayToImage

		public Image byteArrayToImage(byte[] byteArrayIn)
		{
			using (Stream stream = new MemoryStream(byteArrayIn))
			{
				//stream.Write(byteArrayIn, 0, byteArrayIn.Length);
				//return System.Drawing.Bitmap.FromStream(stream, true);
				ImageConverter image = new System.Drawing.ImageConverter();
				Image image2 = image.ConvertFrom(byteArrayIn) as Image;
				return image2;
				//hasilgambar.Image = image2;
			}

			// byteArrayIn = new byte[200 * 200 * 3];
			MemoryStream ms = new System.IO.MemoryStream(byteArrayIn);
			//ms.Seek(0, SeekOrigin.Begin);
			//Image newImage = System.Drawing.Image.FromStream(ms);
			Image returnImage = Image.FromStream(ms);
			return returnImage;
			//Bitmap bmp = new Bitmap(ms);
			//return new Bitmap(bmp);
			//hasilgambar.Image = newImage;
		}
		#endregion

		private string ByteArrayToHexString(byte[] data)
		{
			var hex = new StringBuilder(data.Length * 2);
			foreach (var b in data)
				hex.AppendFormat("{0:x2}", b);
			return hex.ToString();

			StringBuilder sb = new StringBuilder(data.Length * 3);
			foreach (byte b in data)
				sb.Append(Convert.ToString(b, 16).PadLeft(2, '0').PadRight(3, ' '));
			return sb.ToString().ToUpper();
		}

		private byte[] hexToByteArray(string hexString)
		{
			int bytesCount = (hexString.Length) / 2;
			byte[] bytes = new byte[bytesCount];
			for (int x = 0; x < bytesCount; ++x)
			{
				bytes[x] = Convert.ToByte(hexString.Substring(x * 2, 2), 16);
			}
			return bytes;
		}

		public Image hexToImage()
		{
			string hex = "your HexString";
			byte[] imagesByte = hexToByteArray(hex);
			MemoryStream mm = new MemoryStream(imagesByte);
			Image _image = System.Drawing.Image.FromStream(mm);
			return _image;
		}

		#region byteArrayToImage
		int width = 200, height = 200, stride = 3;
		string filepath;
		/*public Image byteArrayToImage(byte[] frame)
        {
            frame = new byte[200 * 200 * 3];

            System.Runtime.InteropServices.GCHandle pinnedArray = GCHandle.Alloc(frame, GCHandleType.Pinned);
            IntPtr pointer = pinnedArray.AddrOfPinnedObject();
            //Bitmap bmp = new Bitmap(width,height,stride, PixelFormat.Format24bppRgb, pointer);
            MemoryStream JPEGStream = new MemoryStream(frame);
            Image returnImage = Image.FromStream(JPEGStream);
            return returnImage;
            //bmp.Save(filepath, System.Drawing.Imaging.ImageFormat.Bmp);
        }*/
		#endregion

		private String IntToHexString(int n, int len)
		{
			char[] ch = new char[len--];
			for (int i = len; i >= 0; i--)
			{
				ch[len - i] = ByteToHexChar((byte)((uint)(n >> 4 * i) & 15));
			}
			return new string(ch);
			//_displayWindowforGambar.Text = 
		}

		private static char ByteToHexChar(byte b)
		{
			if (b < 0 || b > 15)
				throw new Exception("IntToHexChar: input lalala");
			return b < 10 ? (char)(b + 48) : (char)(b = 55);
		}

		private static int HexStringToInt(String str)
		{
			int value = 0;
			for (int i = 0; i < str.Length; i++)
			{
				value += HexCharToInt(str[i]) << ((str.Length - 1 - i) * 4);
			}
			return value;
		}

		private static int HexCharToInt(char ch)
		{
			if (ch < 48 || (ch > 57 && ch < 65) || ch > 70)
				throw new Exception("HexCharToInt: input out of range for Hex value");
			return (ch < 58) ? ch - 48 : ch - 55;
		}

		/*
        public unsafe Image arraytoimage(byte[] tes)//, Bitmap z)
        {
            Bitmap z = new Bitmap(200, 200);
            BitmapData data = z.LockBits(new Rectangle(0, 0, z.Width, z.Height), ImageLockMode.ReadWrite, z.PixelFormat);
            //BitmapData data2 = z.LockBits()
            try
            {
                IntPtr ptr = data.Scan0;
                int bytes = Math.Abs(data.Stride);
                byte[] rgbValues = new byte[bytes];
                //Marshal.Copy(ptr, rgbValues, 0, bytes);
                Marshal.Copy(tes, 0, ptr, bytes);
                return z;
            }
            finally
            {
                z.UnlockBits(data);
            }
        }


        public unsafe byte[] tes(Bitmap c)
        {
            BitmapData data = c.LockBits(new Rectangle(0, 0, c.Width, c.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            int bytecount = data.Stride * c.Height;
            byte[] bmpbyte = new byte[bytecount];
            System.Runtime.InteropServices.Marshal.Copy(data.Scan0, bmpbyte, 0, bytecount);
            c.UnlockBits(data);

            return bmpbyte;
        }
        */
	}
}
