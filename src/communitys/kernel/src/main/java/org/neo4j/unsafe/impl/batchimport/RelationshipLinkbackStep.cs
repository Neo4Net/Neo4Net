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
namespace Neo4Net.@unsafe.Impl.Batchimport
{

	using Direction = Neo4Net.Graphdb.Direction;
	using RelationshipRecord = Neo4Net.Kernel.impl.store.record.RelationshipRecord;
	using NodeRelationshipCache = Neo4Net.@unsafe.Impl.Batchimport.cache.NodeRelationshipCache;
	using StageControl = Neo4Net.@unsafe.Impl.Batchimport.staging.StageControl;
	using StatsProvider = Neo4Net.@unsafe.Impl.Batchimport.stats.StatsProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.cache.idmapping.IdMapper_Fields.ID_NOT_FOUND;

	/// <summary>
	/// Links relationship chains together, the "prev" pointers of them. "next" pointers are set when
	/// initially creating the relationship records. Setting prev pointers at that time would incur
	/// random access and so that is done here separately with help from <seealso cref="NodeRelationshipCache"/>.
	/// </summary>
	public class RelationshipLinkbackStep : RelationshipLinkStep
	{
		 public RelationshipLinkbackStep( StageControl control, Configuration config, NodeRelationshipCache cache, System.Predicate<RelationshipRecord> filter, int nodeTypes, params StatsProvider[] additionalStatsProvider ) : base( control, config, cache, filter, nodeTypes, false, additionalStatsProvider )
		 {
		 }

		 protected internal override void LinkStart( RelationshipRecord record )
		 {
			  int typeId = record.Type;
			  long firstPrevRel = Cache.getAndPutRelationship( record.FirstNode, typeId, Direction.OUTGOING, record.Id, false );
			  if ( firstPrevRel == ID_NOT_FOUND )
			  { // First one
					record.FirstInFirstChain = true;
					firstPrevRel = Cache.getCount( record.FirstNode, typeId, Direction.OUTGOING );
			  }
			  record.FirstPrevRel = firstPrevRel;
		 }

		 protected internal override void LinkEnd( RelationshipRecord record )
		 {
			  int typeId = record.Type;
			  long secondPrevRel = Cache.getAndPutRelationship( record.SecondNode, typeId, Direction.INCOMING, record.Id, false );
			  if ( secondPrevRel == ID_NOT_FOUND )
			  { // First one
					record.FirstInSecondChain = true;
					secondPrevRel = Cache.getCount( record.SecondNode, typeId, Direction.INCOMING );
			  }
			  record.SecondPrevRel = secondPrevRel;
		 }

		 protected internal override void LinkLoop( RelationshipRecord record )
		 {
			  int typeId = record.Type;
			  long prevRel = Cache.getAndPutRelationship( record.FirstNode, typeId, Direction.BOTH, record.Id, false );
			  if ( prevRel == ID_NOT_FOUND )
			  { // First one
					record.FirstInFirstChain = true;
					record.FirstInSecondChain = true;
					prevRel = Cache.getCount( record.FirstNode, typeId, Direction.BOTH );
			  }
			  record.FirstPrevRel = prevRel;
			  record.SecondPrevRel = prevRel;
		 }
	}

}