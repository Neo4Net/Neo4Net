using System;
using System.Collections.Generic;

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
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Test = org.junit.Test;


	using RequestMessage = Neo4Net.Bolt.messaging.RequestMessage;
	using BoltConnection = Neo4Net.Bolt.runtime.BoltConnection;
	using BoltConnectionFactory = Neo4Net.Bolt.runtime.BoltConnectionFactory;
	using BoltSchedulerProvider = Neo4Net.Bolt.runtime.BoltSchedulerProvider;
	using BoltStateMachine = Neo4Net.Bolt.runtime.BoltStateMachine;
	using BoltStateMachineSPI = Neo4Net.Bolt.runtime.BoltStateMachineSPI;
	using CachedThreadPoolExecutorFactory = Neo4Net.Bolt.runtime.CachedThreadPoolExecutorFactory;
	using DefaultBoltConnectionFactory = Neo4Net.Bolt.runtime.DefaultBoltConnectionFactory;
	using ExecutorBoltSchedulerProvider = Neo4Net.Bolt.runtime.ExecutorBoltSchedulerProvider;
	using Neo4NetError = Neo4Net.Bolt.runtime.Neo4NetError;
	using TransactionStateMachineSPI = Neo4Net.Bolt.runtime.TransactionStateMachineSPI;
	using AuthenticationResult = Neo4Net.Bolt.security.auth.AuthenticationResult;
	using BoltResponseRecorder = Neo4Net.Bolt.testing.BoltResponseRecorder;
	using BoltTestUtil = Neo4Net.Bolt.testing.BoltTestUtil;
	using RecordedBoltResponse = Neo4Net.Bolt.testing.RecordedBoltResponse;
	using TransportThrottleGroup = Neo4Net.Bolt.transport.TransportThrottleGroup;
	using DiscardAllMessage = Neo4Net.Bolt.v1.messaging.request.DiscardAllMessage;
	using InitMessage = Neo4Net.Bolt.v1.messaging.request.InitMessage;
	using PullAllMessage = Neo4Net.Bolt.v1.messaging.request.PullAllMessage;
	using ResetMessage = Neo4Net.Bolt.v1.messaging.request.ResetMessage;
	using RunMessage = Neo4Net.Bolt.v1.messaging.request.RunMessage;
	using Iterables = Neo4Net.Collections.Helpers.Iterables;
	using BoltConnector = Neo4Net.Kernel.configuration.BoltConnector;
	using Config = Neo4Net.Kernel.configuration.Config;
	using LifeSupport = Neo4Net.Kernel.Lifecycle.LifeSupport;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using NullLog = Neo4Net.Logging.NullLog;
	using NullLogService = Neo4Net.Logging.Internal.NullLogService;
	using IJobScheduler = Neo4Net.Scheduler.JobScheduler;

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
//	import static org.Neo4Net.bolt.testing.NullResponseHandler.nullResponseHandler;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.bolt.v1.messaging.BoltResponseMessage.SUCCESS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.scheduler.JobSchedulerFactory.createScheduler;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.@virtual.VirtualValues.EMPTY_MAP;

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
		 private IJobScheduler _scheduler;
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
//ORIGINAL LINE: private void assertSchedulerWorks(org.Neo4Net.bolt.runtime.BoltConnection connection) throws InterruptedException
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
//ORIGINAL LINE: throw new AssertionError(String.format("Expected session to return to good state after RESET, but " + "assertion failed: %s.%n" + "Seed: %s%n" + "Messages sent:%n" + "%s", e.getMessage(), seed, org.Neo4Net.helpers.collection.Iterables.toString(sent, "\n")), e);
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

			  public override void ReportError( Neo4NetError err )
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