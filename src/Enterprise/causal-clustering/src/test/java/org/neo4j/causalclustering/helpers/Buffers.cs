using System.Collections.Generic;

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
namespace Neo4Net.causalclustering.helpers
{
	using ByteBuf = io.netty.buffer.ByteBuf;
	using ByteBufAllocator = io.netty.buffer.ByteBufAllocator;
	using CompositeByteBuf = io.netty.buffer.CompositeByteBuf;
	using UnpooledByteBufAllocator = io.netty.buffer.UnpooledByteBufAllocator;
	using ReferenceCounted = io.netty.util.ReferenceCounted;
	using ExternalResource = org.junit.rules.ExternalResource;


	/// <summary>
	/// For tests that uses <seealso cref="ByteBuf"/>. All buffers that are allocated using <seealso cref="ByteBufAllocator"/> will be
	/// released after test has executed.
	/// </summary>
	public class Buffers : ExternalResource, ByteBufAllocator
	{
		 private readonly ByteBufAllocator _allocator;

		 public Buffers( ByteBufAllocator allocator )
		 {
			  this._allocator = allocator;
		 }

		 private readonly IList<ByteBuf> _buffersList = new LinkedList<ByteBuf>();

		 public Buffers() : this(new UnpooledByteBufAllocator(false))
		 {
		 }

		 public virtual BUFFER Add<BUFFER>( BUFFER byteBuf ) where BUFFER : io.netty.buffer.ByteBuf
		 {
			  _buffersList.Add( byteBuf );
			  return byteBuf;
		 }

		 public override ByteBuf Buffer()
		 {
			  return Add( _allocator.buffer() );
		 }

		 public override ByteBuf Buffer( int initialCapacity )
		 {
			  return Add( _allocator.buffer( initialCapacity ) );
		 }

		 public override ByteBuf Buffer( int initialCapacity, int maxCapacity )
		 {
			  return Add( _allocator.buffer( initialCapacity, maxCapacity ) );
		 }

		 public override ByteBuf IoBuffer()
		 {
			  return Add( _allocator.ioBuffer() );
		 }

		 public override ByteBuf IoBuffer( int initialCapacity )
		 {
			  return Add( _allocator.ioBuffer( initialCapacity ) );
		 }

		 public override ByteBuf IoBuffer( int initialCapacity, int maxCapacity )
		 {
			  return Add( _allocator.ioBuffer( initialCapacity, maxCapacity ) );
		 }

		 public override ByteBuf HeapBuffer()
		 {
			  return Add( _allocator.heapBuffer() );
		 }

		 public override ByteBuf HeapBuffer( int initialCapacity )
		 {
			  return Add( _allocator.heapBuffer( initialCapacity ) );
		 }

		 public override ByteBuf HeapBuffer( int initialCapacity, int maxCapacity )
		 {
			  return Add( _allocator.heapBuffer( initialCapacity, maxCapacity ) );
		 }

		 public override ByteBuf DirectBuffer()
		 {
			  return Add( _allocator.directBuffer() );
		 }

		 public override ByteBuf DirectBuffer( int initialCapacity )
		 {
			  return Add( _allocator.directBuffer( initialCapacity ) );
		 }

		 public override ByteBuf DirectBuffer( int initialCapacity, int maxCapacity )
		 {
			  return Add( _allocator.directBuffer( initialCapacity, maxCapacity ) );
		 }

		 public override CompositeByteBuf CompositeBuffer()
		 {
			  return Add( _allocator.compositeBuffer() );
		 }

		 public override CompositeByteBuf CompositeBuffer( int maxNumComponents )
		 {
			  return Add( _allocator.compositeBuffer( maxNumComponents ) );
		 }

		 public override CompositeByteBuf CompositeHeapBuffer()
		 {
			  return Add( _allocator.compositeHeapBuffer() );
		 }

		 public override CompositeByteBuf CompositeHeapBuffer( int maxNumComponents )
		 {
			  return Add( _allocator.compositeHeapBuffer( maxNumComponents ) );
		 }

		 public override CompositeByteBuf CompositeDirectBuffer()
		 {
			  return Add( _allocator.compositeDirectBuffer() );
		 }

		 public override CompositeByteBuf CompositeDirectBuffer( int maxNumComponents )
		 {
			  return Add( _allocator.compositeBuffer( maxNumComponents ) );
		 }

		 public override bool DirectBufferPooled
		 {
			 get
			 {
				  return _allocator.DirectBufferPooled;
			 }
		 }

		 public override int CalculateNewCapacity( int minNewCapacity, int maxCapacity )
		 {
			  return _allocator.calculateNewCapacity( minNewCapacity, maxCapacity );
		 }

		 protected internal override void Before()
		 {
			  _buffersList.removeIf( buf => buf.refCnt() == 0 );
		 }

		 protected internal override void After()
		 {
			  _buffersList.ForEach( ReferenceCounted.release );
		 }
	}

}