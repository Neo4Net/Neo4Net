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
namespace Neo4Net.@unsafe.Impl.Batchimport.staging
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Extractors = Neo4Net.Csv.Reader.Extractors;
	using NullLogService = Neo4Net.Logging.Internal.NullLogService;
	using JobScheduler = Neo4Net.Scheduler.JobScheduler;
	using ThreadPoolJobScheduler = Neo4Net.Scheduler.ThreadPoolJobScheduler;
	using PageCacheAndDependenciesRule = Neo4Net.Test.rule.PageCacheAndDependenciesRule;
	using RandomRule = Neo4Net.Test.rule.RandomRule;
	using SuppressOutput = Neo4Net.Test.rule.SuppressOutput;
	using Collector = Neo4Net.@unsafe.Impl.Batchimport.input.Collector;
	using DataGeneratorInput = Neo4Net.@unsafe.Impl.Batchimport.input.DataGeneratorInput;
	using Input = Neo4Net.@unsafe.Impl.Batchimport.input.Input;
	using IdType = Neo4Net.@unsafe.Impl.Batchimport.input.csv.IdType;
	using ImportStage = Neo4Net.@unsafe.Impl.Batchimport.staging.HumanUnderstandableExecutionMonitor.ImportStage;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Config.defaults;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.format.standard.Standard.LATEST_RECORD_FORMATS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.AdditionalInitialIds.EMPTY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.Configuration.DEFAULT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.ImportLogic.NO_MONITOR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.input.DataGeneratorInput.bareboneNodeHeader;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.input.DataGeneratorInput.bareboneRelationshipHeader;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.input.csv.IdType.INTEGER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.staging.HumanUnderstandableExecutionMonitor.NO_EXTERNAL_MONITOR;

	public class HumanUnderstandableExecutionMonitorIT
	{
		 private const long NODE_COUNT = 1_000;
		 private const long RELATIONSHIP_COUNT = 10_000;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.RandomRule random = new org.neo4j.test.rule.RandomRule();
		 public readonly RandomRule Random = new RandomRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.PageCacheAndDependenciesRule storage = new org.neo4j.test.rule.PageCacheAndDependenciesRule();
		 public readonly PageCacheAndDependenciesRule Storage = new PageCacheAndDependenciesRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.SuppressOutput suppressOutput = org.neo4j.test.rule.SuppressOutput.suppressAll();
		 public readonly SuppressOutput SuppressOutput = SuppressOutput.suppressAll();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportProgressOfNodeImport() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportProgressOfNodeImport()
		 {
			  // given
			  CapturingMonitor progress = new CapturingMonitor();
			  HumanUnderstandableExecutionMonitor monitor = new HumanUnderstandableExecutionMonitor( progress, NO_EXTERNAL_MONITOR );
			  IdType idType = INTEGER;
			  Input input = new DataGeneratorInput( NODE_COUNT, RELATIONSHIP_COUNT, idType, Collector.EMPTY, Random.seed(), 0, bareboneNodeHeader(idType, new Extractors(';')), bareboneRelationshipHeader(idType, new Extractors(';')), 1, 1, 0, 0 );

			  // when
			  using ( JobScheduler jobScheduler = new ThreadPoolJobScheduler() )
			  {
					( new ParallelBatchImporter( Storage.directory().databaseLayout(), Storage.fileSystem(), Storage.pageCache(), DEFAULT, NullLogService.Instance, monitor, EMPTY, defaults(), LATEST_RECORD_FORMATS, NO_MONITOR, jobScheduler ) ).doImport(input);

					// then
					progress.AssertAllProgressReachedEnd();
			  }
		 }

		 private class CapturingMonitor : HumanUnderstandableExecutionMonitor.Monitor
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly Dictionary<ImportStage, AtomicInteger> ProgressConflict = new Dictionary<ImportStage, AtomicInteger>( typeof( ImportStage ) );

			  public override void Progress( ImportStage stage, int percent )
			  {
					if ( percent > 100 )
					{
						 fail( "Expected percentage to be 0..100% but was " + percent );
					}

					AtomicInteger stageProgress = ProgressConflict.computeIfAbsent( stage, s => new AtomicInteger() );
					int previous = stageProgress.getAndSet( percent );
					if ( previous > percent )
					{
						 fail( "Progress should go forwards only, but went from " + previous + " to " + percent );
					}
			  }

			  internal virtual void AssertAllProgressReachedEnd()
			  {
					assertEquals( Enum.GetValues( typeof( ImportStage ) ).length, ProgressConflict.Count );
					ProgressConflict.Values.forEach( p => assertEquals( 100, p.get() ) );
			  }
		 }
	}

}