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
namespace Org.Neo4j.Kernel.impl.transaction.log
{

	/// <summary>
	/// Decorator around a <seealso cref="LogVersionedStoreChannel"/> making it expose <seealso cref="FlushablePositionAwareChannel"/>. This
	/// implementation uses a <seealso cref="PhysicalFlushableChannel"/>, which provides buffering for write operations over the
	/// decorated channel.
	/// </summary>
	public class PositionAwarePhysicalFlushableChannel : FlushablePositionAwareChannel
	{
		 private LogVersionedStoreChannel _logVersionedStoreChannel;
		 private readonly PhysicalFlushableChannel _channel;

		 public PositionAwarePhysicalFlushableChannel( LogVersionedStoreChannel logVersionedStoreChannel )
		 {
			  this._logVersionedStoreChannel = logVersionedStoreChannel;
			  this._channel = new PhysicalFlushableChannel( logVersionedStoreChannel );
		 }

		 public PositionAwarePhysicalFlushableChannel( LogVersionedStoreChannel logVersionedStoreChannel, int bufferSize )
		 {
			  this._logVersionedStoreChannel = logVersionedStoreChannel;
			  this._channel = new PhysicalFlushableChannel( logVersionedStoreChannel, bufferSize );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public LogPositionMarker getCurrentPosition(LogPositionMarker positionMarker) throws java.io.IOException
		 public override LogPositionMarker GetCurrentPosition( LogPositionMarker positionMarker )
		 {
			  positionMarker.Mark( _logVersionedStoreChannel.Version, _channel.position() );
			  return positionMarker;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.io.Flushable prepareForFlush() throws java.io.IOException
		 public override Flushable PrepareForFlush()
		 {
			  return _channel.prepareForFlush();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public FlushableChannel put(byte value) throws java.io.IOException
		 public override FlushableChannel Put( sbyte value )
		 {
			  return _channel.put( value );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public FlushableChannel putShort(short value) throws java.io.IOException
		 public override FlushableChannel PutShort( short value )
		 {
			  return _channel.putShort( value );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public FlushableChannel putInt(int value) throws java.io.IOException
		 public override FlushableChannel PutInt( int value )
		 {
			  return _channel.putInt( value );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public FlushableChannel putLong(long value) throws java.io.IOException
		 public override FlushableChannel PutLong( long value )
		 {
			  return _channel.putLong( value );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public FlushableChannel putFloat(float value) throws java.io.IOException
		 public override FlushableChannel PutFloat( float value )
		 {
			  return _channel.putFloat( value );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public FlushableChannel putDouble(double value) throws java.io.IOException
		 public override FlushableChannel PutDouble( double value )
		 {
			  return _channel.putDouble( value );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public FlushableChannel put(byte[] value, int length) throws java.io.IOException
		 public override FlushableChannel Put( sbyte[] value, int length )
		 {
			  return _channel.put( value, length );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  _channel.Dispose();
		 }

		 public virtual LogVersionedStoreChannel Channel
		 {
			 set
			 {
				  this._logVersionedStoreChannel = value;
				  this._channel.Channel = value;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void setCurrentPosition(LogPosition position) throws java.io.IOException, UnsupportedOperationException
		 public virtual LogPosition CurrentPosition
		 {
			 set
			 {
				  _channel.position( value.ByteOffset );
			 }
		 }
	}

}