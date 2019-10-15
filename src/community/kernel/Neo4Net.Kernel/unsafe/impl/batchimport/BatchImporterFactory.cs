using System.Collections.Generic;

/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
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
namespace Neo4Net.@unsafe.Impl.Batchimport
{

	using Service = Neo4Net.Helpers.Service;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using Config = Neo4Net.Kernel.configuration.Config;
	using RecordFormats = Neo4Net.Kernel.impl.store.format.RecordFormats;
	using LogService = Neo4Net.Logging.Internal.LogService;
	using IJobScheduler = Neo4Net.Scheduler.JobScheduler;
	using ExecutionMonitor = Neo4Net.@unsafe.Impl.Batchimport.staging.ExecutionMonitor;

	public abstract class BatchImporterFactory : Service
	{
		 private readonly int _priority;

		 protected internal BatchImporterFactory( string key, int priority ) : base( key )
		 {
			  this._priority = priority;
		 }

		 public abstract BatchImporter Instantiate( DatabaseLayout directoryStructure, FileSystemAbstraction fileSystem, PageCache externalPageCache, Configuration config, LogService logService, ExecutionMonitor executionMonitor, AdditionalInitialIds additionalInitialIds, Config dbConfig, RecordFormats recordFormats, ImportLogic.Monitor monitor, IJobScheduler jobScheduler );

		 public static BatchImporterFactory WithHighestPriority()
		 {
			  IEnumerable<BatchImporterFactory> candidates = Service.load( typeof( BatchImporterFactory ) );
			  BatchImporterFactory highestPrioritized = null;
			  foreach ( BatchImporterFactory candidate in candidates )
			  {
					if ( highestPrioritized == null || candidate._priority > highestPrioritized._priority )
					{
						 highestPrioritized = candidate;
					}
			  }
			  if ( highestPrioritized == null )
			  {
					throw new NoSuchElementException( "No batch importers found" );
			  }
			  return highestPrioritized;
		 }
	}

}