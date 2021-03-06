﻿/*
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
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Org.Neo4j.Io.layout.DatabaseLayout;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using RecordFormats = Org.Neo4j.Kernel.impl.store.format.RecordFormats;
	using LogService = Org.Neo4j.Logging.@internal.LogService;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;
	using ExecutionMonitor = Org.Neo4j.@unsafe.Impl.Batchimport.staging.ExecutionMonitor;

	public class StandardBatchImporterFactory : BatchImporterFactory
	{
		 public const string NAME = "standard";

		 public StandardBatchImporterFactory() : base(NAME, 1)
		 {
		 }

		 public override BatchImporter Instantiate( DatabaseLayout directoryStructure, FileSystemAbstraction fileSystem, PageCache externalPageCache, Configuration config, LogService logService, ExecutionMonitor executionMonitor, AdditionalInitialIds additionalInitialIds, Config dbConfig, RecordFormats recordFormats, ImportLogic.Monitor monitor, JobScheduler scheduler )
		 {
			  return new ParallelBatchImporter( directoryStructure, fileSystem, externalPageCache, config, logService, executionMonitor, additionalInitialIds, dbConfig, recordFormats, monitor, scheduler );
		 }
	}

}