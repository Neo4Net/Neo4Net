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
namespace Neo4Net.Kernel.impl.transaction.log
{
	using Test = org.junit.Test;

	using EagerlyReversedTransactionCursor = Neo4Net.Kernel.impl.transaction.log.reverse.EagerlyReversedTransactionCursor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertArrayEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.array;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.GivenTransactionCursor.exhaust;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.GivenTransactionCursor.given;

	public class EagerlyReversedTransactionCursorTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReverseTransactionsFromSource() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReverseTransactionsFromSource()
		 {
			  // GIVEN
			  CommittedTransactionRepresentation tx1 = mock( typeof( CommittedTransactionRepresentation ) );
			  CommittedTransactionRepresentation tx2 = mock( typeof( CommittedTransactionRepresentation ) );
			  CommittedTransactionRepresentation tx3 = mock( typeof( CommittedTransactionRepresentation ) );
			  TransactionCursor source = given( tx1, tx2, tx3 );
			  EagerlyReversedTransactionCursor cursor = new EagerlyReversedTransactionCursor( source );

			  // WHEN
			  CommittedTransactionRepresentation[] reversed = exhaust( cursor );

			  // THEN
			  assertArrayEquals( array( tx3, tx2, tx1 ), reversed );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleEmptySource() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleEmptySource()
		 {
			  // GIVEN
			  TransactionCursor source = given();
			  EagerlyReversedTransactionCursor cursor = new EagerlyReversedTransactionCursor( source );

			  // WHEN
			  CommittedTransactionRepresentation[] reversed = exhaust( cursor );

			  // THEN
			  assertEquals( 0, reversed.Length );
		 }
	}

}