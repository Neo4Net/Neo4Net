using System.Collections.Generic;
using System.Threading;

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
namespace Org.Neo4j.Bolt.v1.transport.integration
{
	using StringUtils = org.apache.commons.lang3.StringUtils;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;


	using BoltConnectionReadLimiter = Org.Neo4j.Bolt.runtime.BoltConnectionReadLimiter;
	using Neo4jPackV1 = Org.Neo4j.Bolt.v1.messaging.Neo4jPackV1;
	using DiscardAllMessage = Org.Neo4j.Bolt.v1.messaging.request.DiscardAllMessage;
	using InitMessage = Org.Neo4j.Bolt.v1.messaging.request.InitMessage;
	using RunMessage = Org.Neo4j.Bolt.v1.messaging.request.RunMessage;
	using SocketConnection = Org.Neo4j.Bolt.v1.transport.socket.client.SocketConnection;
	using TransportConnection = Org.Neo4j.Bolt.v1.transport.socket.client.TransportConnection;
	using Org.Neo4j.Collection;
	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using HostnamePort = Org.Neo4j.Helpers.HostnamePort;
	using ProcedureException = Org.Neo4j.@internal.Kernel.Api.exceptions.ProcedureException;
	using Neo4jTypes = Org.Neo4j.@internal.Kernel.Api.procs.Neo4jTypes;
	using ProcedureSignature = Org.Neo4j.@internal.Kernel.Api.procs.ProcedureSignature;
	using ResourceTracker = Org.Neo4j.Kernel.api.ResourceTracker;
	using Status = Org.Neo4j.Kernel.Api.Exceptions.Status;
	using CallableProcedure = Org.Neo4j.Kernel.api.proc.CallableProcedure;
	using Context = Org.Neo4j.Kernel.api.proc.Context;
	using Procedures = Org.Neo4j.Kernel.impl.proc.Procedures;
	using ValueUtils = Org.Neo4j.Kernel.impl.util.ValueUtils;
	using GraphDatabaseAPI = Org.Neo4j.Kernel.@internal.GraphDatabaseAPI;
	using AssertableLogProvider = Org.Neo4j.Logging.AssertableLogProvider;
	using TestGraphDatabaseFactory = Org.Neo4j.Test.TestGraphDatabaseFactory;
	using EphemeralFileSystemRule = Org.Neo4j.Test.rule.fs.EphemeralFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.anything;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.messaging.util.MessageMatchers.msgSuccess;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.procs.ProcedureSignature.procedureSignature;

	public class BoltChannelAutoReadLimiterIT
	{
		private bool InstanceFieldsInitialized = false;

		public BoltChannelAutoReadLimiterIT()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_server = new Neo4jWithSocket( this.GetType(), TestGraphDatabaseFactory, _fsRule, SettingsFunction );
			RuleChain = RuleChain.outerRule( _fsRule ).around( _server );
		}

		 private AssertableLogProvider _logProvider;
		 private EphemeralFileSystemRule _fsRule = new EphemeralFileSystemRule();
		 private Neo4jWithSocket _server;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(fsRule).around(server);
		 public RuleChain RuleChain;

		 private HostnamePort _address;
		 private TransportConnection _connection;
		 private TransportTestUtil _util;

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
				  return settings => settings.put( GraphDatabaseSettings.auth_enabled.name(), "false" );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Setup()
		 {
			  InstallSleepProcedure( _server.graphDatabaseService() );

			  _address = _server.lookupDefaultConnector();
			  _connection = new SocketConnection();
			  _util = new TransportTestUtil( new Neo4jPackV1() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void largeNumberOfSlowRunningJobsShouldChangeAutoReadState() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void LargeNumberOfSlowRunningJobsShouldChangeAutoReadState()
		 {
			  int numberOfRunDiscardPairs = 1000;
			  string largeString = StringUtils.repeat( " ", 8 * 1024 );

			  _connection.connect( _address ).send( _util.defaultAcceptedVersions() ).send(_util.chunk(new InitMessage("TestClient/1.1", emptyMap())));

			  assertThat( _connection, _util.eventuallyReceivesSelectedProtocolVersion() );
			  assertThat( _connection, _util.eventuallyReceives( msgSuccess() ) );

			  // when
			  for ( int i = 0; i < numberOfRunDiscardPairs; i++ )
			  {
					_connection.send( _util.chunk( new RunMessage( "CALL boltissue.sleep( $data )", ValueUtils.asMapValue( singletonMap( "data", largeString ) ) ), DiscardAllMessage.INSTANCE ) );
			  }

			  // expect
			  for ( int i = 0; i < numberOfRunDiscardPairs; i++ )
			  {
					assertThat( _connection, _util.eventuallyReceives( msgSuccess(), msgSuccess() ) );
			  }

			  _logProvider.assertAtLeastOnce( AssertableLogProvider.inLog( typeof( BoltConnectionReadLimiter ) ).warn( containsString( "disabled" ), anything(), anything() ) );
			  _logProvider.assertAtLeastOnce( AssertableLogProvider.inLog( typeof( BoltConnectionReadLimiter ) ).warn( containsString( "enabled" ), anything(), anything() ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void installSleepProcedure(org.neo4j.graphdb.GraphDatabaseService db) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 private static void InstallSleepProcedure( GraphDatabaseService db )
		 {
			  GraphDatabaseAPI dbApi = ( GraphDatabaseAPI ) db;

			  dbApi.DependencyResolver.resolveDependency( typeof( Procedures ) ).register( new CallableProcedure_BasicProcedureAnonymousInnerClass() );
		 }

		 private class CallableProcedure_BasicProcedureAnonymousInnerClass : Org.Neo4j.Kernel.api.proc.CallableProcedure_BasicProcedure
		 {
			 public CallableProcedure_BasicProcedureAnonymousInnerClass() : base(procedureSignature("boltissue", "sleep").@in("data", Neo4jTypes.NTString).@out(ProcedureSignature.VOID).build())
			 {
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.collection.RawIterator<Object[],org.neo4j.internal.kernel.api.exceptions.ProcedureException> apply(org.neo4j.kernel.api.proc.Context context, Object[] objects, org.neo4j.kernel.api.ResourceTracker resourceTracker) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
			 public override RawIterator<object[], ProcedureException> apply( Context context, object[] objects, ResourceTracker resourceTracker )
			 {
				  try
				  {
						Thread.Sleep( 50 );
				  }
				  catch ( InterruptedException e )
				  {
						throw new ProcedureException( Org.Neo4j.Kernel.Api.Exceptions.Status_General.UnknownError, e, "Interrupted" );
				  }
				  return RawIterator.empty();
			 }
		 }

	}

}