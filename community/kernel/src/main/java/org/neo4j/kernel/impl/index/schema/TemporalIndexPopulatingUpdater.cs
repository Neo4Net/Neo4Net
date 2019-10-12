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

	using IndexEntryConflictException = Org.Neo4j.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using Org.Neo4j.Kernel.Api.Index;
	using IndexUpdater = Org.Neo4j.Kernel.Api.Index.IndexUpdater;
	using NodePropertyAccessor = Org.Neo4j.Storageengine.Api.NodePropertyAccessor;
	using ValueGroup = Org.Neo4j.Values.Storable.ValueGroup;

	public class TemporalIndexPopulatingUpdater : TemporalIndexCache<IndexUpdater>, IndexUpdater
	{
		 internal TemporalIndexPopulatingUpdater( TemporalIndexPopulator populator, NodePropertyAccessor nodePropertyAccessor ) : base( new PartFactory( populator, nodePropertyAccessor ) )
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void process(org.neo4j.kernel.api.index.IndexEntryUpdate<?> update) throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
		 public override void Process<T1>( IndexEntryUpdate<T1> update )
		 {
			  switch ( update.UpdateMode() )
			  {
			  case ADDED:
					Select( update.Values()[0].valueGroup() ).process(update);
					break;

			  case CHANGED:
					// These are both temporal, but could belong in different parts
					IndexUpdater from = Select( update.BeforeValues()[0].valueGroup() );
					IndexUpdater to = Select( update.Values()[0].valueGroup() );
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
					Select( update.Values()[0].valueGroup() ).process(update);
					break;

			  default:
					throw new System.ArgumentException( "Unknown update mode" );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
		 public override void Close()
		 {
			  foreach ( IndexUpdater part in this )
			  {
					part.Close();
			  }
		 }

		 internal class PartFactory : TemporalIndexCache.Factory<IndexUpdater>
		 {
			  internal readonly TemporalIndexPopulator Populator;
			  internal NodePropertyAccessor NodePropertyAccessor;

			  internal PartFactory( TemporalIndexPopulator populator, NodePropertyAccessor nodePropertyAccessor )
			  {
					this.Populator = populator;
					this.NodePropertyAccessor = nodePropertyAccessor;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.kernel.api.index.IndexUpdater newDate() throws java.io.IOException
			  public override IndexUpdater NewDate()
			  {
					return Populator.select( ValueGroup.DATE ).newPopulatingUpdater( NodePropertyAccessor );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.kernel.api.index.IndexUpdater newLocalDateTime() throws java.io.IOException
			  public override IndexUpdater NewLocalDateTime()
			  {
					return Populator.select( ValueGroup.LOCAL_DATE_TIME ).newPopulatingUpdater( NodePropertyAccessor );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.kernel.api.index.IndexUpdater newZonedDateTime() throws java.io.IOException
			  public override IndexUpdater NewZonedDateTime()
			  {
					return Populator.select( ValueGroup.ZONED_DATE_TIME ).newPopulatingUpdater( NodePropertyAccessor );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.kernel.api.index.IndexUpdater newLocalTime() throws java.io.IOException
			  public override IndexUpdater NewLocalTime()
			  {
					return Populator.select( ValueGroup.LOCAL_TIME ).newPopulatingUpdater( NodePropertyAccessor );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.kernel.api.index.IndexUpdater newZonedTime() throws java.io.IOException
			  public override IndexUpdater NewZonedTime()
			  {
					return Populator.select( ValueGroup.ZONED_TIME ).newPopulatingUpdater( NodePropertyAccessor );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.kernel.api.index.IndexUpdater newDuration() throws java.io.IOException
			  public override IndexUpdater NewDuration()
			  {
					return Populator.select( ValueGroup.DURATION ).newPopulatingUpdater( NodePropertyAccessor );
			  }
		 }
	}

}