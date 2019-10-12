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

	using RelationshipRecord = Neo4Net.Kernel.impl.store.record.RelationshipRecord;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.allOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThanOrEqualTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.lessThan;

	internal class QueueDistributionTest
	{

		 private const int MAX_NUMBER_OF_THREADS = 1_000_000;
		 private const int NUMBER_OF_DISTRIBUTION_ITERATIONS = 1000;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void roundRobinRecordDistribution() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void RoundRobinRecordDistribution()
		 {
			  TestRecordDistribution( QueueDistribution.ROUND_ROBIN );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void relationshipNodesDistribution() throws InterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void RelationshipNodesDistribution()
		 {
			  TestRecordDistribution( QueueDistribution.RELATIONSHIPS );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void testRecordDistribution(QueueDistribution queueDistribution) throws InterruptedException
		 private static void TestRecordDistribution( QueueDistribution queueDistribution )
		 {
			  ThreadLocalRandom randomGenerator = ThreadLocalRandom.current();
			  int numberOfThreads = randomGenerator.Next( MAX_NUMBER_OF_THREADS );
			  int recordsPerCpu = randomGenerator.Next( int.MaxValue );
			  QueueDistribution_QueueDistributor<RelationshipRecord> distributor = queueDistribution.Distributor( recordsPerCpu, numberOfThreads );
			  for ( int iteration = 0; iteration <= NUMBER_OF_DISTRIBUTION_ITERATIONS; iteration++ )
			  {
					RelationshipRecord relationshipRecord = new RelationshipRecord( 1 );
					relationshipRecord.FirstNode = NextLong( randomGenerator );
					relationshipRecord.SecondNode = NextLong( randomGenerator );
					distributor.Distribute( relationshipRecord, ( record, qIndex ) => assertThat( "Distribution index for record " + record + " should be within a range of available " + "executors, while expected records per cpu is: " + recordsPerCpu, qIndex, allOf( greaterThanOrEqualTo( 0 ), lessThan( numberOfThreads ) ) ) );
			  }
		 }

		 private static long NextLong( ThreadLocalRandom randomGenerator )
		 {
			  return randomGenerator.nextLong();
		 }
	}

}