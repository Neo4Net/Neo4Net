/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.causalclustering.core.consensus.log.segmented
{
	using Test = org.junit.Test;

	using LogProvider = Neo4Net.Logging.LogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;

	public class EntryBasedLogPruningStrategyTest : PruningStrategyTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void indexToKeepTest()
		 public virtual void IndexToKeepTest()
		 {
			  // given
			  Files = CreateSegmentFiles( 10 );
			  EntryBasedLogPruningStrategy strategy = new EntryBasedLogPruningStrategy( 6, mock( typeof( LogProvider ) ) );

			  // when
			  long indexToKeep = strategy.GetIndexToKeep( Segments );

			  // then
			  assertEquals( 2, indexToKeep );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void pruneStrategyExceedsNumberOfEntriesTest()
		 public virtual void PruneStrategyExceedsNumberOfEntriesTest()
		 {
			  //given
			  Files = CreateSegmentFiles( 10 ).subList( 5, 10 );
			  EntryBasedLogPruningStrategy strategy = new EntryBasedLogPruningStrategy( 7, mock( typeof( LogProvider ) ) );

			  //when
			  long indexToKeep = strategy.GetIndexToKeep( Segments );

			  //then
			  assertEquals( 4, indexToKeep );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void onlyFirstActiveLogFileTest()
		 public virtual void OnlyFirstActiveLogFileTest()
		 {
			  //given
			  Files = CreateSegmentFiles( 1 );
			  EntryBasedLogPruningStrategy strategy = new EntryBasedLogPruningStrategy( 6, mock( typeof( LogProvider ) ) );

			  //when
			  long indexToKeep = strategy.GetIndexToKeep( Segments );

			  //then
			  assertEquals( -1, indexToKeep );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void onlyOneActiveLogFileTest()
		 public virtual void OnlyOneActiveLogFileTest()
		 {
			  //given
			  Files = CreateSegmentFiles( 6 ).subList( 4, 6 );
			  EntryBasedLogPruningStrategy strategy = new EntryBasedLogPruningStrategy( 6, mock( typeof( LogProvider ) ) );

			  //when
			  long indexToKeep = strategy.GetIndexToKeep( Segments );

			  //then
			  assertEquals( 3, indexToKeep );
		 }
	}

}