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
namespace Neo4Net.Server.security.enterprise.auth
{

	using Neo4Net.Graphdb;
	using GraphDatabaseBuilder = Neo4Net.Graphdb.factory.GraphDatabaseBuilder;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using EphemeralFileSystemAbstraction = Neo4Net.Graphdb.mockfs.EphemeralFileSystemAbstraction;
	using HostnamePort = Neo4Net.Helpers.HostnamePort;
	using AuthenticationResult = Neo4Net.@internal.Kernel.Api.security.AuthenticationResult;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using BoltConnector = Neo4Net.Kernel.configuration.BoltConnector;
	using ConnectorPortRegister = Neo4Net.Kernel.configuration.ConnectorPortRegister;
	using LegacySslPolicyConfig = Neo4Net.Kernel.configuration.ssl.LegacySslPolicyConfig;
	using EnterpriseAuthManager = Neo4Net.Kernel.enterprise.api.security.EnterpriseAuthManager;
	using EnterpriseLoginContext = Neo4Net.Kernel.enterprise.api.security.EnterpriseLoginContext;
	using InternalTransaction = Neo4Net.Kernel.impl.coreapi.InternalTransaction;
	using GraphDatabaseFacade = Neo4Net.Kernel.impl.factory.GraphDatabaseFacade;
	using TestEnterpriseGraphDatabaseFactory = Neo4Net.Test.TestEnterpriseGraphDatabaseFactory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.BoltConnector.EncryptionLevel.OPTIONAL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.security.auth.SecurityTestUtils.authToken;

	public class EmbeddedInteraction : NeoInteractionLevel<EnterpriseLoginContext>
	{
		 protected internal GraphDatabaseFacade Db;
		 private EnterpriseAuthManager _authManager;
		 private FileSystemAbstraction _fileSystem;
		 private ConnectorPortRegister _connectorRegister;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: EmbeddedInteraction(java.util.Map<String, String> config) throws Throwable
		 internal EmbeddedInteraction( IDictionary<string, string> config ) : this( config, EphemeralFileSystemAbstraction::new )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: EmbeddedInteraction(java.util.Map<String, String> config, System.Func<org.neo4j.io.fs.FileSystemAbstraction> fileSystemSupplier) throws Throwable
		 internal EmbeddedInteraction( IDictionary<string, string> config, System.Func<FileSystemAbstraction> fileSystemSupplier )
		 {
			  TestEnterpriseGraphDatabaseFactory factory = new TestEnterpriseGraphDatabaseFactory();
			  factory.FileSystem = fileSystemSupplier();
			  GraphDatabaseBuilder builder = factory.NewImpermanentDatabaseBuilder();
			  this._fileSystem = factory.FileSystem;
			  Init( builder, config );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public EmbeddedInteraction(org.neo4j.graphdb.factory.GraphDatabaseBuilder builder, java.util.Map<String, String> config) throws Throwable
		 public EmbeddedInteraction( GraphDatabaseBuilder builder, IDictionary<string, string> config )
		 {
			  Init( builder, config );
		 }

		 protected internal EmbeddedInteraction()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void init(org.neo4j.graphdb.factory.GraphDatabaseBuilder builder, java.util.Map<String, String> config) throws Throwable
		 protected internal virtual void Init( GraphDatabaseBuilder builder, IDictionary<string, string> config )
		 {
			  builder.SetConfig( ( new BoltConnector( "bolt" ) ).type, "BOLT" );
			  builder.SetConfig( ( new BoltConnector( "bolt" ) ).enabled, "true" );
			  builder.setConfig( ( new BoltConnector( "bolt" ) ).encryption_level, OPTIONAL.name() );
			  builder.SetConfig( ( new BoltConnector( "bolt" ) ).listen_address, "localhost:0" );
			  builder.setConfig( LegacySslPolicyConfig.tls_key_file, NeoInteractionLevel.tempPath( "key", ".key" ) );
			  builder.setConfig( LegacySslPolicyConfig.tls_certificate_file, NeoInteractionLevel.tempPath( "cert", ".cert" ) );
			  builder.SetConfig( GraphDatabaseSettings.auth_enabled, "true" );

			  builder.Config = config;

			  Db = ( GraphDatabaseFacade ) builder.NewGraphDatabase();
			  _authManager = Db.DependencyResolver.resolveDependency( typeof( EnterpriseAuthManager ) );
			  _connectorRegister = Db.DependencyResolver.resolveDependency( typeof( ConnectorPortRegister ) );
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
				  throw new Exception( "The configuration used does not have a user manager" );
			 }
		 }

		 public virtual GraphDatabaseFacade LocalGraph
		 {
			 get
			 {
				  return Db;
			 }
		 }

		 public override FileSystemAbstraction FileSystem()
		 {
			  return _fileSystem;
		 }

		 public override InternalTransaction BeginLocalTransactionAsUser( EnterpriseLoginContext loginContext, KernelTransaction.Type txType )
		 {
			  return Db.beginTransaction( txType, loginContext );
		 }

		 public override string ExecuteQuery( EnterpriseLoginContext loginContext, string call, IDictionary<string, object> @params, System.Action<ResourceIterator<IDictionary<string, object>>> resultConsumer )
		 {
			  try
			  {
					  using ( InternalTransaction tx = Db.beginTransaction( KernelTransaction.Type.@implicit, loginContext ) )
					  {
						IDictionary<string, object> p = ( @params == null ) ? Collections.emptyMap() : @params;
						resultConsumer( Db.execute( call, p ) );
						tx.Success();
						return "";
					  }
			  }
			  catch ( Exception e )
			  {
					return e.Message;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.kernel.enterprise.api.security.EnterpriseLoginContext login(String username, String password) throws Exception
		 public override EnterpriseLoginContext Login( string username, string password )
		 {
			  return _authManager.login( authToken( username, password ) );
		 }

		 public override void Logout( EnterpriseLoginContext loginContext )
		 {
			  loginContext.Subject().logout();
		 }

		 public override void UpdateAuthToken( EnterpriseLoginContext subject, string username, string password )
		 {
		 }

		 public override string NameOf( EnterpriseLoginContext loginContext )
		 {
			  return loginContext.Subject().username();
		 }

		 public override void TearDown()
		 {
			  Db.shutdown();
		 }

		 public override void AssertAuthenticated( EnterpriseLoginContext loginContext )
		 {
			  assertThat( loginContext.Subject().AuthenticationResult, equalTo(AuthenticationResult.SUCCESS) );
		 }

		 public override void AssertPasswordChangeRequired( EnterpriseLoginContext loginContext )
		 {
			  assertThat( loginContext.Subject().AuthenticationResult, equalTo(AuthenticationResult.PASSWORD_CHANGE_REQUIRED) );
		 }

		 public override void AssertInitFailed( EnterpriseLoginContext loginContext )
		 {
			  assertThat( loginContext.Subject().AuthenticationResult, equalTo(AuthenticationResult.FAILURE) );
		 }

		 public override void AssertSessionKilled( EnterpriseLoginContext loginContext )
		 {
			  // There is no session that could have been killed
		 }

		 public virtual string ConnectionProtocol
		 {
			 get
			 {
				  return "embedded";
			 }
		 }

		 public override HostnamePort LookupConnector( string connectorKey )
		 {
			  return _connectorRegister.getLocalAddress( connectorKey );
		 }
	}

}