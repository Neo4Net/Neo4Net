using System;

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
namespace Neo4Net.Ssl
{
	using ByteBuf = io.netty.buffer.ByteBuf;
	using ByteBufAllocator = io.netty.buffer.ByteBufAllocator;
	using OperatorCreationException = org.bouncycastle.@operator.OperatorCreationException;
	using IsCollectionContaining = org.hamcrest.core.IsCollectionContaining;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Config = Neo4Net.Kernel.configuration.Config;
	using SslPolicyLoader = Neo4Net.Kernel.configuration.ssl.SslPolicyLoader;
	using FormattedLogProvider = Neo4Net.Logging.FormattedLogProvider;
	using Level = Neo4Net.Logging.Level;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.ssl.HostnameVerificationHelper.POLICY_NAME;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.ssl.HostnameVerificationHelper.aConfig;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.ssl.HostnameVerificationHelper.trust;

	public class SslPolicyLoaderIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public Neo4Net.test.rule.TestDirectory testDirectory = Neo4Net.test.rule.TestDirectory.testDirectory();
		 public TestDirectory TestDirectory = TestDirectory.testDirectory();

		 private static readonly LogProvider _logProvider = FormattedLogProvider.withDefaultLogLevel( Level.DEBUG ).toOutputStream( System.out );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void certificatesWithInvalidCommonNameAreRejected() throws java.security.GeneralSecurityException, java.io.IOException, org.bouncycastle.operator.OperatorCreationException, InterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CertificatesWithInvalidCommonNameAreRejected()
		 {
			  // given server has a certificate that matches an invalid hostname
			  Config serverConfig = aConfig( "invalid-not-localhost", TestDirectory );

			  // and client has any certificate (valid), since hostname validation is done from the client side
			  Config clientConfig = aConfig( "localhost", TestDirectory );

			  trust( serverConfig, clientConfig );
			  trust( clientConfig, serverConfig );

			  // and setup
			  SslPolicy serverPolicy = SslPolicyLoader.create( serverConfig, _logProvider ).getPolicy( POLICY_NAME );
			  SslPolicy clientPolicy = SslPolicyLoader.create( clientConfig, _logProvider ).getPolicy( POLICY_NAME );
			  SecureServer secureServer = new SecureServer( serverPolicy );
			  secureServer.Start();
			  int port = secureServer.Port();
			  SecureClient secureClient = new SecureClient( clientPolicy );

			  // when client connects to server with a non-matching hostname
			  try
			  {
					secureClient.Connect( port );

					// then handshake complete with exception describing hostname mismatch
					secureClient.SslHandshakeFuture().get(1, MINUTES);
			  }
			  catch ( ExecutionException e )
			  {
					string expectedMessage = "No subject alternative DNS name matching localhost found.";
					assertThat( Causes( e ).map( Exception.getMessage ).collect( Collectors.toList() ), IsCollectionContaining.hasItem(expectedMessage) );
			  }
			  catch ( TimeoutException e )
			  {
					Console.WriteLine( e.ToString() );
					Console.Write( e.StackTrace );
			  }
			  finally
			  {
					secureServer.Stop();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void normalBehaviourIfServerCertificateMatchesClientExpectation() throws java.security.GeneralSecurityException, java.io.IOException, org.bouncycastle.operator.OperatorCreationException, InterruptedException, java.util.concurrent.TimeoutException, java.util.concurrent.ExecutionException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void NormalBehaviourIfServerCertificateMatchesClientExpectation()
		 {
			  // given server has valid hostname
			  Config serverConfig = aConfig( "localhost", TestDirectory );

			  // and client has invalid hostname (which is irrelevant for hostname verification)
			  Config clientConfig = aConfig( "invalid-localhost", TestDirectory );

			  trust( serverConfig, clientConfig );
			  trust( clientConfig, serverConfig );

			  // and setup
			  SslPolicy serverPolicy = SslPolicyLoader.create( serverConfig, _logProvider ).getPolicy( POLICY_NAME );
			  SslPolicy clientPolicy = SslPolicyLoader.create( clientConfig, _logProvider ).getPolicy( POLICY_NAME );
			  SecureServer secureServer = new SecureServer( serverPolicy );
			  secureServer.Start();
			  SecureClient secureClient = new SecureClient( clientPolicy );

			  // then
			  ClientCanCommunicateWithServer( secureClient, secureServer );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void legacyPolicyDoesNotHaveHostnameVerification() throws java.security.GeneralSecurityException, java.io.IOException, org.bouncycastle.operator.OperatorCreationException, InterruptedException, java.util.concurrent.TimeoutException, java.util.concurrent.ExecutionException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void LegacyPolicyDoesNotHaveHostnameVerification()
		 {
			  // given server has an invalid hostname
			  Config serverConfig = aConfig( "invalid-localhost", TestDirectory );

			  // and client has invalid hostname (which is irrelevant for hostname verification)
			  Config clientConfig = aConfig( "invalid-localhost", TestDirectory );

			  trust( serverConfig, clientConfig );
			  trust( clientConfig, serverConfig );

			  // and setup
			  SslPolicy serverPolicy = SslPolicyLoader.create( serverConfig, _logProvider ).getPolicy( "legacy" );
			  SslPolicy clientPolicy = SslPolicyLoader.create( clientConfig, _logProvider ).getPolicy( "legacy" );
			  SecureServer secureServer = new SecureServer( serverPolicy );
			  secureServer.Start();
			  SecureClient secureClient = new SecureClient( clientPolicy );

			  // then
			  ClientCanCommunicateWithServer( secureClient, secureServer );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void clientCanCommunicateWithServer(SecureClient secureClient, SecureServer secureServer) throws InterruptedException, java.util.concurrent.TimeoutException, java.util.concurrent.ExecutionException
		 private void ClientCanCommunicateWithServer( SecureClient secureClient, SecureServer secureServer )
		 {
			  int port = secureServer.Port();
			  try
			  {
					secureClient.Connect( port );
					ByteBuf request = ByteBufAllocator.DEFAULT.buffer().writeBytes(new sbyte[]{ 1, 2, 3, 4 });
					secureClient.Channel().writeAndFlush(request);

					ByteBuf expected = ByteBufAllocator.DEFAULT.buffer().writeBytes(SecureServer.Response);
					assertTrue( secureClient.SslHandshakeFuture().get(1, MINUTES).Active );
					secureClient.AssertResponse( expected );
			  }
			  finally
			  {
					secureServer.Stop();
			  }
		 }

		 private Stream<Exception> Causes( Exception throwable )
		 {
			  Stream<Exception> thisStream = Stream.of( throwable ).filter( Objects.nonNull );
			  if ( throwable != null && throwable.InnerException != null )
			  {
					return Stream.concat( thisStream, Causes( throwable.InnerException ) );
			  }
			  else
			  {
					return thisStream;
			  }
		 }
	}

}