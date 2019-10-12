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
namespace Neo4Net.Kernel.Impl.Api.index
{

	using Test = org.junit.Test;


	using OnDemandJobScheduler = Neo4Net.Test.OnDemandJobScheduler;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.empty;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.hasSize;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class IndexPopulationJobControllerTest
	{
		private bool InstanceFieldsInitialized = false;

		public IndexPopulationJobControllerTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_jobController = new IndexPopulationJobController( _executer );
		}


		 private readonly OnDemandJobScheduler _executer = new OnDemandJobScheduler();
		 private IndexPopulationJobController _jobController;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void trackPopulationJobs()
		 public virtual void TrackPopulationJobs()
		 {
			  assertThat( _jobController.PopulationJobs, @is( empty() ) );

			  IndexPopulationJob populationJob = mock( typeof( IndexPopulationJob ) );
			  _jobController.startIndexPopulation( populationJob );
			  assertThat( _jobController.PopulationJobs, hasSize( 1 ) );

			  IndexPopulationJob populationJob2 = mock( typeof( IndexPopulationJob ) );
			  _jobController.startIndexPopulation( populationJob2 );
			  assertThat( _jobController.PopulationJobs, hasSize( 2 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void stopOngoingPopulationJobs() throws java.util.concurrent.ExecutionException, InterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void StopOngoingPopulationJobs()
		 {
			  IndexPopulationJob populationJob = IndexPopulationJob;
			  IndexPopulationJob populationJob2 = IndexPopulationJob;
			  _jobController.startIndexPopulation( populationJob );
			  _jobController.startIndexPopulation( populationJob2 );

			  _jobController.stop();

			  verify( populationJob ).cancel();
			  verify( populationJob2 ).cancel();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void untrackFinishedPopulations()
		 public virtual void UntrackFinishedPopulations()
		 {
			  IndexPopulationJob populationJob = IndexPopulationJob;
			  _jobController.startIndexPopulation( populationJob );

			  assertThat( _jobController.PopulationJobs, hasSize( 1 ) );

			  _executer.runJob();

			  assertThat( _jobController.PopulationJobs, hasSize( 0 ) );
			  verify( populationJob ).run();
		 }

		 private IndexPopulationJob IndexPopulationJob
		 {
			 get
			 {
				  IndexPopulationJob populationJob = mock( typeof( IndexPopulationJob ) );
				  when( populationJob.Cancel() ).thenReturn(CompletableFuture.completedFuture(null));
				  return populationJob;
			 }
		 }
	}

}