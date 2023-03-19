using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LongNumerics
{
    
    /// <summary>
    /// text string
    /// </summary>
    public class PlainText
    {
        /* ----------------------------------------------------------------------------------------------
         * Properties:
         * ---------------------------------------------------------------------------------------------- */

        /// <summary>
        /// Class containing the TextString string
        /// </summary>
        public string TextString { get; set; } = "";

        /// <summary>
        /// Name
        /// </summary>
        public string TextID { get; set; } = "not named";

        /// <summary>
        /// EInt Representation of PlainText
        /// </summary>
        public EInt TextEInt
        {
            get
            {
                EInt result = (EInt)this;
                result.ObjectID = TextID;
                return result;
            }
        }


        /// <summary>
        /// returns Number of Chars in the text
        /// </summary>
        public int NumberOfChars { get => TextString.Length; }


        /// <summary>
        /// returns SHA 256 Hash-Code of Text
        /// </summary>
        public EInt HashSHA256 { get => TextEInt.SHA256; }


        /* ----------------------------------------------------------------------------------------------
         * Constructors:
         * ---------------------------------------------------------------------------------------------- */


        /// <summary>
        /// Default .Ctor 
        /// </summary>
        /// 
        public PlainText()
        {
            TextString = "";
        }

        /// <summary>
        /// Main .Ctor
        /// </summary>
        /// <param name="text"> text string </param>
        /// <param name="name"> text name </param>
        /// 
        public PlainText(string text, string name = "not named")
        {
            TextString = text;
            TextID = name;
        }


        /* ----------------------------------------------------------------------------------------------
         * Operators:
         * ---------------------------------------------------------------------------------------------- */

        /// <summary>
        /// Converts Plantext to string or "" if null
        /// </summary>
        /// <param name="t"></param>
        public static explicit operator string(PlainText t)
        {
            return (t.TextString != null) ? t.TextString : "";


        }




        /// <summary>
        /// Convert text-string into Planitext
        /// </summary>
        /// <param name="text"></param>
        public static explicit operator PlainText(string text)
        {
            return new PlainText(text);
        }


        /* ----------------------------------------------------------------------------------------------
         * Methods:
         * ---------------------------------------------------------------------------------------------- */

        /// <summary>
        /// String representation of object
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string s = $"Text;\n{TextString}\n";
            s += $"Number of characters in the text: {this.NumberOfChars}\n";
            s += $"Text-ID: \t\t{this.TextID}\n";
            s += $"Text-Hash (SHA256): \t{HashSHA256.ToString()}\n";
            s += $"Text-EInt-Representation:\n{TextEInt.ToString('h', false, true)}\n";

            return s;

        }

    }
}
