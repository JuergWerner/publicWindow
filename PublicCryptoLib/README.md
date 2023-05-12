Author: Dr. Jürg Werner, Switzerland.

Copy Rights: All rights reserved by the author and owner of the code, Dr. Jürg Werner, Switzerland (see X11 License Notification below).

```
Copyright (C) <19.3.2023> <Dr. Juerg Werner, Hedingen, Switzerland, juerg.werner@bluewin.ch>

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE X CONSORTIUM BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

Except as contained in this notice, the name of <Dr. Juerg Werner> shall not be used in advertising or otherwise to promote the sale, use or other dealings in this Software without prior written authorization from <Dr. Jürg Werner,Hedingen, Switzerland>.
```

```

```

Language: C# (10), .Net (6)

Assembly Name, Namespace: `CryptoLib`

Resources: LongNumerics

#### Purpose of `CryptoLib`:

Library of alternative cryptography. Implemented in this Version:

- SHA2-Hash:
  -  public abstract class Hash, base class of all hash algorithm
  - `public class SHA_256`, according to FIPS PUB 180-4, tested against testvectors of FIPS
  - `public class SHA_512`, according to FIPS PUB 180-4, tested against testvectors of FIPS

- CryptoObject:

  - `public abstract class CryptoObject` is a base class for all cryptographic object in this library.

- Credentials:

  - `public class Credentials : ICloneable` contains all credentials of the key owner provide and prove its identity. Modify to your needs!

- Random number generator

  The`public class RandomGenerator` is used to wrap your random generator used for your cryptography. Modify inner content if other random generator is to be used!

- `public class Lamport : CryptoObject` is a an implementation of the Lamport primitives.



#### Content:

- **Class `Hash ,` (file *Hash.cs*)**

  This abstract class is the base class of all hash-algorithms implemented in this library. Its polymorphic interface enables the implementation of methods, that work with a variety of hash algorithms.

  Interfaces implemented: --

  

  Properties:

  - int `HashLength`: bit-size of the digest (hash).
- int `MaxMessageByteSize`: Maximum length of the message to be hashed in bytes.
  
  
  

Constructors:

- Default-constructor `Hash()`: empty constructor.
  
- Master-Constructor `Hash(int hLength, int mLength)`:
  
  This constructor initialize the `HashLength` and `MaxMessageByteSize` respectively.
  
  
  

Methods:

object-level:

- `public abstract EInt Digest(EInt message, int byteLength)`:
  
  This method calculates the hash (or digest) of the `EInt message` of `int byteLength` bytes.
  

static: --



- **Class `SHA_256` (file *Hash.cs*):**
  
  This class inherits from the `Hash`class and performs the SHA2-256 hash. The length of its digest is 256 bits, follows the definition of ´FIPS PUB 180-4´and is tested against many testvectors provided by FIPS.
  
  
  
  Properties: ---
  
  
  
  Constructors:
  
  - Default-constructor `SHA_256()`: empty constructor.
  - 
  
  Methods:
  
  object-level:
  
  - `public override EInt Digest(EInt message, int byteLength)`:
  
    This method calculates the hash (or digest) of the `EInt message` of `int byteLength` bytes.
  
  static: --
  
  

- **Class `SHA_512 (file *Hash.cs*):**

  This class inherits from the `Hash`class and performs the SHA2-512 hash. The length of its digest is 512 bits, follows the definition of ´FIPS PUB 180-4´and is tested against many testvectors provided by FIPS.

  

  Properties: ---

  

  Constructors:

  - Default-constructor `SHA_512()`: empty constructor.
  - 

  Methods:

  object-level:

  - `public override EInt Digest(EInt message, int byteLength)`:

    This method calculates the hash (or digest) of the `EInt message` of `int byteLength` bytes.

  static: --

  

- **Class ``Lamport,` (file *Lamport.cs*)**

  This class implements the Lamport signature. It is a one-time signature that is believed to withstand quantum computer attacks since its security relies on the security of the one-way hash function, that do not need to be reversible. With the signature of a message, the private key is destroyed to ensure one-time use of it. The public key remains valid to check the signature on the recipients side. For a next signature a new object can be instantiated or alternatively, a new key-pair can be generated in the existing object by the method`GetSigKeyPair()`. The Implementation contains the primitives that do not contain the signature protocol and message preparation.

  You can apply any suitable hash function that inherits from abstract class `hash`.

  Interfaces implemented: --

  

  Properties: ---

  

  Constructors:

  - Master-constructor `public Lamport(Hash? Hs, Credentials? Cr) : base(Cr,Hs)`.

    Instantiates a Lamport object with the chosen hash algorithm you credentials.

    

  Methods:

  object-level:

  - `public override void GetSigKeyPair()` :

    This method is called by the constructor. It can be called to renew the key-pair, if the existing public key is not needed any more.

  -  `public override EInt Sign(EInt? message, int? MessageByteLength)`

    This method performs a Lamport signature of the `message`. It is a primitve in the sense that the signature protocol must be built around the primitive.

  -  `public override bool Verify(EInt? message, int? MessageByteLength, EInt? signature, EInt[]? partnerPublicKey, Hash? partnerHash)`

    This method verifies a `signature` on the base of the `message` of length `MessageByteLength,` the `partnerPublicKey` and the `partnerHash` that where applicable during signature

  static: --

  

  
