using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Neo4Net.Kernel.impl.store
{

	using PropertyBlock = Neo4Net.Kernel.Impl.Store.Records.PropertyBlock;
	using Bits = Neo4Net.Kernel.impl.util.Bits;
	using UnsafeUtil = Neo4Net.@unsafe.Impl.Internal.Dragons.UnsafeUtil;
	using TextValue = Neo4Net.Values.Storable.TextValue;
	using Values = Neo4Net.Values.Storable.Values;

	/// <summary>
	/// Supports encoding alphanumerical and <code>SP . - + , ' : / _</code>
	/// 
	/// (This version assumes 14bytes property block, instead of 8bytes)
	/// 
	/// @author Tobias Ivarsson <tobias.ivarsson@neotechnology.com>
	/// </summary>
	public abstract class LongerShortString
	{
		 /// <summary>
		 /// Binary coded decimal with punctuation.
		 /// 
		 /// <pre>
		 ///    -0 -1 -2 -3 -4 -5 -6 -7   -8 -9 -A -B -C -D -E -F
		 /// 0-  0  1  2  3  4  5  6  7    8  9 SP  .  -  +  ,  '
		 /// </pre>
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       NUMERICAL(1, 4) { int encTranslate(byte b) { if(b >= '0' && b <= '9') { return b - '0'; } switch(b) { case 0: return 0xA; case 2: return 0xB; case 3: return 0xC; case 6: return 0xD; case 7: return 0xE; case 8: return 0xF; default: throw cannotEncode(b); } } int encPunctuation(byte b) { throw cannotEncode(b); } char decTranslate(byte codePoint) { if(codePoint < 10) { return(char)(codePoint + '0'); } return decPunctuation(codePoint - 10 + 6); } },
		 /// <summary>
		 /// Binary coded decimal with punctuation.
		 /// 
		 /// <pre>
		 ///    -0 -1 -2 -3 -4 -5 -6 -7   -8 -9 -A -B -C -D -E -F
		 /// 0-  0  1  2  3  4  5  6  7    8  9 SP  -  :  /  +  ,
		 /// </pre>
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       DATE(2, 4) { int encTranslate(byte b) { if(b >= '0' && b <= '9') { return b - '0'; } switch(b) { case 0: return 0xA; case 3: return 0xB; case 4: return 0xC; case 5: return 0xD; case 6: return 0xE; case 7: return 0xF; default: throw cannotEncode(b); } } int encPunctuation(byte b) { throw cannotEncode(b); } char decTranslate(byte codePoint) { if(codePoint < 0xA) { return(char)(codePoint + '0'); } switch(codePoint) { case 0xA: return ' '; case 0xB: return '-'; case 0xC: return ':'; case 0xD: return '/'; case 0xE: return '+'; default: return ','; } } },
		 /// <summary>
		 /// Upper-case characters with punctuation.
		 /// 
		 /// <pre>
		 ///    -0 -1 -2 -3 -4 -5 -6 -7   -8 -9 -A -B -C -D -E -F
		 /// 0- SP  A  B  C  D  E  F  G    H  I  J  K  L  M  N  O
		 /// 1-  P  Q  R  S  T  U  V  W    X  Y  Z  _  .  -  :  /
		 /// </pre>
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       UPPER(3, 5) { int encTranslate(byte b) { return super.encTranslate(b) - 0x40; } int encPunctuation(byte b) { return b == 0 ? 0x40 : b + 0x5a; } char decTranslate(byte codePoint) { if(codePoint == 0) { return ' '; } if(codePoint <= 0x1A) { return(char)(codePoint + 'A' - 1); } return decPunctuation(codePoint - 0x1A); } },
		 /// <summary>
		 /// Lower-case characters with punctuation.
		 /// 
		 /// <pre>
		 ///    -0 -1 -2 -3 -4 -5 -6 -7   -8 -9 -A -B -C -D -E -F
		 /// 0- SP  a  b  c  d  e  f  g    h  i  j  k  l  m  n  o
		 /// 1-  p  q  r  s  t  u  v  w    x  y  z  _  .  -  :  /
		 /// </pre>
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       LOWER(4, 5) { int encTranslate(byte b) { return super.encTranslate(b) - 0x60; } int encPunctuation(byte b) { return b == 0 ? 0x60 : b + 0x7a; } char decTranslate(byte codePoint) { if(codePoint == 0) { return ' '; } if(codePoint <= 0x1A) { return(char)(codePoint + 'a' - 1); } return decPunctuation(codePoint - 0x1A); } },
		 /// <summary>
		 /// Lower-case characters with punctuation.
		 /// 
		 /// <pre>
		 ///    -0 -1 -2 -3 -4 -5 -6 -7   -8 -9 -A -B -C -D -E -F
		 /// 0-  ,  a  b  c  d  e  f  g    h  i  j  k  l  m  n  o
		 /// 1-  p  q  r  s  t  u  v  w    x  y  z  _  .  -  +  @
		 /// </pre>
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       EMAIL(5, 5) { int encTranslate(byte b) { return super.encTranslate(b) - 0x60; } int encPunctuation(byte b) { int encOffset = 0x60; if(b == 7) { return encOffset; } int offset = encOffset + 0x1B; switch(b) { case 1: return 0 + offset; case 2: return 1 + offset; case 3: return 2 + offset; case 6: return 3 + offset; case 9: return 4 + offset; default: throw cannotEncode(b); } } char decTranslate(byte codePoint) { if(codePoint == 0) { return ','; } if(codePoint <= 0x1A) { return(char)(codePoint + 'a' - 1); } switch(codePoint) { case 0x1E: return '+'; case 0x1F: return '@'; default: return decPunctuation(codePoint - 0x1A); } } },
		 /// <summary>
		 /// Lower-case characters, digits and punctuation and symbols.
		 /// 
		 /// <pre>
		 ///    -0 -1 -2 -3 -4 -5 -6 -7   -8 -9 -A -B -C -D -E -F
		 /// 0- SP  a  b  c  d  e  f  g    h  i  j  k  l  m  n  o
		 /// 1-  p  q  r  s  t  u  v  w    x  y  z
		 /// 2-  0  1  2  3  4  5  6  7    8  9  _  .  -  :  /  +
		 /// 3-  ,  '  @  |  ;  *  ?  &    %  #  (  )  $  <  >  =
		 /// </pre>
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       URI(6, 6) { int encTranslate(byte b) { if(b == 0) { return 0; } if(b >= 0x61 && b <= 0x7A) { return b - 0x60; } if(b >= 0x30 && b <= 0x39) { return b - 0x10; } if(b >= 0x1 && b <= 0x16) { return b + 0x29; } throw cannotEncode(b); } int encPunctuation(byte b) { throw cannotEncode(b); } char decTranslate(byte codePoint) { if(codePoint == 0) { return ' '; } if(codePoint <= 0x1A) { return(char)(codePoint + 'a' - 1); } if(codePoint <= 0x29) { return(char)(codePoint - 0x20 + '0'); } if(codePoint <= 0x2E) { return decPunctuation(codePoint - 0x29); } return decPunctuation(codePoint - 0x2F + 9); } },
		 /// <summary>
		 /// Alpha-numerical characters space and underscore.
		 /// 
		 /// <pre>
		 ///    -0 -1 -2 -3 -4 -5 -6 -7   -8 -9 -A -B -C -D -E -F
		 /// 0- SP  A  B  C  D  E  F  G    H  I  J  K  L  M  N  O
		 /// 1-  P  Q  R  S  T  U  V  W    X  Y  Z  0  1  2  3  4
		 /// 2-  _  a  b  c  d  e  f  g    h  i  j  k  l  m  n  o
		 /// 3-  p  q  r  s  t  u  v  w    x  y  z  5  6  7  8  9
		 /// </pre>
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       ALPHANUM(7, 6) { char decTranslate(byte codePoint) { return EUROPEAN.decTranslate((byte)(codePoint + 0x40)); } int encTranslate(byte b) { if(b < 0x20) { return encPunctuation(b); } return EUROPEAN.encTranslate(b) - 0x40; } int encPunctuation(byte b) { switch(b) { case 0: return 0x00; case 1: return 0x20; default: throw cannotEncode(b); } } },
		 /// <summary>
		 /// Alpha-numerical characters space and underscore.
		 /// 
		 /// <pre>
		 ///    -0 -1 -2 -3 -4 -5 -6 -7   -8 -9 -A -B -C -D -E -F
		 /// 0- SP  A  B  C  D  E  F  G    H  I  J  K  L  M  N  O
		 /// 1-  P  Q  R  S  T  U  V  W    X  Y  Z  _  .  -  :  /
		 /// 2-  ;  a  b  c  d  e  f  g    h  i  j  k  l  m  n  o
		 /// 3-  p  q  r  s  t  u  v  w    x  y  z  +  ,  '  @  |
		 /// </pre>
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       ALPHASYM(8, 6) { char decTranslate(byte codePoint) { if(codePoint == 0x0) { return ' '; } if(codePoint <= 0x1A) { return(char)('A' + codePoint - 0x1); } if(codePoint <= 0x1F) { return decPunctuation(codePoint - 0x1B + 1); } if(codePoint == 0x20) { return ';'; } if(codePoint <= 0x3A) { return(char)('a' + codePoint - 0x21); } return decPunctuation(codePoint - 0x3B + 9); } int encTranslate(byte b) { if(b < 0x20) { return encPunctuation(b); } return b - 0x40; } int encPunctuation(byte b) { switch(b) { case 0x0: return 0x0; case 0x1: return 0x1B; case 0x2: return 0x1C; case 0x3: return 0x1D; case 0x4: return 0x1E; case 0x5: return 0x1F; case 0x6: return 0x3B; case 0x7: return 0x3C; case 0x8: return 0x3D; case 0x9: return 0x3E; case 0xA: return 0x3F; case 0xB: return 0x20; default: throw cannotEncode(b); } } },
		 /// <summary>
		 /// The most common European characters (latin-1 but with less punctuation).
		 /// 
		 /// <pre>
		 ///    -0 -1 -2 -3 -4 -5 -6 -7   -8 -9 -A -B -C -D -E -F
		 /// 0-  À  Á  Â  Ã  Ä  Å  Æ  Ç    È  É  Ê  Ë  Ì  Í  Î  Ï
		 /// 1-  Ð  Ñ  Ò  Ó  Ô  Õ  Ö  .    Ø  Ù  Ú  Û  Ü  Ý  Þ  ß
		 /// 2-  à  á  â  ã  ä  å  æ  ç    è  é  ê  ë  ì  í  î  ï
		 /// 3-  ð  ñ  ò  ó  ô  õ  ö  -    ø  ù  ú  û  ü  ý  þ  ÿ
		 /// 4- SP  A  B  C  D  E  F  G    H  I  J  K  L  M  N  O
		 /// 5-  P  Q  R  S  T  U  V  W    X  Y  Z  0  1  2  3  4
		 /// 6-  _  a  b  c  d  e  f  g    h  i  j  k  l  m  n  o
		 /// 7-  p  q  r  s  t  u  v  w    x  y  z  5  6  7  8  9
		 /// </pre>
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       EUROPEAN(9, 7) { char decTranslate(byte codePoint) { int code = codePoint & 0xFF; if(code < 0x40) { if(code == 0x17) { return '.'; } if(code == 0x37) { return '-'; } return(char)(code + 0xC0); } else { if(code == 0x40) { return ' '; } if(code == 0x60) { return '_'; } if(code >= 0x5B && code < 0x60) { return(char)('0' + code - 0x5B); } if(code >= 0x7B && code < 0x80) { return(char)('5' + code - 0x7B); } return(char) code; } } int encPunctuation(byte b) { switch(b) { case 0x00: return 0x40; case 0x01: return 0x60; case 0x02: return 0x17; case 0x03: return 0x37; case 0x07: return 0; default: throw cannotEncode(b); } } },
		 // ENCODING_LATIN1 is 10th
		 /// <summary>
		 /// Lower-case characters a-f and digits.
		 /// 
		 /// <pre>
		 ///    -0 -1 -2 -3 -4 -5 -6 -7 -8 -9 -A -B -C -D -E -F
		 /// 0-  0  1  2  3  4  5  6  7  8  9  a  b  c  d  e  f
		 /// </pre>
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       LOWERHEX(11, 4) { int encTranslate(byte b) { if(b >= '0' && b <= '9') { return b - '0'; } if(b >= 'a' && b <= 'f') { return b - 'a' + 10; } throw cannotEncode(b); } int encPunctuation(byte b) { throw cannotEncode(b); } char decTranslate(byte codePoint) { if(codePoint < 10) { return(char)(codePoint + '0'); } return(char)(codePoint + 'a' - 10); } },
		 /// <summary>
		 /// Upper-case characters A-F and digits.
		 /// 
		 /// <pre>
		 ///    -0 -1 -2 -3 -4 -5 -6 -7 -8 -9 -A -B -C -D -E -F
		 /// 0-  0  1  2  3  4  5  6  7  8  9  A  B  C  D  E  F
		 /// </pre>
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       UPPERHEX(12, 4) { int encTranslate(byte b) { if(b >= '0' && b <= '9') { return b - '0'; } if(b >= 'A' && b <= 'F') { return b - 'A' + 10; } throw cannotEncode(b); } int encPunctuation(byte b) { throw cannotEncode(b); } char decTranslate(byte codePoint) { if(codePoint < 10) { return(char)(codePoint + '0'); } return(char)(codePoint + 'A' - 10); } };

		 private static readonly IList<LongerShortString> valueList = new List<LongerShortString>();

		 public enum InnerEnum
		 {
			 NUMERICAL,
			 DATE,
			 UPPER,
			 LOWER,
			 EMAIL,
			 URI,
			 ALPHANUM,
			 ALPHASYM,
			 EUROPEAN,
			 LOWERHEX,
			 UPPERHEX
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 private LongerShortString( string name, InnerEnum innerEnum )
		 {
			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }
		 public static readonly int REMOVE_LARGE_ENCODINGS_MASK = invertedBitMask( ALPHANUM, ALPHASYM, URI, EUROPEAN );
		 public static readonly LongerShortString[] ENCODINGS = values();
		 public static readonly int ENCODING_COUNT = ENCODINGS.Length;
		 public static readonly int ALL_BIT_MASK = bitMask( LongerShortString.values() );
		 public const int ENCODING_UTF8 = 0;
		 public const int ENCODING_LATIN1 = 10;
		 private const int HEADER_SIZE = 39; // bits

		 internal readonly int encodingHeader;
		 internal readonly long mask;
		 internal readonly int step;

		 internal LongerShortString( string name, InnerEnum innerEnum, int encodingHeader, int step )
		 {
			  this.EncodingHeader = encodingHeader;
			  this.Mask = Bits.rightOverflowMask( step );
			  this.Step = step;

			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 internal int MaxLength( int payloadSize )
		 {
			  // key-type-encoding-length
			  return ( ( payloadSize << 3 ) - 24 - 4 - 4 - 6 ) / Step;
		 }

		 internal System.ArgumentException CannotEncode( sbyte b )
		 {
			  return new System.ArgumentException( "Cannot encode as " + this.name() + ": " + b );
		 }

		 /// <summary>
		 /// Lookup table for decoding punctuation </summary>
		 private static readonly char[] PUNCTUATION = new char[] { ' ', '_', '.', '-', ':', '/', ' ', '.', '-', '+', ',', '\'', '@', '|', ';', '*', '?', '&', '%', '#', '(', ')', '$', '<', '>', '=' };

		 internal char DecPunctuation( int code )
		 {
			  return _punctuation[code];
		 }

		 internal int EncTranslate( sbyte b )
		 {
			  if ( b < 0 )
			  {
					return ( 0xFF & b ) - 0xC0; // European chars
			  }
			  if ( b < 0x20 )
			  {
					return EncPunctuation( b ); // Punctuation
			  }
			  if ( b >= ( sbyte )'0' && b <= ( sbyte )'4' )
			  {
					return 0x5B + b - '0'; // Numbers
			  }
			  if ( b >= ( sbyte )'5' && b <= ( sbyte )'9' )
			  {
					return 0x7B + b - '5'; // Numbers
			  }
			  return b; // Alphabetical
		 }

		 internal abstract int encPunctuation( sbyte b );

		 internal abstract char decTranslate( sbyte codePoint );

		 /// <summary>
		 /// Encodes a short string.
		 /// </summary>
		 /// <param name="string"> the string to encode. </param>
		 /// <param name="target"> the property record to store the encoded string in </param>
		 /// <returns> <code>true</code> if the string could be encoded as a short
		 ///         string, <code>false</code> if it couldn't. </returns>
		 /*
		  * Intermediate code table
		  *    -0 -1 -2 -3 -4 -5 -6 -7   -8 -9 -A -B -C -D -E -F
		  * 0- SP  _  .  -  :  /  +  ,    '  @  |  ;  *  ?  &  %
		  * 1-  #  (  )  $  <  >  =
		  * 2-
		  * 3-  0  1  2  3  4  5  6  7    8  9
		  * 4-     A  B  C  D  E  F  G    H  I  J  K  L  M  N  O
		  * 5-  P  Q  R  S  T  U  V  W    X  Y  Z
		  * 6-     a  b  c  d  e  f  g    h  i  j  k  l  m  n  o
		  * 7-  p  q  r  s  t  u  v  w    x  y  z
		  * 8-
		  * 9-
		  * A-
		  * B-
		  * C-  À  Á  Â  Ã  Ä  Å  Æ  Ç    È  É  Ê  Ë  Ì  Í  Î  Ï
		  * D-  Ð  Ñ  Ò  Ó  Ô  Õ  Ö       Ø  Ù  Ú  Û  Ü  Ý  Þ  ß
		  * E-  à  á  â  ã  ä  å  æ  ç    è  é  ê  ë  ì  í  î  ï
		  * F-  ð  ñ  ò  ó  ô  õ  ö       ø  ù  ú  û  ü  ý  þ  ÿ
		  */
		 public static bool Encode( int keyId, string @string, Neo4Net.Kernel.Impl.Store.Records.PropertyBlock target, int payloadSize )
		 {
			  // NUMERICAL can carry most characters, so compare to that
			  int dataLength = @string.Length;
			  // We only use 6 bits for storing the string length
			  // TODO could be dealt with by having string length zero and go for null bytes,
			  // at least for LATIN1 (that's what the ShortString implementation initially did)
			  if ( dataLength > NUMERICAL.maxLength( payloadSize ) || dataLength > 63 )
			  {
					return false; // Not handled by any encoding
			  }

			  // Allocate space for the intermediate representation
			  // (using the intermediate representation table above)
			  sbyte[] data = new sbyte[dataLength];

			  // Keep track of the possible encodings that can be used for the string
			  // 0 means none applies
			  int encodings = DetermineEncoding( @string, data, dataLength, payloadSize );
			  if ( encodings != 0 && TryEncode( encodings, keyId, target, payloadSize, data, dataLength ) )
			  {
					return true;
			  }
			  return EncodeWithCharSet( keyId, @string, target, payloadSize, dataLength );
		 }

		 private static bool EncodeWithCharSet( int keyId, string @string, Neo4Net.Kernel.Impl.Store.Records.PropertyBlock target, int payloadSize, int stringLength )
		 {
			  int maxBytes = PropertyType.PayloadSize;
			  if ( stringLength <= maxBytes - 5 )
			  {
					return EncodeLatin1( keyId, @string, target ) || EncodeUTF8( keyId, @string, target, payloadSize );
			  }
			  return false;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private static boolean tryEncode(int encodings, int keyId, org.neo4j.kernel.impl.store.record.PropertyBlock target, int payloadSize, byte[] data, final int length)
		 private static bool TryEncode( int encodings, int keyId, Neo4Net.Kernel.Impl.Store.Records.PropertyBlock target, int payloadSize, sbyte[] data, int length )
		 {
			  // find encoders in order that are still selected and try to encode the data
			  foreach ( LongerShortString encoding in Encodings )
			  {
					if ( ( encoding.BitMask() & encodings ) == 0 )
					{
						 gotoContinue;
					}
					if ( encoding.DoEncode( keyId, data, target, payloadSize, length ) )
					{
						 return true;
					}
			  }
			  return false;
		 }

		 // inverted combined bit-mask for the encoders
		 internal static int InvertedBitMask( params LongerShortString[] encoders )
		 {
			  return ~BitMask( encoders );
		 }

		 // combined bit-mask for the encoders
		 private static int BitMask( LongerShortString[] encoders )
		 {
			  int result = 0;
			  foreach ( LongerShortString encoder in encoders )
			  {
					result |= encoder.BitMask();
			  }
			  return result;
		 }

		 // translation lookup for each ascii character
		 private const int TRANSLATION_COUNT = 256;
		 // transformation for the char to byte according to the default translation table
		 private static readonly sbyte[] TRANSLATION = new sbyte[TRANSLATION_COUNT];
		 // mask for encoders that are not applicable for this character
		 private static readonly int[] REMOVE_MASK = new int[TRANSLATION_COUNT];

		 private static void SetUp( char pos, int value, params LongerShortString[] removeEncodings )
		 {
			  _translation[pos] = ( sbyte ) value;
			  _removeMask[pos] = InvertedBitMask( removeEncodings );
		 }

		 static LongerShortString()
		 {
			  Arrays.fill( _translation, unchecked( ( sbyte ) 0xFF ) );
			  Arrays.fill( _removeMask, InvertedBitMask( Encodings ) );
			  SetUp( ' ', 0, EMAIL, LOWERHEX, UPPERHEX );
			  SetUp( '_', 1, NUMERICAL, DATE, LOWERHEX, UPPERHEX );
			  SetUp( '.', 2, DATE, ALPHANUM, LOWERHEX, UPPERHEX );
			  SetUp( '-', 3, ALPHANUM, LOWERHEX, UPPERHEX );
			  SetUp( ':', 4, ALPHANUM, NUMERICAL, EUROPEAN, EMAIL, LOWERHEX, UPPERHEX );
			  SetUp( '/', 5, ALPHANUM, NUMERICAL, EUROPEAN, EMAIL, LOWERHEX, UPPERHEX );
			  SetUp( '+', 6, UPPER, LOWER, ALPHANUM, EUROPEAN, LOWERHEX, UPPERHEX );
			  SetUp( ',', 7, UPPER, LOWER, ALPHANUM, EUROPEAN, LOWERHEX, UPPERHEX );
			  SetUp( '\'', 8, DATE, UPPER, LOWER, EMAIL, ALPHANUM, EUROPEAN, LOWERHEX, UPPERHEX );
			  SetUp( '@', 9, NUMERICAL, DATE, UPPER, LOWER, ALPHANUM, EUROPEAN, LOWERHEX, UPPERHEX );
			  SetUp( '|', 0xA, NUMERICAL, DATE, UPPER, LOWER, EMAIL, URI, ALPHANUM, EUROPEAN, LOWERHEX, UPPERHEX );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final LongerShortString[] retainUri = {NUMERICAL, DATE, UPPER, LOWER, EMAIL, ALPHANUM, ALPHASYM, EUROPEAN, LOWERHEX, UPPERHEX};
			  LongerShortString[] retainUri = new LongerShortString[] { NUMERICAL, DATE, UPPER, LOWER, EMAIL, ALPHANUM, ALPHASYM, EUROPEAN, LOWERHEX, UPPERHEX };
			  SetUp( ';', 0xB, retainUri );
			  SetUp( '*', 0xC, retainUri );
			  SetUp( '?', 0xD, retainUri );
			  SetUp( '&', 0xE, retainUri );
			  SetUp( '%', 0xF, retainUri );
			  SetUp( '#', 0x10, retainUri );
			  SetUp( '(', 0x11, retainUri );
			  SetUp( ')', 0x12, retainUri );
			  SetUp( '$', 0x13, retainUri );
			  SetUp( '<', 0x14, retainUri );
			  SetUp( '>', 0x15, retainUri );
			  SetUp( '=', 0x16, retainUri );
			  for ( char c = 'A'; c <= 'F'; c++ )
			  {
					SetUp( c, ( sbyte ) c, NUMERICAL, DATE, LOWER, EMAIL, URI, LOWERHEX );
			  }
			  for ( char c = 'G'; c <= 'Z'; c++ )
			  {
					SetUp( c, ( sbyte ) c, NUMERICAL, DATE, LOWER, EMAIL, URI, LOWERHEX, UPPERHEX );
			  }
			  for ( char c = 'a'; c <= 'f'; c++ )
			  {
					SetUp( c, ( sbyte ) c, NUMERICAL, DATE, UPPER, UPPERHEX );
			  }
			  for ( char c = 'g'; c <= 'z'; c++ )
			  {
					SetUp( c, ( sbyte ) c, NUMERICAL, DATE, UPPER, UPPERHEX, LOWERHEX );
			  }
			  for ( char c = '0'; c <= '9'; c++ )
			  {
					SetUp( c, ( sbyte ) c, UPPER, LOWER, EMAIL, ALPHASYM );
			  }
			  for ( char c = 'À'; c <= 'ÿ'; c++ )
			  {
					if ( c != ( char )0xD7 && c != ( char )0xF7 )
					{
						 SetUp( c, ( sbyte ) c, NUMERICAL, DATE, UPPER, LOWER, EMAIL, URI, ALPHANUM, ALPHASYM, LOWERHEX, UPPERHEX );
					}
			  }
			  foreach ( LongerShortString encoding in Encodings )
			  {
					_encodingsByEncoding[encoding.EncodingHeader] = encoding;
			  }

			 valueList.Add( NUMERICAL );
			 valueList.Add( DATE );
			 valueList.Add( UPPER );
			 valueList.Add( LOWER );
			 valueList.Add( EMAIL );
			 valueList.Add( URI );
			 valueList.Add( ALPHANUM );
			 valueList.Add( ALPHASYM );
			 valueList.Add( EUROPEAN );
			 valueList.Add( LOWERHEX );
			 valueList.Add( UPPERHEX );
		 }

		 private static int DetermineEncoding( string @string, sbyte[] data, int length, int payloadSize )
		 {
			  if ( length == 0 )
			  {
					return 0;
			  }
			  int encodings = AllBitMask;
			  // filter out larger encodings in one go
			  if ( length > ALPHANUM.maxLength( payloadSize ) )
			  {
					encodings &= RemoveLargeEncodingsMask;
			  }
			  for ( int i = 0; i < length; i++ )
			  {
					char c = @string[i];
					// non ASCII chars not supported
					if ( c >= ( char )TRANSLATION_COUNT )
					{
						 return 0;
					}
					data[i] = _translation[c];
					// remove not matching encoders
					encodings &= _removeMask[c];
					if ( encodings == 0 )
					{
						 return 0;
					}
			  }
			  return encodings;
		 }

		 internal int BitMask()
		 {
			  return 1 << ordinal();
		 }

		 private static void WriteHeader( Neo4Net.Kernel.impl.util.Bits bits, int keyId, int encoding, int stringLength )
		 {
			  // [][][][ lll,llle][eeee,tttt][kkkk,kkkk][kkkk,kkkk][kkkk,kkkk]
			  bits.Put( keyId, 24 ).put( PropertyType.ShortString.intValue(), 4 ).put(encoding, 5).put(stringLength, 6);
		 }

		 /// <summary>
		 /// Decode a short string represented as a long[]
		 /// </summary>
		 /// <param name="block"> the value to decode to a short string. </param>
		 /// <returns> the decoded short string </returns>
		 public static Neo4Net.Values.Storable.TextValue Decode( Neo4Net.Kernel.Impl.Store.Records.PropertyBlock block )
		 {
			  return Decode( block.ValueBlocks, 0, block.ValueBlocks.Length );
		 }

		 public static Neo4Net.Values.Storable.TextValue Decode( long[] blocks, int offset, int length )
		 {
			  long firstLong = blocks[offset];
			  if ( ( firstLong & 0xFFFFFF0FFFFFFFFFL ) == 0 )
			  {
					return Values.EMPTY_STRING;
			  }
			  // key(24b) + type(4) = 28
			  int encoding = ( int )( ( long )( ( ulong )( firstLong & 0x1F0000000L ) >> 28 ) ); // 5 bits of encoding
			  int stringLength = ( int )( ( long )( ( ulong )( firstLong & 0x7E00000000L ) >> 33 ) ); // 6 bits of stringLength
			  if ( encoding == LongerShortString.ENCODING_UTF8 )
			  {
					return DecodeUTF8( blocks, offset, stringLength );
			  }
			  if ( encoding == ENCODING_LATIN1 )
			  {
					return DecodeLatin1( blocks, offset, stringLength );
			  }

			  LongerShortString table = GetEncodingTable( encoding );
			  Debug.Assert( table != null, "We only decode LongerShortStrings after we have consistently read the PropertyBlock " + );
						 "data from the page cache. Thus, we should never have an invalid encoding header here.";
			  char[] result = new char[stringLength];
			  // encode shifts in the bytes with the first char at the MSB, therefore
			  // we must "unshift" in the reverse order
			  Decode( result, blocks, offset, table );

			  // We know the char array is unshared, so use sharing constructor explicitly
			  return Values.stringValue( UnsafeUtil.newSharedArrayString( result ) );
		 }

		 private static void Decode( char[] result, long[] blocks, int offset, LongerShortString table )
		 {
			  int block = offset;
			  int maskShift = HEADER_SIZE;
			  long baseMask = table.Mask;
			  for ( int i = 0; i < result.Length; i++ )
			  {
					sbyte codePoint = ( sbyte )( ( ( long )( ( ulong )blocks[block] >> maskShift ) ) & baseMask );
					maskShift += table.Step;
					if ( maskShift >= 64 && block + 1 < blocks.Length )
					{
						 maskShift %= 64;
						 codePoint |= ( sbyte )( ( blocks[++block] & ( ( long )( ( ulong )baseMask >> ( table.Step - maskShift ) ) ) ) << ( table.Step - maskShift ) );
					}
					result[i] = table.DecTranslate( codePoint );
			  }
		 }

		 // lookup table by encoding header
		 // +2 because of ENCODING_LATIN1 gap and one based index
		 private static readonly LongerShortString[] ENCODINGS_BY_ENCODING = new LongerShortString[ENCODING_COUNT + 2];


		 /// <summary>
		 /// Get encoding table for the given encoding header, or {@code null} if the encoding header is invalid.
		 /// </summary>
		 private static LongerShortString GetEncodingTable( int encodingHeader )
		 {
			  if ( encodingHeader < 0 | _encodingsByEncoding.Length <= encodingHeader )
			  {
					return null;
			  }
			  return _encodingsByEncoding[encodingHeader];
		 }

		 private static Neo4Net.Kernel.impl.util.Bits NewBits( LongerShortString encoding, int length )
		 {
			  return Bits.bits( CalculateNumberOfBlocksUsed( encoding, length ) << 3 ); //*8
		 }

		 private static Neo4Net.Kernel.impl.util.Bits NewBitsForStep8( int length )
		 {
			  return Bits.bits( CalculateNumberOfBlocksUsedForStep8( length ) << 3 ); //*8
		 }

		 private static bool EncodeLatin1( int keyId, string @string, Neo4Net.Kernel.Impl.Store.Records.PropertyBlock target )
		 {
			  int length = @string.Length;
			  Bits bits = NewBitsForStep8( length );
			  WriteHeader( bits, keyId, ENCODING_LATIN1, length );
			  if ( !WriteLatin1Characters( @string, bits ) )
			  {
					return false;
			  }
			  target.ValueBlocks = bits.Longs;
			  return true;
		 }

		 public static bool WriteLatin1Characters( string @string, Neo4Net.Kernel.impl.util.Bits bits )
		 {
			  int length = @string.Length;
			  for ( int i = 0; i < length; i++ )
			  {
					char c = @string[i];
					if ( c >= ( char )256 )
					{
						 return false;
					}
					bits.put( c, 8 ); // Just the lower byte
			  }
			  return true;
		 }

		 private static bool EncodeUTF8( int keyId, string @string, Neo4Net.Kernel.Impl.Store.Records.PropertyBlock target, int payloadSize )
		 {
			  sbyte[] bytes = @string.GetBytes( Encoding.UTF8 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int length = bytes.length;
			  int length = bytes.Length;
			  if ( length > payloadSize - 3 - 2 )
			  {
					return false;
			  }
			  Bits bits = NewBitsForStep8( length );
			  WriteHeader( bits, keyId, ENCODING_UTF8, length ); // In this case it isn't the string length, but the number of bytes
			  foreach ( sbyte value in bytes )
			  {
					bits.Put( value );
			  }
			  target.ValueBlocks = bits.Longs;
			  return true;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private boolean doEncode(int keyId, byte[] data, org.neo4j.kernel.impl.store.record.PropertyBlock target, int payloadSize, final int length)
		 private bool DoEncode( int keyId, sbyte[] data, Neo4Net.Kernel.Impl.Store.Records.PropertyBlock target, int payloadSize, int length )
		 {
			  if ( length > MaxLength( payloadSize ) )
			  {
					return false;
			  }
			  Bits bits = NewBits( this, length );
			  WriteHeader( bits, keyId, EncodingHeader, length );
			  if ( length > 0 )
			  {
					TranslateData( bits, data, length, Step );
			  }
			  target.ValueBlocks = bits.Longs;
			  return true;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private void translateData(org.neo4j.kernel.impl.util.Bits bits, byte[] data, int length, final int step)
		 private void TranslateData( Neo4Net.Kernel.impl.util.Bits bits, sbyte[] data, int length, int step )
		 {
			  for ( int i = 0; i < length; i++ )
			  {
					bits.Put( EncTranslate( data[i] ), step );
			  }
		 }

		 private static Neo4Net.Values.Storable.TextValue DecodeLatin1( long[] blocks, int offset, int stringLength )
		 {
			  char[] result = new char[stringLength];
			  int block = offset;
			  int maskShift = HEADER_SIZE;
			  for ( int i = 0; i < result.Length; i++ )
			  {
					char codePoint = ( char )( ( ( long )( ( ulong )blocks[block] >> maskShift ) ) & 0xFF );
					maskShift += 8;
					if ( maskShift >= 64 )
					{
						 maskShift %= 64;
						 codePoint |= ( blocks[++block] & ( ( int )( ( uint )0xFF >> ( 8 - maskShift ) ) ) ) << ( 8 - maskShift );
					}
					result[i] = codePoint;
			  }
			  return Values.stringValue( UnsafeUtil.newSharedArrayString( result ) );
		 }

		 private static Neo4Net.Values.Storable.TextValue DecodeUTF8( long[] blocks, int offset, int stringLength )
		 {
			  sbyte[] result = new sbyte[stringLength];
			  int block = offset;
			  int maskShift = HEADER_SIZE;
			  for ( int i = 0; i < result.Length; i++ )
			  {
					sbyte codePoint = ( sbyte )( ( long )( ( ulong )blocks[block] >> maskShift ) );
					maskShift += 8;
					if ( maskShift >= 64 )
					{
						 maskShift %= 64;
						 codePoint |= ( sbyte )( ( blocks[++block] & ( ( int )( ( uint )0xFF >> ( 8 - maskShift ) ) ) ) << ( 8 - maskShift ) );
					}
					result[i] = codePoint;
			  }
			  return Values.utf8Value( result );
		 }

		 public static int CalculateNumberOfBlocksUsed( long firstBlock )
		 {
			  /*
			   * [ lll,llle][eeee,tttt][kkkk,kkkk][kkkk,kkkk][kkkk,kkkk]
			   */
			  int encoding = ( int )( ( firstBlock & 0x1F0000000L ) >> 28 );
			  int length = ( int )( ( firstBlock & 0x7E00000000L ) >> 33 );
			  if ( encoding == ENCODING_UTF8 || encoding == ENCODING_LATIN1 )
			  {
					return CalculateNumberOfBlocksUsedForStep8( length );
			  }

			  LongerShortString encodingTable = GetEncodingTable( encoding );
			  if ( encodingTable == null )
			  {
					// We probably did an inconsistent read of the first block
					return PropertyType.BLOCKS_USED_FOR_BAD_TYPE_OR_ENCODING;
			  }
			  return CalculateNumberOfBlocksUsed( encodingTable, length );
		 }

		 public static int CalculateNumberOfBlocksUsedForStep8( int length )
		 {
			  return TotalBits( length << 3 ); // * 8
		 }

		 public static int CalculateNumberOfBlocksUsed( LongerShortString encoding, int length )
		 {
			  return TotalBits( length * encoding.Step );
		 }

		 private static int TotalBits( int bitsForCharacters )
		 {
			  int bitsInTotal = 24 + 4 + 5 + 6 + bitsForCharacters;
			  return ( ( bitsInTotal - 1 ) >> 6 ) + 1; // /64
		 }

		public static IList<LongerShortString> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public override string ToString()
		{
			return nameValue;
		}

		public static LongerShortString valueOf( string name )
		{
			foreach ( LongerShortString enumInstance in LongerShortString.valueList )
			{
				if ( enumInstance.nameValue == name )
				{
					return enumInstance;
				}
			}
			throw new System.ArgumentException( name );
		}
	}

}