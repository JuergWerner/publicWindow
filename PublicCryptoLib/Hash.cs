using System.Runtime.InteropServices;
using LongNumerics;

namespace CryptoLib
{
    
    /// <summary>
    /// BaseClas of all Hash Algorithms
    /// </summary>
    public abstract class Hash
    {
        /// <summary>
        /// The Property ´HashLength´ denotes the bit-size of the digest
        /// </summary>
        public int HashLength { get; protected set; } 

        /// <summary>
        /// The property ´MaxMessageByteSize´denotes the maximal byte-size of the message
        /// to be digested
        /// </summary>
        public int MaxMessageByteSize { get; protected set; }

        /// <summary>
        /// default (empty) constructor
        /// </summary>
        public Hash() { }   


        /// <summary>
        /// full constructor
        /// </summary>
        /// <param name="hLength">Length of the digest in bits</param>
        /// <param name="mLength">Max byte length of the message to be digested</param>
        /// <exception cref="Exception">input parameters not positive</exception>
        public Hash(int hLength, int mLength) 
        {
            HashLength = hLength > 0 ? hLength : throw new Exception($"HashLength must be set > 0");
            MaxMessageByteSize = mLength > 0 ? mLength : throw new Exception($"MaxMessageByteSize must be set > 0");
        }

        /// <summary>
        /// Abstract Digest Method
        /// </summary>
        /// <param name="message"></param>
        /// <param name="byteLength"></param>
        /// <returns></returns>
        public abstract EInt Digest(EInt message, int byteLength);

    }



    /// <summary>
    /// Public class for SHA2-256 bit objects
    /// </summary>
    public class SHA_256 : Hash
    {
        /// <summary>
        /// SHA2 algorithms have MaxMessageByteSize of ´maxMessage´
        /// </summary>
        const int maxMessage = int.MaxValue / 8;

        /// <summary>
        /// hashSize is 256 bits for SHA256
        /// </summary>
        const int hashSize = 256;


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



        /// <summary>
        /// Default constructor
        /// </summary>
        public SHA_256() : base(hashSize, maxMessage) { }   
        

        /// <summary>
        /// Implementation of the ´Digest´method´
        /// </summary>
        /// <param name="message"></param>
        /// <param name="byteLength"></param>
        /// <returns>digest(hash)</returns>
        /// 
        public override EInt Digest(EInt message, int byteLength) 
        {
            // messageLenth: Length of Message in number of bits, if too long thorow Exception.
            int messageLength = byteLength <= MaxMessageByteSize ? byteLength*8 : throw new Exception($"Message is too long: {byteLength} bytes, max is {MaxMessageByteSize}");

            int _noOfMessageBlocks;
            int _noOfMessageBits;
            int _noOfMessageWords;
            UInt32[] _paddedMessage;

            int _k;
            // ensure that the padded message is a multiple of 512 bits long 
            // the message length is stored in the least significant 64 bits 
            // followed by _k-zeros and a '1', followed by the message 

            _noOfMessageBlocks = ((messageLength + 65) % 512 == 0) ? ((messageLength + 65) / 512) : (1 + (messageLength + 65) / 512);
            _k = _noOfMessageBlocks*512 - 65 - messageLength;

            
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
            //
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
                if (n <= 32 && n >= 0)
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
                if (n < 32 && n >= 0)
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

        }
       
    }





    /// <summary>
    /// Class for SHA2-512 bit objects
    /// </summary>
    public class SHA_512 : Hash
    {
        /// <summary>
        /// SHA2 algorithms have MaxMessageByteSize of ´maxMessage´
        /// </summary>
        const int maxMessage = int.MaxValue / 8;

        /// <summary>
        /// hashSize is 512 bits for SHA512
        /// </summary>
        const int hashSize = 512;


        private static readonly UInt64[] _k512 =
        {
            // checked againts FIPS PUB 180-4, 4.2.3
            0x428a2f98d728ae22, 0x7137449123ef65cd, 0xb5c0fbcfec4d3b2f, 0xe9b5dba58189dbbc, 
            0x3956c25bf348b538, 0x59f111f1b605d019, 0x923f82a4af194f9b, 0xab1c5ed5da6d8118,
            0xd807aa98a3030242, 0x12835b0145706fbe, 0x243185be4ee4b28c, 0x550c7dc3d5ffb4e2,
            0x72be5d74f27b896f, 0x80deb1fe3b1696b1, 0x9bdc06a725c71235, 0xc19bf174cf692694,
            0xe49b69c19ef14ad2, 0xefbe4786384f25e3, 0x0fc19dc68b8cd5b5, 0x240ca1cc77ac9c65, 
            0x2de92c6f592b0275, 0x4a7484aa6ea6e483, 0x5cb0a9dcbd41fbd4, 0x76f988da831153b5, 
            0x983e5152ee66dfab, 0xa831c66d2db43210, 0xb00327c898fb213f, 0xbf597fc7beef0ee4,
            0xc6e00bf33da88fc2, 0xd5a79147930aa725, 0x06ca6351e003826f, 0x142929670a0e6e70,
            0x27b70a8546d22ffc, 0x2e1b21385c26c926, 0x4d2c6dfc5ac42aed, 0x53380d139d95b3df,
            0x650a73548baf63de, 0x766a0abb3c77b2a8, 0x81c2c92e47edaee6, 0x92722c851482353b,
            0xa2bfe8a14cf10364, 0xa81a664bbc423001, 0xc24b8b70d0f89791, 0xc76c51a30654be30,
            0xd192e819d6ef5218, 0xd69906245565a910, 0xf40e35855771202a, 0x106aa07032bbd1b8,
            0x19a4c116b8d2d0c8, 0x1e376c085141ab53, 0x2748774cdf8eeb99, 0x34b0bcb5e19b48a8,
            0x391c0cb3c5c95a63, 0x4ed8aa4ae3418acb, 0x5b9cca4f7763e373, 0x682e6ff3d6b2b8a3,
            0x748f82ee5defb2fc, 0x78a5636f43172f60, 0x84c87814a1f0ab72, 0x8cc702081a6439ec,
            0x90befffa23631e28, 0xa4506cebde82bde9, 0xbef9a3f7b2c67915, 0xc67178f2e372532b,
            0xca273eceea26619c, 0xd186b8c721c0c207, 0xeada7dd6cde0eb1e, 0xf57d4f7fee6ed178,
            0x06f067aa72176fba, 0x0a637dc5a2c898a6, 0x113f9804bef90dae, 0x1b710b35131c471b,
            0x28db77f523047d84, 0x32caab7b40c72493, 0x3c9ebe0a15c9bebc, 0x431d67c49c100d4c,
            0x4cc5d4becb3e42b6, 0x597f299cfc657e2a, 0x5fcb6fab3ad6faec, 0x6c44198c4a475817,
        };



        /// <summary>
        /// Default constructor
        /// </summary>
        /// 
        public SHA_512() : base(hashSize, maxMessage) { }


        /// <summary>
        /// Implementation of the ´Digest´method´
        /// </summary>
        /// <param name="message"></param>
        /// <param name="byteLength"></param>
        /// <returns></returns>
        /// 
        public override EInt Digest(EInt message, int byteLength)
        {
            // messageLenth: Length of Message in number of bits, if too long thorow Exception.
            int messageLength = byteLength <= MaxMessageByteSize ? byteLength * 8 : throw new Exception($"Message is too long: {byteLength} bytes, max is {MaxMessageByteSize}");

            int _noOfMessageBlocks;
            int _noOfMessageBits;
            int _noOfMessageWords;
            UInt64[] _paddedMessage;

            int _k;
            // ensure that the padded message is a multiple of 1024 bits long 
            // the message length is stored in the least significant 128 bits 
            // followed by _k-zeros and a '1', followed by the message 

            _noOfMessageBlocks = ((messageLength + 129) % 1024 == 0) ? ((messageLength + 129) / 1024) : (1 + (messageLength + 129) / 1024);
            _k = _noOfMessageBlocks * 1024 - 129 - messageLength;


            // calculate length of padded message 
            _noOfMessageBits = messageLength + 1 + _k + 128;
            _noOfMessageWords = _noOfMessageBits / 64;
            

            // prepare padded message 
            int j;
            _paddedMessage = new UInt64[_noOfMessageWords];
            for (int i = message.Xuint.Length - 1; i >= 0 ; i--)
            {
                j = i / 2;
                _paddedMessage[j] += message.Xuint[i];
                if (i%2 > 0) _paddedMessage[j] <<= 32;      // every second UInt32 word is placed in the higher order 32 bit of the UInt64 bit word
            }
            
            // make room for padding 1, _k-zeros and 2 words of message length (only lower used) 
            // shift left 129+_k bits
            _paddedMessage = EIntMath.ShiftLeftRight64BitVector(_paddedMessage, 1 + _k + 128); // MSB = bit number _noOfMessageBits -1 (zero-based)
            EIntMath.SetArrayBit64BitVector(_paddedMessage, _k + 128);                         // set bit no. _k + 128 (zero-based)

            // add message length to the lowest array element
            _paddedMessage[0] = (UInt64)messageLength;

            // End of preparation of padded message 
            

            // Instantiate & initialize Variables and Constants 
            UInt64[] _H = new UInt64[8];                                         // instantiate 256-bit Hash vector, H[0] = MSW
            // checked 30.4.2030
            _H[0] = 0x6a09e667f3bcc908;                                          // initialize H according to FIPS 202, Aug. 2015
            _H[1] = 0xbb67ae8584caa73b;
            _H[2] = 0x3c6ef372fe94f82b;
            _H[3] = 0xa54ff53a5f1d36f1;
            _H[4] = 0x510e527fade682d1;
            _H[5] = 0x9b05688c2b3e6c1f;
            _H[6] = 0x1f83d9abfb41bd6b;
            _H[7] = 0x5be0cd19137e2179;

            // Instantiate Message Schedule, initialize in each block cycle 
            UInt64[] _W = new UInt64[80];

            // instantiate 8 working variable for last hash value 
            UInt64 _a, _b, _c, _d, _e, _f, _g, _h;

            // instantiate 2 temporary words 
            UInt64 _T1, _T2;

            // instantiate hashEndresult_256 
            EInt _hashEndresult_512 = new(16);

            for (int i = 1; i <= _noOfMessageBlocks; i++)                            // Message Block Loop (1-based)
            {
                for (int t = 0; t < 80
                    ; t++)                                       // prepare Message Schedule
                {
                    if (t < 16)
                    {
                        _W[t] = Mpadded(i, t, _paddedMessage, _noOfMessageBlocks, _noOfMessageWords);
                    }
                    else
                    {

                        _W[t] = Sigma1_512(_W[t - 2]) + _W[t - 7] + Sigma0_512(_W[t - 15]) + _W[t - 16];
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

                for (int t = 0; t < 80; t++)                                        // 64 rounds of bit-mingle
                {
                    _T1 = _h + BigSigma1(_e) + Ch(_e, _f, _g) + _k512[t] + _W[t];
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

            _hashEndresult_512.Xuint[0]  = (UInt32)(_H[7] & 0xFFFFFFFF);            // EInt hashEndresult_256[0] = LSW
            _hashEndresult_512.Xuint[1]  = (UInt32)(_H[7] >> 32);
            _hashEndresult_512.Xuint[2]  = (UInt32)(_H[6] & 0xFFFFFFFF);
            _hashEndresult_512.Xuint[3]  = (UInt32)(_H[6] >> 32);
            _hashEndresult_512.Xuint[4]  = (UInt32)(_H[5] & 0xFFFFFFFF);
            _hashEndresult_512.Xuint[5]  = (UInt32)(_H[5] >> 32);
            _hashEndresult_512.Xuint[6]  = (UInt32)(_H[4] & 0xFFFFFFFF);
            _hashEndresult_512.Xuint[7]  = (UInt32)(_H[4] >> 32);
            _hashEndresult_512.Xuint[8]  = (UInt32)(_H[3] & 0xFFFFFFFF);
            _hashEndresult_512.Xuint[9]  = (UInt32)(_H[3] >> 32);
            _hashEndresult_512.Xuint[10] = (UInt32)(_H[2] & 0xFFFFFFFF);
            _hashEndresult_512.Xuint[11] = (UInt32)(_H[2] >> 32);
            _hashEndresult_512.Xuint[12] = (UInt32)(_H[1] & 0xFFFFFFFF);
            _hashEndresult_512.Xuint[13] = (UInt32)(_H[1] >> 32);
            _hashEndresult_512.Xuint[14] = (UInt32)(_H[0] & 0xFFFFFFFF);
            _hashEndresult_512.Xuint[15] = (UInt32)(_H[0] >> 32);                   // EInt hashEndresult_256[7] = HSW


            return _hashEndresult_512;



            // <summary> SHA512 Sub-function Mpadded </summary>
            // <param name="i"></param>
            // <param name="j"></param>
            // <param name="paddedMessage"></param>
            // <param name="noOfMessageBlocks"></param>
            // <param name="noOfMessageWords"></param>
            // <returns></returns>

            static UInt64 Mpadded(int i, int j, UInt64[] paddedMessage, int noOfMessageBlocks, int noOfMessageWords)
            {
                // i:   is the index of the 1024-bit-message block (1-based) )
                // j:   is the index of the 64-bit-word in the 1024-bit-message-block  (zero-based)  
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
            // SHA512 Sub-function Maj
            // </summary>
            // <param name="x"></param>
            // <param name="y"></param>
            // <param name="z"></param>
            // <returns></returns>
            // 
            static UInt64 Maj(UInt64 x, UInt64 y, UInt64 z)
            {
                return ((x & y) ^ (x & z) ^ (y & z));
            }




            // <summary>
            // SHA512 Sub-function Ch
            // </summary>
            // <param name="x"></param>
            // <param name="y"></param>
            // <param name="z"></param>
            // <returns></returns>
            // 
            static UInt64 Ch(UInt64 x, UInt64 y, UInt64 z)
            {
                return ((x & y) ^ (~x & z));
            }




            // <summary>
            // SHA512 Sub-function BigSigma1
            // </summary>
            // <param name="x"></param>
            // <returns></returns>
            // 
            static UInt64 BigSigma1(UInt64 x)
            {
                return (RotRn(x, 14) ^ RotRn(x, 18) ^ RotRn(x, 41));
            }




            // <summary>
            // SHA512 Sub-function BigSigma0
            // </summary>
            // <param name="x"></param>
            // <returns></returns>
            // 
            static UInt64 BigSigma0(UInt64 x)
            {
                return (RotRn(x, 28) ^ RotRn(x, 34) ^ RotRn(x, 39));
            }




            // <summary>
            // SHA512 Sub-function Sigma1_512
            // </summary>
            // <param name="x"></param>
            // <returns></returns>
            // 
            static UInt64 Sigma1_512(UInt64 x)
            {
                return (RotRn(x, 19) ^ RotRn(x, 61) ^ ShRn_512(x, 6));
            }




            // <summary>
            // SHA512 Sub-function Sigma0_512
            // </summary>
            // <param name="x"></param>
            // <returns></returns>
            // 
            static UInt64 Sigma0_512(UInt64 x)
            {
                return (RotRn(x, 1) ^ RotRn(x, 8) ^ ShRn_512(x, 7));
            }




            // <summary>
            // SHA512 Sub-function ShRn_512
            // </summary>
            // <param name="x"></param>
            // <param name="n"></param>
            // <returns></returns>
            // 
            static UInt64 ShRn_512(UInt64 x, int n)
            {
                UInt64 _y = x;
                if (n <= 64 && n >= 0)
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
            // SHA512 Sub-function RotRn
            // </summary>
            // <param name="x"></param>
            // <param name="n"></param>
            // <returns></returns>
            // 
            static UInt64 RotRn(UInt64 x, int n)
            {
                UInt64 _y = x;
                UInt64 _z = x;
                if (n < 64 && n >= 0)
                {
                    _y >>= n;
                    _z <<= (64 - n);
                    return (_y | _z);
                }
                else
                {
                    Exception e = new("RotRn called with illegal argument, n: " + n);
                    throw e;
                }

            }

        }

    }



}