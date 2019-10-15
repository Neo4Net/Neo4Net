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

	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using IJobScheduler = Neo4Net.Scheduler.JobScheduler;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.backup.impl.BackupPageCacheContainer.of;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.pagecache.ConfigurableStandalonePageCacheFactory.createPageCache;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.scheduler.JobSchedulerFactory.createInitializedScheduler;
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
			  IJobScheduler scheduler = createInitializedScheduler();
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