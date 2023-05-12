using System.Security.Cryptography;
using static LongNumerics.EInt;

namespace LongNumerics
{
    /// <summary>
    /// Static utility Class for EInt's.
    /// The methods perform specific math tasks for cryptography
    /// </summary>
    public static class EIntMath
    {
        // Constants:

        /// <summary>
        /// minimum Bit-length per default for GetPrime
        /// </summary>
        public const int MinimumBitLength = 3;

        /// <summary>
        /// Length of SHA256 lenth in bytes
        /// </summary>
        public const int hLen_SHA256 = 32;

        /// <summary>
        /// _k256 UInt32[64]: constants for 64 round in SHA 256
        /// </summary>     
        private static readonly UInt32[] _k256 =
        {
            0x428a2f98, 0x71374491, 0xb5c0fbcf, 0xe9b5dba5, 0x3956c25b, 0x59f111f1, 0x923f82a4, 0xab1c5ed5,
            0xd807aa98, 0x12835b01, 0x243185be, 0x550c7dc3, 0x72be5d74, 0x80deb1fe, 0x9bdc06a7, 0xc19bf174,
            0xe49b69c1, 0xefbe4786, 0x0fc19dc6, 0x240ca1cc, 0x2de92c6f, 0x4a7484aa, 0x5cb0a9dc, 0x76f988da,
            0x983e5152, 0xa831c66d, 0xb00327c8, 0xbf597fc7, 0xc6e00bf3, 0xd5a79147, 0x06ca6351, 0x14292967,
            0x27b70a85, 0x2e1b2138, 0x4d2c6dfc, 0x53380d13, 0x650a7354, 0x766a0abb, 0x81c2c92e, 0x92722c85,
            0xa2bfe8a1, 0xa81a664b, 0xc24b8b70, 0xc76c51a3, 0xd192e819, 0xd6990624, 0xf40e3585, 0x106aa070,
            0x19a4c116, 0x1e376c08, 0x2748774c, 0x34b0bcb5, 0x391c0cb3, 0x4ed8aa4a, 0x5b9cca4f, 0x682e6ff3,
            0x748f82ee, 0x78a5636f, 0x84c87814, 0x8cc70208, 0x90befffa, 0xa4506ceb, 0xbef9a3f7, 0xc67178f2
        };

        // constants:
        /// <summary>
        /// _bitMaskMsb 32-bit Xuint with highest _order bit set only
        /// </summary>
        private const UInt32 _bitMaskMsb   = 0x80000000;

        private const UInt64 _bitMaskMsb64 = 0x8000000000000000;


        // Static Functions, Array Math:
        // -----------------------------

        /// <summary>
        /// This Methods Checks in the array 'inArray' if the bit at position 'bitPosition' is set 
        /// If bitPosition is outside the array, also a false value is returned.
        /// </summary>
        /// <param name="inArray">array to be checked</param>
        /// <param name="bitPosition">position (zero-based)</param>
        /// <returns>true: if set, false: otherwise</returns>
        /// 
        public static bool CheckArrayBit(UInt32[] inArray, int bitPosition)
        {
            int _jr, _jf;
            UInt32 _bitMask;

            _jr = bitPosition % 32;                                              // Bit-Position within the Xuint
            _jf = (bitPosition - _jr) / 32;                                       // Array Element number
            if ((inArray.GetUpperBound(0) >= _jf) && (bitPosition >= 0))
            {
                _bitMask = 1;
                _bitMask <<= _jr;

                if ((inArray[_jf] & _bitMask) > 0)
                    return true;
                else
                    return false;
            }
            else
                return false;
        }




        /// <summary>
        /// This Methods sets in the array 'inArray' the bit at position 'bitPosition' to 1,
        /// if successful, a bool value of True is returned. if bitPosition is outside the array, 
        /// a false value is returned.
        /// </summary>
        /// <param name="inArray">array where the bit should be set</param>
        /// <param name="bitPosition">position (zero-based)</param>
        /// <returns><see langword="true"/>: successful, false: bitPosition outside array</returns>
        /// 
        public static bool SetArrayBit(UInt32[] inArray, int bitPosition)
        {
            int _jr, _jf;
            UInt32 _bitMask;

            _jr = bitPosition % 32;                                              // Bit-Position within the Xuint
            _jf = (bitPosition - _jr) / 32;                                      // Array Element index (zero-based)

            if ((inArray.GetUpperBound(0) >= _jf) && (bitPosition >= 0))
            {
                _bitMask = 1;
                _bitMask <<= _jr;

                inArray[_jf] = inArray[_jf] | _bitMask;                           // set bit

                return true;
            }
            else
                return false;
        }




        /// <summary>
        /// This Methods sets in the array 'inArray' the bit at position 'bitPosition' to 1,
        /// if successful, a bool value of True is returned. if bitPosition is outside the array, 
        /// a false value is returned.
        /// </summary>
        /// <param name="inArray">array where the bit should be set</param>
        /// <param name="bitPosition">position (zero-based)</param>
        /// <returns><see langword="true"/>: successful, false: bitPosition outside array</returns>
        /// 
        public static bool SetArrayBit64BitVector(UInt64[] inArray, int bitPosition)
        {
            int _jr, _jf;
            UInt64 _bitMask;

            _jr = bitPosition % 64;                                              // Bit-Position within the Xuint
            _jf = (bitPosition - _jr) / 64;                                      // Array Element index (zero-based)

            if ((inArray.GetUpperBound(0) >= _jf) && (bitPosition >= 0))
            {
                _bitMask = 1;
                _bitMask <<= _jr;

                inArray[_jf] = inArray[_jf] | _bitMask;                           // set bit

                return true;
            }
            else
                return false;
        }




        /// <summary>
        /// This Method performs a bitwise shift of the array by nShifts to the left (to msb). 
        /// Negative values of nShifts shift to the right (to lsb). inArray is not affected. 
        /// Shifts out-of-the MSb of EInt are ignored, shifts into the LSb of EInt are 0.
        /// Exceptions: none
        /// </summary>
        /// <param name="inArray">Clone of this array is to be shifted</param>
        /// <param name="nShifts">number of shifts to the left (msb) or (if negative) to the right (lsb)</param>
        /// <returns>shifted copy of the array</returns>
        /// 
        public static UInt32[] ShiftLeftRightVector(UInt32[] inArray, int nShifts)
        {
            UInt32[]_inVec = (UInt32[]) inArray.Clone();                       // clone inArray
            bool _carryOver;                                                   // Carry over from one UInt64-Xuint to the other
            bool _carryOverLast;

            if (nShifts > 0)
            {
                for (int i = 0; i < nShifts; i += 1)
                {
                    // Shift left: 
                    _carryOver = false;
                    for (int k = 0; k < _inVec.Length; k++)
                    {
                        _carryOverLast = _carryOver;                            // carry over of the last shift operation
                        if ((_inVec[k] & _bitMaskMsb) > 0) _carryOver = true;    // carry over in this shift operation
                        else _carryOver = false;
                        _inVec[k] = _inVec[k] << 1;                             // * 2
                        if (_carryOverLast) _inVec[k] += 1;                     // add last carry over
                    }
                }
            }

            else if (nShifts < 0)
            {
                for (int i = 0; i < (-nShifts); i += 1)
                {
                    // Shift right: 
                    _carryOver = false;
                    for (int k = _inVec.Length - 1; k >= 0; k -= 1)
                    {
                        _carryOverLast = _carryOver;                             // carry over of the last shift operation
                        if ((_inVec[k] & 1) > 0) _carryOver = true;              // carry over in this shift operation
                        else _carryOver = false;
                        _inVec[k] = _inVec[k] >> 1;                              // shift right (divide by 2)
                        if (_carryOverLast) _inVec[k] = _inVec[k] | _bitMaskMsb;   // add last carry over
                    }
                }
            }
            return _inVec;
        }



        /// <summary>
        /// This Method performs a bitwise shift of the array by nShifts to the left (to msb). 
        /// Negative values of nShifts shift to the right (to lsb). inArray is not affected. 
        /// Shifts out-of-the MSb of EInt are ignored, shifts into the LSb of EInt are 0.
        /// Exceptions: none
        /// </summary>
        /// <param name="inArray">Clone of this array is to be shifted</param>
        /// <param name="nShifts">number of shifts to the left (msb) or (if negative) to the right (lsb)</param>
        /// <returns>shifted copy of the array</returns>
        /// 
        public static UInt64[] ShiftLeftRight64BitVector(UInt64[] inArray, int nShifts)
        {
            UInt64[] _inVec = (UInt64[])inArray.Clone();                       // clone inArray
            bool _carryOver;                                                   // Carry over from one UInt64-Xuint to the other
            bool _carryOverLast;

            if (nShifts > 0)
            {
                for (int i = 0; i < nShifts; i += 1)
                {
                    // Shift left: 
                    _carryOver = false;
                    for (int k = 0; k < _inVec.Length; k++)
                    {
                        _carryOverLast = _carryOver;                            // carry over of the last shift operation
                        if ((_inVec[k] & _bitMaskMsb64) > 0) _carryOver = true;    // carry over in this shift operation
                        else _carryOver = false;
                        _inVec[k] = _inVec[k] << 1;                             // * 2
                        if (_carryOverLast) _inVec[k] += 1;                     // add last carry over
                    }
                }
            }

            else if (nShifts < 0)
            {
                for (int i = 0; i < (-nShifts); i += 1)
                {
                    // Shift right: 
                    _carryOver = false;
                    for (int k = _inVec.Length - 1; k >= 0; k -= 1)
                    {
                        _carryOverLast = _carryOver;                             // carry over of the last shift operation
                        if ((_inVec[k] & 1) > 0) _carryOver = true;              // carry over in this shift operation
                        else _carryOver = false;
                        _inVec[k] = _inVec[k] >> 1;                              // shift right (divide by 2)
                        if (_carryOverLast) _inVec[k] = _inVec[k] | _bitMaskMsb;   // add last carry over
                    }
                }
            }
            return _inVec;
        }






        /// <summary>
        /// Calculates the number of words needed to store the number in an Int32-array.
        /// the returned value is less or equal to the length of the array.
        /// If there are leading 0 array elements, the returned value is less than inArray.Length
        /// special cases: inArray = null: return-value = -1,
        /// </summary>
        /// <param name="inArray">array which minimum length is determined</param>
        /// <returns>minimum length in words, -1 if null</returns>
        /// 
        public static int GetLenToOrder(UInt32[] inArray)
        {
            if (inArray != null)
            {
                int _order, _len;
                _order = GetArrayOrder(inArray);
                _len = (_order + 1) / 32;
                if ((_order + 1) % 32 > 0) _len += 1;

                return _len;
            }
            else
            {
                return -1;
            }
        }



        /// <summary>
        /// returns _order of most significant non-zero bit (counting from Lsb = 0, big endian)
        /// special cases: inArray == null: return value = -1;
        /// </summary>
        /// <param name="inArray">array which minimum length in bits is determined</param>
        /// <returns>int _order of msb, -1 if null</returns>
        /// 
        public static int GetArrayOrder(UInt32[] inArray)
        {
            int i, j, _noOfWords;
            UInt32 _a;

            if (inArray != null)
            {
                // Determine the real Xuint length of the array
                i = inArray.GetUpperBound(0);                       // number of array elements - 1 (0 based)
                while (inArray[i] == 0)                             // determine the number of Xuint without leading 0'_s 
                {
                    if (i > 0) i -= 1;
                    else break;
                }

                _noOfWords = i + 1;                                  // number of words without leading 0'_s

                i = _noOfWords - 1;                                  // back to 0-based
                j = 0;
                _a = inArray[i];
                while (_a != 0)                                      // Determine the position of the msb in the MSW
                {
                    _a >>= 1;
                    j += 1;
                }
                if (j == 0) j += 1;                                 // EInt = 0 does not pass while loop

                return ((_noOfWords - 1) * 32 + (j - 1));            // calculate _order
            }
            else
                return -1;
        }





        // Static Functions, EInt Math:
        // ----------------------------

        // Common Divisor, Primality:


        /// <summary>
        /// This method returns the biggest common divisor of int1 and int2
        /// (not modulo, without calculation Diophantine'_s equations coefficient _s and t)
        /// </summary>
        /// <param name="int1">integer1</param>
        /// <param name="int2">integer2</param>
        /// <returns>gcd</returns>
        /// 
        public static EInt Gcd(EInt int1, EInt int2)
        {
            EInt _big, _small, _newSmall, _result;
            if (int1.IsGreaterThan(int2))
            {
                _big = (EInt)int1.Clone();
                _small = (EInt)int2.Clone();
                _newSmall = (EInt)int2.Clone();
            }
            else
            {
                _big = (EInt)int2.Clone();
                _small = (EInt)int1.Clone();
                _newSmall = (EInt)int1.Clone();
            }
            _result = _small;

            while (!_newSmall.IsZero())
            {
                _result = _small;
                _big.Div(_small);
                _newSmall = _big;
                _big = _small;
                _small = _newSmall;
            }
            return _result;
        }



        /// <summary>
        /// This method performs an Extended Euclidean Algorithm to determine the greatest common divisor (gcd)
        /// of int1 and int2 | modulo modBase. The parameters _s and t of the Diophantine equation: 
        /// gcd(int1, int2) = _s * int1 + t * int2 | modulo modBase, are also calculated. All integers are of the EInt-Type.
        /// Special cases: If int1 and/or int2 == 0 then: _s, t and gcd == EInt("0","null")
        /// </summary>
        /// <param name="int1">integer 1 (EInt)</param>
        /// <param name="int2">integer 2 (EInt)</param>
        /// <param name="modBase">all calculations are done modulo modBase</param>
        /// <returns>tuple (gcd, _s, t)</returns>
        /// 
        public static (EInt gCD, EInt s, EInt t) Eea(EInt int1, EInt int2, EInt modBase)
        {
            EInt _big, _small, _newSmall, _resultGdc;
            EInt _sI, _sIM1, _sIM2, _tI, _tIM1, _tIM2, _q, _z, _resultS, _resultT;

            //Initialization:
            _sIM2 = new EInt("1");
            _sIM1 = new EInt("0");
            _tIM2 = new EInt("0");
            _tIM1 = new EInt("1");
            _tI = new EInt("0");
            _sI = new EInt("0");

            if (int1.IsGreaterThan(int2))
            {
                _big = (EInt)int1.Clone();
                _small = (EInt)int2.Clone();
                _newSmall = (EInt)int2.Clone();
            }
            else
            {
                _big = (EInt)int2.Clone();
                _small = (EInt)int1.Clone();
                _newSmall = (EInt)int1.Clone();
            }
            _resultGdc = _small;

            _resultS = new EInt("0", "null");
            _resultT = new EInt("0", "null");

            while (!_newSmall.IsZero())
            {
                _resultGdc = _small;
                _resultS = _sI;
                _resultT = _tI;

                _q = _big.Div(_small);
                _z = (EInt)_sIM1.Clone();
                _z.Mul(_q);
                _sI = _sIM2;
                _sI.Sub(_z, modBase);

                _z = (EInt)_tIM1.Clone();
                _z.Mul(_q);
                _tI = _tIM2;
                _tI.Sub(_z, modBase);

                _newSmall = _big;
                _big = _small;
                _small = _newSmall;
                _sIM2 = _sIM1;
                _sIM1 = _sI;
                _tIM2 = _tIM1;
                _tIM1 = _tI;
            }

            return (_resultGdc, _resultS, _resultT);
        }




        /// <summary>
        /// This method calculates the multiplicative inverse of 'zahl' modulo modBase.
        /// </summary>
        /// <param name="zahl"></param>
        /// <param name="modBase"></param>
        /// <returns></returns>
        /// 
        public static EInt Inv(EInt? zahl, EInt? modBase)
        {
            if (modBase == null) throw new EInt_ArgIsNullException("Inv: modBase is Null");
            if (zahl == null) throw new EInt_ArgIsNullException("Inv: zahl is Null");
            var (_, _, t) = EIntMath.Eea((EInt)modBase, (EInt)zahl, (EInt)modBase);
            return t;
        }



        /// <summary>
        /// This method performs a 'MillerRabin test on the prime candidate.
        /// </summary>
        /// <param name="primeCand">integer to be test for primality</param>
        /// <returns>true: is probable prime, false: is composite</returns>
        /// 
        public static bool MillerRabinTest(EInt primeCand)
        {
            EInt _eins = new("1");
            EInt _drei = new("3");

            if (primeCand.IsLessThan(_drei)) return false;
            if (primeCand.IsEqual(_drei)) return true;

            // primeCand is > 3 
            // primeCand = 2^_s._d + 1 

            EInt _pM1 = (EInt)primeCand.Clone();
            _pM1.Sub(_eins, true);                                               // _pM1 = primeCand - 1;

            int _s = 0;
            EInt _d = (EInt)_pM1.Clone();                                        // _d = primeCand - 1
            while ((_d.Xuint[0] & 1) == 0)                                       // primeCand = 2^_s . _d  + 1
            {
                _s++;
                _d.Xuint = EIntMath.ShiftLeftRightVector(_d.Xuint, -1);
            }
            if (_s == 0) return false;                                           // primeCand is even, no prime!

            foreach (EInt b in EInt._baseList)                                        // base - Loop
            {
                if (!b.IsLessThan(_pM1))                                         // make sure that base value is < primeCand - 1
                    continue;                                                   // or continue (skip)
                EInt _x = (EInt)b.Clone();
                _x = _x.Exp(_d, primeCand);                                        // _x = b^_d mod primeCand
                if (_x.IsEqual(_eins) || _x.IsEqual(_pM1)) continue;                // continue base-loop

                bool _isPrimeToBase = false;

                for (int i = 1; i < _s; i++)                                     // repeat max. (_s-1)-times
                {
                    _x.Sq(primeCand);                                            // _x: _x^2; i.e. _x = b^( 2^i  * _d)
                    if (_x.IsEqual(_pM1))                                         // if _x is = -1 then break    
                    {
                        _isPrimeToBase = true;
                        break;
                    }
                }
                if (_isPrimeToBase)                                              // check if base b is a witness 
                    continue;                                                   // if so: go to next base
                else
                    return false;                                               // if not: definitely not prime
            }
            return true;                                                        // all base values in the list are witnesses
                                                                                // primeCand is most probably a prime
        }





        /// <summary>
        /// Static Method to calculate a prime number with bit-length of
        /// - exactly 'bitLength', if param 'equalOrSamaller'=false, or
        /// - max.'bitLength' but min.'minBitLength', if param 'equalOrSmaller'=true. 
        /// </summary>
        /// <param name="bitLength">(max.) length of prime number.</param>
        /// <param name="file">Switch if number is stored in a XML-File </param>
        /// <param name="equalOrSmaller">switch, if length is exactly equal or equal smaller.</param>
        /// <param name="minBitLength"></param>
        /// <returns>Prime number of desired length</returns>
        /// 
        public static EInt GetPrime(int bitLength = 128, bool file = false, bool equalOrSmaller = false, int minBitLength = MinimumBitLength)
        {

            if (bitLength < minBitLength) bitLength = minBitLength;

            int _nBytes = bitLength / 8;
            if (bitLength % 8 != 0) _nBytes++;

            EInt _randomEInt;
            int _order, _delta;


            // instantiate cryptographic random generator:
            var _rand = RandomNumberGenerator.Create();
            var _byteArray = new byte[_nBytes];
            bool _primeFound;
            bool _lengtOK;
            // Environment.SpecialFolder.Desktop
            string _path = @"c:\Users\Juerg\Desktop\PrimeNumber";

            do
            {
                // get uneven random number of desired length:
                do
                {
                    _rand.GetBytes(_byteArray);
                    _randomEInt = EIntCon.ByteArrayToEInt(_byteArray, _byteArray.Length);

                    // evaluation of bit-length compared to the desired length:
                    _order = GetArrayOrder(_randomEInt.Xuint);
                    _delta = _order - bitLength + 1;
                    if (equalOrSmaller)
                    {
                        if (_delta > 0) _lengtOK = false;
                        else if (_delta == 0) _lengtOK = true;
                        else
                            _lengtOK = (_order >= minBitLength - 1);
                    }
                    else
                        _lengtOK = _delta == 0;
                }
                while (!_lengtOK || !EIntMath.CheckArrayBit(_randomEInt.Xuint, 0));

                _primeFound = MillerRabinTest(_randomEInt);
            }
            while (!_primeFound);

            // write to file if switch 'file' is set:
            if (file)
                _randomEInt.ToXmlFile(_path + $"{bitLength}");

            return _randomEInt;
        }



        /// <summary>
        /// Static Method to calculate a prime number with bit-length of
        /// - exactly 'bitLength', if param 'equalOrSamaller'=false, or
        /// - max.'bitLength' but min.'minBitLength', if param 'equalOrSmaller'=true. 
        /// Multi-threading is employed if ´multiThreads´== <see langword="true"/>.
        /// </summary>
        /// <param name="multiThreads"></param>
        /// <param name="bitLength"></param>
        /// <param name="equalOrSmaller"></param>
        /// <param name="minBitLength"></param>
        /// <returns>EInt with prime number</returns>
        /// <exception cref="ArgumentException"></exception>
        /// 
        public static EInt GetPrime(bool multiThreads, int bitLength = 128, bool equalOrSmaller = false, int minBitLength = MinimumBitLength)
        {
            int noOfThreads = multiThreads ? (Environment.ProcessorCount > 0 ? Environment.ProcessorCount : 1) : 1;

            if (bitLength < minBitLength) bitLength = minBitLength;

            int _nBytes = bitLength / 8;
            if (bitLength % 8 != 0) _nBytes++;

            // Global Vars
            EInt _randomEInt = (EInt)0;
            bool _primeFound = false;



            // instantiate cryptographic random generator:
            var _rand = RandomNumberGenerator.Create();
            var _byteArray = new byte[_nBytes];
            object threadLockRan = new();
            object threadLockRes = new();

            // if more than one thread available, do multy thread calculation
            if (noOfThreads > 1)
            {
                noOfThreads--;
               

                // Make ´noOfThreads´ threads:
                Thread[] threads = new Thread[noOfThreads];
                for (int j = 0; j < noOfThreads; j++)
                {
                    threads[j] = new Thread(new ParameterizedThreadStart(GetPrimeOneThread))
                    {
                        Priority = ThreadPriority.Highest
                    };
                }

                // instatiate parameter and start threads:
                ThreadParams[] pr = new ThreadParams[noOfThreads];
                for (int j = 0; j < noOfThreads; j++)
                {
                    pr[j] = new ThreadParams(j, equalOrSmaller, bitLength, minBitLength);
                    threads[j].Start(pr[j]);
                }
            }
            //do your own homework in the master thread:
            ThreadParams pp = new(-1, equalOrSmaller, bitLength, minBitLength);
            while (!_primeFound)
            {                
                GetPrimeOneThread(pp);
            }
            return _randomEInt;




            // local Method with direct access to global vars:  _primeFound, _rand, _byteArray,

            void GetPrimeOneThread(object? data)
            {
                if (data is not null && _byteArray is not null && data is ThreadParams)
                {
                    var threadParams = (ThreadParams)data;
                    int _threadNr = threadParams.I;
                    bool _thEqualOrSmaller = threadParams.EqualOrSmaller;
                    int _thBitLength = threadParams.BitLength;
                    int _thMinBitLength = threadParams.MinBitLength;

                    EInt _rn;
                    int _order, _delta;
                    bool _lengthOK = false;
                    bool _prime = false;


                    while (!_primeFound)
                    {


                        // get uneven random number of desired length:
                        do
                        {
                            lock (threadLockRan)
                            {
                                _rand.GetBytes(_byteArray);
                                _rn = EIntCon.ByteArrayToEInt(_byteArray, _byteArray.Length);
                            }


                            // evaluation of bit-length compared to the desired length:
                            _order = GetArrayOrder(_rn.Xuint);
                            _delta = _order - _thBitLength + 1;
                            if (_thEqualOrSmaller)
                            {
                                if (_delta > 0) _lengthOK = false;
                                else if (_delta == 0) _lengthOK = true;
                                else
                                    _lengthOK = (_order >= _thMinBitLength - 1);
                            }
                            else
                                _lengthOK = _delta == 0;
                        }
                        while ((!_lengthOK || !CheckArrayBit(_rn.Xuint, 0)) && !_primeFound);
                        if (_primeFound) return;


                        // uneven random number of suitable length found!

                        // check for primality:
                        _prime = MillerRabinTest(_rn);

                        lock (threadLockRes)
                        {
                            if (!_primeFound && _prime)
                            {
                                // this thread found the first prime, pass it over
                                _randomEInt = _rn;
                                _primeFound = true;
                            }

                        }


                    }
                }
                else
                    throw new ArgumentException("Parameter is not of type ThreadParams or _byteArray is null");
            }
        }



        /// <summary>
        /// Class for encapsulating parameter in a object to pass in the Thread-Delegate
        /// </summary>
        class ThreadParams
        {
            public int I { get; }
            public bool EqualOrSmaller { get; }
            public int BitLength { get; }
            public int MinBitLength { get; }

            public ThreadParams(int ii, bool equalOrSmaller, int bitLength, int minBitLength)
            {
                this.I = ii;
                this.EqualOrSmaller = equalOrSmaller;
                this.BitLength = bitLength;
                MinBitLength = minBitLength;
            }
        }




        //  Static functions, Hash and Crypto:

        /* SHA 256 */

        /// <summary>
        /// This method performs a SHA256 Hatch algorithm.
        /// </summary>
        /// <param name="message">EInt to be hatched</param>
        /// <param name="messageLength">length of the message code in bits(incl. padding)</param>
        /// <returns>hatch code</returns>
        /// 
        public static EInt SHA256(EInt message, int messageLength)

        {

            // messageLenth: Length of Message in number of bits 
            // !! maximum message length is limited to 2**31 - 1 bits in this implementation !! 

            int _noOfMessageBlocks;
            int _noOfMessageBits;
            int _noOfMessageWords;
            UInt32[] _paddedMessage;

            int _k;
            // ensure that the padded message is a multiple of 512 bits long 
            // the message length is stored in the least significant 64 bits 
            // followed by _k-zeros and a '1', followed by the message 

            _noOfMessageBlocks = ((messageLength + 65) % 512 == 0) ? ((messageLength + 65) / 512) : (1 + (messageLength + 65) / 512);
            _k = _noOfMessageBlocks * 512 - 65 - messageLength;


            // calculate length of padded message 
            _noOfMessageBits = messageLength + 1 + _k + 64;
            _noOfMessageWords = _noOfMessageBits / 32;


            // prepare padded message 

            _paddedMessage = new UInt32[_noOfMessageWords];
            for (int i = 0; i < message.Xuint.Length; i++)
            {
                _paddedMessage[i] = message.Xuint[i];
            }

            // make room for padding 1, _k-zeros and 2 words of message length
            // (only lower used) 

            _paddedMessage = EIntMath.ShiftLeftRightVector(_paddedMessage, 1 + _k + 64); // MSB = bit number _noOfMessageBits -1 (zero-based)
            EIntMath.SetArrayBit(_paddedMessage, _k + 64);                               // set bit no. _k + 64 (zero-based)
            _paddedMessage[0] = (UInt32)messageLength;

            // End of preparation of padded message 


            // Instantiate & initialize Variables and Constants 
            UInt32[] _H = new UInt32[8];                                             // instantiate 256-bit Hash vector, H[0] = MSW
            _H[0] = 0x6a09e667;                                                      // initialize H according to FIPS 202, Aug. 2015
            _H[1] = 0xbb67ae85;
            _H[2] = 0x3c6ef372;
            _H[3] = 0xa54ff53a;
            _H[4] = 0x510e527f;
            _H[5] = 0x9b05688c;
            _H[6] = 0x1f83d9ab;
            _H[7] = 0x5be0cd19;

            // Instantiate Message Schedule, initialize in each block cycle 
            UInt32[] _W = new UInt32[64];

            // instantiate 8 working variable for last hash value 
            UInt32 _a, _b, _c, _d, _e, _f, _g, _h;

            // instantiate 2 temporary words 
            UInt32 _T1, _T2;

            // instantiate hashEndresult_256 
            EInt _hashEndresult_256 = new(8);

            for (int i = 1; i <= _noOfMessageBlocks; i++)                            // Message Block Loop (1-based)
            {
                for (int t = 0; t < 64; t++)                                       // prepare Message Schedule
                {
                    if (t < 16)
                    {
                        _W[t] = Mpadded(i, t, _paddedMessage, _noOfMessageBlocks, _noOfMessageWords);
                    }
                    else
                    {

                        _W[t] = Sigma1_256(_W[t - 2]) + _W[t - 7] + Sigma0_256(_W[t - 15]) + _W[t - 16];
                    }
                }

                _a = _H[0];                                                           // initialize working vars with previous hash values
                _b = _H[1];
                _c = _H[2];
                _d = _H[3];
                _e = _H[4];
                _f = _H[5];
                _g = _H[6];
                _h = _H[7];

                for (int t = 0; t < 64; t++)                                        // 64 rounds of bit-mingle
                {
                    _T1 = _h + BigSigma1(_e) + Ch(_e, _f, _g) + _k256[t] + _W[t];
                    _T2 = BigSigma0(_a) + Maj(_a, _b, _c);
                    _h = _g;
                    _g = _f;
                    _f = _e;
                    _e = _d + _T1;
                    _d = _c;
                    _c = _b;
                    _b = _a;
                    _a = _T1 + _T2;
                }

                _H[0] = _a + _H[0];                                                    // calculate new intermediate hash values
                _H[1] = _b + _H[1];
                _H[2] = _c + _H[2];
                _H[3] = _d + _H[3];
                _H[4] = _e + _H[4];
                _H[5] = _f + _H[5];
                _H[6] = _g + _H[6];
                _H[7] = _h + _H[7];

            }                                                                       // End of Message Block Loop

            _hashEndresult_256.Xuint[0] = _H[7];                                       // EInt hashEndresult_256[0] = LSW
            _hashEndresult_256.Xuint[1] = _H[6];
            _hashEndresult_256.Xuint[2] = _H[5];
            _hashEndresult_256.Xuint[3] = _H[4];
            _hashEndresult_256.Xuint[4] = _H[3];
            _hashEndresult_256.Xuint[5] = _H[2];
            _hashEndresult_256.Xuint[6] = _H[1];
            _hashEndresult_256.Xuint[7] = _H[0];                                       // EInt hashEndresult_256[7] = HSW


            return _hashEndresult_256;





            // <summary> SHA256 Sub-function Mpadded </summary>
            // <param name="i"></param>
            // <param name="j"></param>
            // <param name="paddedMessage"></param>
            // <param name="noOfMessageBlocks"></param>
            // <param name="noOfMessageWords"></param>
            // <returns></returns>

            static UInt32 Mpadded(int i, int j, UInt32[] paddedMessage, int noOfMessageBlocks, int noOfMessageWords)
            {
                // i:   is the index of the 512-bit-message block (1-based) )
                // j:   is the index of the 32-bit-Xuint in the 512-bit-message-block  (zero-based)  
                // conversion of 'little endian' (_paddedMessage) to 'big endian' (message schedule _W) 

                if (0 < i && i <= noOfMessageBlocks && 0 <= j && j < 16)
                {
                    return paddedMessage[noOfMessageWords - (i - 1) * 16 - j - 1];
                }
                else
                {
                    Exception e = new("M(i,j) called with illegal argument, i,j: " + i + ", " + j);
                    throw e;
                }
            }




            // <summary>
            // SHA25 Sub-function Maj
            // </summary>
            // <param name="x"></param>
            // <param name="y"></param>
            // <param name="z"></param>
            // <returns></returns>
            // 
            static UInt32 Maj(UInt32 x, UInt32 y, UInt32 z)
            {
                return ((x & y) ^ (x & z) ^ (y & z));
            }




            // <summary>
            // SHA256 Sub-function Ch
            // </summary>
            // <param name="x"></param>
            // <param name="y"></param>
            // <param name="z"></param>
            // <returns></returns>
            // 
            static UInt32 Ch(UInt32 x, UInt32 y, UInt32 z)
            {
                return ((x & y) ^ (~x & z));
            }




            // <summary>
            // SHA256 Sub-function BigSigma1
            // </summary>
            // <param name="x"></param>
            // <returns></returns>
            // 
            static UInt32 BigSigma1(UInt32 x)
            {
                return (RotRn(x, 6) ^ RotRn(x, 11) ^ RotRn(x, 25));
            }




            // <summary>
            // SHA256 Sub-function BigSigma0
            // </summary>
            // <param name="x"></param>
            // <returns></returns>
            // 
            static UInt32 BigSigma0(UInt32 x)
            {
                return (RotRn(x, 2) ^ RotRn(x, 13) ^ RotRn(x, 22));
            }




            // <summary>
            // SHA256 Sub-function Sigma1_256
            // </summary>
            // <param name="x"></param>
            // <returns></returns>
            // 
            static UInt32 Sigma1_256(UInt32 x)
            {
                return (RotRn(x, 17) ^ RotRn(x, 19) ^ ShRn_256(x, 10));
            }




            // <summary>
            // SHA256 Sub-function Sigma0_256
            // </summary>
            // <param name="x"></param>
            // <returns></returns>
            // 
            static UInt32 Sigma0_256(UInt32 x)
            {
                return (RotRn(x, 7) ^ RotRn(x, 18) ^ ShRn_256(x, 3));
            }




            // <summary>
            // SHA256 Sub-function ShRn_256
            // </summary>
            // <param name="x"></param>
            // <param name="n"></param>
            // <returns></returns>
            // 
            static UInt32 ShRn_256(UInt32 x, int n)
            {
                UInt32 _y = x;
                if (n <= 32)
                {
                    _y >>= n;
                    return (_y);
                }
                else
                {
                    Exception e = new("ShRn called with illegal argument, n: " + n);
                    throw e;
                }
            }




            // <summary>
            // SHA256 Sub-function RotRn
            // </summary>
            // <param name="x"></param>
            // <param name="n"></param>
            // <returns></returns>
            // 
            static UInt32 RotRn(UInt32 x, int n)
            {
                UInt32 _y = x;
                UInt32 _z = x;
                if (n < 32)
                {
                    _y >>= n;
                    _z <<= (32 - n);
                    return (_y | _z);
                }
                else
                {
                    Exception e = new("RotRn called with illegal argument, n: " + n);
                    throw e;
                }

            }


            /*
            /// <summary>
            /// SHA256 Sub-function F_256
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <param name="z"></param>
            /// <param name="t"></param>
            /// <returns></returns>
            /// 
            static UInt32 F_SHA256(UInt32 x, UInt32 y, UInt32 z, int t)
            {
                if (t >= 0 && t < 80)
                {
                    if (t <= 19)
                    {
                        return ((x & y) ^ (~x & z));
                    }
                    else if ((t >= 20) && (t <= 39))
                    {
                        return (x ^ y ^ z);
                    }
                    else if ((t >= 40) && (t <= 59))
                    {
                        return ((x & y) ^ (x & z) ^ (y ^ z));
                    }
                    else return (x ^ y ^ z);

                }
                else
                {
                    Exception e = new("F_256 called with illegal argument, t: " + t);
                    throw e;

                }
            }*/
        }

        


    }
}
