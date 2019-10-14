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

	using Neo4Net.Helpers.Collections;
	using VersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.VersionContextSupplier;

	internal abstract class DeadState<Key> : ProgressiveState<Key>
	{
		 protected internal override Headers Headers()
		 {
			  throw new System.InvalidOperationException( "Cannot read in state: " + StateName() );
		 }

		 protected internal override bool Lookup( Key key, ValueSink sink )
		 {
			  throw new System.InvalidOperationException( "Cannot read in state: " + StateName() );
		 }

		 protected internal override DataProvider DataProvider()
		 {
			  throw new System.InvalidOperationException( "Cannot read in state: " + StateName() );
		 }

		 protected internal override int StoredEntryCount()
		 {
			  throw new System.InvalidOperationException( "Cannot read in state: " + StateName() );
		 }

		 protected internal override Optional<EntryUpdater<Key>> OptionalUpdater( long version, Lock @lock )
		 {
			  throw new System.InvalidOperationException( "Cannot write in state: " + StateName() );
		 }

		 protected internal override EntryUpdater<Key> UnsafeUpdater( Lock @lock )
		 {
			  throw new System.InvalidOperationException( "Cannot write in state: " + StateName() );
		 }

		 protected internal override bool HasChanges()
		 {
			  return false;
		 }

		 public override void Close()
		 {
			  throw new System.InvalidOperationException( "Cannot close() in state: " + StateName() );
		 }

		 protected internal override File File()
		 {
			  throw new System.InvalidOperationException( "No file assigned in state: " + StateName() );
		 }

		 protected internal override long Version()
		 {
			  return _keys.version( null );
		 }

		 protected internal override KeyFormat<Key> KeyFormat()
		 {
			  return _keys;
		 }

		 private readonly KeyFormat<Key> _keys;
		 internal readonly ActiveState.Factory StateFactory;
		 internal readonly VersionContextSupplier VersionContextSupplier;

		 private DeadState( KeyFormat<Key> keys, ActiveState.Factory stateFactory, VersionContextSupplier versionContextSupplier )
		 {
			  this._keys = keys;
			  this.StateFactory = stateFactory;
			  this.VersionContextSupplier = versionContextSupplier;
		 }

		 internal class Stopped<Key> : DeadState<Key>
		 {
			  internal Stopped( KeyFormat<Key> keys, ActiveState.Factory stateFactory, VersionContextSupplier versionContextSupplier ) : base( keys, stateFactory, versionContextSupplier )
			  {
			  }

			  internal override string StateName()
			  {
					return "stopped";
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: ProgressiveState<Key> initialize(RotationStrategy rotation) throws java.io.IOException
			  internal override ProgressiveState<Key> Initialize( RotationStrategy rotation )
			  {
					Pair<File, KeyValueStoreFile> opened = rotation.Open();
					if ( opened == null )
					{
						 return new NeedsCreation<Key>( KeyFormat(), StateFactory, rotation, VersionContextSupplier );
					}
					return new Prepared<Key>( StateFactory.open( ReadableState.Store( KeyFormat(), opened.Other() ), opened.First(), VersionContextSupplier ) );
			  }

			  internal override ProgressiveState<Key> Stop()
			  {
					return this;
			  }
		 }

		 private class NeedsCreation<Key> : DeadState<Key>, System.Func<ActiveState<Key>, NeedsCreation<Key>>
		 {
			  internal readonly RotationStrategy Rotation;

			  internal NeedsCreation( KeyFormat<Key> keys, ActiveState.Factory stateFactory, RotationStrategy rotation, VersionContextSupplier versionContextSupplier ) : base( keys, stateFactory, versionContextSupplier )
			  {
					this.Rotation = rotation;
			  }

			  internal override ProgressiveState<Key> Stop()
			  {
					return new Stopped<Key>( KeyFormat(), StateFactory, VersionContextSupplier );
			  }

			  internal override string StateName()
			  {
					return "needs creation";
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: ActiveState<Key> start(DataInitializer<EntryUpdater<Key>> initializer) throws java.io.IOException
			  internal override ActiveState<Key> Start( DataInitializer<EntryUpdater<Key>> initializer )
			  {
					if ( initializer == null )
					{
						 throw new System.InvalidOperationException( "Store needs to be created, and no initializer is given." );
					}
					Pair<File, KeyValueStoreFile> created = InitialState( initializer );
					return StateFactory.open( ReadableState.Store( KeyFormat(), created.Other() ), created.First(), VersionContextSupplier );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.helpers.collection.Pair<java.io.File, KeyValueStoreFile> initialState(DataInitializer<EntryUpdater<Key>> initializer) throws java.io.IOException
			  internal virtual Pair<File, KeyValueStoreFile> InitialState( DataInitializer<EntryUpdater<Key>> initializer )
			  {
					long version = initializer.InitialVersion();
					using ( ActiveState<Key> creation = StateFactory.open( ReadableState.Empty( KeyFormat(), version ), null, VersionContextSupplier ) )
					{
						 try (EntryUpdater<Key> updater = creation.resetter(new ReentrantLock(), () =>
						 {
						 }
						))
						{
							  initializer.Initialize( updater );
						}
						 return Rotation.create( KeyFormat().filter(creation.dataProvider()), initializer.InitialVersion() );
					}
			  }

			  /// <summary>
			  /// called during recovery </summary>
			  protected internal override Optional<EntryUpdater<Key>> OptionalUpdater( long version, Lock @lock )
			  {
					return null;
			  }

			  /// <summary>
			  /// for rotating recovered state (none) </summary>
			  internal override RotationState<Key> PrepareRotation( long version )
			  {
					return new RotationAnonymousInnerClass( this );
			  }

			  private class RotationAnonymousInnerClass : Rotation<Key, NeedsCreation<Key>>
			  {
				  private readonly NeedsCreation<Key> _outerInstance;

				  public RotationAnonymousInnerClass( NeedsCreation<Key> outerInstance ) : base( outerInstance )
				  {
					  this.outerInstance = outerInstance;
				  }

				  internal override ProgressiveState<Key> rotate( bool force, RotationStrategy strategy, RotationTimerFactory timerFactory, System.Action<Headers.Builder> headers )
				  {
						return state;
				  }

				  public override void close()
				  {
				  }

				  internal override long rotationVersion()
				  {
						return state.version();
				  }

				  internal override ProgressiveState<Key> markAsFailed()
				  {
						return this;
				  }
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public NeedsCreation<Key> apply(ActiveState<Key> keyActiveState) throws RuntimeException
			  public override NeedsCreation<Key> Apply( ActiveState<Key> keyActiveState )
			  {
					return this;
			  }
		 }

		 private class Prepared<Key> : DeadState<Key>
		 {
			  internal readonly ActiveState<Key> State;

			  internal Prepared( ActiveState<Key> state ) : base( state.KeyFormat(), state.Factory(), state.VersionContextSupplier )
			  {
					this.State = state;
			  }

			  protected internal override Headers Headers()
			  {
					return State.headers();
			  }

			  /// <summary>
			  /// for applying recovered transactions </summary>
			  protected internal override Optional<EntryUpdater<Key>> OptionalUpdater( long version, Lock @lock )
			  {
					if ( version <= State.version() )
					{
						 return null;
					}
					else
					{
						 return State.updater( version, @lock );
					}
			  }

			  /// <summary>
			  /// for rotating recovered state </summary>
			  internal override RotationState<Key> PrepareRotation( long version )
			  {
					return new RotationAnonymousInnerClass( this, State.prepareRotation( version ) );
			  }

			  private class RotationAnonymousInnerClass : Rotation<Key, RotationState.Rotation<Key>>
			  {
				  private readonly Prepared<Key> _outerInstance;

				  public RotationAnonymousInnerClass( Prepared<Key> outerInstance, Neo4Net.Kernel.impl.store.kvstore.RotationState.Rotation<Key> prepareRotation ) : base( prepareRotation )
				  {
					  this.outerInstance = outerInstance;
				  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: ProgressiveState<Key> rotate(boolean force, RotationStrategy strategy, RotationTimerFactory timerFactory, System.Action<Headers.Builder> headers) throws java.io.IOException
				  internal override ProgressiveState<Key> rotate( bool force, RotationStrategy strategy, RotationTimerFactory timerFactory, System.Action<Headers.Builder> headers )
				  {
						return new Prepared<Key>( _outerInstance.state.rotate( force, strategy, timerFactory, headers ) );
				  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
				  public override void close()
				  {
						_outerInstance.state.Dispose();
				  }

				  internal override long rotationVersion()
				  {
						return _outerInstance.state.rotationVersion();
				  }

				  internal override ProgressiveState<Key> markAsFailed()
				  {
						return _outerInstance.state;
				  }
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: ProgressiveState<Key> stop() throws java.io.IOException
			  internal override ProgressiveState<Key> Stop()
			  {
					return State.stop();
			  }

			  internal override string StateName()
			  {
					return "prepared";
			  }

			  internal override ActiveState<Key> Start( DataInitializer<EntryUpdater<Key>> stateInitializer )
			  {
					return State;
			  }

			  protected internal override File File()
			  {
					return State.file();
			  }
		 }

		 private abstract class Rotation<Key, State> : RotationState<Key> where State : ProgressiveState<Key>
		 {
			  internal readonly State State;

			  internal Rotation( State state )
			  {
					this.State = state;
			  }

			  protected internal override File File()
			  {
					return State.file();
			  }

			  internal override Optional<EntryUpdater<Key>> OptionalUpdater( long version, Lock @lock )
			  {
					throw new System.InvalidOperationException( "Cannot write in state: " + outerInstance.stateName() );
			  }

			  protected internal override EntryUpdater<Key> UnsafeUpdater( Lock @lock )
			  {
					throw new System.InvalidOperationException( "Cannot write in state: " + outerInstance.stateName() );
			  }

			  protected internal override bool HasChanges()
			  {
					return State.hasChanges();
			  }

			  protected internal override KeyFormat<Key> KeyFormat()
			  {
					return State.keyFormat();
			  }

			  protected internal override Headers Headers()
			  {
					return State.headers();
			  }

			  protected internal override long Version()
			  {
					return State.version();
			  }

			  protected internal override bool Lookup( Key key, ValueSink sink )
			  {
					throw new System.InvalidOperationException( "Cannot read in state: " + outerInstance.stateName() );
			  }

			  protected internal override DataProvider DataProvider()
			  {
					throw new System.InvalidOperationException( "Cannot read in state: " + outerInstance.stateName() );
			  }

			  protected internal override int StoredEntryCount()
			  {
					throw new System.InvalidOperationException( "Cannot read in state: " + outerInstance.stateName() );
			  }
		 }
	}

}