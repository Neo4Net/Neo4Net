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

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Settings.PATH;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Settings.derivedSetting;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Settings.pathSetting;

	/// <summary>
	/// To be removed in favour of <seealso cref="SslPolicyConfig"/>. The settings below are still
	/// incorporated in a backwards compatible manner, under the "legacy" policy name.
	/// </summary>
	[Description("Legacy SSL policy settings")]
	public class LegacySslPolicyConfig : LoadableConfig
	{
		 public const string LEGACY_POLICY_NAME = "legacy";

		 [Description("Directory for storing certificates to be used by Neo4Net for TLS connections")]
		 public static readonly Setting<File> CertificatesDirectory = pathSetting( "dbms.directories.certificates", "certificates" );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Internal @Description("Path to the X.509 public certificate to be used by Neo4Net for TLS connections") public static final org.Neo4Net.graphdb.config.Setting<java.io.File> tls_certificate_file = derivedSetting("unsupported.dbms.security.tls_certificate_file", certificates_directory, certificates -> new java.io.File(certificates, "Neo4Net.cert"), PATH);
		 [Description("Path to the X.509 public certificate to be used by Neo4Net for TLS connections")]
		 public static readonly Setting<File> TlsCertificateFile = derivedSetting( "unsupported.dbms.security.tls_certificate_file", CertificatesDirectory, certificates => new File( certificates, "Neo4Net.cert" ), PATH );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Internal @Description("Path to the X.509 private key to be used by Neo4Net for TLS connections") public static final org.Neo4Net.graphdb.config.Setting<java.io.File> tls_key_file = derivedSetting("unsupported.dbms.security.tls_key_file", certificates_directory, certificates -> new java.io.File(certificates, "Neo4Net.key"), PATH);
		 [Description("Path to the X.509 private key to be used by Neo4Net for TLS connections")]
		 public static readonly Setting<File> TlsKeyFile = derivedSetting( "unsupported.dbms.security.tls_key_file", CertificatesDirectory, certificates => new File( certificates, "Neo4Net.key" ), PATH );
	}

}