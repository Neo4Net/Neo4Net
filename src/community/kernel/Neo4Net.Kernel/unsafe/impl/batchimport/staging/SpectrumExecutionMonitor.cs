using System;
using System.Text;

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

	using Neo4Net.Collections.Helpers;
	using DetailLevel = Neo4Net.@unsafe.Impl.Batchimport.stats.DetailLevel;
	using Keys = Neo4Net.@unsafe.Impl.Batchimport.stats.Keys;
	using StatsProvider = Neo4Net.@unsafe.Impl.Batchimport.stats.StatsProvider;
	using StepStats = Neo4Net.@unsafe.Impl.Batchimport.stats.StepStats;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.pow;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.Format.date;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.Format.duration;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.collection.Iterables.last;

	/// <summary>
	/// This is supposed to be a beautiful one-line <seealso cref="ExecutionMonitor"/>, looking like:
	/// 
	/// <pre>
	/// NODE |--INPUT--|--NODE--|======NODE=PROPERTY======|-------------WRITER-------------| 1000
	/// </pre>
	/// 
	/// where there's one line per stage, updated rapidly, overwriting the line each time. The width
	/// of the <seealso cref="Step"/> column is based on how slow it is compared to the others.
	/// 
	/// The width of the "spectrum" is user specified, but is dynamic in that it can shrink or expand
	/// based on how many simultaneous <seealso cref="StageExecution executions"/> this monitor is monitoring.
	/// 
	/// The specified width is included stage identifier and progress, so in a console the whole
	/// console width can be specified.
	/// </summary>
	public class SpectrumExecutionMonitor : ExecutionMonitor_Adapter
	{
		 public const int DEFAULT_WIDTH = 100;
		 private const int PROGRESS_WIDTH = 5;
		 private static readonly char[] _weights = new char[] { ' ', 'K', 'M', 'B', 'T' };

		 private readonly PrintStream @out;
		 private readonly int _width;
		 // For tracking delta
		 private long _lastProgress;

		 public SpectrumExecutionMonitor( long interval, TimeUnit unit, PrintStream @out, int width ) : base( interval, unit )
		 {
			  this.@out = @out;
			  this._width = width;
		 }

		 public override void Start( StageExecution execution )
		 {
			  @out.println( execution.Name() + ", started " + date() );
			  _lastProgress = 0;
		 }

		 public override void End( StageExecution execution, long totalTimeMillis )
		 {
			  Check( execution );
			  @out.println();
			  @out.println( "Done in " + duration( totalTimeMillis ) );
		 }

		 public override void Done( bool successful, long totalTimeMillis, string additionalInformation )
		 {
			  @out.println();
			  @out.println( format( "IMPORT %s in %s. %s", successful ? "DONE" : "FAILED", duration( totalTimeMillis ), additionalInformation ) );
		 }

		 public override void Check( StageExecution execution )
		 {
			  StringBuilder builder = new StringBuilder();
			  PrintSpectrum( builder, execution, _width, DetailLevel.IMPORTANT );

			  // add delta
			  long progress = last( execution.Steps() ).stats().stat(Keys.done_batches).asLong() * execution.Config.batchSize();
			  long currentDelta = progress - _lastProgress;
			  builder.Append( " ∆" ).Append( FitInProgress( currentDelta ) );

			  // and remember progress to compare with next check
			  _lastProgress = progress;

			  // print it (overwriting the previous contents on this console line)
			  @out.print( "\r" + builder );
		 }

		 public static void PrintSpectrum( StringBuilder builder, StageExecution execution, int width, DetailLevel additionalStatsLevel )
		 {
			  long[] values = values( execution );
			  long total = total( values );

			  // reduce the width with the known extra characters we know we'll print in and around the spectrum
			  width -= 2 + PROGRESS_WIDTH;

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: Neo4Net.helpers.collection.Pair<Step<?>,float> bottleNeck = execution.stepsOrderedBy(Neo4Net.unsafe.impl.batchimport.stats.Keys.avg_processing_time, false).iterator().next();
			  Pair<Step<object>, float> bottleNeck = execution.StepsOrderedBy( Keys.avg_processing_time, false ).GetEnumerator().next();
			  QuantizedProjection projection = new QuantizedProjection( total, width );
			  long lastDoneBatches = 0;
			  int stepIndex = 0;
			  bool hasProgressed = false;
			  builder.Append( '[' );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (Step<?> step : execution.steps())
			  foreach ( Step<object> step in execution.Steps() )
			  {
					StepStats stats = step.Stats();
					if ( !projection.Next( values[stepIndex] ) )
					{
						 break; // odd though
					}
					long stepWidth = total == 0 && stepIndex == 0 ? width : projection.Step();
					if ( stepWidth > 0 )
					{
						 if ( hasProgressed )
						 {
							  stepWidth--;
							  builder.Append( '|' );
						 }
						 bool isBottleNeck = bottleNeck.First() == step;
						 string name = ( isBottleNeck ? "*" : "" ) + stats.ToString( additionalStatsLevel ) + ( step.Processors( 0 ) > 1 ? "(" + step.Processors( 0 ) + ")" : "" );
						 int charIndex = 0; // negative value "delays" the text, i.e. pushes it to the right
						 char backgroundChar = step.Processors( 0 ) > 1 ? '=' : '-';
						 for ( int i = 0; i < stepWidth; i++, charIndex++ )
						 {
							  char ch = backgroundChar;
							  if ( charIndex >= 0 && charIndex < name.Length && charIndex < stepWidth )
							  {
									ch = name[charIndex];
							  }
							  builder.Append( ch );
						 }
						 hasProgressed = true;
					}
					lastDoneBatches = stats.Stat( Keys.done_batches ).asLong();
					stepIndex++;
			  }

			  long progress = lastDoneBatches * execution.Config.batchSize();
			  builder.Append( "]" ).Append( FitInProgress( progress ) );
		 }

		 private static string FitInProgress( long value )
		 {
			  int weight = weight( value );

			  string progress;
			  if ( weight == 0 )
			  {
					progress = value.ToString();
			  }
			  else
			  {
					double floatValue = value / pow( 1000, weight );
					progress = floatValue.ToString();
					if ( progress.Length > PROGRESS_WIDTH - 1 )
					{
						 progress = progress.Substring( 0, PROGRESS_WIDTH - 1 );
					}
					if ( progress.EndsWith( ".", StringComparison.Ordinal ) )
					{
						 progress = progress.Substring( 0, progress.Length - 1 );
					}
					progress += _weights[weight];
			  }

			  return Pad( progress, PROGRESS_WIDTH, ' ' );
		 }

		 private static string Pad( string result, int length, char padChar )
		 {
			  while ( result.Length < length )
			  {
					result = padChar + result;
			  }
			  return result;
		 }

		 private static int Weight( long value )
		 {
			  int weight = 0;
			  while ( value >= 1000 )
			  {
					value /= 1000;
					weight++;
			  }
			  return weight;
		 }

		 private static long[] Values( StageExecution execution )
		 {
			  long[] values = new long[execution.Size()];
			  int i = 0;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (Step<?> step : execution.steps())
			  foreach ( Step<object> step in execution.Steps() )
			  {
					values[i++] = Avg( step.Stats() );
			  }
			  return values;
		 }

		 private static long Total( long[] values )
		 {
			  long total = 0;
			  foreach ( long value in values )
			  {
					total += value;
			  }
			  return total;
		 }

		 private static long Avg( StatsProvider step )
		 {
			  return step.Stat( Keys.avg_processing_time ).asLong();
		 }
	}

}