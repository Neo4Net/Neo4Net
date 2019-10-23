/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.causalclustering.messaging.marshalling
{

	using ReadableChannel = Neo4Net.Kernel.Api.StorageEngine.ReadableChannel;

	public class InputStreamReadableChannel : ReadableChannel
	{
		 private readonly DataInputStream _dataInputStream;

		 public InputStreamReadableChannel( Stream inputStream )
		 {
			  this._dataInputStream = new DataInputStream( inputStream );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public byte get() throws java.io.IOException
		 public override sbyte Get()
		 {
			  return _dataInputStream.readByte();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public short getShort() throws java.io.IOException
		 public virtual short Short
		 {
			 get
			 {
				  return _dataInputStream.readShort();
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public int getInt() throws java.io.IOException
		 public virtual int Int
		 {
			 get
			 {
				  return _dataInputStream.readInt();
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long getLong() throws java.io.IOException
		 public virtual long Long
		 {
			 get
			 {
				  return _dataInputStream.readLong();
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public float getFloat() throws java.io.IOException
		 public virtual float Float
		 {
			 get
			 {
				  return _dataInputStream.readFloat();
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public double getDouble() throws java.io.IOException
		 public virtual double Double
		 {
			 get
			 {
				  return _dataInputStream.readDouble();
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void get(byte[] bytes, int length) throws java.io.IOException
		 public override void Get( sbyte[] bytes, int length )
		 {
			  _dataInputStream.read( bytes, 0, length );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  _dataInputStream.close();
		 }
	}

}