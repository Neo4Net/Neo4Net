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

	using Description = Neo4Net.Configuration.Description;
	using Internal = Neo4Net.Configuration.Internal;
	using LoadableConfig = Neo4Net.Configuration.LoadableConfig;
	using Neo4Net.GraphDb.config;
	using ClientAuth = Neo4Net.Ssl.ClientAuth;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Settings.BOOLEAN;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Settings.FALSE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Settings.NO_DEFAULT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Settings.PATH;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Settings.STRING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Settings.STRING_LIST;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Settings.derivedSetting;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Settings.optionsIgnoreCase;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Settings.pathSetting;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Settings.setting;

	[Group("dbms.ssl.policy")]
	public class SslPolicyConfig : LoadableConfig
	{
		 public static readonly IList<string> TlsVersionDefaults = singletonList( "TLSv1.2" );
		 public const IList<string> CIPHER_SUITES_DEFAULTS = null;

		 [Description("The mandatory base directory for cryptographic objects of this policy." + " It is also possible to override each individual configuration with absolute paths.")]
		 public readonly Setting<File> BaseDirectory;

		 [Description("Allows the generation of a private key and associated self-signed certificate." + " Only performed when both objects cannot be found.")]
		 public readonly Setting<bool> AllowKeyGeneration;

		 [Description("Makes this policy trust all remote parties." + " Enabling this is not recommended and the trusted directory will be ignored.")]
		 public readonly Setting<bool> TrustAll;

		 [Description("Private PKCS#8 key in PEM format.")]
		 public readonly Setting<File> PrivateKey;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Internal @Description("The password for the private key.") public final org.Neo4Net.graphdb.config.Setting<String> private_key_password;
		 [Description("The password for the private key.")]
		 public readonly Setting<string> PrivateKeyPassword;

		 [Description("X.509 certificate (chain) of this server in PEM format.")]
		 public readonly Setting<File> PublicCertificate;

		 [Description("Path to directory of X.509 certificates in PEM format for trusted parties.")]
		 public readonly Setting<File> TrustedDir;

		 [Description("Path to directory of CRLs (Certificate Revocation Lists) in PEM format.")]
		 public readonly Setting<File> RevokedDir;

		 [Description("Client authentication stance.")]
		 public readonly Setting<ClientAuth> ClientAuth;

		 [Description("Restrict allowed TLS protocol versions.")]
		 public readonly Setting<IList<string>> TlsVersions;

		 [Description("Restrict allowed ciphers.")]
		 public readonly Setting<IList<string>> Ciphers;

		 [Description("When true, this node will verify the hostname of every other instance it connects to by comparing the address it used to connect with it " + "and the patterns described in the remote hosts public certificate Subject Alternative Names")]
		 public readonly Setting<bool> VerifyHostname;

		 public SslPolicyConfig() : this("<policyname>")
		 {
		 }

		 public SslPolicyConfig( string policyName )
		 {
			  GroupSettingSupport group = new GroupSettingSupport( typeof( SslPolicyConfig ), policyName );

			  this.BaseDirectory = group.Scope( pathSetting( "base_directory", NO_DEFAULT ) );
			  this.AllowKeyGeneration = group.Scope( setting( "allow_key_generation", BOOLEAN, FALSE ) );
			  this.TrustAll = group.Scope( setting( "trust_all", BOOLEAN, FALSE ) );

			  this.PrivateKey = group.Scope( DerivedDefault( "private_key", BaseDirectory, "private.key" ) );
			  this.PublicCertificate = group.Scope( DerivedDefault( "public_certificate", BaseDirectory, "public.crt" ) );
			  this.TrustedDir = group.Scope( DerivedDefault( "trusted_dir", BaseDirectory, "trusted" ) );
			  this.RevokedDir = group.Scope( DerivedDefault( "revoked_dir", BaseDirectory, "revoked" ) );

			  this.PrivateKeyPassword = group.Scope( setting( "private_key_password", STRING, NO_DEFAULT ) );
			  this.ClientAuth = group.Scope( setting( "client_auth", optionsIgnoreCase( typeof( ClientAuth ) ), ClientAuth.REQUIRE.name() ) );
			  this.TlsVersions = group.Scope( setting( "tls_versions", STRING_LIST, JoinList( TlsVersionDefaults ) ) );
			  this.Ciphers = group.Scope( setting( "ciphers", STRING_LIST, JoinList( CIPHER_SUITES_DEFAULTS ) ) );
			  this.VerifyHostname = group.Scope( setting( "verify_hostname", BOOLEAN, FALSE ) );
		 }

		 // TODO: can we make this handle relative paths?
		 private Setting<File> DerivedDefault( string settingName, Setting<File> baseDirectory, string defaultFilename )
		 {
			  return derivedSetting( settingName, baseDirectory, @base => new File( @base, defaultFilename ), PATH );
		 }

		 private string JoinList( IList<string> list )
		 {
			  if ( list == null )
			  {
					return null;
			  }
			  else
			  {
					return join( ",", list );
			  }
		 }
	}

}