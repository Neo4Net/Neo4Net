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

	public abstract class PrototypeState<Key> : WritableState<Key>
	{
		 protected internal readonly ActiveState<Key> Store;

		 public PrototypeState( ActiveState<Key> store )
		 {
			  this.Store = store;
		 }

		 protected internal abstract ActiveState<Key> Create( ReadableState<Key> sub, File file, VersionContextSupplier versionContextSupplier );

		 protected internal override Headers Headers()
		 {
			  return Store.headers();
		 }

		 protected internal override int StoredEntryCount()
		 {
			  return Store.storedEntryCount();
		 }

		 protected internal override KeyFormat<Key> KeyFormat()
		 {
			  return Store.keyFormat();
		 }

		 internal override EntryUpdater<Key> Resetter( Lock @lock, ThreadStart runnable )
		 {
			  throw new System.NotSupportedException( "should never be invoked" );
		 }

		 public override void Close()
		 {
			  throw new System.NotSupportedException( "should never be invoked" );
		 }

		 internal override Optional<EntryUpdater<Key>> OptionalUpdater( long version, Lock @lock )
		 {
			  return Updater( version, @lock );
		 }

		 protected internal abstract EntryUpdater<Key> Updater( long version, Lock @lock );
	}

}