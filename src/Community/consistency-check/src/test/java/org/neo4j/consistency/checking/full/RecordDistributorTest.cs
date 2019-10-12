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
namespace Neo4Net.Consistency.checking.full
{
	using Test = org.junit.jupiter.api.Test;

	using Neo4Net.Consistency.checking.full;
	using Neo4Net.Consistency.checking.full.RecordDistributor;
	using RelationshipRecord = Neo4Net.Kernel.impl.store.record.RelationshipRecord;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;

	internal class RecordDistributorTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldDistributeRelationshipRecordsByNodeId() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldDistributeRelationshipRecordsByNodeId()
		 {
			  // GIVEN
			  QueueDistribution_QueueDistributor<RelationshipRecord> distributor = new QueueDistribution_RelationshipNodesQueueDistributor( 5, 100 );
			  RecordConsumer<RelationshipRecord> consumer = mock( typeof( RecordConsumer ) );

			  // WHEN/THEN
			  RelationshipRecord relationship = relationship( 0, 0, 1 );
			  distributor.Distribute( relationship, consumer );
			  verify( consumer, times( 1 ) ).accept( relationship, 0 );

			  relationship = relationship( 1, 0, 7 );
			  distributor.Distribute( relationship, consumer );
			  verify( consumer, times( 1 ) ).accept( relationship, 0 );
			  verify( consumer, times( 1 ) ).accept( relationship, 1 );

			  relationship = relationship( 3, 26, 11 );
			  distributor.Distribute( relationship, consumer );
			  verify( consumer, times( 1 ) ).accept( relationship, 5 );
			  verify( consumer, times( 1 ) ).accept( relationship, 2 );
		 }

		 private static RelationshipRecord Relationship( long id, long startNodeId, long endNodeId )
		 {
			  RelationshipRecord record = new RelationshipRecord( id );
			  record.InUse = true;
			  record.FirstNode = startNodeId;
			  record.SecondNode = endNodeId;
			  return record;
		 }
	}

}