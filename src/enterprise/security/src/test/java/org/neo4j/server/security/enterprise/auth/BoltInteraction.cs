using System;
using System.Collections.Generic;

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
namespace Neo4Net.Server.security.enterprise.auth
{

	using ResponseMessage = Neo4Net.Bolt.messaging.ResponseMessage;
	using AuthenticationException = Neo4Net.Bolt.security.auth.AuthenticationException;
	using Neo4NetPackV1 = Neo4Net.Bolt.v1.messaging.Neo4NetPackV1;
	using InitMessage = Neo4Net.Bolt.v1.messaging.request.InitMessage;
	using PullAllMessage = Neo4Net.Bolt.v1.messaging.request.PullAllMessage;
	using ResetMessage = Neo4Net.Bolt.v1.messaging.request.ResetMessage;
	using RunMessage = Neo4Net.Bolt.v1.messaging.request.RunMessage;
	using FailureMessage = Neo4Net.Bolt.v1.messaging.response.FailureMessage;
	using RecordMessage = Neo4Net.Bolt.v1.messaging.response.RecordMessage;
	using SuccessMessage = Neo4Net.Bolt.v1.messaging.response.SuccessMessage;
	using Neo4NetWithSocket = Neo4Net.Bolt.v1.transport.integration.Neo4NetWithSocket;
	using TransportTestUtil = Neo4Net.Bolt.v1.transport.integration.TransportTestUtil;
	using SocketConnection = Neo4Net.Bolt.v1.transport.socket.client.SocketConnection;
	using TransportConnection = Neo4Net.Bolt.v1.transport.socket.client.TransportConnection;
	using Neo4Net.Functions;
	using Neo4Net.GraphDb;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using EphemeralFileSystemAbstraction = Neo4Net.GraphDb.mockfs.EphemeralFileSystemAbstraction;
	using HostnamePort = Neo4Net.Helpers.HostnamePort;
	using AuthenticationResult = Neo4Net.Internal.Kernel.Api.security.AuthenticationResult;
	using LoginContext = Neo4Net.Internal.Kernel.Api.security.LoginContext;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using EnterpriseAuthManager = Neo4Net.Kernel.enterprise.api.security.EnterpriseAuthManager;
	using InternalTransaction = Neo4Net.Kernel.impl.coreapi.InternalTransaction;
	using GraphDatabaseFacade = Neo4Net.Kernel.impl.factory.GraphDatabaseFacade;
	using ValueUtils = Neo4Net.Kernel.impl.util.ValueUtils;
	using TestEnterpriseGraphDatabaseFactory = Neo4Net.Test.TestEnterpriseGraphDatabaseFactory;
	using AnyValue = Neo4Net.Values.AnyValue;
	using TextValue = Neo4Net.Values.Storable.TextValue;
	using Values = Neo4Net.Values.Storable.Values;
	using ListValue = Neo4Net.Values.@virtual.ListValue;
	using MapValue = Neo4Net.Values.@virtual.MapValue;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.bolt.v1.transport.integration.TransportTestUtil.eventuallyReceives;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.MapUtil.map;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.api.security.AuthToken_Fields.BASIC_SCHEME;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.api.security.AuthToken_Fields.CREDENTIALS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.api.security.AuthToken_Fields.NATIVE_REALM;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.api.security.AuthToken_Fields.PRINCIPAL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.api.security.AuthToken_Fields.REALM_KEY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.api.security.AuthToken_Fields.SCHEME_KEY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.api.security.AuthToken.newBasicAuthToken;

	internal class BoltInteraction : NeoInteractionLevel<BoltInteraction.BoltSubject>
	{
		 private readonly TransportTestUtil _util = new TransportTestUtil( new Neo4NetPackV1() );
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
		 private readonly IFactory<TransportConnection> _connectionFactory = SocketConnection::new;
		 private readonly Neo4NetWithSocket _server;
		 private IDictionary<string, BoltSubject> _subjects = new Dictionary<string, BoltSubject>();
		 private FileSystemAbstraction _fileSystem;
		 private EnterpriseAuthManager _authManager;

		 internal BoltInteraction( IDictionary<string, string> config ) : this( config, EphemeralFileSystemAbstraction::new )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
		 }

		 internal BoltInteraction( IDictionary<string, string> config, System.Func<FileSystemAbstraction> fileSystemSupplier )
		 {
			  TestEnterpriseGraphDatabaseFactory factory = new TestEnterpriseGraphDatabaseFactory();
			  _fileSystem = fileSystemSupplier();
			  _server = new Neo4NetWithSocket(this.GetType(), factory, () => _fileSystem, settings =>
			  {
						  settings.put( GraphDatabaseSettings.auth_enabled.name(), "true" );
						  settings.putAll( config );
			  });
			  _server.ensureDatabase(r =>
			  {
			  });
			  GraphDatabaseFacade db = ( GraphDatabaseFacade ) _server.graphDatabaseService();
			  _authManager = Db.DependencyResolver.resolveDependency( typeof( EnterpriseAuthManager ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public EnterpriseUserManager getLocalUserManager() throws Exception
		 public virtual EnterpriseUserManager LocalUserManager
		 {
			 get
			 {
				  if ( _authManager is EnterpriseAuthAndUserManager )
				  {
						return ( ( EnterpriseAuthAndUserManager ) _authManager ).UserManager;
				  }
				  throw new Exception( "The used configuration does not have a user manager" );
			 }
		 }

		 public virtual GraphDatabaseFacade LocalGraph
		 {
			 get
			 {
				  return ( GraphDatabaseFacade ) _server.graphDatabaseService();
			 }
		 }

		 public override FileSystemAbstraction FileSystem()
		 {
			  return _fileSystem;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.kernel.impl.coreapi.InternalTransaction beginLocalTransactionAsUser(BoltSubject subject, org.Neo4Net.kernel.api.KernelTransaction.Type txType) throws Throwable
		 public override InternalTransaction BeginLocalTransactionAsUser( BoltSubject subject, KernelTransaction.Type txType )
		 {
			  LoginContext loginContext = _authManager.login( newBasicAuthToken( subject.Username, subject.Password ) );
			  return LocalGraph.beginTransaction( txType, loginContext );
		 }

		 public override string ExecuteQuery( BoltSubject subject, string call, IDictionary<string, object> @params, System.Action<ResourceIterator<IDictionary<string, object>>> resultConsumer )
		 {
			  if ( @params == null )
			  {
					@params = Collections.emptyMap();
			  }
			  try
			  {
					subject.Client.send( _util.chunk( new RunMessage( call, ValueUtils.asMapValue( @params ) ), PullAllMessage.INSTANCE ) );
					resultConsumer( CollectResults( subject.Client ) );
					return "";
			  }
			  catch ( Exception e )
			  {
					return e.Message;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public BoltSubject login(String username, String password) throws Exception
		 public override BoltSubject Login( string username, string password )
		 {
			  BoltSubject subject = _subjects[username];
			  if ( subject == null )
			  {
					subject = new BoltSubject( _connectionFactory.newInstance(), username, password );
					_subjects[username] = subject;
			  }
			  else
			  {
					subject.Client.disconnect();
					subject.Client = _connectionFactory.newInstance();
			  }
			  subject.Client.connect( _server.lookupDefaultConnector() ).send(_util.acceptedVersions(1, 0, 0, 0)).send(_util.chunk(new InitMessage("TestClient/1.1", map(REALM_KEY, NATIVE_REALM, PRINCIPAL, username, CREDENTIALS, password, SCHEME_KEY, BASIC_SCHEME))));
			  assertThat( subject.Client, eventuallyReceives( new sbyte[]{ 0, 0, 0, 1 } ) );
			  subject.LoginResult = _util.receiveOneResponseMessage( subject.Client );
			  return subject;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void logout(BoltSubject subject) throws Exception
		 public override void Logout( BoltSubject subject )
		 {
			  subject.Client.disconnect();
			  subject.Client = _connectionFactory.newInstance();
		 }

		 public override void UpdateAuthToken( BoltSubject subject, string username, string password )
		 {

		 }

		 public override string NameOf( BoltSubject subject )
		 {
			  return subject.Username;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void tearDown() throws Throwable
		 public override void TearDown()
		 {
			  foreach ( BoltSubject subject in _subjects.Values )
			  {
					subject.Client.disconnect();
			  }
			  _subjects.Clear();
			  _server.graphDatabaseService().shutdown();
			  _fileSystem.Dispose();
		 }

		 public override void AssertAuthenticated( BoltSubject subject )
		 {
			  assertTrue( "Should be authenticated", subject.Authenticated );
		 }

		 public override void AssertPasswordChangeRequired( BoltSubject subject )
		 {
			  assertTrue( "Should need to change password", subject.PasswordChangeRequired() );
		 }

		 public override void AssertInitFailed( BoltSubject subject )
		 {
			  assertFalse( "Should not be authenticated", subject.Authenticated );
		 }

		 public override void AssertSessionKilled( BoltSubject subject )
		 {
			  assertThat( subject.Client, TransportTestUtil.eventuallyDisconnects() );
		 }

		 public virtual string ConnectionProtocol
		 {
			 get
			 {
				  return "bolt";
			 }
		 }

		 public override HostnamePort LookupConnector( string connectorKey )
		 {
			  return _server.lookupConnector( connectorKey );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private BoltResult collectResults(org.Neo4Net.bolt.v1.transport.socket.client.TransportConnection client) throws Exception
		 private BoltResult CollectResults( TransportConnection client )
		 {
			  ResponseMessage message = _util.receiveOneResponseMessage( client );
			  IList<string> fieldNames = new List<string>();
			  IList<IDictionary<string, object>> result = new List<IDictionary<string, object>>();

			  if ( message is SuccessMessage )
			  {
					MapValue metadata = ( ( SuccessMessage ) message ).meta();
					ListValue fieldNameValues = ( ListValue ) metadata.Get( "fields" );
					foreach ( AnyValue value in fieldNameValues )
					{
						 fieldNames.Add( ( ( TextValue ) value ).stringValue() );
					}
			  }
			  else if ( message is FailureMessage )
			  {
					FailureMessage failMessage = ( FailureMessage ) message;
					// drain ignoredMessage, ack failure, get successMessage
					_util.receiveOneResponseMessage( client );
					client.Send( _util.chunk( ResetMessage.INSTANCE ) );
					_util.receiveOneResponseMessage( client );
					throw new AuthenticationException( failMessage.Status(), failMessage.Message() );
			  }

			  do
			  {
					message = _util.receiveOneResponseMessage( client );
					if ( message is RecordMessage )
					{
						 object[] row = ( ( RecordMessage ) message ).record().fields();
						 IDictionary<string, object> rowMap = new Dictionary<string, object>();
						 for ( int i = 0; i < row.Length; i++ )
						 {
							  rowMap[fieldNames[i]] = row[i];
						 }
						 result.Add( rowMap );
					}
			  } while ( !( message is SuccessMessage ) && !( message is FailureMessage ) );

			  if ( message is FailureMessage )
			  {
					FailureMessage failMessage = ( FailureMessage ) message;
					// ack failure, get successMessage
					client.Send( _util.chunk( ResetMessage.INSTANCE ) );
					_util.receiveOneResponseMessage( client );
					throw new AuthenticationException( failMessage.Status(), failMessage.Message() );
			  }

			  return new BoltResult( result );
		 }

		 internal class BoltSubject
		 {
			  internal TransportConnection Client;
			  internal string Username;
			  internal string Password;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal AuthenticationResult LoginResultConflict = AuthenticationResult.FAILURE;

			  internal BoltSubject( TransportConnection client, string username, string password )
			  {
					this.Client = client;
					this.Username = username;
					this.Password = password;
			  }

			  internal virtual ResponseMessage LoginResult
			  {
				  set
				  {
						if ( value is SuccessMessage )
						{
							 MapValue meta = ( ( SuccessMessage ) value ).meta();
							 if ( meta.ContainsKey( "credentials_expired" ) && meta.Get( "credentials_expired" ).Equals( Values.TRUE ) )
							 {
								  LoginResultConflict = AuthenticationResult.PASSWORD_CHANGE_REQUIRED;
							 }
							 else
							 {
								  LoginResultConflict = AuthenticationResult.SUCCESS;
							 }
						}
						else if ( value is FailureMessage )
						{
							 LoginResultConflict = AuthenticationResult.FAILURE;
							 Status status = ( ( FailureMessage ) value ).status();
							 if ( status.Equals( Neo4Net.Kernel.Api.Exceptions.Status_Security.AuthenticationRateLimit ) )
							 {
								  LoginResultConflict = AuthenticationResult.TOO_MANY_ATTEMPTS;
							 }
						}
				  }
			  }

			  internal virtual bool Authenticated
			  {
				  get
				  {
						return LoginResultConflict.Equals( AuthenticationResult.SUCCESS );
				  }
			  }

			  internal virtual bool PasswordChangeRequired()
			  {
					return LoginResultConflict.Equals( AuthenticationResult.PASSWORD_CHANGE_REQUIRED );
			  }
		 }

		 internal class BoltResult : ResourceIterator<IDictionary<string, object>>
		 {
			  internal int Index;
			  internal IList<IDictionary<string, object>> Data;

			  internal BoltResult( IList<IDictionary<string, object>> data )
			  {
					this.Data = data;
			  }

			  public override void Close()
			  {
					Index = Data.Count;
			  }

			  public override bool HasNext()
			  {
					return Index < Data.Count;
			  }

			  public override IDictionary<string, object> Next()
			  {
					IDictionary<string, object> row = Data[Index];
					Index++;
					return row;
			  }
		 }
	}

}