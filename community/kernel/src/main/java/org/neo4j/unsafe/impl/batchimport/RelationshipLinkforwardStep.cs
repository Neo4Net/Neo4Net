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
namespace Org.Neo4j.@unsafe.Impl.Batchimport
{

	using Direction = Org.Neo4j.Graphdb.Direction;
	using RelationshipRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipRecord;
	using NodeRelationshipCache = Org.Neo4j.@unsafe.Impl.Batchimport.cache.NodeRelationshipCache;
	using StageControl = Org.Neo4j.@unsafe.Impl.Batchimport.staging.StageControl;
	using StatsProvider = Org.Neo4j.@unsafe.Impl.Batchimport.stats.StatsProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.Direction.BOTH;

	public class RelationshipLinkforwardStep : RelationshipLinkStep
	{
		 public RelationshipLinkforwardStep( StageControl control, Configuration config, NodeRelationshipCache cache, System.Predicate<RelationshipRecord> filter, int nodeTypes, params StatsProvider[] additionalStatsProvider ) : base( control, config, cache, filter, nodeTypes, true, additionalStatsProvider )
		 {
		 }

		 protected internal override void LinkStart( RelationshipRecord record )
		 {
			  long firstNextRel = Cache.getAndPutRelationship( record.FirstNode, record.Type, Direction.OUTGOING, record.Id, true );
			  record.FirstNextRel = firstNextRel;
		 }

		 protected internal override void LinkEnd( RelationshipRecord record )
		 {
			  long secondNextRel = Cache.getAndPutRelationship( record.SecondNode, record.Type, Direction.INCOMING, record.Id, true );
			  record.SecondNextRel = secondNextRel;
		 }

		 protected internal override void LinkLoop( RelationshipRecord record )
		 {
			  long firstNextRel = Cache.getAndPutRelationship( record.FirstNode, record.Type, BOTH, record.Id, true );
			  record.FirstNextRel = firstNextRel;
			  record.SecondNextRel = firstNextRel;
		 }
	}

}