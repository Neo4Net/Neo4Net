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
namespace Neo4Net.Bolt.v1.packstream
{

	public class PackedInputArray : PackInput
	{
		 private readonly MemoryStream _bytes;
		 private readonly DataInputStream _data;

		 public PackedInputArray( sbyte[] bytes )
		 {
			  this._bytes = new MemoryStream( bytes );
			  this._data = new DataInputStream( this._bytes );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public byte readByte() throws java.io.IOException
		 public override sbyte ReadByte()
		 {
			  return _data.readByte();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public short readShort() throws java.io.IOException
		 public override short ReadShort()
		 {
			  return _data.readShort();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public int readInt() throws java.io.IOException
		 public override int ReadInt()
		 {
			  return _data.readInt();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long readLong() throws java.io.IOException
		 public override long ReadLong()
		 {
			  return _data.readLong();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public double readDouble() throws java.io.IOException
		 public override double ReadDouble()
		 {
			  return _data.readDouble();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public PackInput readBytes(byte[] into, int offset, int toRead) throws java.io.IOException
		 public override PackInput ReadBytes( sbyte[] into, int offset, int toRead )
		 {
			  // TODO: fix the interface and all implementations - we should probably
			  // TODO: return the no of bytes read instead of the instance
			  _data.read( into, offset, toRead );
			  return this;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public byte peekByte() throws java.io.IOException
		 public override sbyte PeekByte()
		 {
			  _data.mark( 1 );
			  sbyte value = _data.readByte();
			  _data.reset();
			  return value;
		 }
	}

}