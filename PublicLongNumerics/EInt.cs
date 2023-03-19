

#nullable enable

namespace LongNumerics
{
    //
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;
    using System.Xml;



    /// <summary>
    /// This type provides unsigned integers of unlimited length, designed for modulo arithmetic e.g. 
    /// used in cryptography or any other algorithms requiring very long unsigned integers. The value-type 
    /// structure provides various math, casting, conversion methods. It is used in a similar way to the 
    /// uint-type and appears in many ways (with some explicit casting) as 'built-in'.
    /// </summary>
    /// <Copy_Rights>All Copy Rights reserved by Jürg Werner, Licence X11 (X11/MIT-Consortium)</Copy_Rights>
    /// <additional_info>
    /// This class contains the properties, object bound methods, static operators, explicit casting
    /// operators and exceptions. Static methods are to be found in helper classes 'EIntCon' for conversion 
    /// centric static methods and 'EIntMath for calculation centric static methods.
    /// Most methods are designed for not nullable Arguments, to keep the performance up.
    /// </additional_info>
    /// 
    public struct EInt : ICloneable, IComparable //, IEnumerable
    {
        /// <summary>
        /// Copy Rights reserved
        /// ********************
        /// </summary>
        public static string CopyRights()
        {
            string s = "Copyright (C) <19.3.2023> <Dr. Juerg Werner, Hedingen, Switzerland, juerg.werner@bluewin.ch>\r\n\r\nPermission is hereby granted," +
                        "free of charge, to any person obtaining a copy of this software and associated documentation files (the \"Software\"), to deal in" +
                        "the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, " +
                        "sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the" +
                        " following conditions:\r\n\r\nThe above copyright notice and this permission notice shall be included in all copies or " +
                        "substantial portions of the Software.\r\n\r\nTHE SOFTWARE IS PROVIDED \"AS IS\", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR " +
                        "IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. " +
                        "IN NO EVENT SHALL THE X CONSORTIUM BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR" +
                        " OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.\r\n\r\nExcept " +
                        "as contained in this notice, the name of <Dr. Juerg Werner> shall not be used in advertising or otherwise to promote the sale, " +
                        "use or other dealings in this Software without prior written authorization from <Dr. Jürg Werner,Hedingen, Switzerland" +
                        "juerg.werner@bluewin.ch>.";
            return s;
        }

        /* ----------------------------------------------------------------------------------------------
         * Start of Properties:
         * ---------------------------------------------------------------------------------------------- */

        // Backing-field:
        private UInt32[] _xuint;
        /// <summary>
        /// The UInt32-Array Xuint holds number (big endian, highest array-index: MSW).
        /// backing field is UInt32[] Xuint.
        /// </summary>
        /// 
        public UInt32[] Xuint
        {
            get => _xuint;

            set => _xuint = value;

        }



        // Backing-field implicit:
        /// <summary>
        /// This string represents the identifier of the number. It is initialized to "default".
        /// </summary>
        /// 
        public string ObjectID { get; set; } = "default";



        // Backing-field:
        private int _messageByteLength = 0;
        /// <summary>
        /// _messageByteLength is the number of bytes foreseen for the message. It does not necessarily 
        /// reflect the actual number of bytes used. This property is passed along by casting, cloning and 
        /// conversion. This property is initialized in the constructor and only set when needed.
        /// </summary>
        /// 
        public int MessageByteLength
        {
            get => _messageByteLength;
            set => _messageByteLength = (value >= 0) ? value : 0;
        }




        // 'Calculated' properties, without dedicated backing-field:
        // ---------------------------------------------------------


        /// <summary>
        /// Returns the order of the most significant bit actually set (zero based).
        /// </summary>
        /// 
        public int Order
        {
            get => EIntMath.GetArrayOrder(_xuint);
        }


        /// <summary>
        /// Returns the minimum number of bytes needed to store _xuint.
        /// </summary>
        /// 
        public int ByteSize
        {
            get => ((Order + 1) % 8 > 0) ? 1 + (Order + 1) / 8 : (Order + 1) / 8;     // Order is zero-based     
        }

        /// <summary>
        /// Returns the actual bit-size, leading 0´s - except first bit - not counted, minimum is 1.
        /// </summary>
        public int BitSize
        {
            get => Order + 1;
        }



        /// <summary>
        /// Returns number of 32bit words actually used (incl. leading zeros).
        /// It is also the length of the indexer.
        /// </summary>
        /// 
        public int Count
        {
            get => _xuint.Length;
        }

        /// <summary>
        /// returns SHA256 hash-code of EInt
        /// </summary>
        /// 
        public EInt SHA256
        {
            get => EIntMath.SHA256(this, 8*ByteSize);
        }

        /* ---------------------------------------------------------------------------------------------- 
         * End of Properties
         * ---------------------------------------------------------------------------------------------- */





        /* ----------------------------------------------------------------------------------------------
         * Start of Fields:
         * ---------------------------------------------------------------------------------------------- */


        /// <summary>
        /// _lswMask UInt32 mask with all bits set.
        /// </summary>
        /// 
        public const UInt32 _lswMask = 0xffffffff;



        /// <summary>
        /// readonly EInt("1")
        /// </summary>
        /// 
        public static readonly EInt eins = new("1");



        /// <summary>
        /// List with witnesse bases for Miller Rabin
        /// </summary>
        public static readonly List<EInt> _baseList = new()
        {
            new EInt("2"),
            new EInt("3"),
            new EInt("5"),
            new EInt("7"),
            new EInt("B"),
            new EInt("D"),
            new EInt("11"),
            new EInt("13"),
            new EInt("17"),
            new EInt("1D"),
            new EInt("1F"),
            new EInt("25"),
            new EInt("29"),
            EIntCon.DecStringToEInt("671")
        };  // base list with witnesses

        /* ----------------------------------------------------------------------------------------------
         * End of Fields:
         * ---------------------------------------------------------------------------------------------- */





        /* ----------------------------------------------------------------------------------------------
         * Start of Constructors:
         * ---------------------------------------------------------------------------------------------- */


        /// <summary>
        /// Master-Constructor of EInt. Value = parsed decimal string.
        /// (all non-decimal chars are ignored)
        /// </summary>
        /// <param name="numberString">input string representing the decimal Number</param>
        /// <param name="numberSys">'d','D' for decimal or 'h', 'H' for hexadecimal</param>
        /// <param name="Id">ObjectId</param>
        /// <exception cref="EInt_ArgIsNullException">thrown if one ore more of the calling parameters is/are Null</exception>
        /// <exception cref="NotImplementedException">thrown if char numberSys is not implemented</exception>
        /// 
        public EInt(string? numberString, char? numberSys, string? Id)
        {
            // trow Exception if arguments are Null:
            if (numberString == null) throw new EInt_ArgIsNullException($"EInt_ArgIsNullException: {nameof(numberString)} is Null");
            if (numberSys == null) throw new EInt_ArgIsNullException($"EInt_ArgIsNullException: {nameof(numberSys)} is Null");
            if (Id == null) throw new EInt_ArgIsNullException($"EInt_ArgIsNullException: {nameof(Id)} is Null");

            // all input parameters are not Null at this point:
            this.ObjectID = Id;
            switch (numberSys)
            {

                case 'd':
                case 'D':
                    string _s;
                    char _singleChar;
                    List<EInt> ziffer = new()
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

                    if (numberString == "") numberString = "0";       // empty string = number 0 by default

                    _s = numberString;
                    int _x = 0;

                    // parse valid Chars
                    while (_x < _s.Length)
                    {
                        _singleChar = _s[_x];

                        if (EIntCon.DecChar.Contains(_singleChar))
                            _x++;
                        else
                            _s = _s.Remove(_x, 1);

                        if (_s == "")
                        {
                            _s = "0";
                            break;
                        }
                    }

                    EInt _number = new("0");
                    string _oneCharString;
                    int _index;
                    for (int i = 0; i < _s.Length; i++)
                    {
                        _oneCharString = Convert.ToString(_s[i]);
                        _index = Convert.ToInt32(_oneCharString);
                        _number.Mul(ziffer[10]);                              // Multiply by hex-A (decimal 10)
                        _number.Add(ziffer[_index]);                          // Add  digit
                    }

                    this._xuint = _number.Xuint;
                    break;

                case 'h':
                case 'H':
                    this._xuint = EIntCon.ConvertStringToExtendedIntArray(numberString);

                    break;

                default:
                    throw new NotImplementedException($"Master Constructor EInt: '{numberSys}' not implemented");
            }
            this.Trim();
            this._messageByteLength = Xuint.Length * 4;

        }




        /// <summary>
        /// Constructor of structure EINT. Value = parsed Hex-string
        /// </summary>
        /// <param name="inString"> Hex-String representing the number (string in usual notation, MS-Hex left, little endian).</param>
        /// <param name="Id"> string Id is ObjectId, default "not named"</param>
        /// 
        public EInt(string? inString, string? Id = "not named") : this(inString, 'h', Id) { }



        /// <summary>
        /// Constructor of structure EInt, Value = 0, n-words, ObjectID: "not named"
        /// </summary>
        /// <param name="n">length of array Xuint</param>
        /// 
        public EInt(int n)
        {
            this._xuint = new UInt32[n];                                       // instantiate Xuint-array of length n
            this.ObjectID = "not named";
            this.MessageByteLength = n * 4;
        }



        /// <summary>
        /// Empty constructor
        /// </summary>
        /// 
        public EInt() : this("0", 'h', "not named") { }

        /* ---------------------------------------------------------------------------------------------- 
         * End of Constructors 
         * ---------------------------------------------------------------------------------------------- */




        // ----------------------------------------------------------------------------------------------
        // Start of Interfaces
        // ----------------------------------------------------------------------------------------------


        /// <summary>
        /// Implementation of Clone-Interface performs new instantiation and deep-copy
        /// </summary>
        /// <returns>Clone (object-type, to be casted to the concrete type)</returns>
        /// 
        public object Clone()
        {
            EInt _copy = (EInt)this.MemberwiseClone();              // shallow copy
            _copy.Xuint = (UInt32[])this.Xuint.Clone();               // Array copy

            return _copy;
        }



        /// <summary>
        /// Implementation of Indexer
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        /// 
        public UInt32 this[int index]
        {
            get => _xuint[index];
        }


      
        
       // IEnumerator IEnumerable.GetEnumerator() => _xuint.GetEnumerator();

        /*
        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns>An IEnumerator.</returns>
        IEnumerator<uint> IEnumerable<uint>.GetEnumerator()
        {
            return (IEnumerator<uint>)_xuint.GetEnumerator();
        }
        */


        /// <summary>
        /// Implements IComparable.CompareTo
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">If parameter is not EInt</exception>
        /// <exception cref="ArgumentNullException">If parameter is Null</exception>
        /// 
        int IComparable.CompareTo(object? obj)
        {
            if (obj != null)
            {
                if (obj is EInt _temp)
                {


                    if (this == _temp) return 0;
                    if (this > _temp) return 1;
                    return -1;
                }
                throw new ArgumentException("Parameter is not EInt");
            }
            throw new ArgumentNullException(nameof(obj));
        }

        /* ----------------------------------------------------------------------------------------------
         * End of Interfaces
         * ---------------------------------------------------------------------------------------------- */




        /* ----------------------------------------------------------------------------------------------
         * Static Operators
         * ---------------------------------------------------------------------------------------------- */

        //  Static Operators:

        //  Math operators:

        /// <summary>
        /// performs addition of EInts
        /// </summary>
        /// <param name="addend1"></param>
        /// <param name="addend2"></param>
        /// <returns>sum</returns>
        /// 
        public static EInt operator +(EInt addend1, EInt addend2)
        {
            addend1 = (EInt)addend1.Clone();
            addend1.Add(addend2);
            return addend1;
        }



        /// <summary>
        /// performs simple subtraction (must be positiv, otherwise exception is thrown)
        /// </summary>
        /// <param name="minuend"></param>
        /// <param name="subtrahend"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"> if result would be negative </exception>
        public static EInt operator -(EInt minuend, EInt subtrahend)
        {
            if (minuend.IsLessThan(subtrahend)) throw new ArgumentException("result would be minus");
            else
            {
                minuend = (EInt)minuend.Clone();
                subtrahend = (EInt)subtrahend.Clone();
                minuend.Sub(subtrahend);    
                return minuend;
            }
        }

        /// <summary>
        /// performs subtraction modulo modValue
        /// </summary>
        /// <param name="minuend">minuend of subtraction</param>
        /// <param name="minMod">(subtrahend, value of modulo) </param>
        /// <returns> difference minuend - subtrahend modulo modValue </returns>
        /// 
        public static EInt operator -(EInt minuend, (EInt subtrahend, EInt modValue) minMod)
        {
            minuend = (EInt)minuend.Clone();
            minuend.Sub(minMod.subtrahend, minMod.modValue);    // trimming is done in sub
            return minuend;
        }



        /// <summary>
        /// performs multiplication of EInt'_s
        /// </summary>
        /// <param name="fac1">factor 1</param>
        /// <param name="fac2">factor 2</param>
        /// <returns>product</returns>
        /// 
        public static EInt operator *(EInt fac1, EInt fac2)
        {
            EInt _copy = (EInt)fac1.Clone();
            _copy.Mul(fac2);
            return _copy;

        }



        /// <summary>
        /// perform division of EInts
        /// </summary>
        /// <param name="dividend"></param>
        /// <param name="divisor"></param>
        /// <returns>_ratio</returns>
        /// 
        public static EInt operator /(EInt dividend, EInt divisor)
        {
            EInt _ratio;
            dividend = (EInt)dividend.Clone();
            _ratio = dividend.Div(divisor);
            return _ratio;
        }



        /// <summary>
        /// performs modulo operation
        /// </summary>
        /// <param name="operand"></param>
        /// <param name="modulo"></param>
        /// <returns>operand mod modulo</returns>
        /// 
        public static EInt operator %(EInt operand, EInt modulo)
        {
            operand = (EInt)operand.Clone();
            operand.Div(modulo, true);
            return operand;
        }




        // Inc, Dec- Operators:

        /// <summary>
        /// Implementation increment operator
        /// </summary>
        /// <param name="operand"></param>
        /// <returns>operand++</returns>
        public static EInt operator ++(EInt operand)
        {
            operand.Add(eins);
            return operand;
        }


        /// <summary>
        /// Implementation of decrement operator
        /// </summary>
        /// <param name="operand"></param>
        /// <returns>operand--EInt</returns>
        /// <exception> if operand is zero </exception>
        /// 
        public static EInt operator --(EInt operand)
        {
            if (operand.IsZero())
                throw new Exception("Decrement not possible, EInt is zero");
            _ = operand.Sub(eins);
            return operand;
        }


        //  Logical Operators:

        /// <summary>
        /// performs comparison of values left and right: greater than
        /// </summary>
        /// <param name="left"> EInt left </param>
        /// <param name="right"> EInt right </param>
        /// <returns>bool 'true' if left (is greater than)  right, 'false' otherwise </returns>
        /// 
        public static bool operator >(EInt left, EInt right)
        {
            return left.IsGreaterThan(right);
        }



        /// <summary>
        /// performs comparison of values left and right: smaller than
        /// </summary>
        /// <param name="left"> EInt left </param>
        /// <param name="right"> EInt right </param>
        /// <returns> 'true' if left (is smaller than) right, 'false' otherwise </returns>
        /// 
        public static bool operator <(EInt left, EInt right)
        {
            return left.IsLessThan(right);
        }



        /// <summary>
        /// performs comparison of values left and right: greater-equal
        /// </summary>
        /// <param name="left"> EInt left </param>
        /// <param name="right"> EInt right </param>
        /// <returns> 'true' if left (is greater or equal) right, 'false' otherwise </returns>
        /// 
        public static bool operator >=(EInt left, EInt right)
        {
            return !left.IsLessThan(right);
        }



        /// <summary>
        /// performs comparison of values left and right: smaller-equal
        /// </summary>
        /// <param name="left"> EInt left </param>
        /// <param name="right"> EInt right </param>
        /// <returns>bool 'true' if left (smaller or equal) right, 'false' otherwise</returns>
        /// 
        public static bool operator <=(EInt left, EInt right)
        {
            return !left.IsGreaterThan(right);
        }


        /// <summary>
        /// performs comparison of values left and right (this.Xuint)
        /// </summary>
        /// <param name="left">EInt left to compare</param>
        /// <param name="right">with EInt right</param>
        /// <returns>bool 'true' if left == right, 'false' otherwise</returns>
        /// 
        public static bool operator ==(EInt left, EInt right)
        {
            return left.IsEqual(right);
        }


        /// <summary>
        /// performs comparison of values left and right: not-equal
        /// </summary>
        /// <param name="left"> EInt left </param>
        /// <param name="right"> EInt right </param>
        /// <returns> 'true' if left (is not equal) right, 'false' otherwise</returns>
        /// 
        public static bool operator !=(EInt left, EInt right)
        {
            return !left.IsEqual(right);
        }



        //Bitwise operators: 

        /// <summary>
        /// performs bitwise shift of the value to the right (to lower order), a new instance with appropriate length is returned
        /// </summary>
        /// <param name="numbrerToBeShifted"> number to be shifted (is not affected by the operation) </param>
        /// <param name="shift"> no of "shifts" right if positive, left otherwise</param>
        /// <returns> shifted new instance </returns>
        /// 
        public static EInt operator >>(EInt numbrerToBeShifted, int shift)
        {
            EInt _result = (EInt)numbrerToBeShifted.Clone();
            _result.ShiftEintLeftRight(-shift, true);
            return _result;
        }



        /// <summary>
        /// performs bitwise shift of the value to the left (to higher order), a new instance with appropriate length is returned
        /// </summary>
        /// <param name="numbrerToBeShifted"> number to be shifted (is not affected by the operation) </param>
        /// <param name="shift"> no of "shifts" left if positive, right otherwise</param>
        /// <returns> shifted new instance </returns>
        /// 
        public static EInt operator <<(EInt numbrerToBeShifted, int shift)
        {
            EInt _result = (EInt)numbrerToBeShifted.Clone();
            _result.ShiftEintLeftRight(shift, true);
            return _result;
        }



        /// <summary>
        /// performs XOR-operation (bitwise)
        /// </summary>
        /// <param name="eint1"></param>
        /// <param name="eint2"></param>
        /// <returns>EInt eint1 (XOR) eint2 </returns>
        /// 
        public static EInt operator ^(EInt eint1, EInt eint2)
        {
            int _maxLen = (eint1.Xuint.Length >= eint2.Xuint.Length) ? eint1.Xuint.Length : eint2.Xuint.Length;
            EInt _exorResult = new(_maxLen);
            UInt32 _word1, _word2;

            for (int i = 0; i < _maxLen; i++)
            {
                _word1 = (i < eint1.Xuint.Length) ? eint1.Xuint[i] : 0;
                _word2 = (i < eint2.Xuint.Length) ? eint2.Xuint[i] : 0;
                _exorResult.Xuint[i] = _word1 ^ _word2;
            }
            return _exorResult;

        }



        /// <summary>
        /// performs Or-operation (bitwise)
        /// </summary>
        /// <param name="eint1"></param>
        /// <param name="eint2"></param>
        /// <returns>EInt eint1 (OR) eint2 </returns>
        /// 
        public static EInt operator |(EInt eint1, EInt eint2)
        {
            int _maxLen = (eint1.Xuint.Length >= eint2.Xuint.Length) ? eint1.Xuint.Length : eint2.Xuint.Length;
            EInt _orResult = new(_maxLen);
            UInt32 _word1, _word2;

            for (int i = 0; i < _maxLen; i++)
            {
                _word1 = (i < eint1.Xuint.Length) ? eint1.Xuint[i] : 0;
                _word2 = (i < eint2.Xuint.Length) ? eint2.Xuint[i] : 0;
                _orResult.Xuint[i] = _word1 | _word2;
            }
            return _orResult;

        }



        /// <summary>
        /// performs And-operation (bitwise)
        /// </summary>
        /// <param name="eint1"></param>
        /// <param name="eint2"></param>
        /// <returns> eint1 (AND) eint2 </returns>
        /// 
        public static EInt operator &(EInt eint1, EInt eint2)
        {
            int _minLen = (eint1.Xuint.Length <= eint2.Xuint.Length) ? eint1.Xuint.Length : eint2.Xuint.Length;
            EInt _andResult = new(_minLen);


            for (int i = 0; i < _minLen; i++)
            {
                _andResult.Xuint[i] = eint1.Xuint[i] & eint2.Xuint[i];
            }
            return _andResult;

        }


        // Conversion Operators:


        /// <summary>
        /// Explicit Conversion to ByteArray (byte[])
        /// </summary>
        /// <param name="eint1"></param>
        /// 
        public static explicit operator byte[](EInt eint1)
        {
            return eint1.ToByteArray();
        }

        /// <summary>
        /// Explicit Conversion from byte array to EInt
        /// </summary>
        /// <param name="bytes"></param>
        /// 
        public static explicit operator EInt(byte[] bytes)
        {
            return EIntCon.ByteArrayToEInt(bytes);
        }


        /// <summary>
        /// Explicit conversion from int (>= 0) to EInt
        /// </summary>
        /// <param name="i"></param>
        /// 
        public static explicit operator EInt(int i)
        {
            if (i < 0)
                throw new Exception("negative number cannot be casted into EInt");
            EInt _i = new(1);
            _i.Xuint[0] = (UInt32)i;
            _i._messageByteLength = 4;
            return _i; ;

        }


        /// <summary>
        /// Explicit conversion from uint to EInt
        /// </summary>
        /// <param name="i"></param>
        /// 
        public static explicit operator EInt(uint i)
        {
            EInt _i = new(1);
            _i.Xuint[0] = (UInt32)i;
            _i._messageByteLength = 4;
            return _i; ;

        }



        /// <summary>
        /// Explicit conversion from ulong to EInt
        /// </summary>
        /// <param name="i"></param>
        /// 
        public static explicit operator EInt(ulong i)
        {
            EInt _i = new(2);
            _i.Xuint[0] = (UInt32)(i & 0xFFFFFFFF);
            _i.Xuint[1] = (UInt32)(i >> 32);
            _i._messageByteLength = 8;
            return _i; ;

        }

        /// <summary>
        /// Convert textCode to PlainText
        /// </summary>
        /// <param name="textCode"></param>
        /// 
        public static explicit operator PlainText(EInt textCode)
        {
            PlainText plainText = new(textCode.ToTextString());
            PlainText t = plainText;
            t.TextID = textCode.ObjectID;
            return t;
        }



        /// <summary>
        /// Conversion of Plaintext to code (EInt)
        /// </summary>
        /// <param name="text"></param>
        /// 
        public static explicit operator EInt(PlainText text)
        {
            EInt result = EIntCon.TextStringToEInt(text.TextString, out _);
            result.ObjectID = text.TextID;
            return result;
        }



        //  End of Static Operators


        /// <summary>
        /// parse string as value of the returned EInt.
        /// 0x or 0X is Hex, 0b or 0B is binary, rest is decimal
        /// </summary>
        /// <param name="_s"></param>
        /// <returns>EInt</returns>
        /// <exception cref="NullReferenceException"></exception>
        public static EInt Parse(string? _s)
        {
            char _numberSys = 'h';
            string preamble = "";
            if (_s == null)
                throw new NullReferenceException($"EInt.Parse, string argument {nameof(_s)} is null");
            if (_s.Length >= 2)
            {
                preamble = _s[0].ToString() + _s[1].ToString();
                switch (preamble)
                {
                    case "0x":
                    case "0X":
                        _numberSys = 'h';
                        break
;
                    case "0b":
                    case "0B":
                        _numberSys = 'b';
                        break;
                    default:
                        _numberSys = 'd';
                        break;
                }
            }
            else
                _numberSys = 'd';

            EInt result = new(_s, _numberSys, "not named");
            return result;
        }

        //  End of static methods




        //  Object related Methods:




        /// <summary>
        /// Otherwise false is returned. If all these are the same and obj is not null, only
        /// then 'true' is returned. In all other cases 'false' is returned.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>bool, result of comparison.</returns>
        /// 
        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            //Check for null and compare run-time types.
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
                return false;
            // Check for equal properties
            else
            {
                EInt other = (EInt)obj;
                bool result = this.IsEqual(other) && (this.ObjectID == other.ObjectID) &&
                    (this.MessageByteLength == other.MessageByteLength);
                return result;
            }
        }


        /// <summary>
        /// Provides HashCode based on all 3 Properties. 
        /// HashCode = HashCode of ToString with all properties.
        /// </summary>
        /// <returns>HashCode</returns>
        /// 
        public override int GetHashCode()
        {
            return (this.ToString(allProperties: true).GetHashCode());
        }




        /// <summary>
        /// This method return true if the value of the object is 0, otherwise false. 
        /// (leading 0'_s and Xuint-length are ignored).
        /// </summary>
        /// <returns>bool if Xuint is == 0</returns>
        /// 
        public bool IsZero()
        {
            bool _zero = true;
            for (int i = this.Xuint.Length - 1; i >= 0; i--)
            {
                if (this.Xuint[i] != 0)
                {
                    _zero = false;
                    break;
                }
            }
            return _zero;
        }




        /// <summary>
        /// This method returns true, if the numerical value of EInt is equal to the other one, 
        /// otherwise, false is returned. Leading 0'_s and Xuint-length are ignored.
        /// </summary>
        /// <param name="other">EInt to be compared to</param>
        /// <returns>bool if values are equal</returns>
        /// 
        public bool IsEqual(EInt other)
        {
            //Compare run-time types.
            if (!this.GetType().Equals(other.GetType())) return false;

            bool _equal = true;
            int _lenThis = EIntMath.GetLenToOrder(this.Xuint);
            int _lenOther = EIntMath.GetLenToOrder(other.Xuint);

            if (_lenThis != _lenOther)
                _equal = false;
            else
            {
                for (int i = 0; i < _lenThis; i++)
                {
                    if (this.Xuint[i] != other.Xuint[i])
                    {
                        _equal = false;
                        break;
                    }
                }
            }
            return _equal;
        }



        /// <summary>
        /// This method returns true, if the numerical value of EInt 'this' is less than the one of the 'other', 
        /// otherwise, false is returned. Leading 0'_s and Xuint-length are ignored.
        /// </summary>
        /// <param name="other">EInt to be compared to</param>
        /// <returns>bool if value this is less than other</returns>
        /// 
        public bool IsLessThan(EInt other)
        {
            bool _lessThan = false;
            int _orderThis = EIntMath.GetArrayOrder(this.Xuint);
            int _orderOther = EIntMath.GetArrayOrder(other.Xuint);
            int _lenThis = EIntMath.GetLenToOrder(this.Xuint);

            if (_orderThis < _orderOther)
                _lessThan = true;
            else if (_orderThis > _orderOther)
                _lessThan = false;

            else                                                                    // both have equal length corresponding to _order
            {
                for (int i = _lenThis - 1; i >= 0; i--)
                {
                    if (this.Xuint[i] < other.Xuint[i])                               // first (highest) non-zero Xuint decides if not equal.
                    {
                        _lessThan = true;
                        return _lessThan;
                    }
                    else if (this.Xuint[i] > other.Xuint[i])
                    {
                        _lessThan = false;
                        return _lessThan;
                    }
                }
            }
            return _lessThan;
        }



        /// <summary>
        /// his method returns true, if the numerical value of EInt 'this' is greater than the one of the 'other', 
        /// otherwise, false is returned. Leading 0'_s and Xuint-length are ignored.
        /// </summary>
        /// <param name="other">EInt to be compared to</param>
        /// <returns>bool if value this is greater than other</returns>
        /// 
        public bool IsGreaterThan(EInt other)
        {
            // This method returns true, if the value of the object is greater than the one of the other,
            // otherwise, false is returned.
            // Special Conditions: leading 0'_s and Xuint-length are ignored

            bool _greaterThan = false;
            int _orderThis = EIntMath.GetArrayOrder(this.Xuint);
            int _orderOther = EIntMath.GetArrayOrder(other.Xuint);
            int _lenThis = EIntMath.GetLenToOrder(this.Xuint);

            if (_orderThis < _orderOther)
                _greaterThan = false;
            else if (_orderThis > _orderOther)
                _greaterThan = true;

            else                                                                    // both have equal length corresponding to _order
            {
                for (int i = _lenThis - 1; i >= 0; i--)
                {
                    if (this.Xuint[i] > other.Xuint[i])                               // first (highest) non-zero Xuint decides if not equal.
                    {
                        _greaterThan = true;
                        return _greaterThan;
                    }
                    else if (this.Xuint[i] < other.Xuint[i])
                    {
                        _greaterThan = false;
                        return _greaterThan;
                    }
                }
            }
            return _greaterThan;
        }





        /// <summary>
        /// This Method assigns a new value to 'this' object according to the Hex-String. Specifics 
        /// given by ConvertToExtended IntArray. The reference and all attributes besides Xuint
        /// remain unchanged.
        /// </summary>
        /// <param name="hexString">Hex value to be assigned</param>
        /// 
        public void Assign(string hexString)
        {
            UInt32[] _newValue = EIntCon.ConvertStringToExtendedIntArray(hexString);
            this.Xuint = _newValue;
            int _order;
            _order = EIntMath.GetArrayOrder(this.Xuint);
            this._messageByteLength = (_order + 1) / 8;
            if ((_order + 1) % 8 != 0) this._messageByteLength++;
        }




        /// <summary>
        /// This Method shifts this EInt by shiftNumberLeft of bit positions to the left (or right for negative values).
        /// The length is adjusted:
        /// - to longer length if needed to provide _space for the shifted number
        /// - to the appropriate smaller length only if trim = true
        /// 
        /// </summary>
        /// <param name="shiftNumberLeft">number of shifts to the left (*2^shiftNumberLeft) if positive, to the right if negative</param>
        /// <param name="trim">true: reduce array to the _order+1, false: no reduction</param>
        /// 
        public void ShiftEintLeftRight(int shiftNumberLeft, bool trim = false)
        {
            int _newOrder = EIntMath.GetArrayOrder(this.Xuint) + shiftNumberLeft;     // calculate _order of shifted Array
            if (_newOrder < 0) _newOrder = 0;                                     // _order >= 0
            int newWordLength = (_newOrder + 1) / 32;                             // calculate Xuint length needed to accommodate
            if ((_newOrder + 1) % 32 > 0) newWordLength++;                       // shifted array
            if (newWordLength <= 0) newWordLength = 1;                          // minimum Xuint length is 1 (value 0 is of _order 0)

            if (newWordLength > this.Xuint.Length)                               // if array 'Xuint' needs to be enlarged:
                Array.Resize(ref this._xuint, newWordLength);                     // resize Xuint

            // array Xuint has now the size needed or larger
            this.Xuint = EIntMath.ShiftLeftRightVector(this.Xuint, shiftNumberLeft);  // shift array

            if ((newWordLength < this.Xuint.Length) && trim)                     // reduce size if smaller is sufficient and
                Array.Resize(ref this._xuint, newWordLength);                     // trim is set true
        }





        /// <summary>
        /// This methods returns a string according to the value of the EInt. The optional parameter 'Format' is 
        /// set to char 'h' by default, representing hex-format. So far only hex(h) and decimal(_d) and binary (b) 
        /// are implemented. If 'newLine' is true, after every fifth 8-Hex-Digit-block or 32nd bit respectively, 
        /// a "\n" is provided in front of the block. Any other format value returns an empty string and a 
        /// message is sent to the console. Exceptions: none 
        /// </summary>
        /// <param name="format"> char 'h' for HEX, '_d' for decimal and 'b' for binary Format representation are implemented </param>
        /// <param name="newLine"> adds new line to the string if true (default = false) </param>
        /// <param name="allProperties"></param>
        /// <returns> returns string representation of Eint </returns>
        /// 
        public string ToString(char format = 'h', bool newLine = false, bool allProperties = false)
        {
            format = Convert.ToChar((format.ToString()).ToLower()); // lower case char
            string _space, _s = "", _header = "";

            if (allProperties)
                _header += $"ObjectID: {this.ObjectID}\nByteLength: {this.MessageByteLength}\n";

            switch (format)
            {
                case 'h':
                    _s = "0x";
                    for (int i = this.Xuint.Length - 1; i >= 0; i--)
                    {
                        if (newLine && (i % 5 == 0))
                            _space = "\n";
                        else
                            _space = "";
                        _s += $"{_space}{this.Xuint[i]:X8} ";
                    }
                    break;

                case 'd':
                    EInt a = (EInt)this.Clone();
                    EInt ten = new("A");
                    EInt aa;
                    do
                    {
                        aa = a.Div(ten, true);
                        int decNumber = (int)a.Xuint[0];
                        _s += decNumber.ToString();
                        a = aa;
                    }
                    while (!a.IsZero());

                    char[] _charArray = _s.ToCharArray();                     // reverse string
                    Array.Reverse(_charArray);
                    _s = new string(_charArray);
                    break;

                case 'b':
                    for (int ii = 32 * this.Xuint.Length - 1; ii >= 0; ii--)
                    {
                        if (EIntMath.CheckArrayBit(this.Xuint, ii))
                            _s += "1";
                        else
                            _s += "0";
                        if (newLine)
                            if ((ii % 32 == 0) && (ii > 0))
                                _s += "\n";
                    }
                    break;

                default:
                    Console.WriteLine($"The Format selector '{format}' is not implemented, return empty string");
                    break;
            }
            return _header + _s;
        }



        /// <summary>
        /// This Method writes an XML File with the attributes:
        /// - name: string ObjectId, 
        /// - value as Hex-String (currently the only representation implemented
        /// Exceptions are caught and the message sent to the Console
        /// </summary>
        /// <param name="fileName">path</param>
        /// <param name="option">format option not implemented yet</param>
        /// 
        public void ToXmlFile(string fileName, char option = 'h')
        {
            try
            {
                switch (option)
                {
                    case 'h':
                        XmlTextWriter xw = new(fileName, new UnicodeEncoding());
                        xw.WriteStartDocument();
                        string eintString = this.ToString(option);
                        int byteLength = this.MessageByteLength;
                        xw.WriteStartElement("EInt");
                        xw.WriteAttributeString("name", this.ObjectID);
                        xw.WriteAttributeString("MessageByteLength", $"{byteLength}");
                        xw.WriteAttributeString($"Hex-Value", eintString);
                        xw.WriteEndElement();
                        xw.Close();
                        break;
                    default:
                        Console.WriteLine($"Char {option} not implemented yet, break");
                        break;

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex}");
            }
        }


        /// <summary>
        /// This method provides a string corresponding to coded sequence stored in 'this' EInt
        /// if the Parameter 'revers' is 'true', the _order of the string is reversed.
        /// Exceptions:
        /// - if bitLength is 0 or default: take this._messageByteLength instead
        /// - if bitLength is larger than EInt number can store: cap to 4*Xuint.Length
        /// </summary>
        /// <param name="byteLength"></param>
        /// <param name="reverse"></param>
        /// <returns>PlainText string</returns>
        /// 
        public string ToTextString(int byteLength = 0, bool reverse = false)
        {
            EInt _copy = (EInt)this.Clone();
            if (byteLength == 0)                                        // Exception/default byteLength == 0
                byteLength = _messageByteLength;
            if (byteLength > 4 * _copy.Xuint.Length)                           // Exception byteLenth too big
                byteLength = _copy.Xuint.Length * 4;
            if (byteLength != 0)
            {
                byte[] byteArrayNumber = new byte[byteLength];
                for (int i = 0; i < byteLength; i++)                    // fill EInt number into byte array (_order not reversed)
                {
                    byteArrayNumber[i] = Convert.ToByte(_copy.Xuint[0] & 0xFF);
                    _copy.ShiftEintLeftRight(-8);
                }
                if (!reverse) Array.Reverse(byteArrayNumber);           // normally revers byteArrayNumber, unless 'revers' is set 'true'
                return Encoding.Default.GetString(byteArrayNumber);     // get String according to byteArrayNumber value
            }
            else
                return "";
        }



        /// <summary>
        /// This method returns a byte array corresponding to the value of the EInt 'this.Xuint'. 
        /// The length of the byte-array is either given by the _order of the EInt: Ceiling((_order+1)/8)
        /// or MessageByteLength, which ever is larger. The lsb of the byte-array is the lsb of index 0.
        /// </summary>
        /// <returns>Byte Array</returns>
        /// 
        public byte[] ToByteArray()
        {
            EInt _a = (EInt)this.Clone();
            int _order = EIntMath.GetArrayOrder(this.Xuint);
            int _msgLen, len = (_order + 1) / 8;
            if ((_order + 1) % 8 > 0) len++;
            _msgLen = len;
            if (this.MessageByteLength > len) _msgLen = this.MessageByteLength;

            byte[] result = new byte[_msgLen];

            for (int i = 0; i < len; i++)
            {
                result[i] = (byte)(_a.Xuint[0] & 0xFF);
                _a.ShiftEintLeftRight(-8);
            }
            return result;
        }



        /// <summary>
        /// This method adds the UInt32-Xuint addend to this.Xuint[position], including all carry overs 
        /// to more significant words.
        /// </summary>
        /// <param name="addend">UInt32 to be added</param>
        /// <param name="position">zero-based index, where to add in this.Xuint</param>
        /// 
        public void AddWord(UInt32 addend, int position)
        {
            UInt64 _longAddend, _longSum;

            _longAddend = addend;


            while (_longAddend > 0)
            {
                _longSum = this.Xuint[position] + _longAddend;
                this.Xuint[position] = (UInt32)(_longSum & _lswMask);
                _longAddend = _longSum >> 32;
                position += 1;

            }
        }



        /// <summary>
        /// This Method add the 'addend' to the object: this.Xuint = this.Xuint + addend.Xuint. 
        /// Exceptions: none
        /// </summary>
        /// <param name="addend">Addend</param>
        /// 
        public void Add(EInt addend)
        {
            int _oldLength = this.Xuint.Length;
            int _orderThis, _orderAddend, _newOrder, _newLength;
            int i;

            _orderThis = EIntMath.GetArrayOrder(this.Xuint);
            _orderAddend = EIntMath.GetArrayOrder(addend.Xuint);

            _newOrder = 1 + Math.Max(_orderThis, _orderAddend);
            _newLength = (_newOrder + 1) / 32;
            if ((_newOrder + 1) % 32 > 0) _newLength += 1;

            if (_newLength > _oldLength)
            {
                Array.Resize(ref _xuint, _newLength);

                i = _newLength - 1;
                while (i > _oldLength - 1)
                {
                    this.Xuint[i] = 0;
                    i -= 1;
                }
            }

            for (int j = 0; j < addend.Xuint.Length; j++)
            {
                this.AddWord(addend.Xuint[j], j);
            }
        }



        /// <summary>
        /// as above but additionally: 
        /// if modBase is not 0: modulo by modBase and trimming of leading 0 - Words
        /// </summary>
        /// <param name="addend"></param>
        /// <param name="moduloBase"></param>
        public void Add(EInt addend, EInt moduloBase)
        {
            Add(addend);
            if (!moduloBase.IsZero())
                this.Div(moduloBase);
            this.Trim();
        }



        /// <summary>
        /// This Method subtracts the 'subtrahend' from the object: 
        /// this.Xuint = this.Xuint - addend.Xuint. Exceptions: none
        /// </summary>
        /// <param name="subtrahend">UInt32 to be subtracted</param>
        /// <param name="position">zero-based index, where to subtract</param>
        /// 
        public void SubWord(UInt32 subtrahend, int position)
        {
            UInt64 _longSubtrahend;
            _longSubtrahend = subtrahend;

            while (_longSubtrahend > 0)
            {
                bool _borrow;
                if (this.Xuint[position] < _longSubtrahend)
                    _borrow = true;
                else
                    _borrow = false;

                ulong _longDiff = this.Xuint[position];
                if (_borrow)
                    _longDiff += 0x100000000;
                _longDiff -= _longSubtrahend;

                this.Xuint[position] = (UInt32)(_longDiff & _lswMask);

                if (_borrow)
                    _longSubtrahend = 1;
                else
                    _longSubtrahend = 0;
                position += 1;
            }
        }



        /// <summary>
        /// This method resizes the array Xuint to the necessary length, 
        /// removing leading zero Xuint.
        /// </summary>
        public void Trim()
        {
            int _len = EIntMath.GetLenToOrder(Xuint);

            if (_len < this.Xuint.Length)
            {
                Array.Resize(ref _xuint, _len);
            }
        }




        /// <summary>
        /// This Method subtracts the 'subtrahend' from the object: 
        /// this.Xuint = this.Xuint - subtrahend.Xuint. Returns 
        /// OK = true if no negative error occurred, false otherwise.
        /// </summary>
        /// <param name="subtrahend">EInt to subtract</param>
        /// <param name="trim">if true: do trimming</param>
        /// <returns>bool true: OK, false 'negative ERROR'</returns>
        /// 
        public bool Sub(EInt subtrahend, bool trim = true)
        {
            int _LenSubtrahend = EIntMath.GetLenToOrder(subtrahend.Xuint);
            bool _ok;

            if (this.IsLessThan(subtrahend))
            {
                _ok = false;
                return _ok;
            }
            else
            {
                for (int i = 0; i < _LenSubtrahend; i++)
                    this.SubWord(subtrahend.Xuint[i], i);

                if (trim)
                    this.Trim();

                _ok = true;
                return _ok;
            }
        }




        /// <summary>
        /// as Sub but this overload with modulo modBase operation.
        /// </summary>
        /// <param name="subtrahend">EInt to subtract</param>
        /// <param name="modBase">Modulo</param>
        /// 
        public void Sub(EInt subtrahend, EInt modBase)
        {
            bool _ok;
            EInt _sub = (EInt)subtrahend.Clone();
            if (this.IsLessThan(_sub))
            {
                if (_sub.IsGreaterThan(modBase))    // get 0 <= _sub < modBase
                    _sub.Div(modBase);
                if (this.IsLessThan(_sub))          // if 'negative' => modulo
                    this.Add(modBase);
            }

            _ok = this.Sub(_sub);
            this.Div(modBase);
            this.Trim();
        }




        /// <summary>
        /// This Method multiplies the integer of 'this' object with 
        /// the one of the 'factor'-object. Exceptions/specialties: none
        /// </summary>
        /// <param name="factor"></param>
        /// 
        public void Mul(EInt factor)
        {
            int _lenThis = EIntMath.GetLenToOrder(this.Xuint);
            int _lenFactor = EIntMath.GetLenToOrder(factor.Xuint), _position;
            UInt32 _addend;
            UInt64 _longThis, _longFactor, _longProduct;

            int _lenProduct = _lenFactor + _lenThis;
            EInt _product = new(_lenProduct);

            for (int i = 0; i < _lenThis; i++)
            {
                for (int j = 0; j < _lenFactor; j++)
                {
                    _position = i + j;
                    _longThis = this.Xuint[i];
                    _longFactor = factor.Xuint[j];
                    _longProduct = _longThis * _longFactor;
                    _addend = (UInt32)(_longProduct & _lswMask);
                    _product.AddWord(_addend, _position);
                    _longProduct >>= 32;
                    _addend = (UInt32)(_longProduct & _lswMask);
                    if (_addend > 0)
                        _product.AddWord(_addend, _position + 1);
                }
            }
            this.Xuint = _product.Xuint;
            this.Trim();
        }




        /// <summary>
        /// As Mul but this overload with modulo modBase operation and Trim.
        /// </summary>
        /// <param name="factor">factor</param>
        /// <param name="modBase">modulo</param>
        /// 
        public void Mul(EInt factor, EInt modBase)
        {
            this.Mul(factor);
            this.Div(modBase);
            this.Trim();
        }




        /// <summary>
        /// This method performs squaring of EInt 'this' modulo modbase and Trim.
        /// </summary>
        /// <param name="modBase">modulo</param>
        /// 
        public void Sq(EInt modBase)
        {
            EInt _x = (EInt)this.Clone();
            this.Mul(_x, modBase);
            this.Trim();
        }




        /// <summary>
        /// This method performs exponentiation: result = this^_y modulo modBase
        /// </summary>
        /// <param name="y">exponent</param>
        /// <param name="modBase">modulo</param>
        /// <returns>result of exponentiation</returns>
        /// 
        public EInt Exp(EInt y, EInt modBase)
        {
            EInt _result = new("1");
            int _order = EIntMath.GetArrayOrder(y.Xuint);
            bool bitIsSet;

            for (int i = _order; i >= 0; i--)
            {
                bitIsSet = EIntMath.CheckArrayBit(y.Xuint, i);
                _result.Sq(modBase);
                if (bitIsSet)
                    _result.Mul(this, modBase);
            }
            return _result;
        }




        /// <summary>
        /// This Method divides the integer 'this'-object by the one in the 'divisor'-object.
        /// The Ratio EInt is returned an the reminder is stored in 'this'-object.
        /// If this is less than divisor: _ratio = 0, this = remainder
        /// Exception: if divisor = 0 an Exception is thrown.
        /// </summary>
        /// <param name="divisor">divisor</param>
        /// <param name="trim">bool if true trim</param>
        /// <returns>_ratio, (this contains remainder)</returns>
        /// 
        public EInt Div(EInt divisor, bool trim = true)
        {
            EInt _ratio, _nenner;
            int _zaehlerOrder, _nennerOrder, _ratioOrder, _ratioLen;

            if (this.IsLessThan(divisor))
            {
                _ratio = new EInt("0");
                return _ratio;
            }
            else if (divisor.IsZero())
            {
                throw new InvalidOperationException($"Division by Zero: divisor:\n{divisor.ToString()}");
            }
            else
            {
                _zaehlerOrder = EIntMath.GetArrayOrder(this.Xuint);
                _nennerOrder = EIntMath.GetArrayOrder(divisor.Xuint);
                _nenner = new EInt(this.Xuint.Length);
                for (int i = 0; i < divisor.Xuint.Length; i++)
                    _nenner.Xuint[i] = divisor.Xuint[i];


                _ratioOrder = _zaehlerOrder - _nennerOrder;
                _ratioLen = (_ratioOrder + 1) / 32;
                if ((_ratioOrder + 1) % 32 > 0) _ratioLen += 1;
                _ratio = new EInt(_ratioLen);

                _nenner.Xuint = EIntMath.ShiftLeftRightVector(inArray: _nenner.Xuint, nShifts: _ratioOrder);

                for (int i = _ratioOrder; i >= 0; i--)
                {
                    if (!this.IsLessThan(_nenner))
                    {
                        this.Sub(_nenner, false);
                        EIntMath.SetArrayBit(_ratio.Xuint, i);
                    }
                    _nenner.Xuint = EIntMath.ShiftLeftRightVector(inArray: _nenner.Xuint, nShifts: -1);
                }

                if (trim)
                {
                    this.Trim();
                    _ratio.Trim();
                }

                return _ratio;
            }
        }




        /// <summary>
        /// As DIV but everything calculated modulo modBase.
        /// Exceptions: if modBase or divisor is 0 an Exception is thrown.
        /// </summary>
        /// <param name="divisor"></param>
        /// <param name="modbase"></param>
        /// <returns>_ratio, this contains remainder, all modulo modBase</returns>
        /// 
        public EInt Div(EInt divisor, EInt modbase)
        {
            EInt _ratio;
            if (divisor.IsZero() || modbase.IsZero())
            {
                throw new InvalidOperationException($"Division by Zero: divisor:\n{divisor.ToString()}"
                    + $"\nModulo:\n{modbase.ToString()}");
            }
            _ratio = this.Div(divisor, true);
            this.Div(modbase, true);
            _ratio.Div(modbase, true);
            return _ratio;

        }




        /// <summary>
        /// EInt Exception: At least one argument is Null
        /// </summary>
        public class EInt_ArgIsNullException : ApplicationException
        {
            /// <summary>
            /// Constant string describing the cause of the Exception
            /// </summary>
            private const string _causeInit = "At least one of the arguments of the constructor or method is Null!";

            /// <summary>
            /// Property containing the Cause
            /// </summary>
            public string CauseOfException { get; private set; } = _causeInit;


            /// <summary>
            /// Default constructor
            /// </summary>
            public EInt_ArgIsNullException() { }


            /// <summary>
            /// Full constructor
            /// </summary>
            /// <param name="message">detailed user message</param>
            /// <param name="cause">root cause</param>
            public EInt_ArgIsNullException(string message, string cause = _causeInit) : base(message)
            {
                CauseOfException = cause;
            }
        }



        /// <summary>
        /// EInt Exception: Division by Zero attempted
        /// </summary>
        public class EInt_DivByZeroException : ApplicationException
        {
            /// <summary>
            /// Constant string describing the cause of the Exception
            /// </summary>
            private const string _causeInit = "Division by Zero attempted!";

            /// <summary>
            /// Property containing the Cause
            /// </summary>
            public string CauseOfException { get; private set; } = _causeInit;


            /// <summary>
            /// Default constructor
            /// </summary>
            public EInt_DivByZeroException() { }


            /// <summary>
            /// Full constructor
            /// </summary>
            /// <param name="message">detailed user message</param>
            /// <param name="cause">root cause</param>
            public EInt_DivByZeroException(string message, string cause = _causeInit) : base(message)
            {
                CauseOfException = cause;
            }
        }
    }     
}







