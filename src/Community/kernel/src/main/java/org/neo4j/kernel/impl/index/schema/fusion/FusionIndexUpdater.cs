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
namespace Neo4Net.Kernel.Impl.Index.Schema.fusion
{

	using IndexEntryConflictException = Neo4Net.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using Neo4Net.Kernel.Api.Index;
	using IndexUpdater = Neo4Net.Kernel.Api.Index.IndexUpdater;

	internal class FusionIndexUpdater : FusionIndexBase<IndexUpdater>, IndexUpdater
	{
		 internal FusionIndexUpdater( SlotSelector slotSelector, LazyInstanceSelector<IndexUpdater> instanceSelector ) : base( slotSelector, instanceSelector )
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void process(org.neo4j.kernel.api.index.IndexEntryUpdate<?> update) throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
		 public override void Process<T1>( IndexEntryUpdate<T1> update )
		 {
			  switch ( update.UpdateMode() )
			  {
			  case ADDED:
					InstanceSelector.select( SlotSelector.selectSlot( update.Values(), GroupOf ) ).process(update);
					break;
			  case CHANGED:
					// Hmm, here's a little conundrum. What if we change from a value that goes into native
					// to a value that goes into fallback, or vice versa? We also don't want to blindly pass
					// all CHANGED updates to both updaters since not all values will work in them.
					IndexUpdater from = InstanceSelector.select( SlotSelector.selectSlot( update.BeforeValues(), GroupOf ) );
					IndexUpdater to = InstanceSelector.select( SlotSelector.selectSlot( update.Values(), GroupOf ) );
					// There are two cases:
					// - both before/after go into the same updater --> pass update into that updater
					if ( from == to )
					{
						 from.Process( update );
					}
					// - before go into one and after into the other --> REMOVED from one and ADDED into the other
					else
					{
						 from.Process( IndexEntryUpdate.remove( update.EntityId, update.IndexKey(), update.BeforeValues() ) );
						 to.Process( IndexEntryUpdate.add( update.EntityId, update.IndexKey(), update.Values() ) );
					}
					break;
			  case REMOVED:
					InstanceSelector.select( SlotSelector.selectSlot( update.Values(), GroupOf ) ).process(update);
					break;
			  default:
					throw new System.ArgumentException( "Unknown update mode" );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
		 public override void Close()
		 {
			  AtomicReference<IndexEntryConflictException> chainedExceptions = new AtomicReference<IndexEntryConflictException>();

			  InstanceSelector.close(indexUpdater =>
			  {
				try
				{
					 indexUpdater.close();
				}
				catch ( IndexEntryConflictException e )
				{
					 if ( !chainedExceptions.compareAndSet( null, e ) )
					 {
						  chainedExceptions.get().addSuppressed(e);
					 }
				}
			  });

			  if ( chainedExceptions.get() != null )
			  {
					throw chainedExceptions.get();
			  }
		 }
	}

}