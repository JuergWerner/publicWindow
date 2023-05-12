using LongNumerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CryptoLib
{
    /// <summary>
    /// Abstract base class for any cryptographic object in this library
    /// </summary>
    public abstract class CryptoObject
    {
        // ----- Properties: -----

        /// <summary>
        /// This are my credential as owner of the private key
        /// </summary>
        public Credentials? myCredentials { get; protected set; }

        /// <summary>
        /// This property hold an instance of the hash generator selected.
        /// </summary>
        public Hash? myHash { get; protected set; }

        /// <summary>
        /// This property holds an instance of the random generator selected
        /// </summary>
        public RandomGenerator? myRandom { get; protected set; }

        /// <summary>
        /// This Property holds my secret private signature key
        /// </summary>
        protected EInt[]? myPrivateSigKey { get; set;}

        /// <summary>
        /// This property holds my public signature key
        /// </summary>
        public EInt[]? myPublicSigKey { get; protected set; }

        /// <summary>
        /// This property holds my public key for my decoding
        /// </summary>
        public EInt[]? myPrivateDecKey { get; protected set; }

        /// <summary>
        /// This property holds my public key for partner encoding
        /// </summary>
        public EInt[]? myPublicEncKey { get; protected set; }

        // ------- End of properties -----



        // ----- Constructors: -----

        /// <summary>
        /// Default constructor (empty)
        /// </summary>
        public CryptoObject() { }

        /// <summary>
        /// Master constructor
        /// </summary>
        /// <param name="myCr"></param>
        /// <param name="myHs"></param>
        public CryptoObject(Credentials? myCr, Hash? myHs)
        {
            if (myCr == null | myHs == null) throw new ArgumentNullException(nameof(myCr), nameof(myHs));
            else
            {
                myCredentials = myCr;
                myHash = myHs;
            }
                      
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="myHs"></param>
        public CryptoObject(Hash? myHs)
        {
            if (myHs == null) throw new ArgumentNullException(nameof(myHs));
            else myHash = myHs;
        }
        // ----- End of constructors -----



        // ----- Methods: -----

        // object level:


        /// <summary>
        /// Method to sign a message with my private key.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="MessageByteLength"></param>
        /// <returns></returns>
        public virtual EInt Sign(EInt? message, int? MessageByteLength)
        {
            throw new NotImplementedException("Sign");
        }

        /// <summary>
        /// Method to verfy a signed message
        /// </summary>
        /// <param name="message"></param>
        /// <param name="MessageByteLength"></param>
        /// <param name="signature"></param>
        /// <param name="partnerPublicKey"></param>
        /// <param name="partnerHash"></param>
        /// <returns></returns>
        public virtual bool Verify(EInt? message, int? MessageByteLength, EInt? signature, EInt[]? partnerPublicKey, Hash? partnerHash)
        {
            throw new NotSupportedException("Verfy");
        }

        /// <summary>
        /// This method returns a key pair.
        /// </summary>
        /// <returns></returns>
        /// 
        public virtual void GetSigKeyPair()
        { 
            throw new NotSupportedException(); 
            
        }

        // static:
        // -----End of methods -----
    }





    /// <summary>
    /// Structure containing all Id of Owner ior Agent
    /// </summary>
    /// 
    [XmlInclude(typeof(Credentials))]
    public class Credentials : ICloneable
    {
        /// <summary>
        /// If set, the chain of trust ends here.
        /// </summary>
        [XmlElement]
        public bool RootClaimed { get; set; } = false;

        /// <summary>
        /// Name of Holder/Agent
        /// </summary>
        [XmlElement]
        public string Name { get; set; } = string.Empty;


        /// <summary>
        /// Company as Holder/Agent
        /// </summary>
        [XmlElement]
        public string Company { get; set; } = string.Empty;


        /// <summary>
        /// Email address of Holder/Agent
        /// </summary>
        [XmlElement]
        public string Email { get; set; } = string.Empty;



        /// <summary>
        /// Email address of Holder/Agent
        /// </summary>
        [XmlElement]
        public string URLaddress { get; set; } = string.Empty;




        /// <summary>
        /// Constructor (empty)
        /// -------------------
        /// </summary>
        /// 
        public Credentials() { }


        /// <summary>
        /// Full Constructor
        /// ----------------
        /// </summary>
        /// <param name="name"></param>
        /// <param name="company"></param>
        /// <param name="email"></param>
        /// <param name="urladdress"></param>
        /// <param name="root"><see langword="true"/>if root is claimed</param>
        /// 
        public Credentials(string name, string company, string email, string urladdress, bool root = false)
        {
            Name = name;
            Company = company;
            Email = email;
            URLaddress = urladdress;
            RootClaimed = root;
        }


        /// <summary>
        /// ToString
        /// </summary>
        /// <returns>string</returns>
        /// 
        public override string ToString()
        {
            return $"\nName: {Name}\nCompany: {Company}\nEmail: {Email}\nURL: {URLaddress}\nRoot: {RootClaimed}\n";

        }

        /// <summary>
        /// Implementation of IClonable
        /// </summary>
        /// <returns>Clone</returns>
        /// 
        public object Clone()
        {
            Credentials copy = new()
            {
                Name = Name,
                Company = Company,
                Email = Email,
                URLaddress = URLaddress,
                RootClaimed = RootClaimed
            };
            return copy;
        }


    }





    /// <summary>
    /// Class to wrap my selected random generator
    /// </summary>
    public class RandomGenerator 
    {
        private RandomNumberGenerator? rN;

        /// <summary>
        /// Default constructor to initialize Random generator.
        /// </summary>
        public RandomGenerator()
        {
            rN = RandomNumberGenerator.Create();
        }


        /// <summary>
        /// Returns a random EInt
        /// </summary>
        /// <param name="numberOfBytes"></param>
        /// <returns></returns>
        public EInt GetRandomEInt(int numberOfBytes)
        {
            byte[] input = new byte[numberOfBytes];
            rN?.GetBytes(input);
            return (EInt)input;
        }
    }

}
