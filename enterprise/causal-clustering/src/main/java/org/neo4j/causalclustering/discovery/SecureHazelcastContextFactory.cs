using System;

/*
 * Copyright (c) 2002-2018 "Neo Technology,"
 * Network Engine for Objects in Lund AB [http://neotechnology.com]
 *
 * Modifications Copyright (c) 2019 "GraphFoundation" [https://graphfoundation.org]
 *
 * You can redistribute this software and/or modify
 * it under the terms of the GNU Affero General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program. If not, see <http://www.gnu.org/licenses/>.
 *
 * Modified from https://github.com/neo4j/neo4j/blob/3.3/enterprise/causal-clustering/src/main/java/org/neo4j/causalclustering/discovery/HazelcastSslContextFactory.java
 */

namespace Org.Neo4j.causalclustering.discovery
{
	using SSLContextFactory = com.hazelcast.nio.ssl.SSLContextFactory;


	using Log = Org.Neo4j.Logging.Log;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using SslPolicy = Org.Neo4j.Ssl.SslPolicy;

	//import java.security.*;

	internal class SecureHazelcastContextFactory : SSLContextFactory
	{
		 private const string PROTOCOL = "TLS";
		 private readonly SslPolicy _sslPolicy;
		 private readonly Log _log;

		 /// <param name="sslPolicy"> </param>
		 /// <param name="logProvider"> </param>
		 internal SecureHazelcastContextFactory( SslPolicy sslPolicy, LogProvider logProvider )
		 {
			  this._sslPolicy = sslPolicy;
			  this._log = logProvider.getLog( this.GetType() );
		 }

		 /// <param name="properties"> </param>
		 public virtual void Init( Properties properties )
		 {
		 }

		 public override SSLContext SSLContext
		 {
			 get
			 {
				  SSLContext sslCtx;
   
				  try
				  {
						sslCtx = SSLContext.getInstance( PROTOCOL );
				  }
				  catch ( NoSuchAlgorithmException e )
				  {
						throw new Exception( e );
				  }
   
				  KeyManagerFactory keyManagerFactory;
				  try
				  {
						keyManagerFactory = KeyManagerFactory.getInstance( KeyManagerFactory.DefaultAlgorithm );
				  }
				  catch ( NoSuchAlgorithmException e )
				  {
						throw new Exception( e );
				  }
   
				  SecureRandom rand = new SecureRandom();
				  char[] password = new char[32];
				  for ( int i = 0; i < password.Length; i++ )
				  {
						password[i] = ( char ) rand.Next( char.MaxValue + 1 );
				  }
   
				  try
				  {
						KeyStore keyStore = _sslPolicy.getKeyStore( password, password );
						keyManagerFactory.init( keyStore, password );
				  }
				  catch ( Exception e ) when ( e is KeyStoreException || e is NoSuchAlgorithmException || e is UnrecoverableKeyException )
				  {
						throw new Exception( e );
				  }
				  finally
				  {
						for ( int i = 0; i < password.Length; i++ )
						{
							 password[i] = ( char )0;
						}
				  }
   
				  KeyManager[] keyManagers = keyManagerFactory.KeyManagers;
				  TrustManager[] trustManagers = _sslPolicy.TrustManagerFactory.TrustManagers;
   
				  try
				  {
						sslCtx.init( keyManagers, trustManagers, null );
				  }
				  catch ( KeyManagementException e )
				  {
						throw new Exception( e );
				  }
   
				  if ( _sslPolicy.TlsVersions != null )
				  {
						_log.warn( format( "Restricting TLS versions through policy not supported." + " System defaults for %s family will be used.", PROTOCOL ) );
				  }
   
				  if ( _sslPolicy.CipherSuites != null )
				  {
						_log.warn( "Restricting ciphers through policy not supported." + " System defaults will be used." );
				  }
   
				  return sslCtx;
			 }
		 }
	}

}