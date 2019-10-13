﻿/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
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
	using ByteBuf = io.netty.buffer.ByteBuf;


	public class ByteBufInput : PackInput
	{
		 private ByteBuf _buf;

		 public virtual void Start( ByteBuf newBuf )
		 {
			  AssertNotStarted();
			  _buf = requireNonNull( newBuf );
		 }

		 public virtual void Stop()
		 {
			  _buf = null;
		 }

		 public override sbyte ReadByte()
		 {
			  return _buf.readByte();
		 }

		 public override short ReadShort()
		 {
			  return _buf.readShort();
		 }

		 public override int ReadInt()
		 {
			  return _buf.readInt();
		 }

		 public override long ReadLong()
		 {
			  return _buf.readLong();
		 }

		 public override double ReadDouble()
		 {
			  return _buf.readDouble();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public PackInput readBytes(byte[] into, int offset, int toRead) throws java.io.IOException
		 public override PackInput ReadBytes( sbyte[] into, int offset, int toRead )
		 {
			  _buf.readBytes( into, offset, toRead );
			  return this;
		 }

		 public override sbyte PeekByte()
		 {
			  return _buf.getByte( _buf.readerIndex() );
		 }

		 private void AssertNotStarted()
		 {
			  if ( _buf != null )
			  {
					throw new System.InvalidOperationException( "Already started" );
			  }
		 }
	}

}