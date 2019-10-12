using System;
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
namespace Neo4Net.Kernel.impl.store.kvstore
{

	using VersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.VersionContextSupplier;

	public abstract class ActiveState<Key> : ProgressiveState<Key>
	{
		 public interface Factory
		 {
			  ActiveState<Key> open<Key>( ReadableState<Key> store, File file, VersionContextSupplier versionContextSupplier );
		 }

		 protected internal readonly ReadableState<Key> Store;
		 protected internal readonly VersionContextSupplier VersionContextSupplier;

		 public ActiveState( ReadableState<Key> store, VersionContextSupplier versionContextSupplier )
		 {
			  this.Store = store;
			  this.VersionContextSupplier = versionContextSupplier;
		 }

		 protected internal override KeyFormat<Key> KeyFormat()
		 {
			  return Store.keyFormat();
		 }

		 internal override string StateName()
		 {
			  return "active";
		 }

		 protected internal override abstract long StoredVersion();

		 internal override RotationState.Rotation<Key> PrepareRotation( long version )
		 {
			  version = Math.Max( version, version() );
			  return new RotationState.Rotation<Key>( this, Prototype( version ), version );
		 }

		 internal override Optional<EntryUpdater<Key>> OptionalUpdater( long version, Lock @lock )
		 {
			  return Updater( version, @lock );
		 }

		 protected internal abstract EntryUpdater<Key> Updater( long version, Lock @lock );

		 internal override EntryUpdater<Key> Resetter( Lock @lock, ThreadStart closeAction )
		 {
			  if ( HasChanges() )
			  {
					throw new System.InvalidOperationException( "Cannot reset while there are changes." );
			  }
			  return ResettingUpdater( @lock, closeAction );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: final ProgressiveState<Key> stop() throws java.io.IOException
		 internal override ProgressiveState<Key> Stop()
		 {
			  Close();
			  return new DeadState.Stopped<Key>( KeyFormat(), Factory(), VersionContextSupplier );
		 }

		 protected internal override Headers Headers()
		 {
			  return Store.headers();
		 }

		 protected internal override int StoredEntryCount()
		 {
			  return Store.storedEntryCount();
		 }

		 protected internal abstract EntryUpdater<Key> ResettingUpdater( Lock @lock, ThreadStart closeAction );

		 protected internal override abstract bool HasChanges();

		 protected internal abstract PrototypeState<Key> Prototype( long version );

		 protected internal abstract Factory Factory();

		 protected internal abstract long Applied();
	}

}