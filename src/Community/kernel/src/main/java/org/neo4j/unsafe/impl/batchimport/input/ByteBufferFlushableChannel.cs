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
namespace Neo4Net.@unsafe.Impl.Batchimport.input
{

	using FlushableChannel = Neo4Net.Kernel.impl.transaction.log.FlushableChannel;
	using ReadableClosableChannel = Neo4Net.Kernel.impl.transaction.log.ReadableClosableChannel;

	/// <summary>
	/// Wraps a byte[] -> <seealso cref="ByteBuffer"/> -> <seealso cref="ReadableClosableChannel"/>
	/// </summary>
	public class ByteBufferFlushableChannel : FlushableChannel, Flushable
	{
		 private readonly ByteBuffer _buffer;

		 public ByteBufferFlushableChannel( ByteBuffer buffer )
		 {
			  this._buffer = buffer;
		 }

		 public override Flushable PrepareForFlush()
		 {
			  return this;
		 }

		 public override FlushableChannel Put( sbyte value )
		 {
			  _buffer.put( value );
			  return this;
		 }

		 public override FlushableChannel PutShort( short value )
		 {
			  _buffer.putShort( value );
			  return this;
		 }

		 public override FlushableChannel PutInt( int value )
		 {
			  _buffer.putInt( value );
			  return this;
		 }

		 public override FlushableChannel PutLong( long value )
		 {
			  _buffer.putLong( value );
			  return this;
		 }

		 public override FlushableChannel PutFloat( float value )
		 {
			  _buffer.putFloat( value );
			  return this;
		 }

		 public override FlushableChannel PutDouble( double value )
		 {
			  _buffer.putDouble( value );
			  return this;
		 }

		 public override FlushableChannel Put( sbyte[] value, int length )
		 {
			  _buffer.put( value, 0, length );
			  return this;
		 }

		 public override void Close()
		 {
		 }

		 public override void Flush()
		 {
		 }
	}

}