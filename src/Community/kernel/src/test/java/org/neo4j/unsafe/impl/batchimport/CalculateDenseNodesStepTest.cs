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
namespace Neo4Net.@unsafe.Impl.Batchimport
{
	using Test = org.junit.Test;

	using Record = Neo4Net.Kernel.impl.store.record.Record;
	using RelationshipRecord = Neo4Net.Kernel.impl.store.record.RelationshipRecord;
	using NodeRelationshipCache = Neo4Net.@unsafe.Impl.Batchimport.cache.NodeRelationshipCache;
	using StageControl = Neo4Net.@unsafe.Impl.Batchimport.staging.StageControl;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.record.Record.NULL_REFERENCE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.Configuration.DEFAULT;

	public class CalculateDenseNodesStepTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotProcessLoopsTwice() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotProcessLoopsTwice()
		 {
			  // GIVEN
			  NodeRelationshipCache cache = mock( typeof( NodeRelationshipCache ) );
			  using ( CalculateDenseNodesStep step = new CalculateDenseNodesStep( mock( typeof( StageControl ) ), DEFAULT, cache ) )
			  {
					step.Processors( 4 );
					step.Start( 0 );

					// WHEN
					long id = 0;
					RelationshipRecord[] batch = batch( Relationship( id++, 1, 5 ), Relationship( id++, 3, 10 ), Relationship( id++, 2, 2 ), Relationship( id++, 4, 1 ) );
					step.Receive( 0, batch );
					step.endOfUpstream();
					step.awaitCompleted();

					// THEN
					verify( cache, times( 2 ) ).incrementCount( eq( 1L ) );
					verify( cache, times( 1 ) ).incrementCount( eq( 2L ) );
					verify( cache, times( 1 ) ).incrementCount( eq( 3L ) );
					verify( cache, times( 1 ) ).incrementCount( eq( 4L ) );
					verify( cache, times( 1 ) ).incrementCount( eq( 5L ) );
					verify( cache, times( 1 ) ).incrementCount( eq( 10L ) );
			  }
		 }

		 private static RelationshipRecord[] Batch( params RelationshipRecord[] relationships )
		 {
			  return relationships;
		 }

		 private static RelationshipRecord Relationship( long id, long startNodeId, long endNodeId )
		 {
			  return ( new RelationshipRecord( id ) ).initialize( true, Record.NO_NEXT_PROPERTY.longValue(), startNodeId, endNodeId, 0, NULL_REFERENCE.longValue(), NULL_REFERENCE.longValue(), NULL_REFERENCE.longValue(), NULL_REFERENCE.longValue(), false, false );
		 }
	}

}