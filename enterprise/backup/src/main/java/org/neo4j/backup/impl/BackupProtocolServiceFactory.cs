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
namespace Org.Neo4j.backup.impl
{

	using DefaultFileSystemAbstraction = Org.Neo4j.Io.fs.DefaultFileSystemAbstraction;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.backup.impl.BackupPageCacheContainer.of;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.pagecache.ConfigurableStandalonePageCacheFactory.createPageCache;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.scheduler.JobSchedulerFactory.createInitialisedScheduler;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.logging.FormattedLogProvider.toOutputStream;

	public sealed class BackupProtocolServiceFactory
	{

		 private BackupProtocolServiceFactory()
		 {
		 }

		 public static BackupProtocolService BackupProtocolService()
		 {
			  return BackupProtocolService( System.out );
		 }

		 public static BackupProtocolService BackupProtocolService( Stream logDestination )
		 {
			  JobScheduler scheduler = createInitialisedScheduler();
			  PageCache pageCache = createPageCache( new DefaultFileSystemAbstraction(), scheduler );
			  BackupPageCacheContainer pageCacheContainer = of( pageCache, scheduler );
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  return BackupProtocolService( DefaultFileSystemAbstraction::new, toOutputStream( logDestination ), logDestination, new Monitors(), pageCacheContainer );
		 }

		 public static BackupProtocolService BackupProtocolService( System.Func<FileSystemAbstraction> fileSystemSupplier, LogProvider logProvider, Stream logDestination, Monitors monitors, PageCache pageCache )
		 {
			  return BackupProtocolService( fileSystemSupplier, logProvider, logDestination, monitors, of( pageCache ) );
		 }

		 private static BackupProtocolService BackupProtocolService( System.Func<FileSystemAbstraction> fileSystemSupplier, LogProvider logProvider, Stream logDestination, Monitors monitors, BackupPageCacheContainer pageCacheContainer )
		 {
			  requireNonNull( fileSystemSupplier );
			  requireNonNull( logProvider );
			  requireNonNull( logDestination );
			  requireNonNull( monitors );
			  requireNonNull( pageCacheContainer );
			  return new BackupProtocolService( fileSystemSupplier, logProvider, logDestination, monitors, pageCacheContainer );
		 }
	}

}