using LongNumerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoLib
{
    /// <summary>
    /// Lamport signature class
    /// </summary>
    public class Lamport : CryptoObject
    {

        int _noOfRanPairs;



        /// <summary>
        /// Master constructor
        /// </summary>
        /// <param name="Hs"></param>
        /// <param name="Cr"></param>
        /// <exception cref="Exception"></exception>
        public Lamport(Hash? Hs, Credentials? Cr) : base(Cr,Hs)
        {
            myRandom = new RandomGenerator();
            if (myHash == null) throw new Exception("myHash is null!");
            _noOfRanPairs = myHash.HashLength;
            if (_noOfRanPairs % 32 != 0) throw new Exception("Hash bit length must be multiple of 32 bit");
            GetSigKeyPair();
        }


        /// <summary>
        /// Method to asign keys for signature
        /// </summary>
        public override void GetSigKeyPair()
        {
            int _noOfBytes = _noOfRanPairs / 8;
            int _noOfWords = _noOfRanPairs / 32;

            EInt[] _priv0 = new EInt[_noOfRanPairs], _priv1 = new EInt[_noOfRanPairs];
            EInt[] _publ0 = new EInt[_noOfRanPairs], _publ1 = new EInt[_noOfRanPairs];



            for (int i = 0; i < _noOfRanPairs; i++)
            {
                if (myRandom != null)
                {
                    _priv0[i] = myRandom.GetRandomEInt(_noOfBytes);
                    _priv1[i] = myRandom.GetRandomEInt(_noOfBytes);
                    if (myHash != null)
                    {
                        _publ0[i] = myHash.Digest(_priv0[i], _noOfBytes);
                        _publ1[i] = myHash.Digest(_priv1[i], _noOfBytes);
                    }
                    else throw new Exception("GetSigKeyError");
                }
                else throw new Exception("GetSigKeyError");
            }

            EInt _priv0_p = new("0");
            EInt _priv1_p = new("0");
            EInt _publ0_p = new("0");
            EInt _publ1_p = new("0");

            for (int i = 0; i < _noOfRanPairs; i++)
            {
                _priv0_p.PackLow(_priv0[_noOfRanPairs - 1 - i], _noOfRanPairs);
                _priv1_p.PackLow(_priv1[_noOfRanPairs - 1 - i], _noOfRanPairs);
                _publ0_p.PackLow(_publ0[_noOfRanPairs - 1 - i], _noOfRanPairs);
                _publ1_p.PackLow(_publ1[_noOfRanPairs - 1 - i], _noOfRanPairs);
            }

            myPrivateSigKey = new EInt[2];
            myPrivateSigKey[0] = _priv0_p;
            myPrivateSigKey[1] = _priv1_p;


            myPublicSigKey = new EInt[2];
            myPublicSigKey[0] = _publ0_p;
            myPublicSigKey[1] = _publ1_p;
        }

        /// <summary>
        /// Un-pack key into Array
        /// </summary>
        /// <param name="key"></param>
        /// <param name="nHash"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        /// 
        public EInt[] UnPackLpamportKeys(EInt key, int nHash)
        {
            int _n = nHash;
            if (key.Count * 32 < _n * _n) throw new Exception($"Error UnPackLamportKeys: Key Length no suitable");
            else
            {
                EInt[] keyArray = new EInt[_n];

                for (int i = 0; i < _n; i++)
                {
                    keyArray[i] = key.Extract(i * _n,_n);
                }
                return keyArray;
            }          
        }


        /// <summary>
        /// Signatur primitive
        /// </summary>
        /// <param name="message"></param>
        /// <param name="MessageByteLength"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        /// 
        public override EInt Sign(EInt? message, int? MessageByteLength)
        {
            //Check input:
            if (message is null) throw new ArgumentNullException("message");
            if (MessageByteLength is null) throw new ArgumentNullException("MessageByteLength");
            if (MessageByteLength < 0) throw new Exception("MessageByteLength is negative");
            // check properties
            if (myPrivateSigKey is null) throw new Exception("myPrivateKey is not valid (null)");
            if (myHash is null) throw new Exception("Hash not valid (null)");

            EInt signature = new("0"), messageHash;
            int hLength = myHash.HashLength;

            // prepare private key-array:
            EInt[] _priv0 = UnPackLpamportKeys(myPrivateSigKey[0], myHash.HashLength);
            EInt[] _priv1 = UnPackLpamportKeys(myPrivateSigKey[1], myHash.HashLength);

            // Hash the message:
            messageHash = myHash.Digest((EInt)message, (int)MessageByteLength);

            EInt _word;
            //prepare the signature
            for(int i = hLength - 1; i >= 0; i--)
            {
                _word = (EIntMath.CheckArrayBit(messageHash.Xuint, i)) ? _priv1[i] : _priv0[i];
                signature.PackLow(_word, hLength); 
            }

            // devaluate signature key (single use)
            myPrivateSigKey = null;

            return signature;
        }



        /// <summary>
        /// Signatur verification primitive
        /// </summary>
        /// <param name="message"></param>
        /// <param name="MessageByteLength"></param>
        /// <param name="signature"></param>
        /// <param name="partnerPublicKey"></param>
        /// <param name="partnerHash"></param>
        /// <returns></returns>
        /// 
        public override bool Verify(EInt? message, int? MessageByteLength, EInt? signature, EInt[]? partnerPublicKey, Hash? partnerHash)
        {
            // check input params:
            if (message is null) throw new ArgumentNullException("message");
            if (MessageByteLength is null) throw new ArgumentNullException("MessageByteLength");
            if (signature is null) throw new ArgumentNullException("signature");
            if (partnerPublicKey is null) throw new ArgumentNullException("partnerPublicKey");
            if (partnerHash is null) throw new ArgumentNullException("partnerHash");

            int _nHash = partnerHash.HashLength;

            // prepare partner-public key-array:
            EInt[] _publ0 = UnPackLpamportKeys(partnerPublicKey[0], _nHash);
            EInt[] _publ1 = UnPackLpamportKeys(partnerPublicKey[1], _nHash);

            EInt _messageHash = partnerHash.Digest((EInt)message, (int)MessageByteLength);
            bool ok=true;
            EInt _hashTarget, _word,_sigHash;

            for (int i = 0; i < _nHash; i++)
            {
                _hashTarget = (EIntMath.CheckArrayBit(_messageHash.Xuint, i)) ? _publ1[i] : _publ0[i];
                _word = ((EInt)signature).Extract(i * _nHash, _nHash);
                _sigHash = partnerHash.Digest(_word, _nHash/8);
                ok = ok && (_hashTarget.IsEqual(_sigHash));
            }
            return ok;    
        }
    }
}
