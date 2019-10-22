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
namespace Neo4Net.Kernel.configuration.ssl
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using ClientAuth = Neo4Net.Ssl.ClientAuth;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.MapUtil.stringMap;

	public class SslPolicyConfigTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.Neo4Net.test.rule.TestDirectory testDirectory = org.Neo4Net.test.rule.TestDirectory.testDirectory();
		 public TestDirectory TestDirectory = TestDirectory.testDirectory();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFindPolicyDefaults()
		 public virtual void ShouldFindPolicyDefaults()
		 {
			  // given
			  IDictionary<string, string> @params = stringMap();

			  string policyName = "XYZ";
			  SslPolicyConfig policyConfig = new SslPolicyConfig( policyName );

			  File homeDir = TestDirectory.directory( "home" );

			  @params[GraphDatabaseSettings.Neo4Net_home.name()] = homeDir.AbsolutePath;
			  @params[policyConfig.BaseDirectory.name()] = "certificates/XYZ";
			  Config config = Config.defaults( @params );

			  // derived defaults
			  File privateKey = new File( homeDir, "certificates/XYZ/private.key" );
			  File publicCertificate = new File( homeDir, "certificates/XYZ/public.crt" );
			  File trustedDir = new File( homeDir, "certificates/XYZ/trusted" );
			  File revokedDir = new File( homeDir, "certificates/XYZ/revoked" );

			  // when
			  File privateKeyFromConfig = config.Get( policyConfig.PrivateKey );
			  File publicCertificateFromConfig = config.Get( policyConfig.PublicCertificate );
			  File trustedDirFromConfig = config.Get( policyConfig.TrustedDir );
			  File revokedDirFromConfig = config.Get( policyConfig.RevokedDir );
			  string privateKeyPassword = config.Get( policyConfig.PrivateKeyPassword );
			  bool allowKeyGeneration = config.Get( policyConfig.AllowKeyGeneration );
			  bool trustAll = config.Get( policyConfig.TrustAll );
			  IList<string> tlsVersions = config.Get( policyConfig.TlsVersions );
			  IList<string> ciphers = config.Get( policyConfig.Ciphers );
			  ClientAuth clientAuth = config.Get( policyConfig.ClientAuth );

			  // then
			  assertEquals( privateKey, privateKeyFromConfig );
			  assertEquals( publicCertificate, publicCertificateFromConfig );
			  assertEquals( trustedDir, trustedDirFromConfig );
			  assertEquals( revokedDir, revokedDirFromConfig );
			  assertNull( privateKeyPassword );
			  assertFalse( allowKeyGeneration );
			  assertFalse( trustAll );
			  assertEquals( singletonList( "TLSv1.2" ), tlsVersions );
			  assertNull( ciphers );
			  assertEquals( ClientAuth.REQUIRE, clientAuth );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFindPolicyOverrides()
		 public virtual void ShouldFindPolicyOverrides()
		 {
			  // given
			  IDictionary<string, string> @params = stringMap();

			  string policyName = "XYZ";
			  SslPolicyConfig policyConfig = new SslPolicyConfig( policyName );

			  File homeDir = TestDirectory.directory( "home" );

			  @params[GraphDatabaseSettings.Neo4Net_home.name()] = homeDir.AbsolutePath;
			  @params[policyConfig.BaseDirectory.name()] = "certificates/XYZ";

			  File privateKey = TestDirectory.directory( "/path/to/my.key" );
			  File publicCertificate = TestDirectory.directory( "/path/to/my.crt" );
			  File trustedDir = TestDirectory.directory( "/some/other/path/to/trusted" );
			  File revokedDir = TestDirectory.directory( "/some/other/path/to/revoked" );

			  @params[policyConfig.PrivateKey.name()] = privateKey.AbsolutePath;
			  @params[policyConfig.PublicCertificate.name()] = publicCertificate.AbsolutePath;
			  @params[policyConfig.TrustedDir.name()] = trustedDir.AbsolutePath;
			  @params[policyConfig.RevokedDir.name()] = revokedDir.AbsolutePath;

			  @params[policyConfig.AllowKeyGeneration.name()] = "true";
			  @params[policyConfig.TrustAll.name()] = "true";

			  @params[policyConfig.PrivateKeyPassword.name()] = "setecastronomy";
			  @params[policyConfig.TlsVersions.name()] = "TLSv1.1,TLSv1.2";
			  @params[policyConfig.Ciphers.name()] = "TLS_ECDHE_ECDSA_WITH_AES_256_CBC_SHA384,TLS_ECDHE_ECDSA_WITH_AES_256_GCM_SHA384";
			  @params[policyConfig.ClientAuth.name()] = "optional";

			  Config config = Config.defaults( @params );

			  // when
			  File privateKeyFromConfig = config.Get( policyConfig.PrivateKey );
			  File publicCertificateFromConfig = config.Get( policyConfig.PublicCertificate );
			  File trustedDirFromConfig = config.Get( policyConfig.TrustedDir );
			  File revokedDirFromConfig = config.Get( policyConfig.RevokedDir );

			  string privateKeyPassword = config.Get( policyConfig.PrivateKeyPassword );
			  bool allowKeyGeneration = config.Get( policyConfig.AllowKeyGeneration );
			  bool trustAll = config.Get( policyConfig.TrustAll );
			  IList<string> tlsVersions = config.Get( policyConfig.TlsVersions );
			  IList<string> ciphers = config.Get( policyConfig.Ciphers );
			  ClientAuth clientAuth = config.Get( policyConfig.ClientAuth );

			  // then
			  assertEquals( privateKey, privateKeyFromConfig );
			  assertEquals( publicCertificate, publicCertificateFromConfig );
			  assertEquals( trustedDir, trustedDirFromConfig );
			  assertEquals( revokedDir, revokedDirFromConfig );

			  assertTrue( allowKeyGeneration );
			  assertTrue( trustAll );
			  assertEquals( "setecastronomy", privateKeyPassword );
			  assertEquals( asList( "TLSv1.1", "TLSv1.2" ), tlsVersions );
			  assertEquals( asList( "TLS_ECDHE_ECDSA_WITH_AES_256_CBC_SHA384", "TLS_ECDHE_ECDSA_WITH_AES_256_GCM_SHA384" ), ciphers );
			  assertEquals( ClientAuth.OPTIONAL, clientAuth );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailWithIncompletePathOverrides()
		 public virtual void ShouldFailWithIncompletePathOverrides()
		 {
			  // given
			  IDictionary<string, string> @params = stringMap();

			  string policyName = "XYZ";
			  SslPolicyConfig policyConfig = new SslPolicyConfig( policyName );

			  File homeDir = TestDirectory.directory( "home" );

			  @params[GraphDatabaseSettings.Neo4Net_home.name()] = homeDir.AbsolutePath;
			  @params[policyConfig.BaseDirectory.name()] = "certificates";

			  @params[policyConfig.PrivateKey.name()] = "my.key";
			  @params[policyConfig.PublicCertificate.name()] = "path/to/my.crt";

			  Config config = Config.defaults( @params );

			  // when/then
			  try
			  {
					config.Get( policyConfig.PrivateKey );
					fail();
			  }
			  catch ( System.ArgumentException )
			  {
					// expected
			  }

			  try
			  {
					config.Get( policyConfig.PublicCertificate );
					fail();
			  }
			  catch ( System.ArgumentException )
			  {
					// expected
			  }
		 }
	}

}