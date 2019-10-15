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

	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using Config = Neo4Net.Kernel.configuration.Config;
	using RecordFormats = Neo4Net.Kernel.impl.store.format.RecordFormats;
	using LogService = Neo4Net.Logging.Internal.LogService;
	using IJobScheduler = Neo4Net.Scheduler.JobScheduler;
	using Input = Neo4Net.@unsafe.Impl.Batchimport.input.Input;
	using ExecutionMonitor = Neo4Net.@unsafe.Impl.Batchimport.staging.ExecutionMonitor;
	using BatchingNeoStores = Neo4Net.@unsafe.Impl.Batchimport.store.BatchingNeoStores;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.ImportLogic.instantiateNeoStores;

	/// <summary>
	/// <seealso cref="BatchImporter"/> which tries to exercise as much of the available resources to gain performance.
	/// Or rather ensure that the slowest resource (usually I/O) is fully saturated and that enough work is
	/// being performed to keep that slowest resource saturated all the time.
	/// <para>
	/// Overall goals: split up processing cost by parallelizing. Keep CPUs busy, keep I/O busy and writing sequentially.
	/// I/O is only allowed to be read to and written from sequentially, any random access drastically reduces performance.
	/// Goes through multiple stages where each stage has one or more steps executing in parallel, passing
	/// batches between these steps through each stage, i.e. passing batches downstream.
	/// </para>
	/// </summary>
	public class ParallelBatchImporter : BatchImporter
	{
		 private readonly PageCache _externalPageCache;
		 private readonly DatabaseLayout _directoryStructure;
		 private readonly FileSystemAbstraction _fileSystem;
		 private readonly Configuration _config;
		 private readonly LogService _logService;
		 private readonly Config _dbConfig;
		 private readonly RecordFormats _recordFormats;
		 private readonly ExecutionMonitor _executionMonitor;
		 private readonly AdditionalInitialIds _additionalInitialIds;
		 private readonly ImportLogic.Monitor _monitor;
		 private readonly IJobScheduler _jobScheduler;

		 public ParallelBatchImporter( DatabaseLayout directoryStructure, FileSystemAbstraction fileSystem, PageCache externalPageCache, Configuration config, LogService logService, ExecutionMonitor executionMonitor, AdditionalInitialIds additionalInitialIds, Config dbConfig, RecordFormats recordFormats, ImportLogic.Monitor monitor, IJobScheduler jobScheduler )
		 {
			  this._externalPageCache = externalPageCache;
			  this._directoryStructure = directoryStructure;
			  this._fileSystem = fileSystem;
			  this._config = config;
			  this._logService = logService;
			  this._dbConfig = dbConfig;
			  this._recordFormats = recordFormats;
			  this._executionMonitor = executionMonitor;
			  this._additionalInitialIds = additionalInitialIds;
			  this._monitor = monitor;
			  this._jobScheduler = jobScheduler;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void doImport(org.neo4j.unsafe.impl.batchimport.input.Input input) throws java.io.IOException
		 public override void DoImport( Input input )
		 {
			  using ( BatchingNeoStores store = instantiateNeoStores( _fileSystem, _directoryStructure.databaseDirectory(), _externalPageCache, _recordFormats, _config, _logService, _additionalInitialIds, _dbConfig, _jobScheduler ), ImportLogic logic = new ImportLogic(_directoryStructure.databaseDirectory(), _fileSystem, store, _config, _logService, _executionMonitor, _recordFormats, _monitor) )
			  {
					store.CreateNew();
					logic.Initialize( input );

					logic.ImportNodes();
					logic.PrepareIdMapper();
					logic.ImportRelationships();
					logic.CalculateNodeDegrees();
					logic.LinkRelationshipsOfAllTypes();
					logic.DefragmentRelationshipGroups();
					logic.BuildCountsStore();

					logic.Success();
			  }
		 }
	}

}