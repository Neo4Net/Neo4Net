﻿/*
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
	using IndexUpdateMode = Org.Neo4j.Kernel.Impl.Api.index.IndexUpdateMode;
	using CoordinateReferenceSystem = Org.Neo4j.Values.Storable.CoordinateReferenceSystem;
	using PointValue = Org.Neo4j.Values.Storable.PointValue;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.fusion.FusionIndexBase.forAll;

	public class SpatialIndexUpdater : SpatialIndexCache<NativeIndexUpdater<JavaToDotNetGenericWildcard, NativeIndexValue>>, IndexUpdater
	{
		 internal SpatialIndexUpdater( SpatialIndexAccessor accessor, IndexUpdateMode mode ) : base( new PartFactory( accessor, mode ) )
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void process(org.neo4j.kernel.api.index.IndexEntryUpdate<?> update) throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
		 public override void Process<T1>( IndexEntryUpdate<T1> update )
		 {
			  IndexUpdater to = Select( ( ( PointValue )update.Values()[0] ).CoordinateReferenceSystem );
			  switch ( update.UpdateMode() )
			  {
			  case ADDED:
			  case REMOVED:
					to.Process( update );
					break;
			  case CHANGED:
					IndexUpdater from = Select( ( ( PointValue ) update.BeforeValues()[0] ).CoordinateReferenceSystem );
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
			  default:
					throw new System.ArgumentException( "Unknown update mode" );
			  }
		 }

		 public override void Close()
		 {
			  forAll( NativeIndexUpdater.close, this );
		 }

		 internal class PartFactory : Factory<NativeIndexUpdater<JavaToDotNetGenericWildcard, NativeIndexValue>>
		 {

			  internal readonly SpatialIndexAccessor Accessor;
			  internal readonly IndexUpdateMode Mode;

			  internal PartFactory( SpatialIndexAccessor accessor, IndexUpdateMode mode )
			  {
					this.Accessor = accessor;
					this.Mode = mode;
			  }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public NativeIndexUpdater<?,NativeIndexValue> newSpatial(org.neo4j.values.storable.CoordinateReferenceSystem crs)
			  public override NativeIndexUpdater<object, NativeIndexValue> NewSpatial( CoordinateReferenceSystem crs )
			  {
					return Accessor.select( crs ).newUpdater( Mode );
			  }
		 }
	}

}