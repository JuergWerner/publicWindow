using System.Reflection;
using System.Text;
using System.Xml;

namespace LongNumerics
{
    /// <summary>
    /// Static utility class to perform conversion on/to EInt-Types
    /// </summary>
    public static class EIntCon
    {
        // readonly fields:
        /// <summary>
        /// HexChar is a List of characters that are all the valid Hex chars (upper and lower case).
        /// </summary>
        public static readonly List<char> HexChar = new()
        { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'a',
          'B', 'b', 'C', 'c', 'D', 'd', 'E', 'e', 'F', 'f' };

        /// <summary>
        /// DecChar is a List of characters that are all the valid decimal chars.
        /// </summary>
        public static readonly List<char> DecChar = new() { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };




        //  Static functions, Array:

        /// <summary>
        ///  Convert a number, given by the hexadecimal String 'hexString' into an array Int32[] ('big endian')
        ///  All non-Hex characters are ignored. 
        ///  Special cases: hexString = "" returns 0. if the generaturString = null, a message is sent 
        ///  to the console and UInt32[1] with 0 is returned.
        /// </summary>
        /// <example>UIntegerArray = ConvertHexStringToExtendedIntArray("FE.12345678.AaBbCcDd");</example>
        /// <param name="hexString">Hex-string that is parsed</param>
        /// <returns>UInt32[]</returns>
        /// 
        public static UInt32[] ConvertStringToExtendedIntArray(string hexString)
        {
            string _s, _oneWordString;

            int _nWords, _blockStart, _blockEnd, j;
            UInt32[] _a;
            char _singleChar;

            try
            {
                if (hexString is null) throw new ArgumentNullException(nameof(hexString));

                if (hexString == "") hexString = "0";           // empty string = number 0 by default
                _s = hexString;

                int x = 0;

                // parse valid Chars
                while (x < _s.Length)
                {
                    _singleChar = _s[x];

                    if (HexChar.Contains(_singleChar))
                        x++;
                    else
                        _s = _s.Remove(x, 1);

                    if (_s == "")
                    {
                        _s = "0";
                        break;
                    }
                }

                _nWords = _s.Length / 8;
                if (_s.Length % 8 > 0) _nWords += 1;

                _a = new UInt32[_nWords];                                  // create new instance

                for (int i = 0; i < _nWords; i++)
                {
                    _blockStart = Math.Max(_s.Length - (i + 1) * 8, 0);
                    _blockEnd = Math.Min(_s.Length - i * 8, _s.Length);
                    _oneWordString = "0x";
                    j = 0;

                    while ((j < 8) && (_blockStart + j < _blockEnd))
                    {
                        _oneWordString += _s[_blockStart + j];
                        j += 1;
                    }
                    _a[i] = Convert.ToUInt32(_oneWordString, 16);
                }

                return _a;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hex-string null, return 0:\nException: {ex}\n");
                return new UInt32[1];
            }
        }




        // Static functions, string <> EInt conversion:



        /// <summary>
        /// Convert a number given by the numberString into an EInt.
        /// All non-Dec characters are ignored. Special cases: hexString = empty:  returns 0; 
        /// if numberString = null: return EInt("0","null") and a message is sent to the console.
        /// </summary>
        /// <param name="decimalString">decimal string that is parsed </param>
        /// <returns>EInt Number generated </returns>
        /// 
        public static EInt DecStringToEInt(string decimalString)
        {
            string _s;
            char _singleChar;
            List<EInt> _ziffer = new()
            {
                new EInt("0"),
                new EInt("1"),
                new EInt("2"),
                new EInt("3"),
                new EInt("4"),
                new EInt("5"),
                new EInt("6"),
                new EInt("7"),
                new EInt("8"),
                new EInt("9"),
                new EInt("A")
            };

            if (decimalString == null)
            {
                Console.WriteLine($"Dec-string null, return EInt(\"0\", \"null\") ");
                return new EInt("0", "null");
            }

            if (decimalString == "") decimalString = "0";        // empty string = number 0 by default

            _s = decimalString;
            int x = 0;

            // parse valid Chars
            while (x < _s.Length)
            {
                _singleChar = _s[x];

                if (DecChar.Contains(_singleChar))
                    x++;
                else
                    _s = _s.Remove(x, 1);

                if (_s == "")
                {
                    _s = "0";
                    break;
                }
            }

            EInt number = new("0");
            string _oneCharString;
            int _index;
            for (int i = 0; i < _s.Length; i++)
            {
                _oneCharString = Convert.ToString(_s[i]);
                _index = Convert.ToInt32(_oneCharString);
                number.Mul(_ziffer[10]);                              // Multiply by hex-A (decimal 10)
                number.Add(_ziffer[_index]);                          // Add  digit
            }
            number.ObjectID = "not named";
            number.MessageByteLength = number.Xuint.Length * 4;
            return number;

        }




        /// <summary>
        /// This static method converts free text into an EInt. Encoding of textString[0] is stored in the 
        /// most significant position ('msp') of the returned EInt, unless parameter 'reverse' 
        /// is true => ('lsp').
        /// Exceptions: textString  == empty EInt("0") and byteLength = 0 are returned, if textString == null 
        /// EInt("0","null") is returned and a message is sent to the console.
        /// </summary>
        /// <param name="textString">string with free text</param>
        /// <param name="byteLengt"> length of encoded text-message in bytes </param>
        /// <param name="reverse">true: reverse coded array Xuint,false: encode textString[0] > Xuint[length-1] (default)</param>
        /// <returns>EInt with encoded text as one number >= 0</returns>
        /// 
        public static EInt TextStringToEInt(string textString, out int byteLengt, bool reverse = false)
        {

            if (textString == null)                                     // Exception textString == null
            {
                byteLengt = 0;
                Console.WriteLine($"Text-string is null:\nreturn 0");
                return new EInt("0", "null");
            }
            else if (textString == "")                                  // Exception textString = ""
            {
                byteLengt = 0;
                Console.WriteLine($"Text-string is empty:\nreturn 0");
                return new EInt("0");
            }
            else
            {
                byte[] _textBytes = Encoding.Default.GetBytes(textString);   // Encode, default setting of encoding
                byteLengt = _textBytes.Length;                           // fill in byte array of length 'byteLength'
                if (reverse) Array.Reverse(_textBytes);    // reverse _order if parameter is set true

                EInt _result = new(1);                                   // fill byte array in EInt, Order reversed
                for (int i = 0; i < byteLengt; i++)
                {
                    _result.ShiftEintLeftRight(8, true);
                    _result.Xuint[0] += _textBytes[i];
                }
                _result.ObjectID = (byteLengt > 0) ? "Text encoded" : "text-string null or empty";
                _result.MessageByteLength = byteLengt;
                return _result;
            }
        }


        //  Static function, EInt <> XML




        /// <summary>
        /// This method reads the attributes of a properly structured XML-file, instantiates EInt 'result', 
        /// fills in the attributes and returns it to the caller.
        /// Exceptions: 
        /// file not found or corrupted: EInt("0","null") is returned, a message sent to the Console.
        /// </summary>
        /// <param name="filename">File-path as string</param>
        /// <returns>EInt with the encoded file content</returns>
        /// 
        public static EInt XmlToEInt(string filename)
        {
            if (!File.Exists(filename))
            {
                Console.WriteLine($"File '{filename}' does not exist, return 0");
                return new EInt("0", "null");
            }
            else
            {
                var _attributeList = new List<string>                       // fill the list with attribute names
                {                                                           // we would like to read ('to-do-list')
                    "name",
                    "MessageByteLength",
                    "HexValue"
                };

                string _identifier = string.Empty, _messageLength = string.Empty, _hexString = string.Empty;
                EInt _result;
                XmlReader xr = new XmlTextReader(filename);
                try
                {
                    while (xr.Read())                                           // read next node until end
                    {
                        if (xr.NodeType == XmlNodeType.Element)                 // node must be 'Element'
                        {
                            if (xr.AttributeCount > 0)                          // are there attributes?
                            {
                                while (xr.MoveToNextAttribute())                // Move to the next attribute
                                {                                               // until all read
                                    if (_attributeList.Count == 0)               // is the anything on the to-do-list?
                                        break;                                  // break if all is found.
                                    else
                                    {
                                        if (_attributeList.Contains(xr.Name))    // if the attribute found is on the 'to-do-list'
                                        {
                                            _attributeList.Remove(xr.Name);      // remove attribute name from the 'to-do-list'
                                            switch (xr.Name)                    // evaluate the values of the attributes
                                            {                                   // the 'to-do-list' in the beginning should be
                                                                                // equal or a subset of this case list
                                                case "name":
                                                    _identifier = xr.Value;
                                                    break;

                                                case "MessageByteLength":
                                                    _messageLength = xr.Value;
                                                    break;

                                                case "HexValue":
                                                    _hexString = xr.Value;
                                                    break;

                                                default:
                                                    string value = $"The attribute named {xr.Name} is not implemented ";
                                                    Console.WriteLine(value);
                                                    break;

                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    xr.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"File exists, but structure is probably corrupted:\n" +
                                      $"Exception: {ex}");
                    return new EInt("0", "null");
                }


                if (_hexString != null)                                          // fill the values found in the attribute of the EInt result
                {                                                               // but only if a valid hex-string for the value of result
                    _result = new EInt(_hexString);                               // has been found

                    if (_messageLength != null)
                        _result.MessageByteLength = Convert.ToInt32(_messageLength);
                    else
                        _result.MessageByteLength = 0;

                    if (_identifier != null)
                        _result.ObjectID = _identifier;
                    else
                        _result.ObjectID = "";

                    Console.WriteLine($"EInt '{_result.ObjectID}' with {_result.MessageByteLength} bytes of message:");
                    Console.WriteLine(_result.ToString());
                    return _result;
                }
                else
                {
                    Console.WriteLine($"File exists, but Hex-String is missing");
                    return new EInt("0", "null");
                }
            }
        }


        //  Static functions EInt <> byte-array:


        /// <summary>
        /// This method converts a byte array into an EInt. The EInt-property 'MessageByteLength' is 
        /// equal the byte-array length or the parameter 'length, which ever is larger.
        /// </summary>
        /// <param name="byteArray">byte array with the large number (big endian)</param>
        /// <param name="length">Desired Length in bytes </param>
        /// <returns>EInt</returns>
        /// 
        public static EInt ByteArrayToEInt(byte[] byteArray, int length = 0)
        {
            int _wordLength = byteArray.Length / 4;
            if (byteArray.Length % 4 > 0) _wordLength++;

            EInt result = new(_wordLength)
            {
                MessageByteLength = byteArray.Length
            };
            if (length > byteArray.Length) result.MessageByteLength = length;

            for (int i = byteArray.Length - 1; i >= 0; i--)
            {
                result.ShiftEintLeftRight(8);
                result.Xuint[0] += byteArray[i];
            }
            result.Trim();
            return result;
        }




        /// <summary>
        /// This extension to the Object Class returns a string with the Object Type-Name and Assembly
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>string = "Type: NAME, Assembly: ASSEMBLY"</returns>
        public static string GetAssemblyName(this object obj)
        {
            string s;
            if (obj != null)
            {
                s = $"Type: {obj.GetType().Name}, Assembly: ";
                s += Assembly.GetAssembly(obj.GetType())?.GetName().Name;

            }
            else
                s = "null";
            return s;
        }






    }
}
