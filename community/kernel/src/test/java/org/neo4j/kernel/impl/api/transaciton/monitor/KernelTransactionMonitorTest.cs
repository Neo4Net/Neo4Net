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
namespace Org.Neo4j.Kernel.Impl.Api.transaciton.monitor
{
	using Test = org.junit.jupiter.api.Test;

	using Iterators = Org.Neo4j.Helpers.Collection.Iterators;
	using KernelTransactionHandle = Org.Neo4j.Kernel.api.KernelTransactionHandle;
	using NullLogService = Org.Neo4j.Logging.@internal.NullLogService;
	using FakeClock = Org.Neo4j.Time.FakeClock;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.never;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	internal class KernelTransactionMonitorTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotTimeoutSchemaTransactions()
		 internal virtual void ShouldNotTimeoutSchemaTransactions()
		 {
			  // given
			  KernelTransactions kernelTransactions = mock( typeof( KernelTransactions ) );
			  FakeClock clock = new FakeClock( 100, MINUTES );
			  KernelTransactionMonitor monitor = new KernelTransactionMonitor( kernelTransactions, clock, NullLogService.Instance );
			  // a 2 minutes old schema transaction which has a timeout of 1 minute
			  KernelTransactionHandle oldSchemaTransaction = mock( typeof( KernelTransactionHandle ) );
			  when( oldSchemaTransaction.SchemaTransaction ).thenReturn( true );
			  when( oldSchemaTransaction.StartTime() ).thenReturn(clock.Millis() - MINUTES.toMillis(2));
			  when( oldSchemaTransaction.TimeoutMillis() ).thenReturn(MINUTES.toMillis(1));
			  when( kernelTransactions.ActiveTransactions() ).thenReturn(Iterators.asSet(oldSchemaTransaction));

			  // when
			  monitor.Run();

			  // then
			  verify( oldSchemaTransaction, times( 1 ) ).SchemaTransaction;
			  verify( oldSchemaTransaction, never() ).markForTermination(any());
		 }
	}

}