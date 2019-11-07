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
namespace Neo4Net.Server.rest.transactional
{
	using KernelTransaction = Neo4Net.Kernel.Api.KernelTransaction;
	using AvailabilityGuard = Neo4Net.Kernel.availability.AvailabilityGuard;
	using KernelStatement = Neo4Net.Kernel.Impl.Api.KernelStatement;
	using ThreadToStatementContextBridge = Neo4Net.Kernel.impl.core.ThreadToStatementContextBridge;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	internal class TxStateCheckerTestSupport
	{
		 internal static readonly TransitionalPeriodTransactionMessContainer Tptpmc = mock( typeof( TransitionalPeriodTransactionMessContainer ) );
		 private static FakeBridge _fakeBridge = new FakeBridge();

		 static TxStateCheckerTestSupport()
		 {
			  when( Tptpmc.Bridge ).thenReturn( _fakeBridge );
		 }

		 internal class FakeBridge : ThreadToStatementContextBridge
		 {
			  internal readonly KernelTransaction Tx = mock( typeof( KernelTransaction ) );
			  internal readonly KernelStatement Statement = mock( typeof( KernelStatement ) );

			  internal FakeBridge() : base(mock(typeof(AvailabilityGuard)))
			  {
					when( Tx.acquireStatement() ).thenReturn(Statement);
					when( Statement.hasTxStateWithChanges() ).thenReturn(false);
			  }

			  public override KernelTransaction GetKernelTransactionBoundToThisThread( bool strict )
			  {
					return Tx;
			  }
		 }
	}

}