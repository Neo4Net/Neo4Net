﻿using System;
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
namespace Org.Neo4j.backup
{

	using ConfigOptions = Org.Neo4j.Configuration.ConfigOptions;
	using Description = Org.Neo4j.Configuration.Description;
	using LoadableConfig = Org.Neo4j.Configuration.LoadableConfig;
	using Org.Neo4j.Graphdb.config;
	using HostnamePort = Org.Neo4j.Helpers.HostnamePort;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.BOOLEAN;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.HOSTNAME_PORT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.TRUE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.setting;

	/// <summary>
	/// Settings for online backup
	/// </summary>
	[Description("Online backup configuration settings"), Obsolete]
	public class OnlineBackupSettings : LoadableConfig
	{
		 [Description("Enable support for running online backups"), Obsolete]
		 public static readonly Setting<bool> OnlineBackupEnabled = setting( "dbms.backup.enabled", BOOLEAN, TRUE );

		 [Description("Listening server for online backups"), Obsolete]
		 public static readonly Setting<HostnamePort> OnlineBackupServer = setting( "dbms.backup.address", HOSTNAME_PORT, "127.0.0.1:6362-6372" );

		 public virtual IList<ConfigOptions> ConfigOptions
		 {
			 get
			 {
				  return new List<ConfigOptions>();
			 }
		 }
	}

}