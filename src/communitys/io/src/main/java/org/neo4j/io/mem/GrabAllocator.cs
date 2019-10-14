using System;
using System.Threading;

/*
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
namespace Neo4Net.Io.mem
{

	using MemoryAllocationTracker = Neo4Net.Memory.MemoryAllocationTracker;
	using UnsafeUtil = Neo4Net.@unsafe.Impl.@internal.Dragons.UnsafeUtil;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.ByteUnit.kibiBytes;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.util.FeatureToggles.getInteger;

	/// <summary>
	/// This memory allocator is allocating memory in large segments, called "grabs", and the memory returned by the memory
	/// manager is page aligned, and plays well with transparent huge pages and other operating system optimisations.
	/// </summary>
	public sealed class GrabAllocator : MemoryAllocator
	{
		 private static readonly object _globalCleanerInstance = GlobalCleaner();

		 private readonly Grabs _grabs;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings({"unused", "FieldCanBeLocal"}) private final Object cleaner;
		 private readonly object _cleaner;
		 private readonly MethodHandle _cleanHandle;

		 /// <summary>
		 /// Create a new GrabAllocator that will allocate the given amount of memory, to pointers that are aligned to the
		 /// given alignment size. </summary>
		 /// <param name="expectedMaxMemory"> The maximum amount of memory that this memory manager is expected to allocate. The
		 /// actual amount of memory used can end up greater than this value, if some of it gets wasted on alignment padding. </param>
		 /// <param name="memoryTracker"> memory usage tracker </param>
		 internal GrabAllocator( long expectedMaxMemory, MemoryAllocationTracker memoryTracker )
		 {
			  this._grabs = new Grabs( expectedMaxMemory, memoryTracker );
			  try
			  {
					CleanerHandles handles = FindCleanerHandles();
					this._cleaner = handles.Creator.invoke( this, new GrabsDeallocator( _grabs ) );
					this._cleanHandle = handles.Cleaner;
			  }
			  catch ( Exception throwable )
			  {
					throw new LinkageError( "Unable to instantiate cleaner", throwable );
			  }
		 }

		 public override long UsedMemory()
		 {
			 lock ( this )
			 {
				  return _grabs.usedMemory();
			 }
		 }

		 public override long AvailableMemory()
		 {
			 lock ( this )
			 {
				  return _grabs.availableMemory();
			 }
		 }

		 public override long AllocateAligned( long bytes, long alignment )
		 {
			 lock ( this )
			 {
				  return _grabs.allocateAligned( bytes, alignment );
			 }
		 }

		 public override void Close()
		 {
			  try
			  {
					_cleanHandle.invoke( _cleaner );
			  }
			  catch ( Exception throwable )
			  {
					throw new LinkageError( "Unable to clean cleaner.", throwable );
			  }
		 }

		 private class Grab
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  public readonly Grab NextConflict;
			  internal readonly long Address;
			  internal readonly long Limit;
			  internal readonly MemoryAllocationTracker MemoryTracker;
			  internal long NextPointer;

			  internal Grab( Grab next, long size, MemoryAllocationTracker memoryTracker )
			  {
					this.NextConflict = next;
					this.Address = UnsafeUtil.allocateMemory( size, memoryTracker );
					this.Limit = Address + size;
					this.MemoryTracker = memoryTracker;
					NextPointer = Address;
			  }

			  internal Grab( Grab next, long address, long limit, long nextPointer, MemoryAllocationTracker memoryTracker )
			  {
					this.NextConflict = next;
					this.Address = address;
					this.Limit = limit;
					this.NextPointer = nextPointer;
					this.MemoryTracker = memoryTracker;
			  }

			  internal virtual long NextAligned( long pointer, long alignment )
			  {
					if ( alignment == 1 )
					{
						 return pointer;
					}
					long off = pointer % alignment;
					if ( off == 0 )
					{
						 return pointer;
					}
					return pointer + ( alignment - off );
			  }

			  internal virtual long Allocate( long bytes, long alignment )
			  {
					long allocation = NextAligned( NextPointer, alignment );
					NextPointer = allocation + bytes;
					return allocation;
			  }

			  internal virtual void Free()
			  {
					UnsafeUtil.free( Address, Limit - Address, MemoryTracker );
			  }

			  internal virtual bool CanAllocate( long bytes, long alignment )
			  {
					return NextAligned( NextPointer, alignment ) + bytes <= Limit;
			  }

			  internal virtual Grab setNext( Grab grab )
			  {
					return new Grab( grab, Address, Limit, NextPointer, MemoryTracker );
			  }

			  public override string ToString()
			  {
					long size = Limit - Address;
					long reserve = NextPointer > Limit ? 0 : Limit - NextPointer;
					double use = ( 1.0 - reserve / ( ( double ) size ) ) * 100.0;
					return string.Format( "Grab[size = {0:D} bytes, reserve = {1:D} bytes, use = {2,5:F2} %]", size, reserve, use );
			  }
		 }

		 private sealed class Grabs
		 {
			  /// <summary>
			  /// The amount of memory, in bytes, to grab in each Grab.
			  /// </summary>
			  internal static readonly long GrabSize = getInteger( typeof( GrabAllocator ), "GRAB_SIZE", ( int ) kibiBytes( 512 ) );

			  internal readonly MemoryAllocationTracker MemoryTracker;
			  internal long ExpectedMaxMemory;
			  internal Grab Head;

			  internal Grabs( long expectedMaxMemory, MemoryAllocationTracker memoryTracker )
			  {
					this.ExpectedMaxMemory = expectedMaxMemory;
					this.MemoryTracker = memoryTracker;
			  }

			  internal long UsedMemory()
			  {
					long sum = 0;
					Grab grab = Head;
					while ( grab != null )
					{
						 sum += grab.NextPointer - grab.Address;
						 grab = grab.NextConflict;
					}
					return sum;
			  }

			  internal long AvailableMemory()
			  {
					Grab grab = Head;
					long availableInCurrentGrab = 0;
					if ( grab != null )
					{
						 availableInCurrentGrab = grab.Limit - grab.NextPointer;
					}
					return Math.Max( ExpectedMaxMemory, 0L ) + availableInCurrentGrab;
			  }

			  public void Close()
			  {
					Grab current = Head;

					while ( current != null )
					{
						 current.Free();
						 current = current.NextConflict;
					}
					Head = null;
			  }

			  internal long AllocateAligned( long bytes, long alignment )
			  {
					if ( alignment <= 0 )
					{
						 throw new System.ArgumentException( "Invalid alignment: " + alignment + ". Alignment must be positive." );
					}
					long grabSize = Math.Min( GrabSize, ExpectedMaxMemory );
					if ( bytes > GrabSize )
					{
						 // This is a huge allocation. Put it in its own grab and keep any existing grab at the head.
						 grabSize = bytes;
						 Grab nextGrab = Head == null ? null : Head.next;
						 Grab allocationGrab = new Grab( nextGrab, grabSize, MemoryTracker );
						 if ( !allocationGrab.CanAllocate( bytes, alignment ) )
						 {
							  allocationGrab.Free();
							  grabSize = bytes + alignment;
							  allocationGrab = new Grab( nextGrab, grabSize, MemoryTracker );
						 }
						 long allocation = allocationGrab.Allocate( bytes, alignment );
						 Head = Head == null ? allocationGrab : Head.setNext( allocationGrab );
						 ExpectedMaxMemory -= bytes;
						 return allocation;
					}

					if ( Head == null || !Head.canAllocate( bytes, alignment ) )
					{
						 if ( grabSize < bytes )
						 {
							  grabSize = bytes;
							  Grab grab = new Grab( Head, grabSize, MemoryTracker );
							  if ( grab.CanAllocate( bytes, alignment ) )
							  {
									ExpectedMaxMemory -= grabSize;
									Head = grab;
									return Head.allocate( bytes, alignment );
							  }
							  grab.Free();
							  grabSize = bytes + alignment;
						 }
						 Head = new Grab( Head, grabSize, MemoryTracker );
						 ExpectedMaxMemory -= grabSize;
					}
					return Head.allocate( bytes, alignment );
			  }
		 }

		 private static object GlobalCleaner()
		 {
			  MethodHandles.Lookup lookup = MethodHandles.lookup();
			  try
			  {
					Type newCleaner = Type.GetType( "java.lang.ref.Cleaner" );
					MethodHandle createInstance = lookup.findStatic( newCleaner, "create", MethodType.methodType( newCleaner ) );
					return createInstance.invoke();
			  }
			  catch ( Exception )
			  {
					return null;
			  }
		 }

		 private static CleanerHandles FindCleanerHandles()
		 {
			  MethodHandles.Lookup lookup = MethodHandles.lookup();
			  return _globalCleanerInstance == null ? FindHandlesForOldCleaner( lookup ) : FindHandlesForNewCleaner( lookup );
		 }

		 private static CleanerHandles FindHandlesForNewCleaner( MethodHandles.Lookup lookup )
		 {
			  try
			  {
					Objects.requireNonNull( _globalCleanerInstance );
					Type newCleaner = _globalCleanerInstance.GetType();
					Type newCleanable = Type.GetType( "java.lang.ref.Cleaner$Cleanable" );
					MethodHandle registerHandle = FindCreationMethod( "register", lookup, newCleaner );
					registerHandle = registerHandle.bindTo( _globalCleanerInstance );
					return CleanerHandles.Of( registerHandle, FindCleanMethod( lookup, newCleanable ) );
			  }
			  catch ( Exception newCleanerException ) when ( newCleanerException is ClassNotFoundException || newCleanerException is NoSuchMethodException || newCleanerException is IllegalAccessException )
			  {
					throw new LinkageError( "Unable to find cleaner methods.", newCleanerException );
			  }
		 }

		 private static CleanerHandles FindHandlesForOldCleaner( MethodHandles.Lookup lookup )
		 {
			  try
			  {
					Type oldCleaner = Type.GetType( "sun.misc.Cleaner" );
					return CleanerHandles.Of( FindCreationMethod( "create", lookup, oldCleaner ), FindCleanMethod( lookup, oldCleaner ) );
			  }
			  catch ( Exception oldCleanerException ) when ( oldCleanerException is ClassNotFoundException || oldCleanerException is NoSuchMethodException || oldCleanerException is IllegalAccessException )
			  {
					throw new LinkageError( "Unable to find cleaner methods.", oldCleanerException );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static MethodHandle findCleanMethod(MethodHandles.Lookup lookup, Class cleaner) throws IllegalAccessException, NoSuchMethodException
		 private static MethodHandle FindCleanMethod( MethodHandles.Lookup lookup, Type cleaner )
		 {
			  return lookup.unreflect( cleaner.getDeclaredMethod( "clean" ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static MethodHandle findCreationMethod(String methodName, MethodHandles.Lookup lookup, Class cleaner) throws IllegalAccessException, NoSuchMethodException
		 private static MethodHandle FindCreationMethod( string methodName, MethodHandles.Lookup lookup, Type cleaner )
		 {
			  return lookup.unreflect( cleaner.getDeclaredMethod( methodName, typeof( object ), typeof( ThreadStart ) ) );
		 }

		 private sealed class CleanerHandles
		 {
			  internal readonly MethodHandle Creator;
			  internal readonly MethodHandle Cleaner;

			  internal static CleanerHandles Of( MethodHandle creator, MethodHandle cleaner )
			  {
					return new CleanerHandles( creator, cleaner );
			  }

			  internal CleanerHandles( MethodHandle creator, MethodHandle cleaner )
			  {
					this.Creator = creator;
					this.Cleaner = cleaner;
			  }
		 }

		 private sealed class GrabsDeallocator : ThreadStart
		 {
			  internal readonly Grabs Grabs;

			  internal GrabsDeallocator( Grabs grabs )
			  {
					this.Grabs = grabs;
			  }

			  public override void Run()
			  {
					Grabs.close();
			  }
		 }
	}

}