using System;
using System.Collections.Concurrent;
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
	using SslProvider = io.netty.handler.ssl.SslProvider;
	using InsecureTrustManagerFactory = io.netty.handler.ssl.util.InsecureTrustManagerFactory;
	using OperatorCreationException = org.bouncycastle.@operator.OperatorCreationException;


	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using ClientAuth = Neo4Net.Ssl.ClientAuth;
	using PkiUtils = Neo4Net.Ssl.PkiUtils;
	using SslPolicy = Neo4Net.Ssl.SslPolicy;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.default_advertised_address;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.ssl.LegacySslPolicyConfig.LEGACY_POLICY_NAME;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.ssl.SslPolicyConfig.CIPHER_SUITES_DEFAULTS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.ssl.SslPolicyConfig.TLS_VERSION_DEFAULTS;

	/// <summary>
	/// Each component which utilises SSL policies is recommended to provide a component
	/// specific override setting for the name of the SSL policy to use. It is also recommended
	/// that this setting allows null to be specified with the meaning that no SSL security shall
	/// be put into place. What this means practically is up to the component, but it could for
	/// example mean that the traffic will be plaintext over TCP in such case.
	/// </summary>
	/// <seealso cref= SslPolicyConfig </seealso>
	public class SslPolicyLoader
	{
		 private readonly IDictionary<string, SslPolicy> _policies = new ConcurrentDictionary<string, SslPolicy>();
		 private readonly PkiUtils _pkiUtils = new PkiUtils();
		 private readonly Config _config;
		 private readonly SslProvider _sslProvider;
		 private readonly LogProvider _logProvider;

		 private SslPolicy _legacyPolicy;

		 private SslPolicyLoader( Config config, LogProvider logProvider )
		 {
			  this._config = config;
			  this._sslProvider = config.Get( SslSystemSettings.NettySslProvider );
			  this._logProvider = logProvider;
		 }

		 /// <summary>
		 /// Loads all the SSL policies as defined by the config.
		 /// </summary>
		 /// <param name="config"> The configuration for the SSL policies. </param>
		 /// <returns> A factory populated with SSL policies. </returns>
		 public static SslPolicyLoader Create( Config config, LogProvider logProvider )
		 {
			  SslPolicyLoader policyFactory = new SslPolicyLoader( config, logProvider );
			  policyFactory.Load( config, logProvider.GetLog( typeof( SslPolicyLoader ) ) );
			  return policyFactory;
		 }

		 /// <summary>
		 /// Use this for retrieving the SSL policy configured under the specified name.
		 /// </summary>
		 /// <param name="policyName"> The name of the SSL policy to be returned. </param>
		 /// <returns> Returns the policy defined under the requested name. If the policy name is null
		 /// then the null policy will be returned which means that SSL will not be used. It is up
		 /// to each respective SSL policy using component to decide exactly what that means. </returns>
		 /// <exception cref="IllegalArgumentException"> If a policy with the supplied name does not exist. </exception>
		 public virtual SslPolicy GetPolicy( string policyName )
		 {
			  if ( string.ReferenceEquals( policyName, null ) )
			  {
					return null;
			  }
			  else if ( policyName.Equals( LEGACY_POLICY_NAME ) )
			  {
					return OrCreateLegacyPolicy;
			  }

			  SslPolicy sslPolicy = _policies[policyName];

			  if ( sslPolicy == null )
			  {
					throw new System.ArgumentException( format( "Cannot find enabled SSL policy with name '%s' in the configuration", policyName ) );
			  }
			  return sslPolicy;
		 }

		 private SslPolicy OrCreateLegacyPolicy
		 {
			 get
			 {
				 lock ( this )
				 {
					  if ( _legacyPolicy != null )
					  {
							return _legacyPolicy;
					  }
					  _legacyPolicy = LoadOrCreateLegacyPolicy();
					  return _legacyPolicy;
				 }
			 }
		 }

		 private SslPolicy LoadOrCreateLegacyPolicy()
		 {
			  File privateKeyFile = _config.get( LegacySslPolicyConfig.TlsKeyFile ).AbsoluteFile;
			  File certificateFile = _config.get( LegacySslPolicyConfig.TlsCertificateFile ).AbsoluteFile;

			  if ( !privateKeyFile.exists() && !certificateFile.exists() )
			  {
					string hostname = _config.get( default_advertised_address );

					try
					{
						 _pkiUtils.createSelfSignedCertificate( certificateFile, privateKeyFile, hostname );
					}
					catch ( Exception e )
					{
						 throw new Exception( "Failed to generate private key and certificate", e );
					}
			  }

			  PrivateKey privateKey = LoadPrivateKey( privateKeyFile, null );
			  X509Certificate[] keyCertChain = LoadCertificateChain( certificateFile );

			  return new SslPolicy( privateKey, keyCertChain, TLS_VERSION_DEFAULTS, CIPHER_SUITES_DEFAULTS, ClientAuth.NONE, InsecureTrustManagerFactory.INSTANCE, _sslProvider, false, _logProvider );
		 }

		 private void Load( Config config, Log log )
		 {
			  ISet<string> policyNames = config.IdentifiersFromGroup( typeof( SslPolicyConfig ) );

			  foreach ( string policyName in policyNames )
			  {
					if ( policyName.Equals( LEGACY_POLICY_NAME ) )
					{
						 // the legacy policy name is reserved for the legacy policy which derives its configuration from legacy settings
						 throw new System.ArgumentException( "Legacy policy cannot be configured. Please migrate to new SSL policy system." );
					}

					SslPolicyConfig policyConfig = new SslPolicyConfig( policyName );
					File baseDirectory = config.Get( policyConfig.BaseDirectory );
					File trustedCertificatesDir = config.Get( policyConfig.TrustedDir );
					File revokedCertificatesDir = config.Get( policyConfig.RevokedDir );

					if ( !baseDirectory.exists() )
					{
						 throw new System.ArgumentException( format( "Base directory '%s' for SSL policy with name '%s' does not exist.", baseDirectory, policyName ) );
					}

					bool allowKeyGeneration = config.Get( policyConfig.AllowKeyGeneration );

					File privateKeyFile = config.Get( policyConfig.PrivateKey );
					string privateKeyPassword = config.Get( policyConfig.PrivateKeyPassword );
					PrivateKey privateKey;

					X509Certificate[] keyCertChain;
					File keyCertChainFile = config.Get( policyConfig.PublicCertificate );

					if ( allowKeyGeneration && !privateKeyFile.exists() && !keyCertChainFile.exists() )
					{
						 GeneratePrivateKeyAndCertificate( log, policyName, keyCertChainFile, privateKeyFile, trustedCertificatesDir, revokedCertificatesDir );
					}

					privateKey = LoadPrivateKey( privateKeyFile, privateKeyPassword );
					keyCertChain = LoadCertificateChain( keyCertChainFile );

					ClientAuth clientAuth = config.Get( policyConfig.ClientAuth );
					bool trustAll = config.Get( policyConfig.TrustAll );
					bool verifyHostname = config.Get( policyConfig.VerifyHostname );
					TrustManagerFactory trustManagerFactory;

					ICollection<X509CRL> crls = GetCRLs( revokedCertificatesDir );

					try
					{
						 trustManagerFactory = CreateTrustManagerFactory( trustAll, trustedCertificatesDir, crls, clientAuth );
					}
					catch ( Exception e )
					{
						 throw new Exception( "Failed to create trust manager based on: " + trustedCertificatesDir, e );
					}

					IList<string> tlsVersions = config.Get( policyConfig.TlsVersions );
					IList<string> ciphers = config.Get( policyConfig.Ciphers );

					SslPolicy sslPolicy = new SslPolicy( privateKey, keyCertChain, tlsVersions, ciphers, clientAuth, trustManagerFactory, _sslProvider, verifyHostname, _logProvider );
					log.Info( format( "Loaded SSL policy '%s' = %s", policyName, sslPolicy ) );
					_policies[policyName] = sslPolicy;
			  }
		 }

		 private void GeneratePrivateKeyAndCertificate( Log log, string policyName, File keyCertChainFile, File privateKeyFile, File trustedCertificatesDir, File revokedCertificatesDir )
		 {
			  log.Info( format( "Generating key and self-signed certificate for SSL policy '%s'", policyName ) );
			  string hostname = _config.get( default_advertised_address );

			  try
			  {
					_pkiUtils.createSelfSignedCertificate( keyCertChainFile, privateKeyFile, hostname );

					trustedCertificatesDir.mkdir();
					revokedCertificatesDir.mkdir();
			  }
			  catch ( Exception e ) when ( e is GeneralSecurityException || e is IOException || e is OperatorCreationException )
			  {
					throw new Exception( "Failed to generate private key and certificate", e );
			  }
		 }

		 private ICollection<X509CRL> GetCRLs( File revokedCertificatesDir )
		 {
			  ICollection<X509CRL> crls = new List<X509CRL>();
			  File[] revocationFiles = revokedCertificatesDir.listFiles();

			  if ( revocationFiles == null )
			  {
					throw new Exception( format( "Could not find or list files in revoked directory: %s", revokedCertificatesDir ) );
			  }

			  if ( revocationFiles.Length == 0 )
			  {
					return crls;
			  }

			  CertificateFactory certificateFactory;

			  try
			  {
					certificateFactory = CertificateFactory.getInstance( PkiUtils.CERTIFICATE_TYPE );
			  }
			  catch ( CertificateException e )
			  {
					throw new Exception( "Could not generated certificate factory", e );
			  }

			  foreach ( File crl in revocationFiles )
			  {
					try
					{
							using ( Stream input = Files.newInputStream( crl.toPath() ) )
							{
							 crls.addAll( ( ICollection<X509CRL> ) certificateFactory.generateCRLs( input ) );
							}
					}
					catch ( Exception e ) when ( e is IOException || e is CRLException )
					{
						 throw new Exception( format( "Could not load CRL: %s", crl ), e );
					}
			  }

			  return crls;
		 }

		 private X509Certificate[] LoadCertificateChain( File keyCertChainFile )
		 {
			  try
			  {
					return _pkiUtils.loadCertificates( keyCertChainFile );
			  }
			  catch ( Exception e )
			  {
					throw new Exception( "Failed to load public certificate chain: " + keyCertChainFile, e );
			  }
		 }

		 private PrivateKey LoadPrivateKey( File privateKeyFile, string privateKeyPassword )
		 {
			  if ( !string.ReferenceEquals( privateKeyPassword, null ) )
			  {
					// TODO: Support loading of private keys with passwords.
					throw new System.NotSupportedException( "Loading private keys with passwords is not yet supported" );
			  }

			  try
			  {
					return _pkiUtils.loadPrivateKey( privateKeyFile );
			  }
			  catch ( Exception e )
			  {
					throw new Exception( "Failed to load private key: " + privateKeyFile + ( string.ReferenceEquals( privateKeyPassword, null ) ? "" : " (using configured password)" ), e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private javax.net.ssl.TrustManagerFactory createTrustManagerFactory(boolean trustAll, java.io.File trustedCertificatesDir, java.util.Collection<java.security.cert.X509CRL> crls, org.neo4j.ssl.ClientAuth clientAuth) throws Exception
		 private TrustManagerFactory CreateTrustManagerFactory( bool trustAll, File trustedCertificatesDir, ICollection<X509CRL> crls, ClientAuth clientAuth )
		 {
			  if ( trustAll )
			  {
					return InsecureTrustManagerFactory.INSTANCE;
			  }

			  KeyStore trustStore = KeyStore.getInstance( KeyStore.DefaultType );
			  trustStore.load( null, null );

			  File[] trustedCertFiles = trustedCertificatesDir.listFiles();

			  if ( trustedCertFiles == null )
			  {
					throw new Exception( format( "Could not find or list files in trusted directory: %s", trustedCertificatesDir ) );
			  }
			  else if ( clientAuth == ClientAuth.REQUIRE && trustedCertFiles.Length == 0 )
			  {
					throw new Exception( format( "Client auth is required but no trust anchors found in: %s", trustedCertificatesDir ) );
			  }

			  int i = 0;
			  foreach ( File trustedCertFile in trustedCertFiles )
			  {
					CertificateFactory certificateFactory = CertificateFactory.getInstance( PkiUtils.CERTIFICATE_TYPE );
					using ( Stream input = Files.newInputStream( trustedCertFile.toPath() ) )
					{
						 while ( input.available() > 0 )
						 {
							  try
							  {
									X509Certificate cert = ( X509Certificate ) certificateFactory.generateCertificate( input );
									trustStore.setCertificateEntry( Convert.ToString( i++ ), cert );
							  }
							  catch ( Exception e )
							  {
									throw new CertificateException( "Error loading certificate file: " + trustedCertFile, e );
							  }
						 }
					}
			  }

			  TrustManagerFactory trustManagerFactory = TrustManagerFactory.getInstance( TrustManagerFactory.DefaultAlgorithm );

			  if ( crls.Count > 0 )
			  {
					PKIXBuilderParameters pkixParamsBuilder = new PKIXBuilderParameters( trustStore, new X509CertSelector() );
					pkixParamsBuilder.RevocationEnabled = true;

					pkixParamsBuilder.addCertStore( CertStore.getInstance( "Collection", new CollectionCertStoreParameters( crls ) ) );

					trustManagerFactory.init( new CertPathTrustManagerParameters( pkixParamsBuilder ) );
			  }
			  else
			  {
					trustManagerFactory.init( trustStore );
			  }

			  return trustManagerFactory;
		 }
	}

}