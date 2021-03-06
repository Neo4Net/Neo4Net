﻿/*
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
namespace Org.Neo4j.Server.enterprise
{

	using Description = Org.Neo4j.Configuration.Description;
	using DocumentedDefaultValue = Org.Neo4j.Configuration.DocumentedDefaultValue;
	using Internal = Org.Neo4j.Configuration.Internal;
	using LoadableConfig = Org.Neo4j.Configuration.LoadableConfig;
	using Org.Neo4j.Graphdb.config;
	using Settings = Org.Neo4j.Kernel.configuration.Settings;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.BOOLEAN;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.DURATION;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.TRUE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.setting;

	[Description("Settings available in the Enterprise server")]
	public class EnterpriseServerSettings : LoadableConfig
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") @Description("Configure the Neo4j Browser to time out logged in users after this idle period. " + "Setting this to 0 indicates no limit.") public static final org.neo4j.graphdb.config.Setting<java.time.Duration> browser_credentialTimeout = setting("browser.credential_timeout", DURATION, "0");
		 [Description("Configure the Neo4j Browser to time out logged in users after this idle period. " + "Setting this to 0 indicates no limit.")]
		 public static readonly Setting<Duration> BrowserCredentialTimeout = setting( "browser.credential_timeout", DURATION, "0" );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") @Description("Configure the Neo4j Browser to store or not store user credentials.") public static final org.neo4j.graphdb.config.Setting<bool> browser_retainConnectionCredentials = setting("browser.retain_connection_credentials", BOOLEAN, TRUE);
		 [Description("Configure the Neo4j Browser to store or not store user credentials.")]
		 public static readonly Setting<bool> BrowserRetainConnectionCredentials = setting( "browser.retain_connection_credentials", BOOLEAN, TRUE );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") @Description("Configure the policy for outgoing Neo4j Browser connections.") public static final org.neo4j.graphdb.config.Setting<bool> browser_allowOutgoingBrowserConnections = setting("browser.allow_outgoing_connections", BOOLEAN, TRUE);
		 [Description("Configure the policy for outgoing Neo4j Browser connections.")]
		 public static readonly Setting<bool> BrowserAllowOutgoingBrowserConnections = setting( "browser.allow_outgoing_connections", BOOLEAN, TRUE );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Internal @Description("Publicly discoverable bolt+routing:// URI to use for Neo4j Drivers wanting to access a cluster " + "that this instance is a member of. Only applicable to causal clusters.") @DocumentedDefaultValue("Defaults to empty on any deployment that is not a causal cluster core, and a " + "bolt+routing://-schemed URI of the advertised address of the first found bolt connector if the " + "instance is a core member of a causal cluster.") public static final org.neo4j.graphdb.config.Setting<java.net.URI> bolt_routing_discoverable_address = setting("unsupported.dbms.discoverable_bolt_routing_address", org.neo4j.kernel.configuration.Settings.URI, "");
		 [Description("Publicly discoverable bolt+routing:// URI to use for Neo4j Drivers wanting to access a cluster " + "that this instance is a member of. Only applicable to causal clusters."), DocumentedDefaultValue("Defaults to empty on any deployment that is not a causal cluster core, and a " + "bolt+routing://-schemed URI of the advertised address of the first found bolt connector if the " + "instance is a core member of a causal cluster.")]
		 public static readonly Setting<URI> BoltRoutingDiscoverableAddress = setting( "unsupported.dbms.discoverable_bolt_routing_address", Settings.URI, "" );

	}

}