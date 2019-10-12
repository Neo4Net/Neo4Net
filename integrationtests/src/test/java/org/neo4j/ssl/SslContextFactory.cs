using System.Collections.Generic;

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
namespace Org.Neo4j.Ssl
{
	using SslProvider = io.netty.handler.ssl.SslProvider;


	using Config = Org.Neo4j.Kernel.configuration.Config;
	using SslPolicyConfig = Org.Neo4j.Kernel.configuration.ssl.SslPolicyConfig;
	using SslPolicyLoader = Org.Neo4j.Kernel.configuration.ssl.SslPolicyLoader;
	using SslSystemSettings = Org.Neo4j.Kernel.configuration.ssl.SslSystemSettings;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;

	public class SslContextFactory
	{
		 public interface Ciphers
		 {
			  SslParameters Ciphers( params string[] ciphers );
		 }

		 public class SslParameters : Ciphers
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal string ProtocolsConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal string CiphersConflict;

			  internal SslParameters( string protocols, string ciphers )
			  {
					this.ProtocolsConflict = protocols;
					this.CiphersConflict = ciphers;
			  }

			  public static Ciphers Protocols( params string[] protocols )
			  {
					return new SslParameters( JoinOrNull( protocols ), null );
			  }

			  public override SslParameters Ciphers( params string[] ciphers )
			  {
					this.CiphersConflict = JoinOrNull( ciphers );
					return this;
			  }

			  /// <summary>
			  /// The low-level frameworks use null to signify that defaults shall be used, and so does our SSL framework.
			  /// </summary>
			  internal static string JoinOrNull( string[] parts )
			  {
					return parts.Length > 0 ? string.join( ",", parts ) : null;
			  }

			  public override string ToString()
			  {
					return "SslParameters{" + "protocols='" + ProtocolsConflict + '\'' + ", ciphers='" + CiphersConflict + '\'' + '}';
			  }
		 }

		 public static SslPolicy MakeSslPolicy( SslResource sslResource, SslParameters @params )
		 {
			  return MakeSslPolicy( sslResource, SslProvider.JDK, @params.ProtocolsConflict, @params.CiphersConflict );
		 }

		 public static SslPolicy MakeSslPolicy( SslResource sslResource, SslProvider sslProvider )
		 {
			  return MakeSslPolicy( sslResource, sslProvider, null, null );
		 }

		 public static SslPolicy MakeSslPolicy( SslResource sslResource )
		 {
			  return MakeSslPolicy( sslResource, SslProvider.JDK, null, null );
		 }

		 public static SslPolicy MakeSslPolicy( SslResource sslResource, SslProvider sslProvider, string protocols, string ciphers )
		 {
			  IDictionary<string, string> config = new Dictionary<string, string>();
			  config[SslSystemSettings.netty_ssl_provider.name()] = sslProvider.name();

			  SslPolicyConfig policyConfig = new SslPolicyConfig( "default" );
			  File baseDirectory = sslResource.PrivateKey().ParentFile;
			  ( new File( baseDirectory, "trusted" ) ).mkdirs();
			  ( new File( baseDirectory, "revoked" ) ).mkdirs();

			  config[policyConfig.BaseDirectory.name()] = baseDirectory.Path;
			  config[policyConfig.PrivateKey.name()] = sslResource.PrivateKey().Path;
			  config[policyConfig.PublicCertificate.name()] = sslResource.PublicCertificate().Path;
			  config[policyConfig.TrustedDir.name()] = sslResource.TrustedDirectory().Path;
			  config[policyConfig.RevokedDir.name()] = sslResource.RevokedDirectory().Path;
			  config[policyConfig.VerifyHostname.name()] = "false";

			  if ( !string.ReferenceEquals( protocols, null ) )
			  {
					config[policyConfig.TlsVersions.name()] = protocols;
			  }

			  if ( !string.ReferenceEquals( ciphers, null ) )
			  {
					config[policyConfig.Ciphers.name()] = ciphers;
			  }

			  SslPolicyLoader sslPolicyFactory = SslPolicyLoader.create( Config.fromSettings( config ).build(), NullLogProvider.Instance );

			  return sslPolicyFactory.GetPolicy( "default" );
		 }
	}

}