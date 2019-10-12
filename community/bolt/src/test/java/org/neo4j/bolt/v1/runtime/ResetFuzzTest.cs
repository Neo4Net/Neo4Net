using System;
using System.Collections.Generic;

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
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Test = org.junit.Test;


	using RequestMessage = Org.Neo4j.Bolt.messaging.RequestMessage;
	using BoltConnection = Org.Neo4j.Bolt.runtime.BoltConnection;
	using BoltConnectionFactory = Org.Neo4j.Bolt.runtime.BoltConnectionFactory;
	using BoltSchedulerProvider = Org.Neo4j.Bolt.runtime.BoltSchedulerProvider;
	using BoltStateMachine = Org.Neo4j.Bolt.runtime.BoltStateMachine;
	using BoltStateMachineSPI = Org.Neo4j.Bolt.runtime.BoltStateMachineSPI;
	using CachedThreadPoolExecutorFactory = Org.Neo4j.Bolt.runtime.CachedThreadPoolExecutorFactory;
	using DefaultBoltConnectionFactory = Org.Neo4j.Bolt.runtime.DefaultBoltConnectionFactory;
	using ExecutorBoltSchedulerProvider = Org.Neo4j.Bolt.runtime.ExecutorBoltSchedulerProvider;
	using Neo4jError = Org.Neo4j.Bolt.runtime.Neo4jError;
	using TransactionStateMachineSPI = Org.Neo4j.Bolt.runtime.TransactionStateMachineSPI;
	using AuthenticationResult = Org.Neo4j.Bolt.security.auth.AuthenticationResult;
	using BoltResponseRecorder = Org.Neo4j.Bolt.testing.BoltResponseRecorder;
	using BoltTestUtil = Org.Neo4j.Bolt.testing.BoltTestUtil;
	using RecordedBoltResponse = Org.Neo4j.Bolt.testing.RecordedBoltResponse;
	using TransportThrottleGroup = Org.Neo4j.Bolt.transport.TransportThrottleGroup;
	using DiscardAllMessage = Org.Neo4j.Bolt.v1.messaging.request.DiscardAllMessage;
	using InitMessage = Org.Neo4j.Bolt.v1.messaging.request.InitMessage;
	using PullAllMessage = Org.Neo4j.Bolt.v1.messaging.request.PullAllMessage;
	using ResetMessage = Org.Neo4j.Bolt.v1.messaging.request.ResetMessage;
	using RunMessage = Org.Neo4j.Bolt.v1.messaging.request.RunMessage;
	using Iterables = Org.Neo4j.Helpers.Collection.Iterables;
	using BoltConnector = Org.Neo4j.Kernel.configuration.BoltConnector;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using LifeSupport = Org.Neo4j.Kernel.Lifecycle.LifeSupport;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using NullLog = Org.Neo4j.Logging.NullLog;
	using NullLogService = Org.Neo4j.Logging.@internal.NullLogService;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.RETURNS_MOCKS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.testing.NullResponseHandler.nullResponseHandler;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.messaging.BoltResponseMessage.SUCCESS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.scheduler.JobSchedulerFactory.createScheduler;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.@virtual.VirtualValues.EMPTY_MAP;

	public class ResetFuzzTest
	{
		private bool InstanceFieldsInitialized = false;

		public ResetFuzzTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_rand = new Random( _seed );
			_scheduler = _life.add( createScheduler() );
			_boltSchedulerProvider = _life.add( new ExecutorBoltSchedulerProvider( _config, new CachedThreadPoolExecutorFactory( NullLog.Instance ), _scheduler, NullLogService.Instance ) );
			_machine = new BoltStateMachineV1( new FuzzStubSPI( this ), BoltTestUtil.newTestBoltChannel(), _clock );
			_connectionFactory = new DefaultBoltConnectionFactory( _boltSchedulerProvider, TransportThrottleGroup.NO_THROTTLE, _config, NullLogService.Instance, _clock, _monitors );
		}

		 private const string CONNECTOR = "bolt";
		 // Because RESET has a "call ahead" mechanism where it will interrupt
		 // the session before RESET arrives in order to purge any statements
		 // ahead in the message queue, we use this test to convince ourselves
		 // there is no code path where RESET causes a session to not go back
		 // to a good state.

		 private readonly int _seed = new Random().Next();

		 private Random _rand;
		 private readonly LifeSupport _life = new LifeSupport();
		 /// <summary>
		 /// We track the number of un-closed transactions, and fail if we ever leak one </summary>
		 private readonly AtomicLong _liveTransactions = new AtomicLong();
		 private readonly Monitors _monitors = new Monitors();
		 private JobScheduler _scheduler;
		 private readonly Config _config = CreateConfig();
		 private BoltSchedulerProvider _boltSchedulerProvider;
		 private readonly Clock _clock = Clock.systemUTC();
		 private BoltStateMachine _machine;
		 private BoltConnectionFactory _connectionFactory;
		 private BoltChannel _boltChannel;

		 private readonly IList<IList<RequestMessage>> _sequences = new IList<IList<RequestMessage>> { asList( new RunMessage( "test", EMPTY_MAP ), DiscardAllMessage.INSTANCE ), asList( new RunMessage( "test", EMPTY_MAP ), PullAllMessage.INSTANCE ), singletonList( new RunMessage( "test", EMPTY_MAP ) ) };

		 private readonly IList<RequestMessage> _sent = new LinkedList<RequestMessage>();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  _boltChannel = mock( typeof( BoltChannel ), RETURNS_MOCKS );
			  when( _boltChannel.id() ).thenReturn(System.Guid.randomUUID().ToString());
			  when( _boltChannel.connector() ).thenReturn(CONNECTOR);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAlwaysReturnToReadyAfterReset() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAlwaysReturnToReadyAfterReset()
		 {
			  // given
			  _life.start();
			  BoltConnection boltConnection = _connectionFactory.newConnection( _boltChannel, _machine );
			  boltConnection.Enqueue( _machine => _machine.process( new InitMessage( "ResetFuzzTest/0.0", emptyMap() ), nullResponseHandler() ) );

			  // Test random combinations of messages within a small budget of testing time.
			  long deadline = DateTimeHelper.CurrentUnixTimeMillis() + 2 * 1000;

			  // when
			  while ( DateTimeHelper.CurrentUnixTimeMillis() < deadline )
			  {
					DispatchRandomSequenceOfMessages( boltConnection );
					AssertSchedulerWorks( boltConnection );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertSchedulerWorks(org.neo4j.bolt.runtime.BoltConnection connection) throws InterruptedException
		 private void AssertSchedulerWorks( BoltConnection connection )
		 {
			  BoltResponseRecorder recorder = new BoltResponseRecorder();
			  connection.Enqueue( _machine => _machine.process( ResetMessage.INSTANCE, recorder ) );

			  try
			  {
					RecordedBoltResponse response = recorder.NextResponse();
					assertThat( response.Message(), equalTo(SUCCESS) );
					assertThat( ( ( BoltStateMachineV1 ) _machine ).state(), instanceOf(typeof(ReadyState)) );
					assertThat( _liveTransactions.get(), equalTo(0L) );
			  }
			  catch ( AssertionError e )
			  {
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: throw new AssertionError(String.format("Expected session to return to good state after RESET, but " + "assertion failed: %s.%n" + "Seed: %s%n" + "Messages sent:%n" + "%s", e.getMessage(), seed, org.neo4j.helpers.collection.Iterables.toString(sent, "\n")), e);
					throw new AssertionError( string.Format( "Expected session to return to good state after RESET, but " + "assertion failed: %s.%n" + "Seed: %s%n" + "Messages sent:%n" + "%s", e.Message, _seed, Iterables.ToString( _sent, "\n" ) ), e );
			  }
		 }

		 private void DispatchRandomSequenceOfMessages( BoltConnection connection )
		 {
			  IList<RequestMessage> sequence = _sequences[_rand.Next( _sequences.Count )];
			  foreach ( RequestMessage message in sequence )
			  {
					_sent.Add( message );
					connection.Enqueue( stateMachine => stateMachine.process( message, nullResponseHandler() ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void cleanup()
		 public virtual void Cleanup()
		 {
			  _life.shutdown();
		 }

		 private static Config CreateConfig()
		 {
			  IDictionary<string, string> configProps = new Dictionary<string, string>();

			  configProps[( new BoltConnector( CONNECTOR ) ).enabled.name()] = "TRUE";
			  configProps[( new BoltConnector( CONNECTOR ) ).listen_address.name()] = "localhost:0";
			  configProps[( new BoltConnector( CONNECTOR ) ).type.name()] = BoltConnector.ConnectorType.BOLT.name();
			  configProps[( new BoltConnector( CONNECTOR ) ).thread_pool_min_size.name()] = "5";
			  configProps[( new BoltConnector( CONNECTOR ) ).thread_pool_max_size.name()] = "10";

			  return Config.fromSettings( configProps ).build();
		 }

		 /// <summary>
		 /// We can't use mockito to create this, because it stores all invocations,
		 /// so we run out of RAM in like five seconds.
		 /// </summary>
		 private class FuzzStubSPI : BoltStateMachineSPI
		 {
			 private readonly ResetFuzzTest _outerInstance;

			 public FuzzStubSPI( ResetFuzzTest outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }


			  public override TransactionStateMachineSPI TransactionSpi()
			  {
					return null;
			  }

			  public override void ReportError( Neo4jError err )
			  {
					// do nothing
			  }

			  public override AuthenticationResult Authenticate( IDictionary<string, object> authToken )
			  {
					return AuthenticationResult.AUTH_DISABLED;
			  }

			  public override void UdcRegisterClient( string clientName )
			  {
					// do nothing
			  }

			  public override string Version()
			  {
					return "<test-version>";
			  }
		 }
	}

}