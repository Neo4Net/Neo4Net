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
namespace Neo4Net.Kernel.Impl.Api.state
{

	using Memory = Neo4Net.Kernel.impl.util.collection.Memory;
	using MemoryAllocator = Neo4Net.Kernel.impl.util.collection.MemoryAllocator;
	using LocalMemoryTracker = Neo4Net.Memory.LocalMemoryTracker;
	using MemoryAllocationTracker = Neo4Net.Memory.MemoryAllocationTracker;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.toIntExact;

	internal class TestMemoryAllocator : MemoryAllocator
	{
		 internal readonly MemoryAllocationTracker Tracker;

		 internal TestMemoryAllocator() : this(new LocalMemoryTracker())
		 {
		 }

		 internal TestMemoryAllocator( MemoryAllocationTracker tracker )
		 {
			  this.Tracker = tracker;
		 }

		 public override Memory Allocate( long size, bool zeroed )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final ByteBuffer buf = ByteBuffer.allocate(toIntExact(size));
			  ByteBuffer buf = ByteBuffer.allocate( toIntExact( size ) );
			  if ( zeroed )
			  {
					Arrays.fill( buf.array(), (sbyte) 0 );
			  }
			  return new MemoryImpl( this, buf );
		 }

		 internal class MemoryImpl : Memory
		 {
			 private readonly TestMemoryAllocator _outerInstance;

			  internal readonly ByteBuffer Buf;

			  internal MemoryImpl( TestMemoryAllocator outerInstance, ByteBuffer buf )
			  {
				  this._outerInstance = outerInstance;
					this.Buf = buf;
					outerInstance.Tracker.allocated( buf.capacity() );
			  }

			  public override long ReadLong( long offset )
			  {
					return Buf.getLong( toIntExact( offset ) );
			  }

			  public override void WriteLong( long offset, long value )
			  {
					Buf.putLong( toIntExact( offset ), value );
			  }

			  public override void Clear()
			  {
					Arrays.fill( Buf.array(), (sbyte) 0 );
			  }

			  public override long Size()
			  {
					return Buf.capacity();
			  }

			  public override void Free()
			  {
					outerInstance.Tracker.deallocated( Buf.capacity() );
			  }

			  public override Memory Copy()
			  {
					ByteBuffer copyBuf = ByteBuffer.wrap( Arrays.copyOf( Buf.array(), Buf.array().length ) );
					return new MemoryImpl( _outerInstance, copyBuf );
			  }

			  public override ByteBuffer AsByteBuffer()
			  {
					return Buf;
			  }
		 }
	}

}