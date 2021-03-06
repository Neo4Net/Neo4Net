﻿using System;

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
namespace Org.Neo4j.Kernel.configuration
{

	using Description = Org.Neo4j.Configuration.Description;
	using Internal = Org.Neo4j.Configuration.Internal;
	using ReplacedBy = Org.Neo4j.Configuration.ReplacedBy;
	using Org.Neo4j.Graphdb.config;
	using AdvertisedSocketAddress = Org.Neo4j.Helpers.AdvertisedSocketAddress;
	using ListenSocketAddress = Org.Neo4j.Helpers.ListenSocketAddress;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.BoltConnector.EncryptionLevel.OPTIONAL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.DURATION;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.INTEGER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.advertisedAddress;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.legacyFallback;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.listenAddress;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.optionsObeyCase;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.setting;

	[Description("Configuration options for Bolt connectors. " + "\"(bolt-connector-key)\" is a placeholder for a unique name for the connector, for instance " + "\"bolt-public\" or some other name that describes what the connector is for.")]
	public class BoltConnector : Connector
	{
		 [Description("Encryption level to require this connector to use")]
		 public readonly Setting<EncryptionLevel> EncryptionLevel;

		 [Description("Address the connector should bind to. " + "This setting is deprecated and will be replaced by `+listen_address+`"), Obsolete, ReplacedBy("dbms.connector.X.listen_address")]
		 public readonly Setting<ListenSocketAddress> Address;

		 [Description("Address the connector should bind to")]
		 public readonly Setting<ListenSocketAddress> ListenAddress;

		 [Description("Advertised address for this connector")]
		 public readonly Setting<AdvertisedSocketAddress> AdvertisedAddress;

		 [Description("The number of threads to keep in the thread pool bound to this connector, even if they are idle.")]
		 public readonly Setting<int> ThreadPoolMinSize;

		 [Description("The maximum number of threads allowed in the thread pool bound to this connector.")]
		 public readonly Setting<int> ThreadPoolMaxSize;

		 [Description("The maximum time an idle thread in the thread pool bound to this connector will wait for new tasks.")]
		 public readonly Setting<Duration> ThreadPoolKeepAlive;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("The queue size of the thread pool bound to this connector (-1 for unbounded, 0 for direct handoff, > 0 for bounded)") @Internal public final org.neo4j.graphdb.config.Setting<int> unsupported_thread_pool_queue_size;
		 [Description("The queue size of the thread pool bound to this connector (-1 for unbounded, 0 for direct handoff, > 0 for bounded)")]
		 public readonly Setting<int> UnsupportedThreadPoolQueueSize;

		 // Used by config doc generator
		 public BoltConnector() : this("(bolt-connector-key)")
		 {
		 }

		 public BoltConnector( string key ) : base( key )
		 {
			  EncryptionLevel = Group.scope( Settings.Setting( "tls_level", optionsObeyCase( typeof( EncryptionLevel ) ), OPTIONAL.name() ) );
			  Setting<ListenSocketAddress> legacyAddressSetting = listenAddress( "address", 7687 );
			  Setting<ListenSocketAddress> listenAddressSetting = legacyFallback( legacyAddressSetting, listenAddress( "listen_address", 7687 ) );

			  this.Address = Group.scope( legacyAddressSetting );
			  this.ListenAddress = Group.scope( listenAddressSetting );
			  this.AdvertisedAddress = Group.scope( advertisedAddress( "advertised_address", listenAddressSetting ) );
			  this.ThreadPoolMinSize = Group.scope( setting( "thread_pool_min_size", INTEGER, 5.ToString() ) );
			  this.ThreadPoolMaxSize = Group.scope( setting( "thread_pool_max_size", INTEGER, 400.ToString() ) );
			  this.ThreadPoolKeepAlive = Group.scope( setting( "thread_pool_keep_alive", DURATION, "5m" ) );
			  this.UnsupportedThreadPoolQueueSize = Group.scope( setting( "unsupported_thread_pool_queue_size", INTEGER, 0.ToString() ) );
		 }

		 public enum EncryptionLevel
		 {
			  Required,
			  Optional,
			  Disabled
		 }
	}

}