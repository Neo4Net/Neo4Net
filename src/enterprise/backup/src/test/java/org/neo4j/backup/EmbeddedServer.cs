﻿/*
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
namespace Neo4Net.backup
{

	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using GraphDatabaseBuilder = Neo4Net.Graphdb.factory.GraphDatabaseBuilder;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using Settings = Neo4Net.Kernel.configuration.Settings;
	using OnlineBackupSettings = Neo4Net.Kernel.impl.enterprise.configuration.OnlineBackupSettings;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;

	public class EmbeddedServer : ServerInterface
	{
		 private GraphDatabaseService _db;

		 public EmbeddedServer( File storeDir, string serverAddress )
		 {
			  GraphDatabaseBuilder graphDatabaseBuilder = ( new TestGraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(storeDir);
			  graphDatabaseBuilder.SetConfig( OnlineBackupSettings.online_backup_enabled, Settings.TRUE );
			  graphDatabaseBuilder.SetConfig( OnlineBackupSettings.online_backup_server, serverAddress );
			  graphDatabaseBuilder.SetConfig( GraphDatabaseSettings.pagecache_memory, "8m" );
			  this._db = graphDatabaseBuilder.NewGraphDatabase();
		 }

		 public override void Shutdown()
		 {
			  _db.shutdown();
		 }

		 public override void AwaitStarted()
		 {
		 }
	}

}