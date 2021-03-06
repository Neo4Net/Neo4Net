﻿using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

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
namespace Org.Neo4j.Kernel.Impl.Api.index
{

	using Group = Org.Neo4j.Scheduler.Group;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;

	internal class IndexPopulationJobController
	{
		 private readonly ISet<IndexPopulationJob> _populationJobs = Collections.newSetFromMap( new ConcurrentDictionary<IndexPopulationJob>() );
		 private readonly JobScheduler _scheduler;

		 internal IndexPopulationJobController( JobScheduler scheduler )
		 {
			  this._scheduler = scheduler;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void stop() throws java.util.concurrent.ExecutionException, InterruptedException
		 internal virtual void Stop()
		 {
			  foreach ( IndexPopulationJob job in _populationJobs )
			  {
					job.Cancel().get();
			  }
		 }

		 internal virtual void StartIndexPopulation( IndexPopulationJob job )
		 {
			  _populationJobs.Add( job );
			  _scheduler.schedule( Group.INDEX_POPULATION, new IndexPopulationJobWrapper( job, this ) );
		 }

		 internal virtual void IndexPopulationCompleted( IndexPopulationJob populationJob )
		 {
			  _populationJobs.remove( populationJob );
		 }

		 internal virtual ISet<IndexPopulationJob> PopulationJobs
		 {
			 get
			 {
				  return _populationJobs;
			 }
		 }

		 private class IndexPopulationJobWrapper : ThreadStart
		 {
			  internal readonly IndexPopulationJob IndexPopulationJob;
			  internal readonly IndexPopulationJobController JobController;

			  internal IndexPopulationJobWrapper( IndexPopulationJob indexPopulationJob, IndexPopulationJobController jobController )
			  {
					this.IndexPopulationJob = indexPopulationJob;
					this.JobController = jobController;
			  }

			  public override void Run()
			  {
					try
					{
						 IndexPopulationJob.run();
					}
					finally
					{
						 JobController.indexPopulationCompleted( IndexPopulationJob );
					}
			  }
		 }
	}

}