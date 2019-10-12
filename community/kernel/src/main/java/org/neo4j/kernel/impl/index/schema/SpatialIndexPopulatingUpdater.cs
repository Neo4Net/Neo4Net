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
	using CoordinateReferenceSystem = Org.Neo4j.Values.Storable.CoordinateReferenceSystem;
	using PointValue = Org.Neo4j.Values.Storable.PointValue;

	public class SpatialIndexPopulatingUpdater : SpatialIndexCache<IndexUpdater>, IndexUpdater
	{
		 internal SpatialIndexPopulatingUpdater( SpatialIndexPopulator populator, NodePropertyAccessor nodePropertyAccessor ) : base( new PartFactory( populator, nodePropertyAccessor ) )
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void process(org.neo4j.kernel.api.index.IndexEntryUpdate<?> update) throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
		 public override void Process<T1>( IndexEntryUpdate<T1> update )
		 {
			  PointValue value = ( PointValue ) update.Values()[0];
			  switch ( update.UpdateMode() )
			  {
			  case ADDED:
					Select( value.CoordinateReferenceSystem ).process( update );
					break;

			  case CHANGED:
					// These are both spatial, but could belong in different parts
					PointValue fromValue = ( PointValue ) update.BeforeValues()[0];
					IndexUpdater from = Select( fromValue.CoordinateReferenceSystem );
					IndexUpdater to = Select( value.CoordinateReferenceSystem );
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
					Select( value.CoordinateReferenceSystem ).process( update );
					break;

			  default:
					throw new System.ArgumentException( "Unknown update mode" );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
		 public override void Close()
		 {
			  foreach ( IndexUpdater updater in this )
			  {
					updater.Close();
			  }
		 }

		 internal class PartFactory : Factory<IndexUpdater>
		 {
			  internal readonly SpatialIndexPopulator Populator;
			  internal NodePropertyAccessor NodePropertyAccessor;

			  internal PartFactory( SpatialIndexPopulator populator, NodePropertyAccessor nodePropertyAccessor )
			  {
					this.Populator = populator;
					this.NodePropertyAccessor = nodePropertyAccessor;
			  }

			  public override IndexUpdater NewSpatial( CoordinateReferenceSystem crs )
			  {
					return Populator.select( crs ).newPopulatingUpdater( NodePropertyAccessor );
			  }
		 }
	}

}