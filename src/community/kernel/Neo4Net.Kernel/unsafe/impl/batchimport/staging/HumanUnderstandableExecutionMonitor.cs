using System;
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

	using DependencyResolver = Neo4Net.GraphDb.DependencyResolver;
	using NodeRelationshipCache = Neo4Net.@unsafe.Impl.Batchimport.cache.NodeRelationshipCache;
	using PageCacheArrayFactoryMonitor = Neo4Net.@unsafe.Impl.Batchimport.cache.PageCacheArrayFactoryMonitor;
	using IdMapper = Neo4Net.@unsafe.Impl.Batchimport.cache.idmapping.IdMapper;
	using Input = Neo4Net.@unsafe.Impl.Batchimport.input.Input;
	using Input_Estimates = Neo4Net.@unsafe.Impl.Batchimport.input.Input_Estimates;
	using Keys = Neo4Net.@unsafe.Impl.Batchimport.stats.Keys;
	using Stat = Neo4Net.@unsafe.Impl.Batchimport.stats.Stat;
	using BatchingNeoStores = Neo4Net.@unsafe.Impl.Batchimport.store.BatchingNeoStores;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Integer.min;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Long.max;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static System.currentTimeMillis;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.Format.bytes;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.Format.count;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.Format.date;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.Format.duration;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterables.last;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.@unsafe.impl.batchimport.ImportMemoryCalculator.defensivelyPadMemoryEstimate;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.@unsafe.impl.batchimport.ImportMemoryCalculator.estimatedCacheSize;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.@unsafe.impl.batchimport.cache.GatheringMemoryStatsVisitor.totalMemoryUsageOf;

	/// <summary>
	/// Prints progress you can actually understand, with capabilities to on demand print completely incomprehensible
	/// details only understandable to a select few.
	/// </summary>
	public class HumanUnderstandableExecutionMonitor : ExecutionMonitor
	{
		 public interface Monitor
		 {
			  void Progress( ImportStage stage, int percent );
		 }

		 public static readonly Monitor NoMonitor = ( stage, percent ) =>
		 {
		 };

		 public interface ExternalMonitor
		 {
			  bool SomethingElseBrokeMyNiceOutput();
		 }

		 internal static readonly ExternalMonitor NoExternalMonitor = () => false;

		 internal enum ImportStage
		 {
			  NodeImport,
			  RelationshipImport,
			  Linking,
			  PostProcessing
		 }

		 private const string ESTIMATED_REQUIRED_MEMORY_USAGE = "Estimated required memory usage";
		 private const string ESTIMATED_DISK_SPACE_USAGE = "Estimated disk space usage";
		 private const string ESTIMATED_NUMBER_OF_RELATIONSHIP_PROPERTIES = "Estimated number of relationship properties";
		 private const string ESTIMATED_NUMBER_OF_RELATIONSHIPS = "Estimated number of relationships";
		 private const string ESTIMATED_NUMBER_OF_NODE_PROPERTIES = "Estimated number of node properties";
		 private const string ESTIMATED_NUMBER_OF_NODES = "Estimated number of nodes";
		 private const int DOT_GROUP_SIZE = 10;
		 private const int DOT_GROUPS_PER_LINE = 5;
		 private const int PERCENTAGES_PER_LINE = 5;

		 private readonly Monitor _monitor;
		 private readonly ExternalMonitor _externalMonitor;
		 private DependencyResolver _dependencyResolver;
		 private bool _newInternalStage;
		 private PageCacheArrayFactoryMonitor _pageCacheArrayFactoryMonitor;

		 // progress of current stage
		 private long _goal;
		 private long _stashedProgress;
		 private long _progress;
		 private ImportStage _currentStage;
		 private long _lastReportTime;

		 internal HumanUnderstandableExecutionMonitor( Monitor monitor, ExternalMonitor externalMonitor )
		 {
			  this._monitor = monitor;
			  this._externalMonitor = externalMonitor;
		 }

		 public override void Initialize( DependencyResolver dependencyResolver )
		 {
			  this._dependencyResolver = dependencyResolver;
			  Input_Estimates estimates = dependencyResolver.ResolveDependency( typeof( Input_Estimates ) );
			  BatchingNeoStores neoStores = dependencyResolver.ResolveDependency( typeof( BatchingNeoStores ) );
			  IdMapper idMapper = dependencyResolver.ResolveDependency( typeof( IdMapper ) );
			  NodeRelationshipCache nodeRelationshipCache = dependencyResolver.ResolveDependency( typeof( NodeRelationshipCache ) );
			  _pageCacheArrayFactoryMonitor = dependencyResolver.ResolveDependency( typeof( PageCacheArrayFactoryMonitor ) );

			  long biggestCacheMemory = estimatedCacheSize( neoStores, nodeRelationshipCache.MemoryEstimation( estimates.NumberOfNodes() ), idMapper.MemoryEstimation(estimates.NumberOfNodes()) );
			  PrintStageHeader( "Import starting", ESTIMATED_NUMBER_OF_NODES, count( estimates.NumberOfNodes() ), ESTIMATED_NUMBER_OF_NODE_PROPERTIES, count(estimates.NumberOfNodeProperties()), ESTIMATED_NUMBER_OF_RELATIONSHIPS, count(estimates.NumberOfRelationships()), ESTIMATED_NUMBER_OF_RELATIONSHIP_PROPERTIES, count(estimates.NumberOfRelationshipProperties()), ESTIMATED_DISK_SPACE_USAGE, bytes(NodesDiskUsage(estimates, neoStores) + RelationshipsDiskUsage(estimates, neoStores) + estimates.SizeOfNodeProperties() + estimates.SizeOfRelationshipProperties()), ESTIMATED_REQUIRED_MEMORY_USAGE, bytes(biggestCacheMemory) );
			  Console.WriteLine();
		 }

		 private static long BaselineMemoryRequirement( BatchingNeoStores neoStores )
		 {
			  return totalMemoryUsageOf( neoStores );
		 }

		 private static long NodesDiskUsage( Input_Estimates estimates, BatchingNeoStores neoStores )
		 {
			  return estimates.NumberOfNodes() * neoStores.NodeStore.RecordSize + estimates.NumberOfNodeLabels();
		 }

		 private static long RelationshipsDiskUsage( Input_Estimates estimates, BatchingNeoStores neoStores )
		 {
			  return estimates.NumberOfRelationships() * neoStores.RelationshipStore.RecordSize * (neoStores.UsesDoubleRelationshipRecordUnits() ? 2 : 1);
		 }

		 public override void Start( StageExecution execution )
		 {
			  // Divide into 4 progress stages:
			  if ( execution.StageName.Equals( DataImporter.NODE_IMPORT_NAME ) )
			  {
					// Import nodes:
					// - import nodes
					// - prepare id mapper
					InitializeNodeImport( _dependencyResolver.resolveDependency( typeof( Input_Estimates ) ), _dependencyResolver.resolveDependency( typeof( IdMapper ) ), _dependencyResolver.resolveDependency( typeof( BatchingNeoStores ) ) );
			  }
			  else if ( execution.StageName.Equals( DataImporter.RELATIONSHIP_IMPORT_NAME ) )
			  {
					EndPrevious();

					// Import relationships:
					// - import relationships
					InitializeRelationshipImport( _dependencyResolver.resolveDependency( typeof( Input_Estimates ) ), _dependencyResolver.resolveDependency( typeof( IdMapper ) ), _dependencyResolver.resolveDependency( typeof( BatchingNeoStores ) ) );
			  }
			  else if ( execution.StageName.Equals( NodeDegreeCountStage.NAME ) )
			  {
					EndPrevious();

					// Link relationships:
					// - read node degrees
					// - backward linking
					// - node relationship linking
					// - forward linking
					InitializeLinking( _dependencyResolver.resolveDependency( typeof( BatchingNeoStores ) ), _dependencyResolver.resolveDependency( typeof( NodeRelationshipCache ) ), _dependencyResolver.resolveDependency( typeof( DataStatistics ) ) );
			  }
			  else if ( execution.StageName.Equals( CountGroupsStage.NAME ) )
			  {
					EndPrevious();

					// Misc:
					// - relationship group defragmentation
					// - counts store
					InitializeMisc( _dependencyResolver.resolveDependency( typeof( BatchingNeoStores ) ), _dependencyResolver.resolveDependency( typeof( DataStatistics ) ) );
			  }
			  else if ( IncludeStage( execution ) )
			  {
					_stashedProgress += _progress;
					_progress = 0;
					_newInternalStage = true;
			  }
			  _lastReportTime = currentTimeMillis();
		 }

		 private void EndPrevious()
		 {
			  UpdateProgress( _goal );
		 }

		 private void InitializeNodeImport( Input_Estimates estimates, IdMapper idMapper, BatchingNeoStores neoStores )
		 {
			  long numberOfNodes = estimates.NumberOfNodes();
			  PrintStageHeader( "(1/4) Node import", ESTIMATED_NUMBER_OF_NODES, count( numberOfNodes ), ESTIMATED_DISK_SPACE_USAGE, bytes( NodesDiskUsage( estimates, neoStores ) + estimates.SizeOfNodeProperties() ), ESTIMATED_REQUIRED_MEMORY_USAGE, bytes(BaselineMemoryRequirement(neoStores) + defensivelyPadMemoryEstimate(idMapper.MemoryEstimation(numberOfNodes))) );

			  // A difficulty with the goal here is that we don't know how much work there is to be done in id mapper preparation stage.
			  // In addition to nodes themselves and SPLIT,SORT,DETECT there may be RESOLVE,SORT,DEDUPLICATE too, if there are collisions
			  long goal = idMapper.NeedsPreparation() ? numberOfNodes + Weighted(IdMapperPreparationStage.NAME, numberOfNodes * 4) : numberOfNodes;
			  InitializeProgress( goal, ImportStage.NodeImport );
		 }

		 private void InitializeRelationshipImport( Input_Estimates estimates, IdMapper idMapper, BatchingNeoStores neoStores )
		 {
			  long numberOfRelationships = estimates.NumberOfRelationships();
			  PrintStageHeader( "(2/4) Relationship import", ESTIMATED_NUMBER_OF_RELATIONSHIPS, count( numberOfRelationships ), ESTIMATED_DISK_SPACE_USAGE, bytes( RelationshipsDiskUsage( estimates, neoStores ) + estimates.SizeOfRelationshipProperties() ), ESTIMATED_REQUIRED_MEMORY_USAGE, bytes(BaselineMemoryRequirement(neoStores) + totalMemoryUsageOf(idMapper)) );
			  InitializeProgress( numberOfRelationships, ImportStage.RelationshipImport );
		 }

		 private void InitializeLinking( BatchingNeoStores neoStores, NodeRelationshipCache nodeRelationshipCache, DataStatistics distribution )
		 {
			  PrintStageHeader( "(3/4) Relationship linking", ESTIMATED_REQUIRED_MEMORY_USAGE, bytes( BaselineMemoryRequirement( neoStores ) + defensivelyPadMemoryEstimate( nodeRelationshipCache.MemoryEstimation( distribution.NodeCount ) ) ) );
			  // The reason the highId of the relationship store is used, as opposed to actual number of imported relationships
			  // is that the stages underneath operate on id ranges, not knowing which records are actually in use.
			  long relationshipRecordIdCount = neoStores.RelationshipStore.HighId;
			  // The progress counting of linking stages is special anyway, in that it uses the "progress" stats key,
			  // which is based on actual number of relationships, not relationship ids.
			  long actualRelationshipCount = distribution.RelationshipCount;
			  InitializeProgress( relationshipRecordIdCount + actualRelationshipCount * 2 + actualRelationshipCount * 2, ImportStage.Linking );
		 }

		 private void InitializeMisc( BatchingNeoStores neoStores, DataStatistics distribution )
		 {
			  PrintStageHeader( "(4/4) Post processing", ESTIMATED_REQUIRED_MEMORY_USAGE, bytes( BaselineMemoryRequirement( neoStores ) ) );
			  long actualNodeCount = distribution.NodeCount;
			  // The reason the highId of the relationship store is used, as opposed to actual number of imported relationships
			  // is that the stages underneath operate on id ranges, not knowing which records are actually in use.
			  long relationshipRecordIdCount = neoStores.RelationshipStore.HighId;
			  long groupCount = neoStores.TemporaryRelationshipGroupStore.HighId;
			  InitializeProgress( groupCount + groupCount + groupCount + actualNodeCount + relationshipRecordIdCount, ImportStage.PostProcessing );
		 }

		 private void InitializeProgress( long goal, ImportStage stage )
		 {
			  this._goal = goal;
			  this._stashedProgress = 0;
			  this._progress = 0;
			  this._currentStage = stage;
			  this._newInternalStage = false;
		 }

		 private void UpdateProgress( long progress )
		 {
			  // OK so format goes something like 5 groups of 10 dots per line, which is 5%, i.e. 50 dots for 5%, i.e. 1000 dots for 100%,
			  // i.e. granularity is 1/1000

			  int maxDot = DotOf( _goal );
			  int currentProgressDot = DotOf( _stashedProgress + this._progress );
			  int currentLine = currentProgressDot / DotsPerLine();
			  int currentDotOnLine = currentProgressDot % DotsPerLine();

			  int progressDot = min( maxDot, DotOf( _stashedProgress + progress ) );
			  int line = progressDot / DotsPerLine();
			  int dotOnLine = progressDot % DotsPerLine();

			  while ( currentLine < line || ( currentLine == line && currentDotOnLine < dotOnLine ) )
			  {
					int target = currentLine < line ? DotsPerLine() : dotOnLine;
					PrintDots( currentDotOnLine, target );
					currentDotOnLine = target;

					if ( currentLine < line || currentDotOnLine == DotsPerLine() )
					{
						 int percentage = percentage( currentLine );
						 Console.WriteLine( format( "%4d%% ∆%s", percentage, DurationSinceLastReport() ) );
						 _monitor.progress( _currentStage, percentage );
						 currentLine++;
						 if ( currentLine == Lines() )
						 {
							  Console.WriteLine();
						 }
						 currentDotOnLine = 0;
					}
			  }

			  this._progress = max( this._progress, progress );
		 }

		 private string DurationSinceLastReport()
		 {
			  long diff = currentTimeMillis() - _lastReportTime;
			  _lastReportTime = currentTimeMillis();
			  return duration( diff );
		 }

		 private static int Percentage( int line )
		 {
			  return ( line + 1 ) * PERCENTAGES_PER_LINE;
		 }

		 private void PrintDots( int from, int target )
		 {
			  int current = from;
			  while ( current < target )
			  {
					if ( current > 0 && current % DOT_GROUP_SIZE == 0 )
					{
						 Console.Write( ' ' );
					}
					char dotChar = '.';
					if ( _newInternalStage )
					{
						 _newInternalStage = false;
						 dotChar = '-';
					}
					Console.Write( dotChar );
					current++;

					PrintPageCacheAllocationWarningIfUsed();
			  }
		 }

		 private void PrintPageCacheAllocationWarningIfUsed()
		 {
			  string allocation = _pageCacheArrayFactoryMonitor.pageCacheAllocationOrNull();
			  if ( !string.ReferenceEquals( allocation, null ) )
			  {
					Console.Error.WriteLine();
					Console.Error.WriteLine( "WARNING:" );
					Console.Error.WriteLine( allocation );
			  }
		 }

		 private int DotOf( long progress )
		 {
			  // calculated here just to reduce amount of state kept in this instance
			  int dots = DotsPerLine() * Lines();
			  double dotSize = _goal / ( double ) dots;

			  return ( int )( progress / dotSize );
		 }

		 private static int Lines()
		 {
			  return 100 / PERCENTAGES_PER_LINE;
		 }

		 private static int DotsPerLine()
		 {
			  return DOT_GROUPS_PER_LINE * DOT_GROUP_SIZE;
		 }

		 private void PrintStageHeader( string name, params object[] data )
		 {
			  Console.WriteLine( name + " " + date( TimeZone.Default ) );
			  if ( data.Length > 0 )
			  {
					for ( int i = 0; i < data.Length; )
					{
						 Console.WriteLine( "  " + data[i++] + ": " + data[i++] );
					}
			  }
		 }

		 public override void End( StageExecution execution, long totalTimeMillis )
		 {
		 }

		 public override void Done( bool successful, long totalTimeMillis, string additionalInformation )
		 {
			  EndPrevious();

			  Console.WriteLine();
			  Console.WriteLine( format( "IMPORT %s in %s. %s", successful ? "DONE" : "FAILED", duration( totalTimeMillis ), additionalInformation ) );
		 }

		 public override long NextCheckTime()
		 {
			  return currentTimeMillis() + 200;
		 }

		 public override void Check( StageExecution execution )
		 {
			  ReprintProgressIfNecessary();
			  if ( IncludeStage( execution ) )
			  {
					UpdateProgress( ProgressOf( execution ) );
			  }
		 }

		 private void ReprintProgressIfNecessary()
		 {
			  if ( _externalMonitor.somethingElseBrokeMyNiceOutput() )
			  {
					long prevProgress = this._progress;
					long prevStashedProgress = this._stashedProgress;
					this._progress = 0;
					this._stashedProgress = 0;
					UpdateProgress( prevProgress + prevStashedProgress );
					this._progress = prevProgress;
					this._stashedProgress = prevStashedProgress;
			  }
		 }

		 private static bool IncludeStage( StageExecution execution )
		 {
			  string name = execution.StageName;
			  return !name.Equals( RelationshipGroupStage.NAME ) && !name.Equals( SparseNodeFirstRelationshipStage.NAME ) && !name.Equals( ScanAndCacheGroupsStage.NAME );
		 }

		 private static double WeightOf( string stageName )
		 {
			  if ( stageName.Equals( IdMapperPreparationStage.NAME ) )
			  {
					return 0.5D;
			  }
			  return 1;
		 }

		 private static long Weighted( string stageName, long progress )
		 {
			  return ( long )( progress * WeightOf( stageName ) );
		 }

		 private static long ProgressOf( StageExecution execution )
		 {
			  // First see if there's a "progress" stat
			  Stat progressStat = FindProgressStat( execution.Steps() );
			  if ( progressStat != null )
			  {
					return Weighted( execution.StageName, progressStat.AsLong() );
			  }

			  // No, then do the generic progress calculation by looking at "done_batches"
			  long doneBatches = last( execution.Steps() ).stats().stat(Keys.done_batches).asLong();
			  int batchSize = execution.Config.batchSize();
			  return Weighted( execution.StageName, doneBatches * batchSize );
		 }

		 private static Stat FindProgressStat<T1>( IEnumerable<T1> steps )
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (Step<?> step : steps)
			  foreach ( Step<object> step in steps )
			  {
					Stat stat = step.Stats().stat(Keys.progress);
					if ( stat != null )
					{
						 return stat;
					}
			  }
			  return null;
		 }
	}

}