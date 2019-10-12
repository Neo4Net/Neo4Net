using System;

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
namespace Org.Neo4j.causalclustering.stresstests
{

	using CopiedStoreRecovery = Org.Neo4j.causalclustering.catchup.storecopy.CopiedStoreRecovery;
	using TemporaryStoreDirectory = Org.Neo4j.causalclustering.catchup.storecopy.TemporaryStoreDirectory;
	using ConsistencyCheckService = Org.Neo4j.Consistency.ConsistencyCheckService;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using StandalonePageCacheFactory = Org.Neo4j.Io.pagecache.impl.muninn.StandalonePageCacheFactory;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;
	using ThreadPoolJobScheduler = Org.Neo4j.Scheduler.ThreadPoolJobScheduler;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.consistency.ConsistencyCheckTool.runConsistencyCheckTool;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.facade.GraphDatabaseDependencies.newDependencies;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.NullOutputStream.NULL_OUTPUT_STREAM;

	internal class ConsistencyHelper
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: static void assertStoreConsistent(org.neo4j.io.fs.FileSystemAbstraction fs, java.io.File storeDir) throws Exception
		 internal static void AssertStoreConsistent( FileSystemAbstraction fs, File storeDir )
		 {
			  File parent = storeDir.ParentFile;
			  using ( JobScheduler jobScheduler = new ThreadPoolJobScheduler(), PageCache pageCache = StandalonePageCacheFactory.createPageCache(fs, jobScheduler), TemporaryStoreDirectory tempStore = new TemporaryStoreDirectory(fs, pageCache, parent) )
			  {
					fs.CopyRecursively( storeDir, tempStore.StoreDir() );

					( new CopiedStoreRecovery( Config.defaults(), newDependencies().kernelExtensions(), pageCache ) ).recoverCopiedStore(tempStore.DatabaseLayout());

					ConsistencyCheckService.Result result = runConsistencyCheckTool( new string[]{ storeDir.AbsolutePath }, new PrintStream( NULL_OUTPUT_STREAM ), new PrintStream( NULL_OUTPUT_STREAM ) );

					if ( !result.Successful )
					{
						 throw new Exception( "Not consistent database in " + storeDir );
					}
			  }
		 }
	}

}