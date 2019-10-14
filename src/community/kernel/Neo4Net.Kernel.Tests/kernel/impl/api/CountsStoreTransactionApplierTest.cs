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
namespace Neo4Net.Kernel.Impl.Api
{
	using Test = org.junit.Test;

	using StatementConstants = Neo4Net.Kernel.api.StatementConstants;
	using CountsTracker = Neo4Net.Kernel.impl.store.counts.CountsTracker;
	using Command = Neo4Net.Kernel.impl.transaction.command.Command;
	using TransactionApplicationMode = Neo4Net.Storageengine.Api.TransactionApplicationMode;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class CountsStoreTransactionApplierTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotifyCacheAccessOnHowManyUpdatesOnCountsWeHadSoFar() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotifyCacheAccessOnHowManyUpdatesOnCountsWeHadSoFar()
		 {
			  // GIVEN
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.store.counts.CountsTracker tracker = mock(org.neo4j.kernel.impl.store.counts.CountsTracker.class);
			  CountsTracker tracker = mock( typeof( CountsTracker ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final CountsAccessor_Updater updater = mock(CountsAccessor_Updater.class);
			  CountsAccessor_Updater updater = mock( typeof( CountsAccessor_Updater ) );
			  when( tracker.Apply( anyLong() ) ).thenReturn(updater);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final CountsStoreBatchTransactionApplier applier = new CountsStoreBatchTransactionApplier(tracker, org.neo4j.storageengine.api.TransactionApplicationMode.INTERNAL);
			  CountsStoreBatchTransactionApplier applier = new CountsStoreBatchTransactionApplier( tracker, TransactionApplicationMode.INTERNAL );

			  // WHEN
			  using ( TransactionApplier txApplier = applier.StartTx( new TransactionToApply( null, 2L ) ) )
			  {
					txApplier.VisitNodeCountsCommand( new Command.NodeCountsCommand( StatementConstants.ANY_LABEL, 1 ) );
			  }

			  // THEN
			  verify( updater, times( 1 ) ).incrementNodeCount( StatementConstants.ANY_LABEL, 1 );
		 }
	}

}