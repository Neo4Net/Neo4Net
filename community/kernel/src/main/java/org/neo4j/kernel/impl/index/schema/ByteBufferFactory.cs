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
namespace Org.Neo4j.Kernel.Impl.Index.Schema
{

	using Preconditions = Org.Neo4j.Util.Preconditions;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.toIntExact;

	/// <summary>
	/// Factory for <seealso cref="ByteBuffer"/> instances. The type of <seealso cref="ByteBuffer"/> allocated will be decided by the given <seealso cref="Allocator"/> passed into it.
	/// This factory provides three differently scoped buffers:
	/// 
	/// <ul>
	///     <li><seealso cref="globalAllocator() Global buffers"/> which will be closed when this factory <seealso cref="close() closes"/></li>
	///     <li><seealso cref="newLocalAllocator() Local buffers"/> where caller gets a new <seealso cref="Allocator"/> and gets the responsibility of its
	///     life cycle, i.e. allocations from it and must call <seealso cref="Allocator.close()"/> close on it after use</li>
	///     <li><seealso cref="acquireThreadLocalBuffer() Thread-local buffers"/> created lazily on first call by any given thread into this buffer factory.
	///     These buffers are allocated from the global allocator on first use and then only cleared and handed out on further requests.
	///     After use it must be <seealso cref="releaseThreadLocalBuffer() released"/> so that other code paths in that thread's execution can acquire it.</li>
	/// </ul>
	/// 
	/// These scopes together allows for efficient allocation, de-allocation and sharing of buffers.
	/// </summary>
	public class ByteBufferFactory : AutoCloseable
	{
		 private readonly Allocator _globalAllocator;
		 private readonly int _threadLocalBufferSize;
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
		 private readonly ThreadLocal<ThreadLocalByteBuffer> _threadLocalBuffers = ThreadLocal.withInitial( ThreadLocalByteBuffer::new );
		 private readonly System.Func<Allocator> _allocatorFactory;

		 public ByteBufferFactory( System.Func<Allocator> allocatorFactory, int threadLocalBufferSize )
		 {
			  this._allocatorFactory = allocatorFactory;
			  this._globalAllocator = allocatorFactory();
			  this._threadLocalBufferSize = threadLocalBufferSize;
		 }

		 /// <returns> the global <seealso cref="Allocator"/> for private buffer allocation. </returns>
		 internal virtual Allocator GlobalAllocator()
		 {
			  return _globalAllocator;
		 }

		 /// <returns> a new <seealso cref="Allocator"/> for local use. Must be closed by the caller when done. </returns>
		 internal virtual Allocator NewLocalAllocator()
		 {
			  return _allocatorFactory.get();
		 }

		 /// <returns> thread-local buffer. The returned buffer is meant to be used in a limited closure and then <seealso cref="releaseThreadLocalBuffer() released"/>
		 /// so that other pieces of code can use it again for this thread. </returns>
		 internal virtual ByteBuffer AcquireThreadLocalBuffer()
		 {
			  return _threadLocalBuffers.get().acquire();
		 }

		 /// <summary>
		 /// Releases a previously <seealso cref="acquireThreadLocalBuffer() acquired"/> thread-local buffer.
		 /// </summary>
		 internal virtual void ReleaseThreadLocalBuffer()
		 {
			  ThreadLocalByteBuffer managedByteBuffer = _threadLocalBuffers.get();
			  Preconditions.checkState( managedByteBuffer != null, "Buffer doesn't exist" );
			  managedByteBuffer.Release();
		 }

		 public virtual int BufferSize()
		 {
			  return _threadLocalBufferSize;
		 }

		 public override void Close()
		 {
			  _globalAllocator.close();
		 }

		 public static ByteBufferFactory HeapBufferFactory( int sharedBuffersSize )
		 {
			  return new ByteBufferFactory( () => HEAP_ALLOCATOR, sharedBuffersSize );
		 }

		 /// <summary>
		 /// Allocator of <seealso cref="ByteBuffer"/> instances. Also is responsible for freeing memory of allocated buffers on <seealso cref="close()"/>.
		 /// </summary>
		 public interface Allocator : AutoCloseable
		 {
			  ByteBuffer Allocate( int bufferSize );

			  void Close();
		 }

		 internal static Allocator HEAP_ALLOCATOR = new AllocatorAnonymousInnerClass();

		 private class AllocatorAnonymousInnerClass : Allocator
		 {
			 public ByteBuffer allocate( int bufferSize )
			 {
				  return ByteBuffer.allocate( toIntExact( bufferSize ) );
			 }

			 public void close()
			 {
				  // Nothing to close
			 }
		 }

		 private class ThreadLocalByteBuffer
		 {
			 private readonly ByteBufferFactory _outerInstance;

			 public ThreadLocalByteBuffer( ByteBufferFactory outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  internal bool Acquired;
			  internal ByteBuffer Buffer;

			  internal virtual ByteBuffer Acquire()
			  {
					Preconditions.checkState( !Acquired, "Already acquired" );
					Acquired = true;
					if ( Buffer == null )
					{
						 Buffer = outerInstance.globalAllocator.Allocate( outerInstance.threadLocalBufferSize );
					}
					else
					{
						 Buffer.clear();
					}
					return Buffer;
			  }

			  internal virtual void Release()
			  {
					Preconditions.checkState( Acquired, "Not acquired" );
					Acquired = false;
			  }
		 }
	}

}