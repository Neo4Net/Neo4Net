/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.Kernel.ha
{
	using Before = org.junit.Before;
	using Test = org.junit.Test;

	using InstanceId = Neo4Net.cluster.InstanceId;
	using Suppliers = Neo4Net.Functions.Suppliers;
	using HighAvailabilityMemberChangeEvent = Neo4Net.Kernel.ha.cluster.HighAvailabilityMemberChangeEvent;
	using HighAvailabilityMemberListener = Neo4Net.Kernel.ha.cluster.HighAvailabilityMemberListener;
	using HighAvailabilityMemberStateMachine = Neo4Net.Kernel.ha.cluster.HighAvailabilityMemberStateMachine;
	using TransactionIdStore = Neo4Net.Kernel.impl.transaction.log.TransactionIdStore;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyBoolean;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doAnswer;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class UpdatePullingTransactionObligationFulfillerTest
	{
		 private readonly UpdatePuller _updatePuller = mock( typeof( UpdatePuller ) );
		 private readonly HighAvailabilityMemberStateMachine _machine = mock( typeof( HighAvailabilityMemberStateMachine ) );
		 private readonly InstanceId _serverId = new InstanceId( 42 );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Setup()
		 {
			  doAnswer( invocation => ( ( UpdatePuller_Condition ) invocation.getArgument( 0 ) ).Evaluate( 33, 34 ) ).when( _updatePuller ).pullUpdates( any( typeof( UpdatePuller_Condition ) ), anyBoolean() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotThrowNPEWhenAskedToFulFilledButNotYetHavingARoleAssigned() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotThrowNPEWhenAskedToFulFilledButNotYetHavingARoleAssigned()
		 {
			  // Given
			  UpdatePullingTransactionObligationFulfiller fulfiller = new UpdatePullingTransactionObligationFulfiller( _updatePuller, _machine, _serverId, Suppliers.singleton( mock( typeof( TransactionIdStore ) ) ) );

			  // When
			  fulfiller.Fulfill( 1 );

			  // Then
			  // it doesn't blow up
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUpdateTransactionIdStoreCorrectly() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldUpdateTransactionIdStoreCorrectly()
		 {
			  // Given
			  TransactionIdStore store1 = mock( typeof( TransactionIdStore ) );
			  TransactionIdStore store2 = mock( typeof( TransactionIdStore ) );
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") System.Func<Neo4Net.kernel.impl.transaction.log.TransactionIdStore> supplier = mock(System.Func.class);
			  System.Func<TransactionIdStore> supplier = mock( typeof( System.Func ) );
			  when( supplier() ).thenReturn(store1, store2);

			  doAnswer(invocation =>
			  {
				( ( HighAvailabilityMemberListener ) invocation.getArgument( 0 ) ).slaveIsAvailable(new HighAvailabilityMemberChangeEvent(null, null, _serverId, null)
			  );
				return null;
			  }).when( _machine ).addHighAvailabilityMemberListener( any( typeof( HighAvailabilityMemberListener ) ) );

			  doAnswer(invocation =>
			  {
				( ( HighAvailabilityMemberListener ) invocation.getArgument( 0 ) ).instanceStops(new HighAvailabilityMemberChangeEvent(null, null, _serverId, null)
			  );
				return null;
			  }).when( _machine ).removeHighAvailabilityMemberListener( any( typeof( HighAvailabilityMemberListener ) ) );

			  UpdatePullingTransactionObligationFulfiller fulfiller = new UpdatePullingTransactionObligationFulfiller( _updatePuller, _machine, _serverId, supplier );

			  // When
			  fulfiller.Start();
			  fulfiller.Fulfill( 1 );
			  fulfiller.Stop();
			  fulfiller.Fulfill( 2 );
			  fulfiller.Start();
			  fulfiller.Fulfill( 3 );
			  fulfiller.Stop();
			  fulfiller.Fulfill( 4 );

			  // Then
			  verify( store1, times( 1 ) ).LastClosedTransactionId;
			  verify( store2, times( 1 ) ).LastClosedTransactionId;
		 }
	}

}