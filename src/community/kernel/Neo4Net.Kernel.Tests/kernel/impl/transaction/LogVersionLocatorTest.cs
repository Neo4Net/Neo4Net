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
namespace Neo4Net.Kernel.impl.transaction
{
	using Test = org.junit.Test;

	using LogPosition = Neo4Net.Kernel.impl.transaction.log.LogPosition;
	using NoSuchTransactionException = Neo4Net.Kernel.impl.transaction.log.NoSuchTransactionException;
	using PhysicalLogicalTransactionStore = Neo4Net.Kernel.impl.transaction.log.PhysicalLogicalTransactionStore;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	public class LogVersionLocatorTest
	{
		 private readonly long _firstTxIdInLog = 3;
		 private readonly long _lastTxIdInLog = 67;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFindLogPosition() throws Neo4Net.kernel.impl.transaction.log.NoSuchTransactionException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFindLogPosition()
		 {
			  // given
			  const long txId = 42L;

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.kernel.impl.transaction.log.PhysicalLogicalTransactionStore.LogVersionLocator locator = new Neo4Net.kernel.impl.transaction.log.PhysicalLogicalTransactionStore.LogVersionLocator(txId);
			  PhysicalLogicalTransactionStore.LogVersionLocator locator = new PhysicalLogicalTransactionStore.LogVersionLocator( txId );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.kernel.impl.transaction.log.LogPosition position = new Neo4Net.kernel.impl.transaction.log.LogPosition(1, 128);
			  LogPosition position = new LogPosition( 1, 128 );

			  // when
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final boolean result = locator.visit(position, firstTxIdInLog, lastTxIdInLog);
			  bool result = locator.Visit( position, _firstTxIdInLog, _lastTxIdInLog );

			  // then
			  assertFalse( result );
			  assertEquals( position, locator.LogPosition );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotFindLogPosition()
		 public virtual void ShouldNotFindLogPosition()
		 {
			  // given
			  const long txId = 1L;

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.kernel.impl.transaction.log.PhysicalLogicalTransactionStore.LogVersionLocator locator = new Neo4Net.kernel.impl.transaction.log.PhysicalLogicalTransactionStore.LogVersionLocator(txId);
			  PhysicalLogicalTransactionStore.LogVersionLocator locator = new PhysicalLogicalTransactionStore.LogVersionLocator( txId );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.kernel.impl.transaction.log.LogPosition position = new Neo4Net.kernel.impl.transaction.log.LogPosition(1, 128);
			  LogPosition position = new LogPosition( 1, 128 );

			  // when
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final boolean result = locator.visit(position, firstTxIdInLog, lastTxIdInLog);
			  bool result = locator.Visit( position, _firstTxIdInLog, _lastTxIdInLog );

			  // then
			  assertTrue( result );

			  try
			  {
					locator.LogPosition;
					fail( "should have thrown" );
			  }
			  catch ( NoSuchTransactionException e )
			  {
					assertEquals( "Unable to find transaction " + txId + " in any of my logical logs: " + "Couldn't find any log containing " + txId, e.Message );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAlwaysThrowIfVisitIsNotCalled()
		 public virtual void ShouldAlwaysThrowIfVisitIsNotCalled()
		 {
			  // given
			  const long txId = 1L;

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.kernel.impl.transaction.log.PhysicalLogicalTransactionStore.LogVersionLocator locator = new Neo4Net.kernel.impl.transaction.log.PhysicalLogicalTransactionStore.LogVersionLocator(txId);
			  PhysicalLogicalTransactionStore.LogVersionLocator locator = new PhysicalLogicalTransactionStore.LogVersionLocator( txId );

			  // then
			  try
			  {
					locator.LogPosition;
					fail( "should have thrown" );
			  }
			  catch ( NoSuchTransactionException e )
			  {
					assertEquals( "Unable to find transaction " + txId + " in any of my logical logs: " + "Couldn't find any log containing " + txId, e.Message );
			  }
		 }
	}

}