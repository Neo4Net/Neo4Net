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
namespace Neo4Net.Dbms.archive
{
	using ZstdInputStream = com.github.luben.zstd.ZstdInputStream;
	using ZstdOutputStream = com.github.luben.zstd.ZstdOutputStream;


	public abstract class CompressionFormat
	{
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       GZIP { public java.io.OutputStream compress(java.io.OutputStream stream) throws java.io.IOException { return new java.util.zip.GZIPOutputStream(stream); } public java.io.InputStream decompress(java.io.InputStream stream) throws java.io.IOException { return new java.util.zip.GZIPInputStream(stream); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       ZSTD { private final byte[] HEADER = new byte[] {'z', 's', 't', 'd'}; public java.io.OutputStream compress(java.io.OutputStream stream) throws java.io.IOException { com.github.luben.zstd.ZstdOutputStream zstdout = new com.github.luben.zstd.ZstdOutputStream(stream); zstdout.setChecksum(true); zstdout.write(HEADER); return zstdout; } public java.io.InputStream decompress(java.io.InputStream stream) throws java.io.IOException { com.github.luben.zstd.ZstdInputStream zstdin = new com.github.luben.zstd.ZstdInputStream(stream); byte[] header = new byte[HEADER.length]; if(zstdin.read(header) != HEADER.length || !java.util.Arrays.equals(header, HEADER)) { throw new java.io.IOException("Not in ZSTD format"); } return zstdin; } };

		 private static readonly IList<CompressionFormat> valueList = new List<CompressionFormat>();

		 static CompressionFormat()
		 {
			 valueList.Add( GZIP );
			 valueList.Add( ZSTD );
		 }

		 public enum InnerEnum
		 {
			 GZIP,
			 ZSTD
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 private CompressionFormat( string name, InnerEnum innerEnum )
		 {
			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public abstract java.io.OutputStream compress(java.io.OutputStream stream) throws java.io.IOException;
		 public abstract Stream compress( Stream stream );
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public abstract java.io.InputStream decompress(java.io.InputStream stream) throws java.io.IOException;
		 public abstract Stream decompress( Stream stream );

		public static IList<CompressionFormat> values()
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

		public static CompressionFormat valueOf( string name )
		{
			foreach ( CompressionFormat enumInstance in CompressionFormat.valueList )
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