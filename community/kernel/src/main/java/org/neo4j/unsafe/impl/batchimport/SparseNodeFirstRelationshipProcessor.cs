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
	using NodeRecord = Org.Neo4j.Kernel.impl.store.record.NodeRecord;
	using NodeRelationshipCache = Org.Neo4j.@unsafe.Impl.Batchimport.cache.NodeRelationshipCache;

	/// <summary>
	/// Sets the <seealso cref="NodeRecord.setNextRel(long) relationship field"/> on sparse nodes.
	/// This is done after all sparse node relationship links have been done and the <seealso cref="NodeRelationshipCache node cache"/>
	/// points to the first relationship for sparse each node.
	/// </summary>
	public class SparseNodeFirstRelationshipProcessor : RecordProcessor<NodeRecord>
	{
		 private readonly NodeRelationshipCache _cache;

		 public SparseNodeFirstRelationshipProcessor( NodeRelationshipCache cache )
		 {
			  this._cache = cache;
		 }

		 public override bool Process( NodeRecord node )
		 {
			  long nodeId = node.Id;
			  long firstRel = _cache.getFirstRel( nodeId, NodeRelationshipCache.NO_GROUP_VISITOR );
			  if ( firstRel != -1 )
			  {
					node.NextRel = firstRel;
			  }
			  return true;
		 }

		 public override void Done()
		 { // Nothing to do here
		 }

		 public override void Close()
		 { // Nothing to do here
		 }
	}

}