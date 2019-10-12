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
namespace Org.Neo4j.Bolt.v1.packstream
{

	public class PackedOutputArray : PackOutput
	{
		 internal MemoryStream Raw;
		 internal DataOutputStream Data;

		 public PackedOutputArray()
		 {
			  Raw = new MemoryStream();
			  Data = new DataOutputStream( Raw );
		 }

		 public override void BeginMessage()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void messageSucceeded() throws java.io.IOException
		 public override void MessageSucceeded()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void messageFailed() throws java.io.IOException
		 public override void MessageFailed()
		 {
		 }

		 public override PackOutput Flush()
		 {
			  return this;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public PackOutput writeByte(byte value) throws java.io.IOException
		 public override PackOutput WriteByte( sbyte value )
		 {
			  Data.write( value );
			  return this;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public PackOutput writeBytes(ByteBuffer buffer) throws java.io.IOException
		 public override PackOutput WriteBytes( ByteBuffer buffer )
		 {
			  while ( buffer.remaining() > 0 )
			  {
					Data.writeByte( buffer.get() );
			  }
			  return this;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public PackOutput writeBytes(byte[] bytes, int offset, int amountToWrite) throws java.io.IOException
		 public override PackOutput WriteBytes( sbyte[] bytes, int offset, int amountToWrite )
		 {
			  Data.write( bytes, offset, amountToWrite );
			  return this;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public PackOutput writeShort(short value) throws java.io.IOException
		 public override PackOutput WriteShort( short value )
		 {
			  Data.writeShort( value );
			  return this;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public PackOutput writeInt(int value) throws java.io.IOException
		 public override PackOutput WriteInt( int value )
		 {
			  Data.writeInt( value );
			  return this;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public PackOutput writeLong(long value) throws java.io.IOException
		 public override PackOutput WriteLong( long value )
		 {
			  Data.writeLong( value );
			  return this;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public PackOutput writeDouble(double value) throws java.io.IOException
		 public override PackOutput WriteDouble( double value )
		 {
			  Data.writeDouble( value );
			  return this;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  Data.close();
		 }

		 public virtual sbyte[] Bytes()
		 {
			  return Raw.toByteArray();
		 }

	}

}