﻿using System;
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
namespace Org.Neo4j.Bolt.runtime.integration
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using DiscardAllMessage = Org.Neo4j.Bolt.v1.messaging.request.DiscardAllMessage;
	using InitMessage = Org.Neo4j.Bolt.v1.messaging.request.InitMessage;
	using RunMessage = Org.Neo4j.Bolt.v1.messaging.request.RunMessage;
	using Neo4jWithSocket = Org.Neo4j.Bolt.v1.transport.integration.Neo4jWithSocket;
	using TransportConnection = Org.Neo4j.Bolt.v1.transport.socket.client.TransportConnection;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using Status = Org.Neo4j.Kernel.Api.Exceptions.Status;
	using BoltConnector = Org.Neo4j.Kernel.configuration.BoltConnector;
	using AssertableLogProvider = Org.Neo4j.Logging.AssertableLogProvider;
	using TestGraphDatabaseFactory = Org.Neo4j.Test.TestGraphDatabaseFactory;
	using EphemeralFileSystemRule = Org.Neo4j.Test.rule.fs.EphemeralFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.isA;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.startsWith;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.messaging.util.MessageMatchers.msgFailure;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.messaging.util.MessageMatchers.msgSuccess;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.transport.integration.Neo4jWithSocket.DEFAULT_CONNECTOR_KEY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.transport.integration.TransportTestUtil.eventuallyReceives;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class BoltSchedulerBusyIT extends org.neo4j.bolt.AbstractBoltTransportsTest
	public class BoltSchedulerBusyIT : AbstractBoltTransportsTest
	{
		private bool InstanceFieldsInitialized = false;

		public BoltSchedulerBusyIT()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_server = new Neo4jWithSocket( this.GetType(), TestGraphDatabaseFactory, _fsRule.get, SettingsFunction );
			RuleChain = RuleChain.outerRule( _fsRule ).around( _server );
		}

		 private AssertableLogProvider _internalLogProvider = new AssertableLogProvider();
		 private AssertableLogProvider _userLogProvider = new AssertableLogProvider();
		 private EphemeralFileSystemRule _fsRule = new EphemeralFileSystemRule();
		 private Neo4jWithSocket _server;
		 private TransportConnection _connection1;
		 private TransportConnection _connection2;
		 private TransportConnection _connection3;
		 private TransportConnection _connection4;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(fsRule).around(server);
		 public RuleChain RuleChain;

		 protected internal virtual TestGraphDatabaseFactory TestGraphDatabaseFactory
		 {
			 get
			 {
				  TestGraphDatabaseFactory factory = new TestGraphDatabaseFactory();
				  factory.InternalLogProvider = _internalLogProvider;
				  factory.UserLogProvider = _userLogProvider;
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
					settings.put( ( new BoltConnector( DEFAULT_CONNECTOR_KEY ) ).enabled.name(), "TRUE" );
					settings.put( ( new BoltConnector( DEFAULT_CONNECTOR_KEY ) ).listen_address.name(), "localhost:0" );
					settings.put( ( new BoltConnector( DEFAULT_CONNECTOR_KEY ) ).type.name(), BoltConnector.ConnectorType.BOLT.name() );
					settings.put( ( new BoltConnector( DEFAULT_CONNECTOR_KEY ) ).thread_pool_min_size.name(), "0" );
					settings.put( ( new BoltConnector( DEFAULT_CONNECTOR_KEY ) ).thread_pool_max_size.name(), "2" );
				  };
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Setup()
		 {
			  Address = _server.lookupDefaultConnector();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void cleanup() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Cleanup()
		 {
			  Close( _connection1 );
			  Close( _connection2 );
			  Close( _connection3 );
			  Close( _connection4 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportFailureWhenAllThreadsInThreadPoolAreBusy() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportFailureWhenAllThreadsInThreadPoolAreBusy()
		 {
			  // it's enough to get the bolt state machine into streaming mode to have
			  // the thread sticked to the connection, causing all the available threads
			  // to be busy (logically)
			  _connection1 = EnterStreaming();
			  _connection2 = EnterStreaming();

			  try
			  {
					_connection3 = ConnectAndPerformBoltHandshake( NewConnection() );

					_connection3.send( Util.chunk( new InitMessage( "TestClient/1.1", emptyMap() ) ) );
					assertThat( _connection3, Util.eventuallyReceives( msgFailure( Org.Neo4j.Kernel.Api.Exceptions.Status_Request.NoThreadsAvailable, "There are no available threads to serve this request at the moment" ) ) );

					_userLogProvider.rawMessageMatcher().assertContains("since there are no available threads to serve it at the moment. You can retry at a later time");
					_internalLogProvider.assertAtLeastOnce( AssertableLogProvider.inLog( startsWith( typeof( BoltConnection ).Assembly.GetName().Name ) ).error(containsString("since there are no available threads to serve it at the moment. You can retry at a later time"), isA(typeof(RejectedExecutionException))) );
			  }
			  finally
			  {
					ExitStreaming( _connection1 );
					ExitStreaming( _connection2 );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldStopConnectionsWhenRelatedJobIsRejectedOnShutdown() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldStopConnectionsWhenRelatedJobIsRejectedOnShutdown()
		 {
			  // Connect and get two connections into idle state
			  _connection1 = EnterStreaming();
			  ExitStreaming( _connection1 );
			  _connection2 = EnterStreaming();
			  ExitStreaming( _connection2 );

			  // Connect and get other set of connections to keep threads busy
			  _connection3 = EnterStreaming();
			  _connection4 = EnterStreaming();

			  // Clear any log output till now
			  _internalLogProvider.clear();

			  // Shutdown the server
			  _server.shutdownDatabase();

			  // Expect no scheduling error logs
			  _userLogProvider.rawMessageMatcher().assertNotContains("since there are no available threads to serve it at the moment. You can retry at a later time");
			  _internalLogProvider.rawMessageMatcher().assertNotContains("since there are no available threads to serve it at the moment. You can retry at a later time");
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.bolt.v1.transport.socket.client.TransportConnection enterStreaming() throws Throwable
		 private TransportConnection EnterStreaming()
		 {
			  TransportConnection connection = null;
			  Exception error = null;

			  // retry couple times because worker threads might seem busy
			  for ( int i = 1; i <= 7; i++ )
			  {
					try
					{
						 connection = NewConnection();
						 EnterStreaming( connection, i );
						 error = null;
						 return connection;
					}
					catch ( Exception t )
					{
						 // failed to enter the streaming state, record the error and retry
						 if ( error == null )
						 {
							  error = t;
						 }
						 else
						 {
							  error.addSuppressed( t );
						 }

						 Close( connection );
						 SECONDS.sleep( i );
					}
			  }

			  if ( error != null )
			  {
					throw error;
			  }

			  throw new System.InvalidOperationException( "Unable to enter the streaming state" );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void enterStreaming(org.neo4j.bolt.v1.transport.socket.client.TransportConnection connection, int sleepSeconds) throws Exception
		 private void EnterStreaming( TransportConnection connection, int sleepSeconds )
		 {
			  ConnectAndPerformBoltHandshake( connection );

			  connection.Send( Util.chunk( new InitMessage( "TestClient/1.1", emptyMap() ) ) );
			  assertThat( connection, Util.eventuallyReceives( msgSuccess() ) );

			  SECONDS.sleep( sleepSeconds ); // sleep a bit to allow worker thread return back to the pool

			  connection.Send( Util.chunk( new RunMessage( "UNWIND RANGE (1, 100) AS x RETURN x" ) ) );
			  assertThat( connection, Util.eventuallyReceives( msgSuccess() ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.bolt.v1.transport.socket.client.TransportConnection connectAndPerformBoltHandshake(org.neo4j.bolt.v1.transport.socket.client.TransportConnection connection) throws Exception
		 private TransportConnection ConnectAndPerformBoltHandshake( TransportConnection connection )
		 {
			  connection.Connect( Address ).send( Util.acceptedVersions( 1, 0, 0, 0 ) );
			  assertThat( connection, eventuallyReceives( new sbyte[]{ 0, 0, 0, 1 } ) );
			  return connection;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void exitStreaming(org.neo4j.bolt.v1.transport.socket.client.TransportConnection connection) throws Exception
		 private void ExitStreaming( TransportConnection connection )
		 {
			  connection.Send( Util.chunk( DiscardAllMessage.INSTANCE ) );

			  assertThat( connection, Util.eventuallyReceives( msgSuccess() ) );
		 }

		 private void Close( TransportConnection connection )
		 {
			  if ( connection != null )
			  {
					try
					{
						 connection.Disconnect();
					}
					catch ( IOException )
					{
					}
			  }
		 }
	}

}