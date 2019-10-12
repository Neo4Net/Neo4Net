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
namespace Neo4Net.Kernel.Impl.Api
{
	using Test = org.junit.Test;

	using Applier = Neo4Net.Kernel.Impl.Api.TransactionQueue.Applier;
	using TransactionRepresentation = Neo4Net.Kernel.impl.transaction.TransactionRepresentation;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.reset;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyNoMoreInteractions;

	public class TransactionQueueTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldEmptyIfTooMany() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldEmptyIfTooMany()
		 {
			  // GIVEN
			  Applier applier = mock( typeof( Applier ) );
			  int batchSize = 10;
			  TransactionQueue queue = new TransactionQueue( batchSize, applier );

			  // WHEN
			  for ( int i = 0; i < 9; i++ )
			  {
					queue.Queue( mock( typeof( TransactionToApply ) ) );
					verifyNoMoreInteractions( applier );
			  }
			  queue.Queue( mock( typeof( TransactionToApply ) ) );
			  verify( applier, times( 1 ) ).apply( any(), any() );
			  reset( applier );

			  // THEN
			  queue.Queue( mock( typeof( TransactionToApply ) ) );

			  // and WHEN emptying in the end
			  for ( int i = 0; i < 2; i++ )
			  {
					queue.Queue( mock( typeof( TransactionToApply ) ) );
					verifyNoMoreInteractions( applier );
			  }
			  queue.Empty();
			  verify( applier, times( 1 ) ).apply( any(), any() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLinkTogetherTransactions() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLinkTogetherTransactions()
		 {
			  // GIVEN
			  Applier applier = mock( typeof( Applier ) );
			  int batchSize = 10;
			  TransactionQueue queue = new TransactionQueue( batchSize, applier );

			  // WHEN
			  TransactionToApply[] txs = new TransactionToApply[batchSize];
			  for ( int i = 0; i < batchSize; i++ )
			  {
					queue.Queue( txs[i] = new TransactionToApply( mock( typeof( TransactionRepresentation ) ) ) );
			  }

			  // THEN
			  verify( applier, times( 1 ) ).apply( any(), any() );
			  for ( int i = 0; i < txs.Length - 1; i++ )
			  {
					assertEquals( txs[i + 1], txs[i].Next() );
			  }
		 }
	}

}