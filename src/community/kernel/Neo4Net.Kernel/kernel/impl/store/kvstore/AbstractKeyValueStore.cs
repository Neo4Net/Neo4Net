using System;
using System.Collections.Generic;
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
namespace Neo4Net.Kernel.impl.store.kvstore
{

	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using FileIsNotMappedException = Neo4Net.Io.pagecache.impl.FileIsNotMappedException;
	using VersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.VersionContextSupplier;
	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;
	using Logger = Neo4Net.Logging.Logger;
	using FeatureToggles = Neo4Net.Utils.FeatureToggles;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.store.kvstore.LockWrapper.readLock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.store.kvstore.LockWrapper.writeLock;

	/// <summary>
	/// The base for building a key value store based on rotating immutable
	/// <seealso cref="KeyValueStoreFile key/value store files"/>
	/// </summary>
	/// @param <Key> a base type for the keys stored in this store. </param>
	[State(State.Strategy.CONCURRENT_HASH_MAP)]
	public abstract class AbstractKeyValueStore<Key> : LifecycleAdapter
	{
		 internal static readonly long MaxLookupRetryCount = FeatureToggles.getLong( typeof( AbstractKeyValueStore ), "maxLookupRetryCount", 1024 );

		 private readonly UpdateLock _updateLock = new UpdateLock();
		 private readonly Format _format;
		 internal readonly RotationStrategy RotationStrategy;
		 private readonly RotationTimerFactory _rotationTimerFactory;
		 private readonly Logger _logger;
		 internal volatile ProgressiveState<Key> State;
		 private DataInitializer<EntryUpdater<Key>> _stateInitializer;
		 private readonly FileSystemAbstraction _fs;
		 internal readonly int KeySize;
		 internal readonly int ValueSize;
		 private volatile bool _stopped;

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public AbstractKeyValueStore(Neo4Net.io.fs.FileSystemAbstraction fs, Neo4Net.io.pagecache.PageCache pages, Neo4Net.io.layout.DatabaseLayout databaseLayout, RotationMonitor monitor, Neo4Net.logging.Logger logger, RotationTimerFactory timerFactory, Neo4Net.io.pagecache.tracing.cursor.context.VersionContextSupplier versionContextSupplier, int keySize, int valueSize, HeaderField<?>... headerFields)
		 public AbstractKeyValueStore( FileSystemAbstraction fs, PageCache pages, DatabaseLayout databaseLayout, RotationMonitor monitor, Logger logger, RotationTimerFactory timerFactory, VersionContextSupplier versionContextSupplier, int keySize, int valueSize, params HeaderField<object>[] headerFields )
		 {
			  this._fs = fs;
			  this.KeySize = keySize;
			  this.ValueSize = valueSize;
			  Rotation rotation = this.GetType().getAnnotation(typeof(Rotation));
			  if ( monitor == null )
			  {
					monitor = RotationMonitor.NONE;
			  }
			  this._format = new Format( this, headerFields );
			  this._logger = logger;
			  this.RotationStrategy = rotation.value().create(fs, pages, _format, monitor, databaseLayout);
			  this._rotationTimerFactory = timerFactory;
			  this.State = new DeadState.Stopped<Key>( _format, this.GetType().getAnnotation(typeof(State)).value(), versionContextSupplier );
		 }

		 protected internal DataInitializer<EntryUpdater<Key>> EntryUpdaterInitializer
		 {
			 set
			 {
				  this._stateInitializer = value;
			 }
		 }

		 public override string ToString()
		 {
			  return string.Format( "{0}[state={1}, hasChanges={2}]", this.GetType().Name, State, State.hasChanges() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected final <Value> Value lookup(Key key, Reader<Value> reader) throws java.io.IOException
		 protected internal Value Lookup<Value>( Key key, Reader<Value> reader )
		 {
			  ValueLookup<Value> lookup = new ValueLookup<Value>( reader );
			  long retriesLeft = MaxLookupRetryCount;
			  while ( retriesLeft > 0 )
			  {
					ProgressiveState<Key> originalState = this.State;
					try
					{
						 return lookup.Value( !originalState.lookup( key, lookup ) );
					}
					catch ( FileIsNotMappedException e )
					{
						 // if the state has changed we think the exception is caused by a rotation event. In this
						 // case we simply retry the lookup on the rotated state. Otherwise we rethrow.
						 if ( originalState == this.State )
						 {
							  throw e;
						 }
					}
					retriesLeft--;
			  }
			  throw new IOException( string.Format( "Failed to lookup `{0}` in key value store, after {1:D} retries", key, MaxLookupRetryCount ) );
		 }

		 /// <summary>
		 /// Introspective feature, not thread safe. </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected final void visitAll(Visitor visitor) throws java.io.IOException
		 protected internal void VisitAll( Visitor visitor )
		 {
			  ProgressiveState<Key> state = this.State;
			  if ( visitor is MetadataVisitor )
			  {
					( ( MetadataVisitor ) visitor ).VisitMetadata( state.File(), Headers(), state.storedEntryCount() );
			  }
			  using ( DataProvider provider = state.dataProvider() )
			  {
					Transfer( provider, visitor );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected final void visitFile(java.io.File path, Visitor visitor) throws java.io.IOException
		 protected internal void VisitFile( File path, Visitor visitor )
		 {
			  using ( KeyValueStoreFile file = RotationStrategy.openStoreFile( path ) )
			  {
					if ( visitor is MetadataVisitor )
					{
						 ( ( MetadataVisitor ) visitor ).VisitMetadata( path, file.Headers(), file.EntryCount() );
					}
					using ( DataProvider provider = file.DataProvider() )
					{
						 Transfer( provider, visitor );
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected abstract Key readKey(ReadableBuffer key) throws UnknownKey;
		 protected internal abstract Key ReadKey( ReadableBuffer key );

		 protected internal abstract void WriteKey( Key key, WritableBuffer buffer );

		 protected internal abstract void WriteFormatSpecifier( WritableBuffer formatSpecifier );

		 protected internal abstract Headers InitialHeaders( long version );

		 protected internal abstract int CompareHeaders( Headers lhs, Headers rhs );

		 protected internal virtual bool Include( Key key, ReadableBuffer value )
		 {
			  return true;
		 }

		 protected internal Headers Headers()
		 {
			  return State.headers();
		 }

		 public virtual int TotalEntriesStored()
		 {
			  return State.storedEntryCount();
		 }

		 public File CurrentFile()
		 {
			  return State.file();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public final void init() throws java.io.IOException
		 public override void Init()
		 {
			  using ( LockWrapper ignored = writeLock( _updateLock, _logger ) )
			  {
					State = State.initialize( RotationStrategy );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public final void start() throws java.io.IOException
		 public override void Start()
		 {
			  using ( LockWrapper ignored = writeLock( _updateLock, _logger ) )
			  {
					State = State.start( _stateInitializer );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: protected final java.util.Optional<EntryUpdater<Key>> updater(final long version)
		 protected internal Optional<EntryUpdater<Key>> Updater( long version )
		 {
			  using ( LockWrapper @lock = readLock( _updateLock, _logger ) )
			  {
					return State.optionalUpdater( version, @lock.Get() );
			  }
		 }

		 protected internal EntryUpdater<Key> Updater()
		 {
			  using ( LockWrapper @lock = readLock( _updateLock, _logger ) )
			  {
					return State.unsafeUpdater( @lock.Get() );
			  }
		 }

		 protected internal EntryUpdater<Key> Resetter( long version )
		 {
			  using ( LockWrapper @lock = writeLock( _updateLock, _logger ) )
			  {
					ProgressiveState<Key> current = State;
					return current.Resetter( @lock.Get(), new RotationTask(this, version) );
			  }
		 }

		 /// <summary>
		 /// Prepare for rotation. Sets up the internal structures to ensure that all changes up to and including the changes
		 /// of the specified version are applied before rotation takes place. This method does not block, however if all
		 /// required changes have not been applied <seealso cref="PreparedRotation.rotate() the rotate method"/> will block
		 /// waiting for all changes to be applied. Invoking <seealso cref="PreparedRotation.rotate() the rotate method"/> some
		 /// time after all requested transactions have been applied is ok, since setting the store up for rotation does
		 /// not block updates, it just sorts them into updates that apply before rotation and updates that apply after.
		 /// </summary>
		 /// <param name="version"> the smallest version to include in the rotation. Note that the actual rotated version might be a
		 /// later version than this version. The actual rotated version is returned by
		 /// <seealso cref="PreparedRotation.rotate()"/>. </param>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: protected final PreparedRotation prepareRotation(final long version)
		 protected internal PreparedRotation PrepareRotation( long version )
		 {
			  using ( LockWrapper ignored = writeLock( _updateLock, _logger ) )
			  {
					ProgressiveState<Key> prior = State;
					if ( prior.StoredVersion() == version && !prior.HasChanges() )
					{
						 return () => version;
					}
					return new RotationTask( this, version );
			  }
		 }

		 protected internal abstract void UpdateHeaders( Headers.Builder headers, long version );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public final void shutdown() throws java.io.IOException
		 public override void Shutdown()
		 {
			  using ( LockWrapper ignored = writeLock( _updateLock, _logger ) )
			  {
					_stopped = true;
					State = State.stop();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private boolean transfer(EntryVisitor<WritableBuffer> producer, EntryVisitor<ReadableBuffer> consumer) throws java.io.IOException
		 private bool Transfer( EntryVisitor<WritableBuffer> producer, EntryVisitor<ReadableBuffer> consumer )
		 {
			  BigEndianByteArrayBuffer key = new BigEndianByteArrayBuffer( KeySize );
			  BigEndianByteArrayBuffer value = new BigEndianByteArrayBuffer( ValueSize );
			  while ( producer.Visit( key, value ) )
			  {
					if ( !consumer.Visit( key, value ) )
					{
						 return false;
					}
			  }
			  return true;
		 }

		 public virtual IEnumerable<File> AllFiles()
		 {
			  return StreamSupport.stream( RotationStrategy.candidateFiles().spliterator(), false ).filter(_fs.fileExists).collect(Collectors.toList());
		 }

		 private class RotationTask : PreparedRotation, ThreadStart
		 {
			 private readonly AbstractKeyValueStore<Key> _outerInstance;

			  internal readonly RotationState<Key> Rotation;

			  internal RotationTask( AbstractKeyValueStore<Key> outerInstance, long version )
			  {
				  this._outerInstance = outerInstance;
					outerInstance.State = this.Rotation = outerInstance.State.prepareRotation( version );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long rotate() throws java.io.IOException
			  public override long Rotate()
			  {
					return Rotate( false );
			  }

			  public override void Run()
			  {
					try
					{
							using ( LockWrapper ignored = writeLock( outerInstance.updateLock, outerInstance.logger ) )
							{
							 Rotate( true );
							}
					}
					catch ( IOException e )
					{
						 throw new UnderlyingStorageException( e );
					}
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private long rotate(boolean force) throws java.io.IOException
			  internal virtual long Rotate( bool force )
			  {
					using ( RotationState<Key> rotation = this.Rotation )
					{
						 try
						 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long version = rotation.rotationVersion();
							  long version = rotation.RotationVersion();
							  ProgressiveState<Key> next = rotation.Rotate( force, outerInstance.RotationStrategy, outerInstance.rotationTimerFactory, value => updateHeaders( value, version ) );
							  using ( LockWrapper ignored = writeLock( outerInstance.updateLock, outerInstance.logger ) )
							  {
									outerInstance.State = next;
							  }
							  return version;
						 }
						 catch ( Exception t )
						 {
							  // Rotation failed. Here we assume that rotation state remembers this so that closing it
							  // won't close the state as it was before rotation began, which we're reverting to right here.
							  using ( LockWrapper ignored = writeLock( outerInstance.updateLock, outerInstance.logger ) )
							  {
									// Only mark as failed if we're still running.
									// If shutdown has been called while being in rotation state then shutdown will fail
									// without closing the store. This means that rotation takes over that responsibility.
									// Therefore avoid marking rotation state as failed in this case and let the store
									// be naturally closed before leaving this method.
									if ( !outerInstance.stopped )
									{
										 outerInstance.State = rotation.MarkAsFailed();
									}
							  }
							  throw t;
						 }
					}
			  }
		 }

		 public abstract class Reader<Value>
		 {
			  protected internal abstract Value ParseValue( ReadableBuffer value );

			  protected internal virtual Value DefaultValue()
			  {
					return default( Value );
			  }
		 }

		 public abstract class Visitor : KeyValueVisitor
		 {
			 private readonly AbstractKeyValueStore<Key> _outerInstance;

			 public Visitor( AbstractKeyValueStore<Key> outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  public override bool Visit( ReadableBuffer key, ReadableBuffer value )
			  {
					try
					{
						 return VisitKeyValuePair( outerInstance.ReadKey( key ), value );
					}
					catch ( UnknownKey e )
					{
						 return VisitUnknownKey( e, key, value );
					}
			  }

			  protected internal virtual bool VisitUnknownKey( UnknownKey exception, ReadableBuffer key, ReadableBuffer value )
			  {
					throw new System.ArgumentException( exception.Message, exception );
			  }

			  protected internal abstract bool VisitKeyValuePair( Key key, ReadableBuffer value );
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private HeaderField<?>[] headerFieldsForFormat(ReadableBuffer formatSpecifier)
		 private HeaderField<object>[] HeaderFieldsForFormat( ReadableBuffer formatSpecifier )
		 {
			  return _format.defaultHeaderFieldsForFormat( formatSpecifier );
		 }

		 protected internal abstract long Version( Headers headers );

		 private sealed class Format : ProgressiveFormat, KeyFormat<Key>
		 {
			 private readonly AbstractKeyValueStore<Key> _outerInstance;

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: Format(HeaderField<?>... headerFields)
			  internal Format( AbstractKeyValueStore<Key> outerInstance, params HeaderField<object>[] headerFields ) : base( 512, headerFields )
			  {
				  this._outerInstance = outerInstance;
			  }

			  protected internal override void WriteFormatSpecifier( WritableBuffer formatSpecifier )
			  {
					_outerInstance.writeFormatSpecifier( formatSpecifier );
			  }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: protected HeaderField<?>[] headerFieldsForFormat(ReadableBuffer formatSpecifier)
			  protected internal override HeaderField<object>[] HeaderFieldsForFormat( ReadableBuffer formatSpecifier )
			  {
					return _outerInstance.headerFieldsForFormat( formatSpecifier );
			  }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: HeaderField<?>[] defaultHeaderFieldsForFormat(ReadableBuffer formatSpecifier)
			  internal HeaderField<object>[] DefaultHeaderFieldsForFormat( ReadableBuffer formatSpecifier )
			  {
					return base.HeaderFieldsForFormat( formatSpecifier );
			  }

			  public override void WriteKey( Key key, WritableBuffer buffer )
			  {
					_outerInstance.writeKey( key, buffer );
			  }

			  public override int CompareHeaders( Headers lhs, Headers rhs )
			  {
					return _outerInstance.compareHeaders( lhs, rhs );
			  }

			  public override Headers InitialHeaders( long version )
			  {
					return _outerInstance.initialHeaders( version );
			  }

			  public override int KeySize()
			  {
					return _outerInstance.keySize;
			  }

			  public override long Version( Headers headers )
			  {
					return _outerInstance.version( headers );
			  }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public DataProvider filter(final DataProvider provider)
			  public override DataProvider Filter( DataProvider provider )
			  {
					return new DataProviderAnonymousInnerClass( this, provider );
			  }

			  private class DataProviderAnonymousInnerClass : DataProvider
			  {
				  private readonly Format _outerInstance;

				  private Neo4Net.Kernel.impl.store.kvstore.DataProvider _provider;

				  public DataProviderAnonymousInnerClass( Format outerInstance, Neo4Net.Kernel.impl.store.kvstore.DataProvider provider )
				  {
					  this.outerInstance = outerInstance;
					  this._provider = provider;
				  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean visit(WritableBuffer key, WritableBuffer value) throws java.io.IOException
				  public bool visit( WritableBuffer key, WritableBuffer value )
				  {
						while ( _provider.visit( key, value ) )
						{
							 try
							 {
								  if ( outerInstance.outerInstance.Include( outerInstance.outerInstance.ReadKey( key ), value ) )
								  {
										return true;
								  }
							 }
							 catch ( UnknownKey e )
							 {
								  throw new System.ArgumentException( e.Message, e );
							 }
						}
						return false;
				  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
				  public void close()
				  {
						_provider.Dispose();
				  }
			  }

			  public override int ValueSize()
			  {
					return _outerInstance.valueSize;
			  }
		 }
	}

}