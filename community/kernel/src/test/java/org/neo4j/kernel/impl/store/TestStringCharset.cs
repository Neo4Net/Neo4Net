using System;
using System.Collections.Generic;

/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
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
namespace Org.Neo4j.Kernel.impl.store
{

	public abstract class TestStringCharset
	{
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       UNIFORM_ASCII { String randomString(int maxLen) { char[] chars = new char[random.nextInt(maxLen + 1)]; for(int i = 0; i < chars.length; i++) { chars[i] = (char)(0x20 + random.nextInt(94)); } return new String(chars); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       SYMBOLS { String randomString(int maxLen) { char[] chars = new char[random.nextInt(maxLen + 1)]; for(int i = 0; i < chars.length; i++) { chars[i] = SYMBOL_CHARS[random.nextInt(SYMBOL_CHARS.length)]; } return new String(chars); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       UNIFORM_LATIN { String randomString(int maxLen) { char[] chars = new char[random.nextInt(maxLen + 1)]; for(int i = 0; i < chars.length; i++) { chars[i] = (char)(0x20 + random.nextInt(0xC0)); if(chars[i] > 0x7f) { chars[i] += 0x20; } } return new String(chars); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       LONG { String randomString(int maxLen) { return System.Convert.ToString(random.nextLong() % ((long) Math.pow(10, maxLen))); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       INT { String randomString(int maxLen) { return System.Convert.ToString(random.nextInt()); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       UNICODE { String randomString(int maxLen) { char[] chars = new char[random.nextInt(maxLen + 1)]; for(int i = 0; i < chars.length; i++) { chars[i] = (char)(1 + random.nextInt(0xD7FE)); } return new String(chars); } },
		 public static readonly TestStringCharset  = new TestStringCharset( "", InnerEnum. );

		 private static readonly IList<TestStringCharset> valueList = new List<TestStringCharset>();

		 public enum InnerEnum
		 {
			 UNIFORM_ASCII,
			 SYMBOLS,
			 UNIFORM_LATIN,
			 LONG,
			 INT,
			 UNICODE,
          
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 private TestStringCharset( string name, InnerEnum innerEnum )
		 {
			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }
		 internal static char[] SYMBOL_CHARS = new char[26 + 26 + 10 + 1];

		 static TestStringCharset()
		 {
			  SymbolChars[0] = '_';
			  int i = 1;
			  for ( char c = '0'; c <= '9'; c++ )
			  {
					SymbolChars[i++] = c;
			  }
			  for ( char c = 'A'; c <= 'Z'; c++ )
			  {
					SymbolChars[i++] = c;
			  }
			  for ( char c = 'a'; c <= 'z'; c++ )
			  {
					SymbolChars[i++] = c;
			  }

			 valueList.Add( UNIFORM_ASCII );
			 valueList.Add( SYMBOLS );
			 valueList.Add( UNIFORM_LATIN );
			 valueList.Add( LONG );
			 valueList.Add( INT );
			 valueList.Add( UNICODE );
			 valueList.Add();
		 }

		 private static Random random = new Random();

		 internal abstract string randomString( int maxLen );

		public static IList<TestStringCharset> values()
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

		public static TestStringCharset valueOf( string name )
		{
			foreach ( TestStringCharset enumInstance in TestStringCharset.valueList )
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