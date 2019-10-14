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
namespace Neo4Net.Kernel.impl.enterprise.configuration
{
	using Description = Neo4Net.Configuration.Description;
	using LoadableConfig = Neo4Net.Configuration.LoadableConfig;
	using Neo4Net.Graphdb.config;
	using HostnamePort = Neo4Net.Helpers.HostnamePort;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.BOOLEAN;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.HOSTNAME_PORT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.NO_DEFAULT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.STRING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.TRUE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.prefixSetting;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.setting;

	/// <summary>
	/// Settings for online backup
	/// </summary>
	[Description("Online backup configuration settings")]
	public class OnlineBackupSettings : LoadableConfig
	{
		 [Description("Enable support for running online backups")]
		 public static readonly Setting<bool> OnlineBackupEnabled = setting( "dbms.backup.enabled", BOOLEAN, TRUE );

		 [Description("Listening server for online backups. The protocol running varies depending on deployment. In a Causal Clustering environment this is the " + "same protocol that runs on causal_clustering.transaction_listen_address. The port range is only respected in a HA or single instance deployment." + " In Causal Clustering a single port should be used")]
		 public static readonly Setting<HostnamePort> OnlineBackupServer = setting( "dbms.backup.address", HOSTNAME_PORT, "127.0.0.1:6362-6372" );

		 [Description("Name of the SSL policy to be used by backup, as defined under the dbms.ssl.policy.* settings." + " If no policy is configured then the communication will not be secured.")]
		 public static readonly Setting<string> SslPolicy = prefixSetting( "dbms.backup.ssl_policy", STRING, NO_DEFAULT );
	}

}