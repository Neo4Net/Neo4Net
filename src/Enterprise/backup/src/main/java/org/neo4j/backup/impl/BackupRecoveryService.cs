using System.Collections.Generic;

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
namespace Neo4Net.backup.impl
{

	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using FileUtils = Neo4Net.Io.fs.FileUtils;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Settings = Neo4Net.Kernel.configuration.Settings;
	using GraphDatabaseAPI = Neo4Net.Kernel.@internal.GraphDatabaseAPI;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.backup.impl.BackupProtocolService.startTemporaryDb;

	internal class BackupRecoveryService
	{
		 internal virtual void RecoverWithDatabase( Path databaseDirectory, PageCache pageCache, Config config )
		 {
			  DatabaseLayout databaseLayout = DatabaseLayout.of( databaseDirectory.toFile() );
			  IDictionary<string, string> configParams = config.Raw;
			  configParams[GraphDatabaseSettings.logical_logs_location.name()] = databaseDirectory.ToString();
			  configParams[GraphDatabaseSettings.active_database.name()] = databaseLayout.DatabaseName;
			  configParams[GraphDatabaseSettings.pagecache_warmup_enabled.name()] = Settings.FALSE;
			  GraphDatabaseAPI targetDb = startTemporaryDb( databaseLayout.DatabaseDirectory(), pageCache, configParams );
			  targetDb.Shutdown();
			  // as soon as recovery will be extracted we will not gonna need this
			  File lockFile = databaseLayout.StoreLayout.storeLockFile();
			  if ( lockFile.exists() )
			  {
					FileUtils.deleteFile( lockFile );
			  }
		 }
	}

}