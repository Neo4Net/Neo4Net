/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
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
namespace Neo4Net.Ssl
{
	using OperatorCreationException = org.bouncycastle.@operator.OperatorCreationException;


	using Config = Neo4Net.Kernel.configuration.Config;
	using SslPolicyConfig = Neo4Net.Kernel.configuration.ssl.SslPolicyConfig;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

	public class HostnameVerificationHelper
	{
		 public const string POLICY_NAME = "fakePolicy";
		 public static readonly SslPolicyConfig SslPolicyConfig = new SslPolicyConfig( POLICY_NAME );
		 private static readonly PkiUtils _pkiUtils = new PkiUtils();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static org.neo4j.kernel.configuration.Config aConfig(String hostname, org.neo4j.test.rule.TestDirectory testDirectory) throws java.security.GeneralSecurityException, java.io.IOException, org.bouncycastle.operator.OperatorCreationException
		 public static Config AConfig( string hostname, TestDirectory testDirectory )
		 {
			  string random = System.Guid.randomUUID().ToString();
			  File baseDirectory = testDirectory.Directory( "base_directory_" + random );
			  File validCertificatePath = new File( baseDirectory, "certificate.crt" );
			  File validPrivateKeyPath = new File( baseDirectory, "private.pem" );
			  File revoked = new File( baseDirectory, "revoked" );
			  File trusted = new File( baseDirectory, "trusted" );
			  trusted.mkdirs();
			  revoked.mkdirs();
			  _pkiUtils.createSelfSignedCertificate( validCertificatePath, validPrivateKeyPath, hostname ); // Sets Subject Alternative Name(s) to hostname
			  return Config.builder().withSetting(SslPolicyConfig.base_directory, baseDirectory.ToString()).withSetting(SslPolicyConfig.trusted_dir, trusted.ToString()).withSetting(SslPolicyConfig.revoked_dir, revoked.ToString()).withSetting(SslPolicyConfig.private_key, validPrivateKeyPath.ToString()).withSetting(SslPolicyConfig.public_certificate, validCertificatePath.ToString()).withSetting(SslPolicyConfig.tls_versions, "TLSv1.2").withSetting(SslPolicyConfig.ciphers, "TLS_ECDHE_RSA_WITH_AES_128_CBC_SHA").withSetting(SslPolicyConfig.client_auth, "none").withSetting(SslPolicyConfig.allow_key_generation, "false").withSetting(SslPolicyConfig.trust_all, "false").withSetting(SslPolicyConfig.verify_hostname, "true").build();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void trust(org.neo4j.kernel.configuration.Config target, org.neo4j.kernel.configuration.Config subject) throws java.io.IOException
		 public static void Trust( Config target, Config subject )
		 {
			  SslPolicyConfig sslPolicyConfig = new SslPolicyConfig( POLICY_NAME );
			  File trustedDirectory = target.Get( sslPolicyConfig.TrustedDir );
			  File certificate = subject.Get( sslPolicyConfig.PublicCertificate );
			  Path trustedCertFilePath = trustedDirectory.toPath().resolve(certificate.Name);
			  Files.copy( certificate.toPath(), trustedCertFilePath );
		 }
	}

}