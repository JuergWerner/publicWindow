<<<<<<< HEAD
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

Assembly Name, Namespace: LongNumerics 

#### Purpose of *LongNumerics*:

Handling of unsigned integers for any application using unsigned integers of exceeding or unpredictable length, especially cryptography, modulo arithmetic, etc.. 

The type `EInt` is well integrated and works with the operators `++, --, +, *,  /,  %,  ^, &, |, >>, <<` and logical operators `==, >=, >=, !=, >, <` as you would expect with any built-in types such as `uint`. 

There are many explicit conversion built-in, such as : 

- `(byte[]) EInt`
- `(EInt) byte[]`
- `(EInt) int`
- `(EInt) uint`
- `(EInt) ulong`
- `(EInt) PlainText`
- `(PlainText) EInt`

Various methods support cryptographic applications and modulo calculus.





#### Content:

- **Class `EInt ,` (file *EInt.cs*)**

  There is the `EInt.parse(string)`strings beginning with `"0x"` or `"0X"`are interpreted as Hex-String, "`0b`" or "`0B`" as binary, everything else as decimal string. 

  All characters not fitting into the hex-, binary or  or decimal-schema respectively, are ignored. A resulting empty string results in a 0-value. 

  If a argument is `null` a `NullReferenceException`is thrown.

  Interfaces implemented:

  - `ICloneable`

  - `IComparable`

    

  Properties:

  - `Xuint{get; set;}` is a `Uint32[]`holding the number of any practical length. The msb of highest-index Array-Element is the msb of the number (big endian), including any possible leading 0´s.

  - `Count`: Array length of `Xuint`

  - `BitSize`: Order of highest-order non-zero bit + 1, (number of bits)

  - `ByteSize`: number of bytes

  - `MessageByteLength`: does not necessarily reflect the actual number of bytes, this property passed along conversion, used e.g. for text to number and number to text conversion.

  - `ObjectID`: Identifier of `EInt`

  - `this[int index]`: indexer to access single `UInt32`-words, zero-based. Example: `EInt a; a[1]`returns 2nd word

  - `SHA256`returns SHA256 hash-code (8*`ByteLength` bits )

  

  the object-class method `Euquals(object?)` is overridden and returns true if all properties are the same (`ToString()` is evaluated), false otherwise, regardless if it is the same instance or not.

  

  Constructors:
  
  - Master-Constructor `EInt(string? numberString, char? numberSys, string? Id )`. 
  
    Returns an `EInt`, where the `string numberString`is interpreted as Hex-, as binary- or as decimal-string depending if `char numberSys` is `´h´`or`´H´`, `´b´`or `´B´`, ´d´or ´D´(default) respectively. 
  
    Any non-matching char is ignored. Zero is assigned to an empty string. `ObjectId = ID`, `MessageByteLength = 4*Count`.
  
  - `EInt() => this("0", ´h´, "not named")`
  
  - `EInt(string? inString, string? Id = "not named") => this(inString,´h´, Id)`
  
  - `EInt(int n) => EInt` with n `Uint32` words of value 0. `ObjectId = "not named`, `MessageByteLength = 4*n`.
  
  
  
  
  
  Methods:
  
  object-level:
  
  - Arithmetic methods: `Add(), Sub(), Mul(), Div(), Sq()` (square), `Trim()`(cut of leading zero words)
  - Modulo arithmetic (overloaded) operations: `Add(), Sub(), Mul(), Div(), Exp()`
  - Boolean operations: `IsZero(), IsEqual(), IsGreaterThan, IsLessThan()`
  - conversion methods: `ToString(), ToText(), ToXml()`
  - `IClonable: Clone()`
  
  static:
  
  - Operators as mentioned in the introduction
  - see static helper class `EIntMath`
  
  



- **Static Helper Class `EIntMath` (file *EintMath.cs*) with static, math- and cryptocentric methods:**
  
  - `GetPrime(int, bool,bool,int)` single thread, returns random prime number of desired length
  - `GetPrime(bool,int,bool,int)` single & multi-thread(s), high power calculation of random prime (on INTEL i9, 4 GHz: 1000 bit prime in 20 s, 2000 bit prime in about 3 min 45 s , a 4500 bit prime in ca. 15 min)
  - `SHA256(EInt,int)` provides SHA256-Hash-Code
  - `MillerRabinTest(EInt,)` checks primality by the Miller-Rabin-Algorithm
  - `Inv(EInt?,Eint?)` provides the multiplicative inverse in the modulo-class
  - `Gcd(EInt,Eint)` provides the greatest common divisor
  - `Eea(Eint,Eint,Eint)` performs the EEA-algorithm (modulo).
  - some further low level helper methods and helper class (`ThreadParams)`...
  
  
  
- Static Helper Class `EIntCon` with some static, conversion centric methods:
  - `ByteArrayToEInt(byte[],int)` converts a *byte[*] into `EInt`
  - `TextStringToEInt(string,out int,bool)` converts string-text to `EInt`
  - `GetAssemblyName(object)` (Extension to the object class) gets the name of the type and assembly
  - some further, low level helper methods ...

  
  
- Class `PlainText` to switch between string and number-representation of a text.
  - explicit operator `string` to `PlainText`
  - explicit operator `PlainText` to `string`
  - `HashSHA245`: SHA256-Hashcode of number-representation of text
  - `NumberOfChars`: number of Characters count in text
  - `TextEInt` number (`EInt`) representing the text
  - `TextString`:`String` representing the text
  - `TextID`: identifier of the text
  - `ToString()`:  string output of the object content

Outlook:

RSA-Key-Algorithms with multi-thread implementation for very long keys are as  alpha-versions under test, with signature functions to distribute keys. This may be interesting for users who do not trust the usual implementation. 
=======
# publicWindow
>>>>>>> 213c04fb6632f9bd4cc47ef4ffdf4588a8de2b6e
