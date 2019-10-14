using System;
using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4j Enterprise Edition. The included source
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
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.Server.rest.security
{
	using JsonNode = org.codehaus.jackson.JsonNode;
	using ArrayNode = org.codehaus.jackson.node.ArrayNode;
	using IntNode = org.codehaus.jackson.node.IntNode;
	using LongNode = org.codehaus.jackson.node.LongNode;
	using ObjectNode = org.codehaus.jackson.node.ObjectNode;
	using TextNode = org.codehaus.jackson.node.TextNode;


	using Neo4Net.Graphdb;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using HostnamePort = Neo4Net.Helpers.HostnamePort;
	using LoginContext = Neo4Net.@internal.Kernel.Api.security.LoginContext;
	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using BoltConnector = Neo4Net.Kernel.configuration.BoltConnector;
	using ConnectorPortRegister = Neo4Net.Kernel.configuration.ConnectorPortRegister;
	using LegacySslPolicyConfig = Neo4Net.Kernel.configuration.ssl.LegacySslPolicyConfig;
	using EnterpriseAuthManager = Neo4Net.Kernel.enterprise.api.security.EnterpriseAuthManager;
	using InternalTransaction = Neo4Net.Kernel.impl.coreapi.InternalTransaction;
	using GraphDatabaseFacade = Neo4Net.Kernel.impl.factory.GraphDatabaseFacade;
	using EnterpriseServerBuilder = Neo4Net.Server.enterprise.helpers.EnterpriseServerBuilder;
	using CommunityServerBuilder = Neo4Net.Server.helpers.CommunityServerBuilder;
	using JsonHelper = Neo4Net.Server.rest.domain.JsonHelper;
	using JsonParseException = Neo4Net.Server.rest.domain.JsonParseException;
	using EnterpriseAuthAndUserManager = Neo4Net.Server.security.enterprise.auth.EnterpriseAuthAndUserManager;
	using EnterpriseUserManager = Neo4Net.Server.security.enterprise.auth.EnterpriseUserManager;
	using Neo4Net.Server.security.enterprise.auth;
	using HTTP = Neo4Net.Test.server.HTTP;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.not;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.security.AuthToken.newBasicAuthToken;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.BoltConnector.EncryptionLevel.OPTIONAL;

	internal abstract class AbstractRESTInteraction : CommunityServerTestBase, NeoInteractionLevel<RESTSubject>
	{
		public abstract void AssertSessionKilled( S subject );
		public abstract void AssertInitFailed( S subject );
		public abstract void AssertPasswordChangeRequired( S subject );
		public abstract void AssertAuthenticated( S subject );
		public abstract string TempPath( string prefix, string suffix );
		public abstract string NameOf( S subject );
		public abstract void UpdateAuthToken( S subject, string username, string password );
		public abstract void Logout( S subject );
		public abstract string ExecuteQuery( S subject, string call, IDictionary<string, object> @params, System.Action<ResourceIterator<IDictionary<string, object>>> resultConsumer );
		public abstract InternalTransaction BeginLocalTransactionAsUser( S subject, KernelTransaction.Type txType );

		 private ConnectorPortRegister _connectorPortRegister;

		 internal abstract string CommitPath();

		 internal abstract void Consume( System.Action<ResourceIterator<IDictionary<string, object>>> resultConsumer, JsonNode data );

		 internal abstract HTTP.RawPayload ConstructQuery( string query );

		 protected internal abstract HTTP.Response Authenticate( string principalCredentials );

		 internal const string POST = "POST";

		 private EnterpriseAuthManager _authManager;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: AbstractRESTInteraction(java.util.Map<String,String> config) throws java.io.IOException
		 internal AbstractRESTInteraction( IDictionary<string, string> config )
		 {
			  CommunityServerBuilder builder = EnterpriseServerBuilder.serverOnRandomPorts();
			  builder = builder.WithProperty( ( new BoltConnector( "bolt" ) ).type.name(), "BOLT" ).withProperty((new BoltConnector("bolt")).enabled.name(), "true").withProperty((new BoltConnector("bolt")).encryption_level.name(), OPTIONAL.name()).withProperty(LegacySslPolicyConfig.tls_key_file.name(), NeoInteractionLevel.tempPath("key", ".key")).withProperty(LegacySslPolicyConfig.tls_certificate_file.name(), NeoInteractionLevel.tempPath("cert", ".cert")).withProperty(GraphDatabaseSettings.auth_enabled.name(), Convert.ToString(true));

			  foreach ( KeyValuePair<string, string> entry in config.SetOfKeyValuePairs() )
			  {
					builder = builder.WithProperty( entry.Key, entry.Value );
			  }
			  this.Server = builder.Build();
			  this.Server.start();
			  _authManager = this.Server.DependencyResolver.resolveDependency( typeof( EnterpriseAuthManager ) );
			  _connectorPortRegister = Server.DependencyResolver.resolveDependency( typeof( ConnectorPortRegister ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.server.security.enterprise.auth.EnterpriseUserManager getLocalUserManager() throws Exception
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
				  return Server.Database.Graph;
			 }
		 }

		 public override FileSystemAbstraction FileSystem()
		 {
			  return new DefaultFileSystemAbstraction();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.kernel.impl.coreapi.InternalTransaction beginLocalTransactionAsUser(RESTSubject subject, org.neo4j.kernel.api.KernelTransaction.Type txType) throws Throwable
		 public override InternalTransaction BeginLocalTransactionAsUser( RESTSubject subject, KernelTransaction.Type txType )
		 {
			  LoginContext loginContext = _authManager.login( newBasicAuthToken( subject.Username, subject.Password ) );
			  return LocalGraph.beginTransaction( txType, loginContext );
		 }

		 public override string ExecuteQuery( RESTSubject subject, string call, IDictionary<string, object> @params, System.Action<ResourceIterator<IDictionary<string, object>>> resultConsumer )
		 {
			  HTTP.RawPayload payload = ConstructQuery( call );
			  HTTP.Response response = HTTP.withHeaders( HttpHeaders.AUTHORIZATION, subject.PrincipalCredentials ).request( POST, CommitURL(), payload );

			  try
			  {
					string error = ParseErrorMessage( response );
					if ( error.Length > 0 )
					{
						 return error;
					}
					Consume( resultConsumer, JsonHelper.jsonNode( response.RawContent() ) );
			  }
			  catch ( JsonParseException )
			  {
					fail( "Unexpected error parsing Json!" );
			  }

			  return "";
		 }

		 public override RESTSubject Login( string username, string password )
		 {
			  string principalCredentials = HTTP.basicAuthHeader( username, password );
			  return new RESTSubject( username, password, principalCredentials );
		 }

		 public override void Logout( RESTSubject subject )
		 {
		 }

		 public override void UpdateAuthToken( RESTSubject subject, string username, string password )
		 {
			  subject.PrincipalCredentials = HTTP.basicAuthHeader( username, password );
		 }

		 public override string NameOf( RESTSubject subject )
		 {
			  return subject.Username;
		 }

		 public override void TearDown()
		 {
			  if ( Server != null )
			  {
					Server.stop();
			  }
		 }

		 public override void AssertAuthenticated( RESTSubject subject )
		 {
			  HTTP.Response authenticate = authenticate( subject.PrincipalCredentials );
			  assertThat( authenticate.RawContent(), authenticate.Status(), equalTo(200) );
		 }

		 public override void AssertPasswordChangeRequired( RESTSubject subject )
		 {
			  HTTP.Response response = Authenticate( subject.PrincipalCredentials );
			  assertThat( response.Status(), equalTo(403) );
			  assertThat( ParseErrorMessage( response ), containsString( "User is required to change their password." ) );
		 }

		 public override void AssertInitFailed( RESTSubject subject )
		 {
			  assertThat( Authenticate( subject.PrincipalCredentials ).status(), not(equalTo(200)) );
		 }

		 public override void AssertSessionKilled( RESTSubject subject )
		 {
			  // There is no session that could have been killed
		 }

		 public virtual string ConnectionProtocol
		 {
			 get
			 {
				  return "http";
			 }
		 }

		 public override HostnamePort LookupConnector( string connectorKey )
		 {
			  return _connectorPortRegister.getLocalAddress( connectorKey );
		 }

		 private string ParseErrorMessage( HTTP.Response response )
		 {
			  try
			  {
					JsonNode data = JsonHelper.jsonNode( response.RawContent() );
					if ( data.has( "errors" ) && data.get( "errors" ).has( 0 ) )
					{
						 JsonNode firstError = data.get( "errors" ).get( 0 );
						 if ( firstError.has( "message" ) )
						 {
							  return firstError.get( "message" ).asText();
						 }
					}
			  }
			  catch ( JsonParseException )
			  {
					fail( "Unexpected error parsing Json!" );
			  }
			  return "";
		 }

		 internal virtual string CommitURL()
		 {
			  return Server.baseUri().resolve(CommitPath()).ToString();
		 }

		 internal abstract class AbstractRESTResult : ResourceIterator<IDictionary<string, object>>
		 {
			 public abstract ResourceIterator<R> Map( System.Func<T, R> map );
			 public abstract java.util.stream.Stream<T> Stream();
			 private readonly AbstractRESTInteraction _outerInstance;

			  internal JsonNode Data;
			  internal JsonNode Columns;
			  internal int Index;

			  internal AbstractRESTResult( AbstractRESTInteraction outerInstance, JsonNode fullResult )
			  {
				  this._outerInstance = outerInstance;
					this.Data = fullResult.get( "data" );
					this.Columns = fullResult.get( "columns" );
			  }

			  public override void Close()
			  {
					Index = Data.size();
			  }

			  public override bool HasNext()
			  {
					return Index < Data.size();
			  }

			  public override IDictionary<string, object> Next()
			  {
					JsonNode row = GetRow( Data, Index++ );
					SortedDictionary<string, object> map = new SortedDictionary<string, object>();
					for ( int i = 0; i < Columns.size(); i++ )
					{
						 string key = Columns.get( i ).asText();
						 object value = outerInstance.getValue( row.get( i ) );
						 map[key] = value;
					}
					return map;
			  }

			  protected internal abstract JsonNode GetRow( JsonNode data, int i );
		 }

		 private object GetValue( JsonNode valueNode )
		 {
			  object value;

			  if ( valueNode is TextNode )
			  {
					value = valueNode.asText();
			  }
			  else if ( valueNode is ObjectNode )
			  {
					value = MapValue( valueNode.FieldNames, valueNode );
			  }
			  else if ( valueNode is ArrayNode )
			  {
					ArrayNode aNode = ( ArrayNode ) valueNode;
					List<string> listValue = new List<string>( aNode.size() );
					for ( int j = 0; j < aNode.size(); j++ )
					{
						 listValue.Add( aNode.get( j ).asText() );
					}
					value = listValue;
			  }
			  else if ( valueNode is IntNode )
			  {
					value = valueNode.IntValue;
			  }
			  else if ( valueNode is LongNode )
			  {
					value = valueNode.LongValue;
			  }
			  else if ( valueNode.Null )
			  {
					return null;
			  }
			  else
			  {
					throw new Exception( string.Format( "Unhandled REST value type '{0}'. Need String (TextNode), List (ArrayNode), Object (ObjectNode), " + "long (LongNode), or int (IntNode).", valueNode.GetType() ) );
			  }
			  return value;
		 }

		 private IDictionary<string, object> MapValue( IEnumerator<string> columns, JsonNode node )
		 {
			  SortedDictionary<string, object> map = new SortedDictionary<string, object>();
			  while ( columns.MoveNext() )
			  {
					string key = columns.Current;
					object value = GetValue( node.get( key ) );
					map[key] = value;
			  }

			  return map;
		 }
	}

}