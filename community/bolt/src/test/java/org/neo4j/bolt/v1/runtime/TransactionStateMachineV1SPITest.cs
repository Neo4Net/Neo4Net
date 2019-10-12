using System;

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
namespace Org.Neo4j.Bolt.v1.runtime
{
	using EmbeddedChannel = io.netty.channel.embedded.EmbeddedChannel;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using DependencyResolver = Org.Neo4j.Graphdb.DependencyResolver;
	using TransactionFailureException = Org.Neo4j.@internal.Kernel.Api.exceptions.TransactionFailureException;
	using GraphDatabaseQueryService = Org.Neo4j.Kernel.GraphDatabaseQueryService;
	using DatabaseAvailabilityGuard = Org.Neo4j.Kernel.availability.DatabaseAvailabilityGuard;
	using ThreadToStatementContextBridge = Org.Neo4j.Kernel.impl.core.ThreadToStatementContextBridge;
	using QueryExecutionEngine = Org.Neo4j.Kernel.impl.query.QueryExecutionEngine;
	using TransactionIdStore = Org.Neo4j.Kernel.impl.transaction.log.TransactionIdStore;
	using GraphDatabaseAPI = Org.Neo4j.Kernel.@internal.GraphDatabaseAPI;
	using NullLog = Org.Neo4j.Logging.NullLog;
	using Org.Neo4j.Test.rule.concurrent;
	using FakeClock = Org.Neo4j.Time.FakeClock;

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
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.DEFAULT_DATABASE_NAME;

	public class TransactionStateMachineV1SPITest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.concurrent.OtherThreadRule<Void> otherThread = new org.neo4j.test.rule.concurrent.OtherThreadRule<>();
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