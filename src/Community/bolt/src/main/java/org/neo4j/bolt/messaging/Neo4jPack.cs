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
namespace Neo4Net.Bolt.messaging
{

	using AnyValue = Neo4Net.Values.AnyValue;
	using MapValue = Neo4Net.Values.@virtual.MapValue;

	/// <summary>
	/// Represents a single Bolt message format by exposing a <seealso cref="Packer packer"/> and <seealso cref="Unpacker unpacker"/>
	/// for primitives of this format.
	/// </summary>
	public interface Neo4jPack : PackProvider, UnpackerProvider
	{
		 long Version();
	}

	 public interface Neo4jPack_Packer
	 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void pack(String value) throws java.io.IOException;
		  void Pack( string value );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void pack(org.neo4j.values.AnyValue value) throws java.io.IOException;
		  void Pack( AnyValue value );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void packStructHeader(int size, byte signature) throws java.io.IOException;
		  void PackStructHeader( int size, sbyte signature );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void packMapHeader(int size) throws java.io.IOException;
		  void PackMapHeader( int size );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void packListHeader(int size) throws java.io.IOException;
		  void PackListHeader( int size );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void flush() throws java.io.IOException;
		  void Flush();
	 }

	 public interface Neo4jPack_Unpacker
	 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.neo4j.values.AnyValue unpack() throws java.io.IOException;
		  AnyValue Unpack();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: String unpackString() throws java.io.IOException;
		  string UnpackString();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.neo4j.values.virtual.MapValue unpackMap() throws java.io.IOException;
		  MapValue UnpackMap();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: long unpackStructHeader() throws java.io.IOException;
		  long UnpackStructHeader();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: char unpackStructSignature() throws java.io.IOException;
		  char UnpackStructSignature();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: long unpackListHeader() throws java.io.IOException;
		  long UnpackListHeader();
	 }

}