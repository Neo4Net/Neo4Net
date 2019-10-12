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
namespace Org.Neo4j.@unsafe.Impl.Batchimport.input
{

	using ReadableClosableChannel = Org.Neo4j.Kernel.impl.transaction.log.ReadableClosableChannel;

	/// <summary>
	/// Wraps a byte[] -> <seealso cref="ByteBuffer"/> -> <seealso cref="ReadableClosableChannel"/>
	/// </summary>
	public class ByteBufferReadableChannel : ReadableClosableChannel
	{
		 private readonly ByteBuffer _buffer;

		 public ByteBufferReadableChannel( ByteBuffer buffer )
		 {
			  this._buffer = buffer;
		 }

		 public override sbyte Get()
		 {
			  return _buffer.get();
		 }

		 public virtual short Short
		 {
			 get
			 {
				  return _buffer.Short;
			 }
		 }

		 public virtual int Int
		 {
			 get
			 {
				  return _buffer.Int;
			 }
		 }

		 public virtual long Long
		 {
			 get
			 {
				  return _buffer.Long;
			 }
		 }

		 public virtual float Float
		 {
			 get
			 {
				  return _buffer.Float;
			 }
		 }

		 public virtual double Double
		 {
			 get
			 {
				  return _buffer.Double;
			 }
		 }

		 public override void Get( sbyte[] bytes, int length )
		 {
			  _buffer.get( bytes, 0, length );
		 }

		 public override void Close()
		 {
		 }
	}

}