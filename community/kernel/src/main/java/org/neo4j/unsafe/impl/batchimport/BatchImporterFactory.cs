﻿using System.Collections.Generic;

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
namespace Org.Neo4j.@unsafe.Impl.Batchimport
{

	using Service = Org.Neo4j.Helpers.Service;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Org.Neo4j.Io.layout.DatabaseLayout;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using RecordFormats = Org.Neo4j.Kernel.impl.store.format.RecordFormats;
	using LogService = Org.Neo4j.Logging.@internal.LogService;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;
	using ExecutionMonitor = Org.Neo4j.@unsafe.Impl.Batchimport.staging.ExecutionMonitor;

	public abstract class BatchImporterFactory : Service
	{
		 private readonly int _priority;

		 protected internal BatchImporterFactory( string key, int priority ) : base( key )
		 {
			  this._priority = priority;
		 }

		 public abstract BatchImporter Instantiate( DatabaseLayout directoryStructure, FileSystemAbstraction fileSystem, PageCache externalPageCache, Configuration config, LogService logService, ExecutionMonitor executionMonitor, AdditionalInitialIds additionalInitialIds, Config dbConfig, RecordFormats recordFormats, ImportLogic.Monitor monitor, JobScheduler jobScheduler );

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