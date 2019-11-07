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
namespace Neo4Net.Bolt.v1.transport.integration
{
	using StringUtils = org.apache.commons.lang3.StringUtils;
	using Matchers = org.hamcrest.Matchers;
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using BoltConnection = Neo4Net.Bolt.runtime.BoltConnection;
	using Neo4NetPackV1 = Neo4Net.Bolt.v1.messaging.Neo4NetPackV1;
	using InitMessage = Neo4Net.Bolt.v1.messaging.request.InitMessage;
	using PullAllMessage = Neo4Net.Bolt.v1.messaging.request.PullAllMessage;
	using RunMessage = Neo4Net.Bolt.v1.messaging.request.RunMessage;
	using SecureSocketConnection = Neo4Net.Bolt.v1.transport.socket.client.SecureSocketConnection;
	using SocketConnection = Neo4Net.Bolt.v1.transport.socket.client.SocketConnection;
	using TransportConnection = Neo4Net.Bolt.v1.transport.socket.client.TransportConnection;
	using Neo4Net.Functions;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using Exceptions = Neo4Net.Helpers.Exceptions;
	using HostnamePort = Neo4Net.Helpers.HostnamePort;
	using ValueUtils = Neo4Net.Kernel.impl.util.ValueUtils;
	using AssertableLogProvider = Neo4Net.Logging.AssertableLogProvider;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using Neo4Net.Test.rule.concurrent;
	using EphemeralFileSystemRule = Neo4Net.Test.rule.fs.EphemeralFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.startsWith;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.hasProperty;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.bolt.v1.messaging.util.MessageMatchers.msgSuccess;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.bolt.v1.transport.integration.TransportTestUtil.eventuallyReceives;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.logging.AssertableLogProvider.inLog;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.test.matchers.CommonMatchers.matchesExceptionMessage;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class BoltThrottleMaxDurationIT
	public class BoltThrottleMaxDurationIT
	{
		private bool InstanceFieldsInitialized = false;

		public BoltThrottleMaxDurationIT()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_server = new Neo4NetWithSocket( this.GetType(), TestGraphDatabaseFactory, _fsRule, SettingsFunction );
			RuleChain = RuleChain.outerRule( _fsRule ).around( _server );
		}

		 private AssertableLogProvider _logProvider;
		 private EphemeralFileSystemRule _fsRule = new EphemeralFileSystemRule();
		 private Neo4NetWithSocket _server;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(fsRule).around(server);
		 public RuleChain RuleChain;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public Neo4Net.test.rule.concurrent.OtherThreadRule<Void> otherThread = new Neo4Net.test.rule.concurrent.OtherThreadRule<>(5, java.util.concurrent.TimeUnit.MINUTES);
		 public OtherThreadRule<Void> OtherThread = new OtherThreadRule<Void>( 5, TimeUnit.MINUTES );
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter public Neo4Net.function.Factory<Neo4Net.bolt.v1.transport.socket.client.TransportConnection> cf;
		 public IFactory<TransportConnection> Cf;

		 private HostnamePort _address;
		 private TransportConnection _client;
		 private TransportTestUtil _util;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters public static java.util.Collection<Neo4Net.function.Factory<Neo4Net.bolt.v1.transport.socket.client.TransportConnection>> transports()
		 public static ICollection<Factory<TransportConnection>> Transports()
		 {
			  // we're not running with WebSocketChannels because of their duplex communication model
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  return asList( SocketConnection::new, SecureSocketConnection::new );
		 }

		 protected internal virtual TestGraphDatabaseFactory TestGraphDatabaseFactory
		 {
			 get
			 {
				  TestGraphDatabaseFactory factory = new TestGraphDatabaseFactory();
   
				  _logProvider = new AssertableLogProvider();
   
				  factory.InternalLogProvider = _logProvider;
   
				  return factory;
			 }
		 }

		 protected internal virtual System.Action<IDictionary<string, string>> SettingsFunction
		 {
			 get
			 {
				  return settings =>
				  {
					settings.put( GraphDatabaseSettings.auth_enabled.name(), "false" );
					settings.put( GraphDatabaseSettings.bolt_outbound_buffer_throttle_max_duration.name(), "30s" );
				  };
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  _client = Cf.newInstance();
			  _address = _server.lookupDefaultConnector();
			  _util = new TransportTestUtil( new Neo4NetPackV1() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void after() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void After()
		 {
			  if ( _client != null )
			  {
					_client.disconnect();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void sendingButNotReceivingClientShouldBeKilledWhenWriteThrottleMaxDurationIsReached() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SendingButNotReceivingClientShouldBeKilledWhenWriteThrottleMaxDurationIsReached()
		 {
			  int numberOfRunDiscardPairs = 10_000;
			  string largeString = StringUtils.repeat( " ", 8 * 1024 );

			  _client.connect( _address ).send( _util.acceptedVersions( 1, 0, 0, 0 ) ).send( _util.chunk( new InitMessage( "TestClient/1.1", emptyMap() ) ) );

			  assertThat( _client, eventuallyReceives( new sbyte[]{ 0, 0, 0, 1 } ) );
			  assertThat( _client, _util.eventuallyReceives( msgSuccess() ) );

			  Future sender = OtherThread.execute(state =>
			  {
				for ( int i = 0; i < numberOfRunDiscardPairs; i++ )
				{
					 _client.send( _util.chunk( new RunMessage( "RETURN $data as data", ValueUtils.asMapValue( singletonMap( "data", largeString ) ) ), PullAllMessage.INSTANCE ) );
				}

				return null;
			  });

			  try
			  {
					OtherThread.get().awaitFuture(sender);

					fail( "should throw ExecutionException instead" );
			  }
			  catch ( ExecutionException e )
			  {
					assertThat( Exceptions.rootCause( e ), instanceOf( typeof( SocketException ) ) );
			  }

			  _logProvider.assertAtLeastOnce( inLog( Matchers.containsString( typeof( BoltConnection ).Assembly.GetName().Name ) ).error(startsWith("Unexpected error detected in bolt session"), hasProperty("cause", matchesExceptionMessage(containsString("will be closed because the client did not consume outgoing buffers for ")))) );
		 }

	}

}