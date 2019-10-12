using System;
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
namespace Org.Neo4j.Ssl
{
	using Channel = io.netty.channel.Channel;
	using ChannelHandler = io.netty.channel.ChannelHandler;
	using SslContext = io.netty.handler.ssl.SslContext;
	using SslContextBuilder = io.netty.handler.ssl.SslContextBuilder;
	using SslHandler = io.netty.handler.ssl.SslHandler;
	using SslProvider = io.netty.handler.ssl.SslProvider;


	using Log = Org.Neo4j.Logging.Log;
	using LogProvider = Org.Neo4j.Logging.LogProvider;

	public class SslPolicy
	{
		 /* cryptographic objects */
		 private readonly PrivateKey _privateKey;
		 private readonly X509Certificate[] _keyCertChain;

		 /* cryptographic parameters */
		 private readonly IList<string> _ciphers;
		 private readonly string[] _tlsVersions;
		 private readonly ClientAuth _clientAuth;

		 private readonly TrustManagerFactory _trustManagerFactory;
		 private readonly SslProvider _sslProvider;

		 private readonly bool _verifyHostname;
		 private readonly Log _log;

		 public SslPolicy( PrivateKey privateKey, X509Certificate[] keyCertChain, IList<string> tlsVersions, IList<string> ciphers, ClientAuth clientAuth, TrustManagerFactory trustManagerFactory, SslProvider sslProvider, bool verifyHostname, LogProvider logProvider )
		 {
			  this._privateKey = privateKey;
			  this._keyCertChain = keyCertChain;
			  this._tlsVersions = tlsVersions == null ? null : tlsVersions.ToArray();
			  this._ciphers = ciphers;
			  this._clientAuth = clientAuth;
			  this._trustManagerFactory = trustManagerFactory;
			  this._sslProvider = sslProvider;
			  this._verifyHostname = verifyHostname;
			  this._log = logProvider.GetLog( typeof( SslPolicy ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public io.netty.handler.ssl.SslContext nettyServerContext() throws javax.net.ssl.SSLException
		 public virtual SslContext NettyServerContext()
		 {
			  return SslContextBuilder.forServer( _privateKey, _keyCertChain ).sslProvider( _sslProvider ).clientAuth( ForNetty( _clientAuth ) ).protocols( _tlsVersions ).ciphers( _ciphers ).trustManager( _trustManagerFactory ).build();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public io.netty.handler.ssl.SslContext nettyClientContext() throws javax.net.ssl.SSLException
		 public virtual SslContext NettyClientContext()
		 {
			  return SslContextBuilder.forClient().sslProvider(_sslProvider).keyManager(_privateKey, _keyCertChain).protocols(_tlsVersions).ciphers(_ciphers).trustManager(_trustManagerFactory).build();
		 }

		 private io.netty.handler.ssl.ClientAuth ForNetty( ClientAuth clientAuth )
		 {
			  switch ( clientAuth )
			  {
			  case Org.Neo4j.Ssl.ClientAuth.None:
					return io.netty.handler.ssl.ClientAuth.NONE;
			  case Org.Neo4j.Ssl.ClientAuth.Optional:
					return io.netty.handler.ssl.ClientAuth.OPTIONAL;
			  case Org.Neo4j.Ssl.ClientAuth.Require:
					return io.netty.handler.ssl.ClientAuth.REQUIRE;
			  default:
					throw new System.ArgumentException( "Cannot translate to netty equivalent: " + clientAuth );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") public io.netty.channel.ChannelHandler nettyServerHandler(io.netty.channel.Channel channel) throws javax.net.ssl.SSLException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual ChannelHandler NettyServerHandler( Channel channel )
		 {
			  return NettyServerHandler( channel, NettyServerContext() );
		 }

		 private ChannelHandler NettyServerHandler( Channel channel, SslContext sslContext )
		 {
			  SSLEngine sslEngine = sslContext.newEngine( channel.alloc() );
			  return new SslHandler( sslEngine );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") public io.netty.channel.ChannelHandler nettyClientHandler(io.netty.channel.Channel channel) throws javax.net.ssl.SSLException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual ChannelHandler NettyClientHandler( Channel channel )
		 {
			  return NettyClientHandler( channel, NettyClientContext() );
		 }

		 internal virtual ChannelHandler NettyClientHandler( Channel channel, SslContext sslContext )
		 {
			  return new ClientSideOnConnectSslHandler( channel, sslContext, _verifyHostname, _tlsVersions );
		 }

		 public virtual PrivateKey PrivateKey()
		 {
			  return _privateKey;
		 }

		 public virtual X509Certificate[] CertificateChain()
		 {
			  return _keyCertChain;
		 }

		 public virtual KeyStore GetKeyStore( char[] keyStorePass, char[] privateKeyPass )
		 {
			  KeyStore keyStore;
			  try
			  {
					keyStore = KeyStore.getInstance( KeyStore.DefaultType );
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
					_log.debug( "Keystore loaded is of type " + keyStore.GetType().FullName );
					keyStore.load( null, keyStorePass );
					keyStore.setKeyEntry( "key", _privateKey, privateKeyPass, _keyCertChain );
			  }
			  catch ( Exception e )
			  {
					throw new Exception( e );
			  }

			  return keyStore;
		 }

		 public virtual TrustManagerFactory TrustManagerFactory
		 {
			 get
			 {
				  return _trustManagerFactory;
			 }
		 }

		 public virtual IList<string> CipherSuites
		 {
			 get
			 {
				  return _ciphers;
			 }
		 }

		 public virtual string[] TlsVersions
		 {
			 get
			 {
				  return _tlsVersions;
			 }
		 }

		 public virtual ClientAuth ClientAuth
		 {
			 get
			 {
				  return _clientAuth;
			 }
		 }

		 public virtual bool VerifyHostname
		 {
			 get
			 {
				  return _verifyHostname;
			 }
		 }

		 public override string ToString()
		 {
			  return "SslPolicy{" +
						"keyCertChain=" + DescribeCertChain() +
						", ciphers=" + _ciphers +
						", tlsVersions=" + Arrays.ToString( _tlsVersions ) +
						", clientAuth=" + _clientAuth +
						'}';
		 }

		 private string DescribeCertificate( X509Certificate certificate )
		 {
			  return "Subject: " + certificate.SubjectDN +
						", Issuer: " + certificate.IssuerDN;
		 }

		 private string DescribeCertChain()
		 {
			  IList<string> certificates = java.util.keyCertChain.Select( this.describeCertificate ).ToList();
			  return string.join( ", ", certificates );
		 }
	}

}