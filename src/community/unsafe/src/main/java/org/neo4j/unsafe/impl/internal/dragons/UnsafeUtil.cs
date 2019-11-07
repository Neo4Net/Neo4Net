using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;

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
namespace Neo4Net.@unsafe.Impl.Internal.Dragons
{
	using Unsafe = sun.misc.Unsafe;


	using GlobalMemoryTracker = Neo4Net.Memory.GlobalMemoryTracker;
	using IMemoryAllocationTracker = Neo4Net.Memory.IMemoryAllocationTracker;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Long.compareUnsigned;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.util.FeatureToggles.flag;

	/// <summary>
	/// Always check that the Unsafe utilities are available with the <seealso cref="UnsafeUtil.assertHasUnsafe"/> method, before
	/// calling any of the other methods.
	/// <para>
	/// Avoid `import static` for these individual methods. Always qualify method usages with `UnsafeUtil` so use sites
	/// show up in code greps.
	/// </para>
	/// </summary>
	public sealed class UnsafeUtil
	{
		 /// <summary>
		 /// Whether or not to explicitly dirty the allocated memory. This is off by default.
		 /// The <seealso cref="UnsafeUtil.allocateMemory(long, IMemoryAllocationTracker)"/> method is not guaranteed to allocate
		 /// zeroed out memory, but might often do so by pure chance.
		 /// <para>
		 /// Enabling this feature will make sure that the allocated memory is full of random data, such that we can test
		 /// and verify that our code does not assume that memory is clean when allocated.
		 /// </para>
		 /// </summary>
		 private static readonly bool _dirtyMemory = flag( typeof( UnsafeUtil ), "DIRTY_MEMORY", false );
		 private static readonly bool _checkNativeAccess = flag( typeof( UnsafeUtil ), "CHECK_NATIVE_ACCESS", false );
		 // this allows us to temporarily disable the checking, for performance:
		 private static bool _nativeAccessCheckEnabled = true;

		 private static readonly Unsafe @unsafe;
		 private static readonly MethodHandle _sharedStringConstructor;
		 private const string ALLOW_UNALIGNED_MEMORY_ACCESS_PROPERTY = "Neo4Net.unsafe.impl.internal.dragons.UnsafeUtil.allowUnalignedMemoryAccess";

		 private static readonly ConcurrentSkipListMap<long, Allocation> _allocations = new ConcurrentSkipListMap<long, Allocation>();
		 private static readonly ThreadLocal<Allocation> _lastUsedAllocation = new ThreadLocal<Allocation>();
		 private static readonly FreeTrace[] _freeTraces = _checkNativeAccess ? new FreeTrace[4096] : null;
		 private static readonly AtomicLong _freeCounter = new AtomicLong();

		 public static readonly Type DirectByteBufferClass;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private static final Constructor<?> directByteBufferCtor;
		 private static readonly System.Reflection.ConstructorInfo<object> _directByteBufferCtor;
		 private static readonly long _directByteBufferMarkOffset;
		 private static readonly long _directByteBufferPositionOffset;
		 private static readonly long _directByteBufferLimitOffset;
		 private static readonly long _directByteBufferCapacityOffset;
		 private static readonly long _directByteBufferAddressOffset;

		 private static readonly int _pageSize;

		 public static readonly bool AllowUnalignedMemoryAccess;
		 public static readonly bool StoreByteOrderIsNative;

		 static UnsafeUtil()
		 {
			  @unsafe = Unsafe;

			  MethodHandles.Lookup lookup = MethodHandles.lookup();
			  _sharedStringConstructor = GetSharedStringConstructorMethodHandle( lookup );

			  Type dbbClass = null;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: Constructor<?> ctor = null;
			  System.Reflection.ConstructorInfo<object> ctor = null;
			  long dbbMarkOffset = 0;
			  long dbbPositionOffset = 0;
			  long dbbLimitOffset = 0;
			  long dbbCapacityOffset = 0;
			  long dbbAddressOffset = 0;
			  int ps = 4096;
			  try
			  {
					dbbClass = Type.GetType( "java.nio.DirectByteBuffer" );
					Type bufferClass = Type.GetType( "java.nio.Buffer" );
					dbbMarkOffset = @unsafe.objectFieldOffset( bufferClass.getDeclaredField( "mark" ) );
					dbbPositionOffset = @unsafe.objectFieldOffset( bufferClass.getDeclaredField( "position" ) );
					dbbLimitOffset = @unsafe.objectFieldOffset( bufferClass.getDeclaredField( "limit" ) );
					dbbCapacityOffset = @unsafe.objectFieldOffset( bufferClass.getDeclaredField( "capacity" ) );
					dbbAddressOffset = @unsafe.objectFieldOffset( bufferClass.getDeclaredField( "address" ) );
					ps = @unsafe.pageSize();
			  }
			  catch ( Exception e )
			  {
					if ( dbbClass == null )
					{
						 throw new LinkageError( "Cannot to link java.nio.DirectByteBuffer", e );
					}
					try
					{
						 ctor = dbbClass.GetConstructor( Long.TYPE, Integer.TYPE );
						 ctor.Accessible = true;
					}
					catch ( NoSuchMethodException e1 )
					{
						 throw new LinkageError( "Cannot find JNI constructor for java.nio.DirectByteBuffer", e1 );
					}
			  }
			  DirectByteBufferClass = dbbClass;
			  _directByteBufferCtor = ctor;
			  _directByteBufferMarkOffset = dbbMarkOffset;
			  _directByteBufferPositionOffset = dbbPositionOffset;
			  _directByteBufferLimitOffset = dbbLimitOffset;
			  _directByteBufferCapacityOffset = dbbCapacityOffset;
			  _directByteBufferAddressOffset = dbbAddressOffset;
			  _pageSize = ps;

			  // See java.nio.Bits.unaligned() and its uses.
			  string alignmentProperty = System.getProperty( ALLOW_UNALIGNED_MEMORY_ACCESS_PROPERTY );
			  if ( !string.ReferenceEquals( alignmentProperty, null ) && ( alignmentProperty.Equals( "true", StringComparison.OrdinalIgnoreCase ) || alignmentProperty.Equals( "false", StringComparison.OrdinalIgnoreCase ) ) )
			  {
					AllowUnalignedMemoryAccess = bool.Parse( alignmentProperty );
			  }
			  else
			  {
					bool unaligned;
					string arch = System.getProperty( "os.arch", "?" );
					switch ( arch ) // list of architectures that support unaligned access to memory
					{
					case "x86_64":
					case "i386":
					case "x86":
					case "amd64":
					case "ppc64":
					case "ppc64le":
					case "ppc64be":
						 unaligned = true;
						 break;
					default:
						 unaligned = false;
						 break;
					}
					AllowUnalignedMemoryAccess = unaligned;
			  }
			  StoreByteOrderIsNative = ByteOrder.nativeOrder() == ByteOrder.BIG_ENDIAN;
		 }

		 private UnsafeUtil()
		 {
		 }

		 private static Unsafe Unsafe
		 {
			 get
			 {
				  try
				  {
						PrivilegedExceptionAction<Unsafe> getUnsafe = () =>
						{
						 try
						 {
							  return Unsafe.Unsafe;
						 }
						 catch ( Exception e )
						 {
							  Type<Unsafe> type = typeof( Unsafe );
							  System.Reflection.FieldInfo[] fields = type.GetFields( BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance );
							  foreach ( System.Reflection.FieldInfo field in fields )
							  {
									if ( Modifier.isStatic( field.Modifiers ) && type.IsAssignableFrom( field.Type ) )
									{
										 field.Accessible = true;
										 return type.cast( field.get( null ) );
									}
							  }
							  LinkageError error = new LinkageError( "No static field of type sun.misc.Unsafe" );
							  error.addSuppressed( e );
							  throw error;
						 }
						};
						return AccessController.doPrivileged( getUnsafe );
				  }
				  catch ( Exception e )
				  {
						throw new LinkageError( "Cannot access sun.misc.Unsafe", e );
				  }
			 }
		 }

		 /// <exception cref="java.lang.LinkageError"> if the Unsafe tools are not available on in this JVM. </exception>
		 public static void AssertHasUnsafe()
		 {
			  if ( @unsafe == null )
			  {
					throw new LinkageError( "Unsafe not available" );
			  }
		 }

		 private static MethodHandle GetSharedStringConstructorMethodHandle( MethodHandles.Lookup lookup )
		 {
			  try
			  {
					System.Reflection.ConstructorInfo<string> constructor = typeof( string ).getDeclaredConstructor( typeof( char[] ), Boolean.TYPE );
					constructor.Accessible = true;
					return lookup.unreflectConstructor( constructor );
			  }
			  catch ( Exception )
			  {
					return null;
			  }
		 }

		 /// <summary>
		 /// Get the object-relative field offset.
		 /// </summary>
		 public static long GetFieldOffset( Type type, string field )
		 {
			  try
			  {
					return @unsafe.objectFieldOffset( type.getDeclaredField( field ) );
			  }
			  catch ( NoSuchFieldException e )
			  {
					string message = "Could not get offset of '" + field + "' field on type " + type;
					throw new LinkageError( message, e );
			  }
		 }

		 /// <summary>
		 /// Atomically add the given delta to the int field, and return its previous value.
		 /// <para>
		 /// This has the memory visibility semantics of a volatile read followed by a volatile write.
		 /// </para>
		 /// </summary>
		 public static int GetAndAddInt( object obj, long offset, int delta )
		 {
			  return @unsafe.getAndAddInt( obj, offset, delta );
		 }

		 /// <summary>
		 /// Atomically add the given delta to the long field, and return its previous value.
		 /// <para>
		 /// This has the memory visibility semantics of a volatile read followed by a volatile write.
		 /// </para>
		 /// </summary>
		 public static long GetAndAddLong( object obj, long offset, long delta )
		 {
			  return @unsafe.getAndAddLong( obj, offset, delta );
		 }

		 /// <summary>
		 /// Orders loads before the fence, with loads and stores after the fence.
		 /// </summary>
		 public static void LoadFence()
		 {
			  @unsafe.loadFence();
		 }

		 /// <summary>
		 /// Orders stores before the fence, with loads and stores after the fence.
		 /// </summary>
		 public static void StoreFence()
		 {
			  @unsafe.storeFence();
		 }

		 /// <summary>
		 /// Orders loads and stores before the fence, with loads and stores after the fence.
		 /// </summary>
		 public static void FullFence()
		 {
			  @unsafe.fullFence();
		 }

		 /// <summary>
		 /// Atomically compare the current value of the given long field with the expected value, and if they are the
		 /// equal, set the field to the updated value and return true. Otherwise return false.
		 /// <para>
		 /// If this method returns true, then it has the memory visibility semantics of a volatile read followed by a
		 /// volatile write.
		 /// </para>
		 /// </summary>
		 public static bool CompareAndSwapLong( object obj, long offset, long expected, long update )
		 {
			  return @unsafe.compareAndSwapLong( obj, offset, expected, update );
		 }

		 /// <summary>
		 /// Same as compareAndSwapLong, but for object references.
		 /// </summary>
		 public static bool CompareAndSwapObject( object obj, long offset, object expected, object update )
		 {
			  return @unsafe.compareAndSwapObject( obj, offset, expected, update );
		 }

		 /// <summary>
		 /// Atomically return the current object reference value, and exchange it with the given new reference value.
		 /// </summary>
		 public static object GetAndSetObject( object obj, long offset, object newValue )
		 {
			  return @unsafe.getAndSetObject( obj, offset, newValue );
		 }

		 /// <summary>
		 /// Atomically exchanges provided <code>newValue</code> with the current value of field or array element, with
		 /// provided <code>offset</code>.
		 /// </summary>
		 public static long GetAndSetLong( object @object, long offset, long newValue )
		 {
			  return @unsafe.getAndSetLong( @object, offset, newValue );
		 }

		 /// <summary>
		 /// Atomically set field or array element to a maximum between current value and provided <code>newValue</code>
		 /// </summary>
		 public static void CompareAndSetMaxLong( object @object, long fieldOffset, long newValue )
		 {
			  long currentValue;
			  do
			  {
					currentValue = UnsafeUtil.GetLongVolatile( @object, fieldOffset );
					if ( currentValue >= newValue )
					{
						 return;
					}
			  } while ( !UnsafeUtil.CompareAndSwapLong( @object, fieldOffset, currentValue, newValue ) );
		 }

		 /// <summary>
		 /// Create a string with a char[] that you know is not going to be modified, so avoid the copy constructor.
		 /// </summary>
		 /// <param name="chars"> array that will back the new string </param>
		 /// <returns> the created string </returns>
		 public static string NewSharedArrayString( char[] chars )
		 {
			  if ( _sharedStringConstructor != null )
			  {
					try
					{
						 return ( string ) _sharedStringConstructor.invokeExact( chars, true );
					}
					catch ( Exception throwable )
					{
						 throw new LinkageError( "Unexpected 'String constructor' intrinsic failure", throwable );
					}
			  }
			  else
			  {
					return new string( chars );
			  }
		 }

		 /// <summary>
		 /// Allocate a block of memory of the given size in bytes, and return a pointer to that memory.
		 /// <para>
		 /// The memory is aligned such that it can be used for any data type.
		 /// The memory is uninitialised, so it may contain random garbage, or it may not.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <returns> a pointer to the allocated memory </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static long allocateMemory(long bytes) throws NativeMemoryAllocationRefusedError
		 public static long AllocateMemory( long bytes )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long pointer;
			  long pointer;
			  try
			  {
					pointer = @unsafe.allocateMemory( bytes );
			  }
			  catch ( Exception e )
			  {
					throw new NativeMemoryAllocationRefusedError( bytes, GlobalMemoryTracker.INSTANCE.usedDirectMemory(), e );
			  }
			  if ( _dirtyMemory )
			  {
					SetMemory( pointer, bytes, unchecked( ( sbyte ) 0xA5 ) );
			  }
			  AddAllocatedPointer( pointer, bytes );
			  GlobalMemoryTracker.INSTANCE.allocated( bytes );
			  return pointer;
		 }

		 /// <summary>
		 /// Allocate a block of memory of the given size in bytes and update memory allocation tracker accordingly.
		 /// <para>
		 /// The memory is aligned such that it can be used for any data type.
		 /// The memory is uninitialised, so it may contain random garbage, or it may not.
		 /// </para>
		 /// </summary>
		 /// <returns> a pointer to the allocated memory </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static long allocateMemory(long bytes, Neo4Net.memory.IMemoryAllocationTracker allocationTracker) throws NativeMemoryAllocationRefusedError
		 public static long AllocateMemory( long bytes, IMemoryAllocationTracker allocationTracker )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long pointer = allocateMemory(bytes);
			  long pointer = AllocateMemory( bytes );
			  allocationTracker.Allocated( bytes );
			  return pointer;
		 }

		 /// <summary>
		 /// Returns address pointer equal to or slightly after the given {@code pointer}.
		 /// The returned pointer as aligned with {@code alignBy} such that {@code pointer % alignBy == 0}.
		 /// The given pointer should be allocated with at least the requested size + {@code alignBy - 1},
		 /// where the additional bytes will serve as padding for the worst case where the start of the usable
		 /// area of the allocated memory will need to be shifted at most {@code alignBy - 1} bytes to the right.
		 /// <para>
		 /// <pre><code>
		 /// 0   4   8   12  16  20        ; 4-byte alignments
		 /// |---|---|---|---|---|         ; memory
		 ///        --------===            ; allocated memory (-required, =padding)
		 ///         ^------^              ; used memory
		 /// </code></pre>
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="pointer"> pointer to allocated memory from <seealso cref="allocateMemory(long, IMemoryAllocationTracker)"/> )}. </param>
		 /// <param name="alignBy"> power-of-two size to align to, e.g. 4 or 8. </param>
		 /// <returns> pointer to place inside the allocated memory to consider the effective start of the
		 /// memory, which from that point is aligned by {@code alignBy}. </returns>
		 public static long AlignedMemory( long pointer, int alignBy )
		 {
			  Debug.Assert( Integer.bitCount( alignBy ) == 1, "Requires alignment to be power of 2, but was " + alignBy );

			  long misalignment = pointer % alignBy;
			  return misalignment == 0 ? pointer : pointer + ( alignBy - misalignment );
		 }

		 /// <summary>
		 /// Free the memory that was allocated with <seealso cref="allocateMemory"/> and update memory allocation tracker accordingly.
		 /// </summary>
		 public static void Free( long pointer, long bytes, IMemoryAllocationTracker allocationTracker )
		 {
			  Free( pointer, bytes );
			  allocationTracker.Deallocated( bytes );
		 }

		 /// <summary>
		 /// Free the memory that was allocated with <seealso cref="allocateMemory"/>.
		 /// </summary>
		 public static void Free( long pointer, long bytes )
		 {
			  CheckFree( pointer );
			  @unsafe.freeMemory( pointer );
			  GlobalMemoryTracker.INSTANCE.deallocated( bytes );
		 }

		 private static void AddAllocatedPointer( long pointer, long sizeInBytes )
		 {
			  if ( _checkNativeAccess )
			  {
					_allocations.put( pointer, new Allocation( pointer, sizeInBytes, _freeCounter.get() ) );
			  }
		 }

		 private static void CheckFree( long pointer )
		 {
			  if ( _checkNativeAccess )
			  {
					DoCheckFree( pointer );
			  }
		 }

		 private static void DoCheckFree( long pointer )
		 {
			  long count = _freeCounter.AndIncrement;
			  Allocation allocation = _allocations.remove( pointer );
			  if ( allocation == null )
			  {
					StringBuilder sb = new StringBuilder( format( "Bad free: 0x%x, valid pointers are:", pointer ) );
					_allocations.forEach( ( k, v ) => sb.Append( '\n' ).Append( k ) );
					throw new AssertionError( sb.ToString() );
			  }
			  int idx = ( int )( count & 4095 );
			  _freeTraces[idx] = new FreeTrace( pointer, allocation, count );
		 }

		 private static void CheckAccess( long pointer, int size )
		 {
			  if ( _checkNativeAccess && _nativeAccessCheckEnabled )
			  {
					DoCheckAccess( pointer, size );
			  }
		 }

		 private static void DoCheckAccess( long pointer, int size )
		 {
			  long boundary = pointer + size;
			  Allocation allocation = _lastUsedAllocation.get();
			  if ( allocation != null )
			  {
					if ( compareUnsigned( allocation.Pointer, pointer ) <= 0 && compareUnsigned( allocation.Boundary, boundary ) > 0 && allocation.FreeCounter == _freeCounter.get() )
					{
						 return;
					}
			  }

			  KeyValuePair<long, Allocation> fentry = _allocations.floorEntry( boundary );
			  if ( fentry == null || compareUnsigned( fentry.Value.boundary, boundary ) < 0 )
			  {
					KeyValuePair<long, Allocation> centry = _allocations.ceilingEntry( pointer );
					ThrowBadAccess( pointer, size, fentry, centry );
			  }
			  //noinspection ConstantConditions
			  _lastUsedAllocation.set( fentry.Value );
		 }

		 private static void ThrowBadAccess( long pointer, int size, KeyValuePair<long, Allocation> fentry, KeyValuePair<long, Allocation> centry )
		 {
			  long now = System.nanoTime();
			  long faddr = fentry == null ? 0 : fentry.Key;
			  long fsize = fentry == null ? 0 : fentry.Value.sizeInBytes;
			  long foffset = pointer - ( faddr + fsize );
			  long caddr = centry == null ? 0 : centry.Key;
			  long csize = centry == null ? 0 : centry.Value.sizeInBytes;
			  long coffset = caddr - ( pointer + size );
			  bool floorIsNearest = foffset < coffset;
			  long naddr = floorIsNearest ? faddr : caddr;
			  long nsize = floorIsNearest ? fsize : csize;
			  long noffset = floorIsNearest ? foffset : coffset;
			  IList<FreeTrace> recentFrees = java.util.freeTraces.Where( Objects.nonNull ).Where( trace => trace.contains( pointer ) ).OrderBy( c => c ).ToList();
			  AssertionError error = new AssertionError( format( "Bad access to address 0x%x with size %s, nearest valid allocation is " + "0x%x (%s bytes, off by %s bytes). " + "Recent relevant frees (of %s) are attached as suppressed exceptions.", pointer, size, naddr, nsize, noffset, _freeCounter.get() ) );
			  foreach ( FreeTrace recentFree in recentFrees )
			  {
					recentFree.ReferenceTime = now;
					error.addSuppressed( recentFree );
			  }
			  throw error;
		 }

		 /// <summary>
		 /// Return the power-of-2 native memory page size.
		 /// </summary>
		 public static int PageSize()
		 {
			  return _pageSize;
		 }

		 public static void PutBoolean( object obj, long offset, bool value )
		 {
			  @unsafe.putBoolean( obj, offset, value );
		 }

		 public static bool GetBoolean( object obj, long offset )
		 {
			  return @unsafe.getBoolean( obj, offset );
		 }

		 public static void PutBooleanVolatile( object obj, long offset, bool value )
		 {
			  @unsafe.putBooleanVolatile( obj, offset, value );
		 }

		 public static bool GetBooleanVolatile( object obj, long offset )
		 {
			  return @unsafe.getBooleanVolatile( obj, offset );
		 }

		 public static void PutByte( long address, sbyte value )
		 {
			  CheckAccess( address, Byte.BYTES );
			  @unsafe.putByte( address, value );
		 }

		 public static sbyte GetByte( long address )
		 {
			  CheckAccess( address, Byte.BYTES );
			  return @unsafe.getByte( address );
		 }

		 public static void PutByteVolatile( long address, sbyte value )
		 {
			  CheckAccess( address, Byte.BYTES );
			  @unsafe.putByteVolatile( null, address, value );
		 }

		 public static sbyte GetByteVolatile( long address )
		 {
			  CheckAccess( address, Byte.BYTES );
			  return @unsafe.getByteVolatile( null, address );
		 }

		 public static void PutByte( object obj, long offset, sbyte value )
		 {
			  @unsafe.putByte( obj, offset, value );
		 }

		 public static sbyte GetByte( object obj, long offset )
		 {
			  return @unsafe.getByte( obj, offset );
		 }

		 public static sbyte GetByteVolatile( object obj, long offset )
		 {
			  return @unsafe.getByteVolatile( obj, offset );
		 }

		 public static void PutByteVolatile( object obj, long offset, sbyte value )
		 {
			  @unsafe.putByteVolatile( obj, offset, value );
		 }

		 public static void PutShort( long address, short value )
		 {
			  CheckAccess( address, Short.BYTES );
			  @unsafe.putShort( address, value );
		 }

		 public static short GetShort( long address )
		 {
			  CheckAccess( address, Short.BYTES );
			  return @unsafe.getShort( address );
		 }

		 public static void PutShortVolatile( long address, short value )
		 {
			  CheckAccess( address, Short.BYTES );
			  @unsafe.putShortVolatile( null, address, value );
		 }

		 public static short GetShortVolatile( long address )
		 {
			  CheckAccess( address, Short.BYTES );
			  return @unsafe.getShortVolatile( null, address );
		 }

		 public static void PutShort( object obj, long offset, short value )
		 {
			  @unsafe.putShort( obj, offset, value );
		 }

		 public static short GetShort( object obj, long offset )
		 {
			  return @unsafe.getShort( obj, offset );
		 }

		 public static void PutShortVolatile( object obj, long offset, short value )
		 {
			  @unsafe.putShortVolatile( obj, offset, value );
		 }

		 public static short GetShortVolatile( object obj, long offset )
		 {
			  return @unsafe.getShortVolatile( obj, offset );
		 }

		 public static void PutFloat( long address, float value )
		 {
			  CheckAccess( address, Float.BYTES );
			  @unsafe.putFloat( address, value );
		 }

		 public static float GetFloat( long address )
		 {
			  CheckAccess( address, Float.BYTES );
			  return @unsafe.getFloat( address );
		 }

		 public static void PutFloatVolatile( long address, float value )
		 {
			  CheckAccess( address, Float.BYTES );
			  @unsafe.putFloatVolatile( null, address, value );
		 }

		 public static float GetFloatVolatile( long address )
		 {
			  CheckAccess( address, Float.BYTES );
			  return @unsafe.getFloatVolatile( null, address );
		 }

		 public static void PutFloat( object obj, long offset, float value )
		 {
			  @unsafe.putFloat( obj, offset, value );
		 }

		 public static float GetFloat( object obj, long offset )
		 {
			  return @unsafe.getFloat( obj, offset );
		 }

		 public static void PutFloatVolatile( object obj, long offset, float value )
		 {
			  @unsafe.putFloatVolatile( obj, offset, value );
		 }

		 public static float GetFloatVolatile( object obj, long offset )
		 {
			  return @unsafe.getFloatVolatile( obj, offset );
		 }

		 public static void PutChar( long address, char value )
		 {
			  CheckAccess( address, Character.BYTES );
			  @unsafe.putChar( address, value );
		 }

		 public static char GetChar( long address )
		 {
			  CheckAccess( address, Character.BYTES );
			  return @unsafe.getChar( address );
		 }

		 public static void PutCharVolatile( long address, char value )
		 {
			  CheckAccess( address, Character.BYTES );
			  @unsafe.putCharVolatile( null, address, value );
		 }

		 public static char GetCharVolatile( long address )
		 {
			  CheckAccess( address, Character.BYTES );
			  return @unsafe.getCharVolatile( null, address );
		 }

		 public static void PutChar( object obj, long offset, char value )
		 {
			  @unsafe.putChar( obj, offset, value );
		 }

		 public static char GetChar( object obj, long offset )
		 {
			  return @unsafe.getChar( obj, offset );
		 }

		 public static void PutCharVolatile( object obj, long offset, char value )
		 {
			  @unsafe.putCharVolatile( obj, offset, value );
		 }

		 public static char GetCharVolatile( object obj, long offset )
		 {
			  return @unsafe.getCharVolatile( obj, offset );
		 }

		 public static void PutInt( long address, int value )
		 {
			  CheckAccess( address, Integer.BYTES );
			  @unsafe.putInt( address, value );
		 }

		 public static int GetInt( long address )
		 {
			  CheckAccess( address, Integer.BYTES );
			  return @unsafe.getInt( address );
		 }

		 public static void PutIntVolatile( long address, int value )
		 {
			  CheckAccess( address, Integer.BYTES );
			  @unsafe.putIntVolatile( null, address, value );
		 }

		 public static int GetIntVolatile( long address )
		 {
			  CheckAccess( address, Integer.BYTES );
			  return @unsafe.getIntVolatile( null, address );
		 }

		 public static void PutInt( object obj, long offset, int value )
		 {
			  @unsafe.putInt( obj, offset, value );
		 }

		 public static int GetInt( object obj, long offset )
		 {
			  return @unsafe.getInt( obj, offset );
		 }

		 public static void PutIntVolatile( object obj, long offset, int value )
		 {
			  @unsafe.putIntVolatile( obj, offset, value );
		 }

		 public static int GetIntVolatile( object obj, long offset )
		 {
			  return @unsafe.getIntVolatile( obj, offset );
		 }

		 public static void PutLongVolatile( long address, long value )
		 {
			  CheckAccess( address, Long.BYTES );
			  @unsafe.putLongVolatile( null, address, value );
		 }

		 public static long GetLongVolatile( long address )
		 {
			  CheckAccess( address, Long.BYTES );
			  return @unsafe.getLongVolatile( null, address );
		 }

		 public static void PutLong( long address, long value )
		 {
			  CheckAccess( address, Long.BYTES );
			  @unsafe.putLong( address, value );
		 }

		 public static long GetLong( long address )
		 {
			  CheckAccess( address, Long.BYTES );
			  return @unsafe.getLong( address );
		 }

		 public static void PutLong( object obj, long offset, long value )
		 {
			  @unsafe.putLong( obj, offset, value );
		 }

		 public static long GetLong( object obj, long offset )
		 {
			  return @unsafe.getLong( obj, offset );
		 }

		 public static void PutLongVolatile( object obj, long offset, long value )
		 {
			  @unsafe.putLongVolatile( obj, offset, value );
		 }

		 public static long GetLongVolatile( object obj, long offset )
		 {
			  return @unsafe.getLongVolatile( obj, offset );
		 }

		 public static void PutDouble( long address, double value )
		 {
			  CheckAccess( address, Double.BYTES );
			  @unsafe.putDouble( address, value );
		 }

		 public static double GetDouble( long address )
		 {
			  CheckAccess( address, Double.BYTES );
			  return @unsafe.getDouble( address );
		 }

		 public static void PutDoubleVolatile( long address, double value )
		 {
			  CheckAccess( address, Double.BYTES );
			  @unsafe.putDoubleVolatile( null, address, value );
		 }

		 public static double GetDoubleVolatile( long address )
		 {
			  CheckAccess( address, Double.BYTES );
			  return @unsafe.getDoubleVolatile( null, address );
		 }

		 public static void PutDouble( object obj, long offset, double value )
		 {
			  @unsafe.putDouble( obj, offset, value );
		 }

		 public static double GetDouble( object obj, long offset )
		 {
			  return @unsafe.getDouble( obj, offset );
		 }

		 public static void PutDoubleVolatile( object obj, long offset, double value )
		 {
			  @unsafe.putDoubleVolatile( obj, offset, value );
		 }

		 public static double GetDoubleVolatile( object obj, long offset )
		 {
			  return @unsafe.getDoubleVolatile( obj, offset );
		 }

		 public static void PutObject( object obj, long offset, object value )
		 {
			  @unsafe.putObject( obj, offset, value );
		 }

		 public static object GetObject( object obj, long offset )
		 {
			  return @unsafe.getObject( obj, offset );
		 }

		 public static object GetObjectVolatile( object obj, long offset )
		 {
			  return @unsafe.getObjectVolatile( obj, offset );
		 }

		 public static void PutObjectVolatile( object obj, long offset, object value )
		 {
			  @unsafe.putObjectVolatile( obj, offset, value );
		 }

		 public static int ArrayBaseOffset( Type klass )
		 {
			  return @unsafe.arrayBaseOffset( klass );
		 }

		 public static int ArrayIndexScale( Type klass )
		 {
			  int scale = @unsafe.arrayIndexScale( klass );
			  if ( scale == 0 )
			  {
					throw new AssertionError( "Array type too narrow for unsafe access: " + klass );
			  }
			  return scale;
		 }

		 public static int ArrayOffset( int index, int @base, int scale )
		 {
			  return @base + index * scale;
		 }

		 /// <summary>
		 /// Set the given number of bytes to the given value, starting from the given address.
		 /// </summary>
		 public static void SetMemory( long address, long bytes, sbyte value )
		 {
			  if ( 0 == ( address & 1 ) && bytes > 64 )
			  {
					@unsafe.putByte( address, value );
					@unsafe.setMemory( address + 1, bytes - 1, value );
			  }
			  else
			  {
					@unsafe.setMemory( address, bytes, value );
			  }
		 }

		 /// <summary>
		 /// Copy the given number of bytes from the source address to the destination address.
		 /// </summary>
		 public static void CopyMemory( long srcAddress, long destAddress, long bytes )
		 {
			  @unsafe.copyMemory( srcAddress, destAddress, bytes );
		 }

		 /// <summary>
		 /// Create a new DirectByteBuffer that wraps the given address and has the given capacity.
		 /// <para>
		 /// The ByteBuffer does NOT create a Cleaner, or otherwise register the pointer for freeing.
		 /// </para>
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static ByteBuffer newDirectByteBuffer(long addr, int cap) throws Exception
		 public static ByteBuffer NewDirectByteBuffer( long addr, int cap )
		 {
			  if ( _directByteBufferCtor == null )
			  {
					// Simulate the JNI NewDirectByteBuffer(void*, long) invocation.
					object dbb = @unsafe.allocateInstance( DirectByteBufferClass );
					InitDirectByteBuffer( dbb, addr, cap );
					return ( ByteBuffer ) dbb;
			  }
			  // Reflection based fallback code.
			  return ( ByteBuffer ) _directByteBufferCtor.newInstance( addr, cap );
		 }

		 /// <summary>
		 /// Initialize (simulate calling the constructor of) the given DirectByteBuffer.
		 /// </summary>
		 public static void InitDirectByteBuffer( object dbb, long addr, int cap )
		 {
			  @unsafe.putInt( dbb, _directByteBufferMarkOffset, -1 );
			  @unsafe.putInt( dbb, _directByteBufferPositionOffset, 0 );
			  @unsafe.putInt( dbb, _directByteBufferLimitOffset, cap );
			  @unsafe.putInt( dbb, _directByteBufferCapacityOffset, cap );
			  @unsafe.putLong( dbb, _directByteBufferAddressOffset, addr );
		 }

		 /// <summary>
		 /// Read the value of the address field in the (assumed to be) DirectByteBuffer.
		 /// <para>
		 /// <strong>NOTE:</strong> calling this method on a non-direct ByteBuffer is undefined behaviour.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="dbb"> The direct byte buffer to read the address field from. </param>
		 /// <returns> The native memory address in the given direct byte buffer. </returns>
		 public static long GetDirectByteBufferAddress( ByteBuffer dbb )
		 {
			  return @unsafe.getLong( dbb, _directByteBufferAddressOffset );
		 }

		 /// <summary>
		 /// Change if native access checking is enabled by setting it to the given new setting, and returning the old
		 /// setting.
		 /// <para>
		 /// This is only useful for speeding up tests when you have a lot of them, and they access native memory a lot.
		 /// This does not disable the recording of memory allocations or frees.
		 /// </para>
		 /// <para>
		 /// Remember to restore the old value so other tests in the same JVM get the benefit of native access checks.
		 /// </para>
		 /// <para>
		 /// The changing of this setting is completely unsynchronised, so you have to order this modification before and
		 /// after the tests that you want to run without native access checks.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="newSetting"> The new setting. </param>
		 /// <returns> the previous value of this setting. </returns>
		 public static bool ExchangeNativeAccessCheckEnabled( bool newSetting )
		 {
			  bool previousSetting = _nativeAccessCheckEnabled;
			  _nativeAccessCheckEnabled = newSetting;
			  return previousSetting;
		 }

		 /// <summary>
		 /// Gets a {@code short} at memory address {@code p} by reading byte for byte, instead of the whole value
		 /// in one go. This can be useful, even necessary in some scenarios where <seealso cref="allowUnalignedMemoryAccess"/>
		 /// is {@code false} and {@code p} isn't aligned properly. Values read with this method should have been
		 /// previously put using <seealso cref="putShortByteWiseLittleEndian(long, short)"/>.
		 /// </summary>
		 /// <param name="p"> address pointer to start reading at. </param>
		 /// <returns> the read value, which was read byte for byte. </returns>
		 public static short GetShortByteWiseLittleEndian( long p )
		 {
			  short a = ( short )( UnsafeUtil.GetByte( p ) & 0xFF );
			  short b = ( short )( UnsafeUtil.GetByte( p + 1 ) & 0xFF );
			  return ( short )( ( b << 8 ) | a );
		 }

		 /// <summary>
		 /// Gets a {@code int} at memory address {@code p} by reading byte for byte, instead of the whole value
		 /// in one go. This can be useful, even necessary in some scenarios where <seealso cref="allowUnalignedMemoryAccess"/>
		 /// is {@code false} and {@code p} isn't aligned properly. Values read with this method should have been
		 /// previously put using <seealso cref="putIntByteWiseLittleEndian(long, int)"/>.
		 /// </summary>
		 /// <param name="p"> address pointer to start reading at. </param>
		 /// <returns> the read value, which was read byte for byte. </returns>
		 public static int GetIntByteWiseLittleEndian( long p )
		 {
			  int a = UnsafeUtil.GetByte( p ) & 0xFF;
			  int b = UnsafeUtil.GetByte( p + 1 ) & 0xFF;
			  int c = UnsafeUtil.GetByte( p + 2 ) & 0xFF;
			  int d = UnsafeUtil.GetByte( p + 3 ) & 0xFF;
			  return ( d << 24 ) | ( c << 16 ) | ( b << 8 ) | a;
		 }

		 /// <summary>
		 /// Gets a {@code long} at memory address {@code p} by reading byte for byte, instead of the whole value
		 /// in one go. This can be useful, even necessary in some scenarios where <seealso cref="allowUnalignedMemoryAccess"/>
		 /// is {@code false} and {@code p} isn't aligned properly. Values read with this method should have been
		 /// previously put using <seealso cref="putLongByteWiseLittleEndian(long, long)"/>.
		 /// </summary>
		 /// <param name="p"> address pointer to start reading at. </param>
		 /// <returns> the read value, which was read byte for byte. </returns>
		 public static long GetLongByteWiseLittleEndian( long p )
		 {
			  long a = UnsafeUtil.GetByte( p ) & 0xFF;
			  long b = UnsafeUtil.GetByte( p + 1 ) & 0xFF;
			  long c = UnsafeUtil.GetByte( p + 2 ) & 0xFF;
			  long d = UnsafeUtil.GetByte( p + 3 ) & 0xFF;
			  long e = UnsafeUtil.GetByte( p + 4 ) & 0xFF;
			  long f = UnsafeUtil.GetByte( p + 5 ) & 0xFF;
			  long g = UnsafeUtil.GetByte( p + 6 ) & 0xFF;
			  long h = UnsafeUtil.GetByte( p + 7 ) & 0xFF;
			  return ( h << 56 ) | ( g << 48 ) | ( f << 40 ) | ( e << 32 ) | ( d << 24 ) | ( c << 16 ) | ( b << 8 ) | a;
		 }

		 /// <summary>
		 /// Puts a {@code short} at memory address {@code p} by writing byte for byte, instead of the whole value
		 /// in one go. This can be useful, even necessary in some scenarios where <seealso cref="allowUnalignedMemoryAccess"/>
		 /// is {@code false} and {@code p} isn't aligned properly. Values written with this method should be
		 /// read using <seealso cref="getShortByteWiseLittleEndian(long)"/>.
		 /// </summary>
		 /// <param name="p"> address pointer to start writing at. </param>
		 /// <param name="value"> value to write byte for byte. </param>
		 public static void PutShortByteWiseLittleEndian( long p, short value )
		 {
			  UnsafeUtil.PutByte( p, ( sbyte ) value );
			  UnsafeUtil.PutByte( p + 1, ( sbyte )( value >> 8 ) );
		 }

		 /// <summary>
		 /// Puts a {@code int} at memory address {@code p} by writing byte for byte, instead of the whole value
		 /// in one go. This can be useful, even necessary in some scenarios where <seealso cref="allowUnalignedMemoryAccess"/>
		 /// is {@code false} and {@code p} isn't aligned properly. Values written with this method should be
		 /// read using <seealso cref="getIntByteWiseLittleEndian(long)"/>.
		 /// </summary>
		 /// <param name="p"> address pointer to start writing at. </param>
		 /// <param name="value"> value to write byte for byte. </param>
		 public static void PutIntByteWiseLittleEndian( long p, int value )
		 {
			  UnsafeUtil.PutByte( p, ( sbyte ) value );
			  UnsafeUtil.PutByte( p + 1, ( sbyte )( value >> 8 ) );
			  UnsafeUtil.PutByte( p + 2, ( sbyte )( value >> 16 ) );
			  UnsafeUtil.PutByte( p + 3, ( sbyte )( value >> 24 ) );
		 }

		 /// <summary>
		 /// Puts a {@code long} at memory address {@code p} by writing byte for byte, instead of the whole value
		 /// in one go. This can be useful, even necessary in some scenarios where <seealso cref="allowUnalignedMemoryAccess"/>
		 /// is {@code false} and {@code p} isn't aligned properly. Values written with this method should be
		 /// read using <seealso cref="getShortByteWiseLittleEndian(long)"/>.
		 /// </summary>
		 /// <param name="p"> address pointer to start writing at. </param>
		 /// <param name="value"> value to write byte for byte. </param>
		 public static void PutLongByteWiseLittleEndian( long p, long value )
		 {
			  UnsafeUtil.PutByte( p, ( sbyte ) value );
			  UnsafeUtil.PutByte( p + 1, ( sbyte )( value >> 8 ) );
			  UnsafeUtil.PutByte( p + 2, ( sbyte )( value >> 16 ) );
			  UnsafeUtil.PutByte( p + 3, ( sbyte )( value >> 24 ) );
			  UnsafeUtil.PutByte( p + 4, ( sbyte )( value >> 32 ) );
			  UnsafeUtil.PutByte( p + 5, ( sbyte )( value >> 40 ) );
			  UnsafeUtil.PutByte( p + 6, ( sbyte )( value >> 48 ) );
			  UnsafeUtil.PutByte( p + 7, ( sbyte )( value >> 56 ) );
		 }

		 private sealed class Allocation
		 {
			  internal readonly long Pointer;
			  internal readonly long SizeInBytes;
			  internal readonly long Boundary;
			  internal readonly long FreeCounter;

			  internal Allocation( long pointer, long sizeInBytes, long freeCounter )
			  {
					this.Pointer = pointer;
					this.SizeInBytes = sizeInBytes;
					this.FreeCounter = freeCounter;
					this.Boundary = pointer + sizeInBytes;
			  }
		 }

		 private sealed class FreeTrace : Exception, IComparable<FreeTrace>
		 {
			  internal readonly long Pointer;
			  internal readonly Allocation Allocation;
			  internal readonly long Id;
			  internal readonly long NanoTime;
			  internal long ReferenceTime;

			  internal FreeTrace( long pointer, Allocation allocation, long id )
			  {
					this.Pointer = pointer;
					this.Allocation = allocation;
					this.Id = id;
					this.NanoTime = System.nanoTime();
			  }

			  internal bool Contains( long pointer )
			  {
					return this.Pointer <= pointer && pointer <= this.Pointer + Allocation.sizeInBytes;
			  }

			  public override int CompareTo( FreeTrace that )
			  {
					return Long.compare( this.Id, that.Id );
			  }

			  public override string Message
			  {
				  get
				  {
						return format( "0x%x of %6d bytes, freed %s µs ago at", Pointer, Allocation.sizeInBytes, ( ReferenceTime - NanoTime ) / 1000 );
				  }
			  }
		 }

	}

}