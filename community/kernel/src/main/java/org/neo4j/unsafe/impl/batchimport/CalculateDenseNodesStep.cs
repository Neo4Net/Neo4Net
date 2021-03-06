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
namespace Org.Neo4j.@unsafe.Impl.Batchimport
{
	using RelationshipRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipRecord;
	using NodeRelationshipCache = Org.Neo4j.@unsafe.Impl.Batchimport.cache.NodeRelationshipCache;
	using Org.Neo4j.@unsafe.Impl.Batchimport.staging;
	using StageControl = Org.Neo4j.@unsafe.Impl.Batchimport.staging.StageControl;
	using StatsProvider = Org.Neo4j.@unsafe.Impl.Batchimport.stats.StatsProvider;

	/// <summary>
	/// Increments counts for each visited relationship, once for start node and once for end node
	/// (unless for loops). This to be able to determine which nodes are dense before starting to import relationships.
	/// </summary>
	public class CalculateDenseNodesStep : ForkedProcessorStep<RelationshipRecord[]>
	{
		 private readonly NodeRelationshipCache _cache;

		 public CalculateDenseNodesStep( StageControl control, Configuration config, NodeRelationshipCache cache, params StatsProvider[] statsProviders ) : base( control, "CALCULATE", config, statsProviders )
		 {
			  this._cache = cache;
		 }

		 protected internal override void ForkedProcess( int id, int processors, RelationshipRecord[] batch )
		 {
			  foreach ( RelationshipRecord record in batch )
			  {
					if ( record.InUse() )
					{
						 long startNodeId = record.FirstNode;
						 long endNodeId = record.SecondNode;
						 ProcessNodeId( id, processors, startNodeId );
						 if ( startNodeId != endNodeId ) // avoid counting loops twice
						 {
							  // Loops only counts as one
							  ProcessNodeId( id, processors, endNodeId );
						 }
					}
			  }
		 }

		 private void ProcessNodeId( int id, int processors, long nodeId )
		 {
			  if ( nodeId % processors == id )
			  {
					_cache.incrementCount( nodeId );
			  }
		 }
	}

}