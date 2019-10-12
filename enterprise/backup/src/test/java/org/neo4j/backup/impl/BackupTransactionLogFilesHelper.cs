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
namespace Org.Neo4j.backup.impl
{

	using DefaultFileSystemAbstraction = Org.Neo4j.Io.fs.DefaultFileSystemAbstraction;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Org.Neo4j.Io.layout.DatabaseLayout;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using ConfigurableStandalonePageCacheFactory = Org.Neo4j.Kernel.impl.pagecache.ConfigurableStandalonePageCacheFactory;
	using LogFiles = Org.Neo4j.Kernel.impl.transaction.log.files.LogFiles;
	using LogFilesBuilder = Org.Neo4j.Kernel.impl.transaction.log.files.LogFilesBuilder;
	using ThreadPoolJobScheduler = Org.Neo4j.Scheduler.ThreadPoolJobScheduler;

	internal class BackupTransactionLogFilesHelper
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: static org.neo4j.kernel.impl.transaction.log.files.LogFiles readLogFiles(org.neo4j.io.layout.DatabaseLayout databaseLayout) throws java.io.IOException
		 internal static LogFiles ReadLogFiles( DatabaseLayout databaseLayout )
		 {
			  FileSystemAbstraction fileSystemAbstraction = new DefaultFileSystemAbstraction();
			  PageCache pageCache = ConfigurableStandalonePageCacheFactory.createPageCache( fileSystemAbstraction, new ThreadPoolJobScheduler() );
			  return LogFilesBuilder.activeFilesBuilder( databaseLayout, fileSystemAbstraction, pageCache ).build();
		 }
	}

}