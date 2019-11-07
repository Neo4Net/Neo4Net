using System;

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
namespace Neo4Net.Bolt.v1.runtime
{
	using EmbeddedChannel = io.netty.channel.embedded.EmbeddedChannel;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using DependencyResolver = Neo4Net.GraphDb.DependencyResolver;
	using TransactionFailureException = Neo4Net.Kernel.Api.Internal.Exceptions.TransactionFailureException;
	using GraphDatabaseQueryService = Neo4Net.Kernel.GraphDatabaseQueryService;
	using DatabaseAvailabilityGuard = Neo4Net.Kernel.availability.DatabaseAvailabilityGuard;
	using ThreadToStatementContextBridge = Neo4Net.Kernel.impl.core.ThreadToStatementContextBridge;
	using QueryExecutionEngine = Neo4Net.Kernel.impl.query.QueryExecutionEngine;
	using TransactionIdStore = Neo4Net.Kernel.impl.transaction.log.TransactionIdStore;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using NullLog = Neo4Net.Logging.NullLog;
	using Neo4Net.Test.rule.concurrent;
	using FakeClock = Neo4Net.Time.FakeClock;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.spy;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.graphdb.factory.GraphDatabaseSettings.DEFAULT_DATABASE_NAME;

	public class TransactionStateMachineV1SPITest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final Neo4Net.test.rule.concurrent.OtherThreadRule<Void> otherThread = new Neo4Net.test.rule.concurrent.OtherThreadRule<>();
		 public readonly OtherThreadRule<Void> OtherThread = new OtherThreadRule<Void>();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void throwsWhenTxAwaitDurationExpires()
		 public virtual void ThrowsWhenTxAwaitDurationExpires()
		 {
			  long lastClosedTransactionId = 100;
			  System.Func<TransactionIdStore> txIdStore = () => FixedTxIdStore(lastClosedTransactionId);
			  Duration txAwaitDuration = Duration.ofSeconds( 42 );
			  FakeClock clock = new FakeClock();

			  DatabaseAvailabilityGuard databaseAvailabilityGuard = spy( new DatabaseAvailabilityGuard( DEFAULT_DATABASE_NAME, clock, NullLog.Instance ) );
			  when( databaseAvailabilityGuard.Available ).then(invocation =>
			  {
				// move clock forward on the first availability check
				// this check is executed on every tx id polling iteration
				bool available = ( bool ) invocation.callRealMethod();
				clock.Forward( txAwaitDuration.Seconds + 1, SECONDS );
				return available;
			  });

			  TransactionStateMachineV1SPI txSpi = CreateTxSpi( txIdStore, txAwaitDuration, databaseAvailabilityGuard, clock );

			  Future<Void> result = OtherThread.execute(state =>
			  {
				txSpi.AwaitUpToDate( lastClosedTransactionId + 42 );
				return null;
			  });

			  try
			  {
					result.get( 20, SECONDS );
			  }
			  catch ( Exception e )
			  {
					assertThat( e, instanceOf( typeof( ExecutionException ) ) );
					assertThat( e.InnerException, instanceOf( typeof( TransactionFailureException ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void doesNotWaitWhenTxIdUpToDate() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void DoesNotWaitWhenTxIdUpToDate()
		 {
			  long lastClosedTransactionId = 100;
			  System.Func<TransactionIdStore> txIdStore = () => FixedTxIdStore(lastClosedTransactionId);

			  TransactionStateMachineV1SPI txSpi = CreateTxSpi( txIdStore, Duration.ZERO, Clock.systemUTC() );

			  Future<Void> result = OtherThread.execute(state =>
			  {
				txSpi.AwaitUpToDate( lastClosedTransactionId - 42 );
				return null;
			  });

			  assertNull( result.get( 20, SECONDS ) );
		 }

		 private static TransactionIdStore FixedTxIdStore( long lastClosedTransactionId )
		 {
			  TransactionIdStore txIdStore = mock( typeof( TransactionIdStore ) );
			  when( txIdStore.LastClosedTransactionId ).thenReturn( lastClosedTransactionId );
			  return txIdStore;
		 }

		 private static TransactionStateMachineV1SPI CreateTxSpi( System.Func<TransactionIdStore> txIdStore, Duration txAwaitDuration, Clock clock )
		 {
			  DatabaseAvailabilityGuard databaseAvailabilityGuard = new DatabaseAvailabilityGuard( DEFAULT_DATABASE_NAME, clock, NullLog.Instance );
			  return CreateTxSpi( txIdStore, txAwaitDuration, databaseAvailabilityGuard, clock );
		 }

		 private static TransactionStateMachineV1SPI CreateTxSpi( System.Func<TransactionIdStore> txIdStore, Duration txAwaitDuration, DatabaseAvailabilityGuard availabilityGuard, Clock clock )
		 {
			  QueryExecutionEngine queryExecutionEngine = mock( typeof( QueryExecutionEngine ) );

			  DependencyResolver dependencyResolver = mock( typeof( DependencyResolver ) );
			  ThreadToStatementContextBridge bridge = new ThreadToStatementContextBridge( availabilityGuard );
			  when( dependencyResolver.ResolveDependency( typeof( ThreadToStatementContextBridge ) ) ).thenReturn( bridge );
			  when( dependencyResolver.ResolveDependency( typeof( QueryExecutionEngine ) ) ).thenReturn( queryExecutionEngine );
			  when( dependencyResolver.ResolveDependency( typeof( DatabaseAvailabilityGuard ) ) ).thenReturn( availabilityGuard );
			  when( dependencyResolver.ProvideDependency( typeof( TransactionIdStore ) ) ).thenReturn( txIdStore );

			  GraphDatabaseAPI db = mock( typeof( GraphDatabaseAPI ) );
			  when( Db.DependencyResolver ).thenReturn( dependencyResolver );

			  GraphDatabaseQueryService queryService = mock( typeof( GraphDatabaseQueryService ) );
			  when( queryService.DependencyResolver ).thenReturn( dependencyResolver );
			  when( dependencyResolver.ResolveDependency( typeof( GraphDatabaseQueryService ) ) ).thenReturn( queryService );

			  BoltChannel boltChannel = new BoltChannel( "bolt-42", "bolt", new EmbeddedChannel() );

			  return new TransactionStateMachineV1SPI( db, boltChannel, txAwaitDuration, clock );
		 }
	}

}