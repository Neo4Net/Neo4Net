/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4j Enterprise Edition. The included source
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
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.causalclustering.messaging.marshalling
{

	using WritableChannel = Neo4Net.Storageengine.Api.WritableChannel;

	public class OutputStreamWritableChannel : WritableChannel
	{
		 private readonly DataOutputStream _dataOutputStream;

		 public OutputStreamWritableChannel( Stream outputStream )
		 {
			  this._dataOutputStream = new DataOutputStream( outputStream );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.storageengine.api.WritableChannel put(byte value) throws java.io.IOException
		 public override WritableChannel Put( sbyte value )
		 {
			  _dataOutputStream.writeByte( value );
			  return this;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.storageengine.api.WritableChannel putShort(short value) throws java.io.IOException
		 public override WritableChannel PutShort( short value )
		 {
			  _dataOutputStream.writeShort( value );
			  return this;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.storageengine.api.WritableChannel putInt(int value) throws java.io.IOException
		 public override WritableChannel PutInt( int value )
		 {
			  _dataOutputStream.writeInt( value );
			  return this;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.storageengine.api.WritableChannel putLong(long value) throws java.io.IOException
		 public override WritableChannel PutLong( long value )
		 {
			  _dataOutputStream.writeLong( value );
			  return this;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.storageengine.api.WritableChannel putFloat(float value) throws java.io.IOException
		 public override WritableChannel PutFloat( float value )
		 {
			  _dataOutputStream.writeFloat( value );
			  return this;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.storageengine.api.WritableChannel putDouble(double value) throws java.io.IOException
		 public override WritableChannel PutDouble( double value )
		 {
			  _dataOutputStream.writeDouble( value );
			  return this;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.storageengine.api.WritableChannel put(byte[] value, int length) throws java.io.IOException
		 public override WritableChannel Put( sbyte[] value, int length )
		 {
			  _dataOutputStream.write( value, 0, length );
			  return this;
		 }
	}

}