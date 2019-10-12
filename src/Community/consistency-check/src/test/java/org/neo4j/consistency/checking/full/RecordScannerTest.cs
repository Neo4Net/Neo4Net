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

	using CacheAccess = Neo4Net.Consistency.checking.cache.CacheAccess;
	using Statistics = Neo4Net.Consistency.statistics.Statistics;
	using Neo4Net.Helpers.Collection;
	using ProgressListener = Neo4Net.Helpers.progress.ProgressListener;
	using ProgressMonitorFactory = Neo4Net.Helpers.progress.ProgressMonitorFactory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	internal class RecordScannerTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldProcessRecordsSequentiallyAndUpdateProgress() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldProcessRecordsSequentiallyAndUpdateProgress()
		 {
			  // given
			  ProgressMonitorFactory.MultiPartBuilder progressBuilder = mock( typeof( ProgressMonitorFactory.MultiPartBuilder ) );
			  ProgressListener progressListener = mock( typeof( ProgressListener ) );
			  when( progressBuilder.ProgressForPart( anyString(), anyLong() ) ).thenReturn(progressListener);

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") org.neo4j.helpers.collection.BoundedIterable<int> store = mock(org.neo4j.helpers.collection.BoundedIterable.class);
			  BoundedIterable<int> store = mock( typeof( BoundedIterable ) );

			  when( store.GetEnumerator() ).thenReturn(asList(42, 75, 192).GetEnumerator());

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") RecordProcessor<int> recordProcessor = mock(RecordProcessor.class);
			  RecordProcessor<int> recordProcessor = mock( typeof( RecordProcessor ) );

			  RecordScanner<int> scanner = new SequentialRecordScanner<int>( "our test task", Statistics.NONE, 1, store, progressBuilder, recordProcessor );

			  // when
			  scanner.Run();

			  // then
			  VerifyProcessCloseAndDone( recordProcessor, store, progressListener );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldProcessRecordsParallelAndUpdateProgress() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldProcessRecordsParallelAndUpdateProgress()
		 {
			  // given
			  ProgressMonitorFactory.MultiPartBuilder progressBuilder = mock( typeof( ProgressMonitorFactory.MultiPartBuilder ) );
			  ProgressListener progressListener = mock( typeof( ProgressListener ) );
			  when( progressBuilder.ProgressForPart( anyString(), anyLong() ) ).thenReturn(progressListener);

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") org.neo4j.helpers.collection.BoundedIterable<int> store = mock(org.neo4j.helpers.collection.BoundedIterable.class);
			  BoundedIterable<int> store = mock( typeof( BoundedIterable ) );

			  when( store.GetEnumerator() ).thenReturn(asList(42, 75, 192).GetEnumerator());

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") RecordProcessor<int> recordProcessor = mock(RecordProcessor.class);
			  RecordProcessor<int> recordProcessor = mock( typeof( RecordProcessor ) );

			  RecordScanner<int> scanner = new ParallelRecordScanner<int>( "our test task", Statistics.NONE, 1, store, progressBuilder, recordProcessor, CacheAccess.EMPTY, QueueDistribution.ROUND_ROBIN );

			  // when
			  scanner.Run();

			  // then
			  VerifyProcessCloseAndDone( recordProcessor, store, progressListener );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void verifyProcessCloseAndDone(RecordProcessor<int> recordProcessor, org.neo4j.helpers.collection.BoundedIterable<int> store, org.neo4j.helpers.progress.ProgressListener progressListener) throws Exception
		 private static void VerifyProcessCloseAndDone( RecordProcessor<int> recordProcessor, BoundedIterable<int> store, ProgressListener progressListener )
		 {
			  verify( recordProcessor ).process( 42 );
			  verify( recordProcessor ).process( 75 );
			  verify( recordProcessor ).process( 192 );
			  verify( recordProcessor ).close();

			  verify( store ).close();

			  verify( progressListener, times( 3 ) ).add( 1 );
			  verify( progressListener ).done();
		 }
	}

}