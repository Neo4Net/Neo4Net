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

	internal abstract class ProgressiveState<Key> : WritableState<Key>
	{
		 // state transitions

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: ProgressiveState<Key> initialize(RotationStrategy rotation) throws java.io.IOException
		 internal virtual ProgressiveState<Key> Initialize( RotationStrategy rotation )
		 {
			  throw new System.InvalidOperationException( "Cannot initialize in state: " + StateName() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: ActiveState<Key> start(DataInitializer<EntryUpdater<Key>> stateInitializer) throws java.io.IOException
		 internal virtual ActiveState<Key> Start( DataInitializer<EntryUpdater<Key>> stateInitializer )
		 {
			  throw new System.InvalidOperationException( "Cannot start in state: " + StateName() );
		 }

		 internal virtual RotationState<Key> PrepareRotation( long version )
		 {
			  throw new System.InvalidOperationException( "Cannot rotate in state: " + StateName() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: ProgressiveState<Key> stop() throws java.io.IOException
		 internal virtual ProgressiveState<Key> Stop()
		 {
			  throw new System.InvalidOperationException( "Cannot stop in state: " + StateName() );
		 }

		 // methods of subtypes

		 internal abstract string StateName();

		 protected internal abstract File File();

		 protected internal virtual long StoredVersion()
		 {
			  return version();
		 }

		 // default implementations

		 internal override EntryUpdater<Key> Resetter( Lock @lock, ThreadStart closeAction )
		 {
			  throw new System.InvalidOperationException( "Cannot reset in state: " + StateName() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  throw new System.InvalidOperationException( "Cannot close() in state: " + StateName() );
		 }

		 public override string ToString()
		 {
			  return this.GetType().Name;
		 }
	}

}