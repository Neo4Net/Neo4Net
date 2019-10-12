using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

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
namespace Org.Neo4j.Kernel.impl.store.kvstore
{

	using VersionContext = Org.Neo4j.Io.pagecache.tracing.cursor.context.VersionContext;
	using VersionContextSupplier = Org.Neo4j.Io.pagecache.tracing.cursor.context.VersionContextSupplier;
	using TransactionIdStore = Org.Neo4j.Kernel.impl.transaction.log.TransactionIdStore;

	internal class ConcurrentMapState<Key> : ActiveState<Key>
	{
		 private readonly ConcurrentMap<Key, ChangeEntry> _changes;
		 private readonly new VersionContextSupplier _versionContextSupplier;
		 private readonly File _file;
		 private readonly AtomicLong _highestAppliedVersion;
		 private readonly AtomicLong _appliedChanges;
		 private readonly AtomicBoolean _hasTrackedChanges;
		 private readonly long _previousVersion;

		 internal ConcurrentMapState( ReadableState<Key> store, File file, VersionContextSupplier versionContextSupplier ) : base( store, versionContextSupplier )
		 {
			  this._previousVersion = store.Version();
			  this._file = file;
			  this._versionContextSupplier = versionContextSupplier;
			  this._highestAppliedVersion = new AtomicLong( _previousVersion );
			  this._changes = new ConcurrentDictionary<Key, ChangeEntry>();
			  this._appliedChanges = new AtomicLong();
			  _hasTrackedChanges = new AtomicBoolean();
		 }

		 private ConcurrentMapState( Prototype<Key> prototype, ReadableState<Key> store, File file, VersionContextSupplier versionContextSupplier ) : base( store, versionContextSupplier )
		 {
			  this._previousVersion = store.Version();
			  this._versionContextSupplier = versionContextSupplier;
			  this._file = file;
			  this._hasTrackedChanges = prototype.HasTrackedChanges;
			  this._changes = prototype.Changes;
			  this._highestAppliedVersion = prototype.HighestAppliedVersion;
			  this._appliedChanges = prototype.AppliedChanges;
		 }

		 public override string ToString()
		 {
			  return base.ToString() + "[" + _file + "]";
		 }

		 public override EntryUpdater<Key> Updater( long version, Lock @lock )
		 {
			  if ( version <= _previousVersion )
			  {
					return EntryUpdater.NoUpdates();
			  }
			  Update( _highestAppliedVersion, version );
			  _hasTrackedChanges.set( true );
			  return new Updater<Key>( @lock, Store, _changes, _appliedChanges, version );
		 }

		 public override EntryUpdater<Key> UnsafeUpdater( Lock @lock )
		 {
			  _hasTrackedChanges.set( true );
			  return new Updater<Key>( @lock, Store, _changes, null, Org.Neo4j.Kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_ID );
		 }

		 private class Updater<Key> : EntryUpdater<Key>
		 {
			  internal AtomicLong ChangeCounter;
			  internal readonly ReadableState<Key> Store;
			  internal readonly ConcurrentMap<Key, ChangeEntry> Changes;
			  internal readonly long Version;

			  internal Updater( Lock @lock, ReadableState<Key> store, ConcurrentMap<Key, ChangeEntry> changes, AtomicLong changeCounter, long version ) : base( @lock )
			  {
					this.ChangeCounter = changeCounter;
					this.Store = store;
					this.Changes = changes;
					this.Version = version;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void apply(Key key, ValueUpdate update) throws java.io.IOException
			  public override void Apply( Key key, ValueUpdate update )
			  {
					EnsureOpenOnSameThread();
					ApplyUpdate( Store, Changes, key, update, false, Version );
			  }

			  public override void Close()
			  {
					if ( ChangeCounter != null )
					{
						 ChangeCounter.incrementAndGet();
						 ChangeCounter = null;
					}
					base.Close();
			  }
		 }

		 protected internal override long StoredVersion()
		 {
			  return _previousVersion;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: protected EntryUpdater<Key> resettingUpdater(java.util.concurrent.locks.Lock lock, final Runnable closeAction)
		 protected internal override EntryUpdater<Key> ResettingUpdater( Lock @lock, ThreadStart closeAction )
		 {
			  if ( HasChanges() )
			  {
					throw new System.InvalidOperationException( "Cannot reset when there are changes!" );
			  }
			  return new EntryUpdaterAnonymousInnerClass( this, @lock, closeAction );
		 }

		 private class EntryUpdaterAnonymousInnerClass : EntryUpdater<Key>
		 {
			 private readonly ConcurrentMapState<Key> _outerInstance;

			 private ThreadStart _closeAction;

			 public EntryUpdaterAnonymousInnerClass( ConcurrentMapState<Key> outerInstance, Lock @lock, ThreadStart closeAction ) : base( @lock )
			 {
				 this.outerInstance = outerInstance;
				 this._closeAction = closeAction;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void apply(Key key, ValueUpdate update) throws java.io.IOException
			 public override void apply( Key key, ValueUpdate update )
			 {
				  ensureOpen();
				  ApplyUpdate( _outerInstance.store, _outerInstance.changes, key, update, true, _outerInstance.highestAppliedVersion.get() );
			 }

			 public override void close()
			 {
				  try
				  {
						_closeAction.run();
				  }
				  finally
				  {
						base.close();
				  }
			 }
		 }

		 protected internal override PrototypeState<Key> Prototype( long version )
		 {
			  return new Prototype<Key>( this, version );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("SynchronizationOnLocalVariableOrMethodParameter") static <Key> void applyUpdate(ReadableState<Key> store, java.util.concurrent.ConcurrentMap<Key, ChangeEntry> changes, Key key, ValueUpdate update, boolean reset, long version) throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal static void ApplyUpdate<Key>( ReadableState<Key> store, ConcurrentMap<Key, ChangeEntry> changes, Key key, ValueUpdate update, bool reset, long version )
		 {
			 ChangeEntry value = changes.get( key );
			  if ( value == null )
			  {
					ChangeEntry newEntry = ChangeEntry.Of( new sbyte[store.KeyFormat().valueSize()], version );
					lock ( newEntry )
					{
						 value = changes.putIfAbsent( key, newEntry );
						 if ( value == null )
						 {
							  BigEndianByteArrayBuffer buffer = new BigEndianByteArrayBuffer( newEntry.Data );
							  if ( !reset )
							  {
									PreviousValue lookup = new PreviousValue( newEntry.Data );
									if ( !store.Lookup( key, lookup ) )
									{
										 buffer.Clear();
									}
							  }
							  update.Update( buffer );
							  return;
						 }
					}
			  }
			  lock ( value )
			  {
					BigEndianByteArrayBuffer target = new BigEndianByteArrayBuffer( value.Data );
					value.Version = version;
					if ( reset )
					{
						 target.Clear();
					}
					update.Update( target );
			  }
		 }

		 private static void Update( AtomicLong highestAppliedVersion, long version )
		 {
			  for ( long high; ; )
			  {
					high = highestAppliedVersion.get();
					if ( version <= high )
					{
						 return;
					}
					if ( highestAppliedVersion.compareAndSet( high, version ) )
					{
						 return;
					}
			  }
		 }

		 private class Prototype<Key> : PrototypeState<Key>
		 {
			  internal readonly ConcurrentMap<Key, ChangeEntry> Changes = new ConcurrentDictionary<Key, ChangeEntry>();
			  internal readonly AtomicLong HighestAppliedVersion;
			  internal readonly AtomicLong AppliedChanges = new AtomicLong();
			  internal readonly VersionContextSupplier VersionContextSupplier;
			  internal readonly AtomicBoolean HasTrackedChanges;
			  internal readonly long Threshold;

			  internal Prototype( ConcurrentMapState<Key> state, long version ) : base( state )
			  {
					this.VersionContextSupplier = state.VersionContextSupplier;
					Threshold = version;
					HasTrackedChanges = new AtomicBoolean();
					this.HighestAppliedVersion = new AtomicLong( version );
			  }

			  protected internal override ActiveState<Key> Create( ReadableState<Key> sub, File file, VersionContextSupplier versionContextSupplier )
			  {
					return new ConcurrentMapState<Key>( this, sub, file, versionContextSupplier );
			  }

			  protected internal override EntryUpdater<Key> Updater( long version, Lock @lock )
			  {
					Update( HighestAppliedVersion, version );
					if ( version > Threshold )
					{
						 HasTrackedChanges.set( true );
						 return new Updater<Key>( @lock, Store, Changes, AppliedChanges, version );
					}
					else
					{
						 return new Updater<Key>( @lock, Store, Changes, null, version );
					}
			  }

			  protected internal override EntryUpdater<Key> UnsafeUpdater( Lock @lock )
			  {
					HasTrackedChanges.set( true );
					return new Updater<Key>( @lock, Store, Changes, null, HighestAppliedVersion.get() );
			  }

			  protected internal override bool HasChanges()
			  {
					return HasTrackedChanges.get() && !Changes.Empty;
			  }

			  protected internal override long Version()
			  {
					return HighestAppliedVersion.get();
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected boolean lookup(Key key, ValueSink sink) throws java.io.IOException
			  protected internal override bool Lookup( Key key, ValueSink sink )
			  {
					return PerformLookup( Store, VersionContextSupplier.VersionContext, Changes, key, sink );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected DataProvider dataProvider() throws java.io.IOException
			  protected internal override DataProvider DataProvider()
			  {
					return ConcurrentMapState.dataProvider( Store, Changes );
			  }
		 }

		 private class PreviousValue : ValueSink
		 {
			  internal readonly sbyte[] Proposal;

			  internal PreviousValue( sbyte[] proposal )
			  {
					this.Proposal = proposal;
			  }

			  protected internal override void Value( ReadableBuffer value )
			  {
					value.Get( 0, Proposal );
			  }
		 }

		 protected internal override long Version()
		 {
			  return _highestAppliedVersion.get();
		 }

		 protected internal override long Applied()
		 {
			  return _appliedChanges.get();
		 }

		 protected internal override bool HasChanges()
		 {
			  return _hasTrackedChanges.get() && !_changes.Empty;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  Store.Dispose();
		 }

		 protected internal override File File()
		 {
			  return _file;
		 }

		 protected internal override Factory Factory()
		 {
			  return State.Strategy.CONCURRENT_HASH_MAP;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected boolean lookup(Key key, ValueSink sink) throws java.io.IOException
		 protected internal override bool Lookup( Key key, ValueSink sink )
		 {
			  return PerformLookup( Store, _versionContextSupplier.VersionContext, _changes, key, sink );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static <Key> boolean performLookup(ReadableState<Key> store, org.neo4j.io.pagecache.tracing.cursor.context.VersionContext versionContext, java.util.concurrent.ConcurrentMap<Key,ChangeEntry> changes, Key key, ValueSink sink) throws java.io.IOException
		 private static bool PerformLookup<Key>( ReadableState<Key> store, VersionContext versionContext, ConcurrentMap<Key, ChangeEntry> changes, Key key, ValueSink sink )
		 {
			  ChangeEntry change = changes.get( key );
			  if ( change != null )
			  {
					if ( change.Version > versionContext.LastClosedTransactionId() )
					{
						 versionContext.MarkAsDirty();
					}
					sink.Value( new BigEndianByteArrayBuffer( change.Data ) );
					return true;
			  }
			  return store.Lookup( key, sink );
		 }

		 /// <summary>
		 /// This method is expected to be called under a lock preventing modification to the state.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public DataProvider dataProvider() throws java.io.IOException
		 public override DataProvider DataProvider()
		 {
			  return DataProvider( Store, _changes );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static <Key> DataProvider dataProvider(ReadableState<Key> store, java.util.concurrent.ConcurrentMap<Key, ChangeEntry> changes) throws java.io.IOException
		 private static DataProvider DataProvider<Key>( ReadableState<Key> store, ConcurrentMap<Key, ChangeEntry> changes )
		 {
			  if ( changes.Empty )
			  {
					return store.DataProvider();
			  }
			  else
			  {
					KeyFormat<Key> keys = store.KeyFormat();
					return new KeyValueMerger( store.DataProvider(), new UpdateProvider(SortedUpdates(keys, changes)), keys.KeySize(), keys.ValueSize() );
			  }
		 }

		 private static sbyte[][] SortedUpdates<Key>( KeyFormat<Key> keys, ConcurrentMap<Key, ChangeEntry> changes )
		 {
			  Entry[] buffer = new Entry[changes.size()];
			  IEnumerator<KeyValuePair<Key, ChangeEntry>> entries = changes.entrySet().GetEnumerator();
			  for ( int i = 0; i < buffer.Length; i++ )
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					KeyValuePair<Key, ChangeEntry> next = entries.next(); // we hold the lock, so this should succeed
					sbyte[] key = new sbyte[keys.KeySize()];
					keys.WriteKey( next.Key, new BigEndianByteArrayBuffer( key ) );
					buffer[i] = new Entry( key, next.Value.data );
			  }
			  Arrays.sort( buffer );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  Debug.Assert( !entries.hasNext(), "We hold the lock, so we should see 'size' entries." );
			  sbyte[][] result = new sbyte[buffer.Length * 2][];
			  for ( int i = 0; i < buffer.Length; i++ )
			  {
					result[i * 2] = buffer[i].Key;
					result[i * 2 + 1] = buffer[i].Value;
			  }
			  return result;
		 }

		 private class Entry : IComparable<Entry>
		 {
			  internal readonly sbyte[] Key;
			  internal readonly sbyte[] Value;

			  internal Entry( sbyte[] key, sbyte[] value )
			  {
					this.Key = key;
					this.Value = value;
			  }

			  public override int CompareTo( Entry that )
			  {
					return BigEndianByteArrayBuffer.Compare( this.Key, that.Key, 0 );
			  }
		 }

		 private class UpdateProvider : DataProvider
		 {
			  internal readonly sbyte[][] Data;
			  internal int I;

			  internal UpdateProvider( sbyte[][] data )
			  {
					this.Data = data;
			  }

			  public override bool Visit( WritableBuffer key, WritableBuffer value )
			  {
					if ( I < Data.Length )
					{
						 key.Put( 0, Data[I] );
						 value.Put( 0, Data[I + 1] );
						 I += 2;
						 return true;
					}
					return false;
			  }

			  public override void Close()
			  {
			  }
		 }

		 private class ChangeEntry
		 {
			  internal sbyte[] Data;
			  internal long Version;

			  internal static ChangeEntry Of( sbyte[] data, long version )
			  {
					return new ChangeEntry( data, version );
			  }

			  internal ChangeEntry( sbyte[] data, long version )
			  {
					this.Data = data;
					this.Version = version;
			  }
		 }
	}

}