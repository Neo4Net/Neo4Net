﻿using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.helper
{

	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using OnlineBackupSettings = Neo4Net.Kernel.impl.enterprise.configuration.OnlineBackupSettings;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Settings.TRUE;

	public class DatabaseConfiguration
	{
		 private DatabaseConfiguration()
		 {
			  // no instances
		 }

		 public static IDictionary<string, string> ConfigureTxLogRotationAndPruning( IDictionary<string, string> settings, string txPrune )
		 {
			  settings[GraphDatabaseSettings.keep_logical_logs.name()] = txPrune;
			  settings[GraphDatabaseSettings.logical_log_rotation_threshold.name()] = "1M";
			  return settings;
		 }

		 public static IDictionary<string, string> ConfigureBackup( IDictionary<string, string> settings, string hostname, int port )
		 {
			  settings[OnlineBackupSettings.online_backup_enabled.name()] = TRUE;
			  settings[OnlineBackupSettings.online_backup_server.name()] = hostname + ":" + port;
			  return settings;
		 }
	}

}