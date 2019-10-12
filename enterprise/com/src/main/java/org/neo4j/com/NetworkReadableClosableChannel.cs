/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
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
namespace Org.Neo4j.com
{

	using ChannelBuffer = org.jboss.netty.buffer.ChannelBuffer;

	using LogPositionMarker = Org.Neo4j.Kernel.impl.transaction.log.LogPositionMarker;
	using ReadableClosablePositionAwareChannel = Org.Neo4j.Kernel.impl.transaction.log.ReadableClosablePositionAwareChannel;
	using ReadPastEndException = Org.Neo4j.Storageengine.Api.ReadPastEndException;

	public class NetworkReadableClosableChannel : ReadableClosablePositionAwareChannel
	{
		 private readonly ChannelBuffer @delegate;

		 public NetworkReadableClosableChannel( ChannelBuffer input )
		 {
			  this.@delegate = input;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public byte get() throws java.io.IOException
		 public override sbyte Get()
		 {
			  try
			  {
					return @delegate.readByte();
			  }
			  catch ( System.IndexOutOfRangeException )
			  {
					throw ReadPastEndException.INSTANCE;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public short getShort() throws java.io.IOException
		 public virtual short Short
		 {
			 get
			 {
				  try
				  {
						return @delegate.readShort();
				  }
				  catch ( System.IndexOutOfRangeException )
				  {
						throw ReadPastEndException.INSTANCE;
				  }
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public int getInt() throws java.io.IOException
		 public virtual int Int
		 {
			 get
			 {
				  try
				  {
						return @delegate.readInt();
				  }
				  catch ( System.IndexOutOfRangeException )
				  {
						throw ReadPastEndException.INSTANCE;
				  }
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long getLong() throws java.io.IOException
		 public virtual long Long
		 {
			 get
			 {
				  try
				  {
						return @delegate.readLong();
				  }
				  catch ( System.IndexOutOfRangeException )
				  {
						throw ReadPastEndException.INSTANCE;
				  }
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public float getFloat() throws java.io.IOException
		 public virtual float Float
		 {
			 get
			 {
				  try
				  {
						return @delegate.readFloat();
				  }
				  catch ( System.IndexOutOfRangeException )
				  {
						throw ReadPastEndException.INSTANCE;
				  }
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public double getDouble() throws java.io.IOException
		 public virtual double Double
		 {
			 get
			 {
				  try
				  {
						return @delegate.readDouble();
				  }
				  catch ( System.IndexOutOfRangeException )
				  {
						throw ReadPastEndException.INSTANCE;
				  }
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void get(byte[] bytes, int length) throws java.io.IOException
		 public override void Get( sbyte[] bytes, int length )
		 {
			  try
			  {
					@delegate.readBytes( bytes, 0, length );
			  }
			  catch ( System.IndexOutOfRangeException )
			  {
					throw ReadPastEndException.INSTANCE;
			  }
		 }

		 public override LogPositionMarker GetCurrentPosition( LogPositionMarker positionMarker )
		 {
			  positionMarker.Unspecified();
			  return positionMarker;
		 }

		 public override void Close()
		 {
			  // no op
		 }
	}

}