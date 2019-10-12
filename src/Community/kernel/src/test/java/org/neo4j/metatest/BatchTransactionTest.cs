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
namespace Neo4Net.Metatest
{
	using Test = org.junit.jupiter.api.Test;

	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using ProgressListener = Neo4Net.Helpers.progress.ProgressListener;
	using BatchTransaction = Neo4Net.Test.BatchTransaction;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.BatchTransaction.beginBatchTx;

	internal class BatchTransactionTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldUseProgressListener()
		 internal virtual void ShouldUseProgressListener()
		 {
			  // GIVEN
			  Transaction transaction = mock( typeof( Transaction ) );
			  GraphDatabaseService db = mock( typeof( GraphDatabaseService ) );
			  when( Db.beginTx() ).thenReturn(transaction);
			  ProgressListener progress = mock( typeof( ProgressListener ) );
			  BatchTransaction tx = beginBatchTx( db ).withIntermediarySize( 10 ).withProgress( progress );

			  // WHEN
			  tx.Increment();
			  tx.Increment( 9 );

			  // THEN
			  verify( db, times( 2 ) ).beginTx();
			  verify( transaction, times( 1 ) ).close();
			  verify( progress, times( 1 ) ).add( 1 );
			  verify( progress, times( 1 ) ).add( 9 );
		 }
	}

}