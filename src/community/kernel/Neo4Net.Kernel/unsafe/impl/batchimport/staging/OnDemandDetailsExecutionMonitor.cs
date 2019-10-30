using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

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
	using Format = Neo4Net.Helpers.Format;
	using Neo4Net.Collections.Helpers;
	using OsBeanUtil = Neo4Net.Io.os.OsBeanUtil;
	using VmPauseMonitor = Neo4Net.Kernel.monitoring.VmPauseMonitor;
	using VmPauseInfo = Neo4Net.Kernel.monitoring.VmPauseMonitor.VmPauseInfo;
	using NullLog = Neo4Net.Logging.NullLog;
	using IJobScheduler = Neo4Net.Scheduler.JobScheduler;
	using DetailLevel = Neo4Net.@unsafe.Impl.Batchimport.stats.DetailLevel;
	using Keys = Neo4Net.@unsafe.Impl.Batchimport.stats.Keys;
	using Stat = Neo4Net.@unsafe.Impl.Batchimport.stats.Stat;
	using StepStats = Neo4Net.@unsafe.Impl.Batchimport.stats.StepStats;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Long.max;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static System.currentTimeMillis;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.Format.bytes;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.Format.date;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.Format.duration;

	/// <summary>
	/// Sits in the background and collect stats about stages that are executing.
	/// Reacts to console input from <seealso cref="System.in"/> and on certain commands print various things.
	/// Commands (complete all with ENTER):
	/// <para>
	/// <table border="1">
	///   <tr>
	///     <th>Command</th>
	///     <th>Description</th>
	///   </tr>
	///   <tr>
	///     <th>i</th>
	///     <th>Print <seealso cref="SpectrumExecutionMonitor compact information"/> about each executed stage up to this point</th>
	///   </tr>
	/// </table>
	/// </para>
	/// </summary>
	public class OnDemandDetailsExecutionMonitor : ExecutionMonitor
	{
		 internal interface Monitor
		 {
			  void DetailsPrinted();
		 }

		 private readonly IList<StageDetails> _details = new List<StageDetails>();
		 private readonly PrintStream @out;
		 private readonly Stream @in;
		 private readonly IDictionary<string, Pair<string, ThreadStart>> _actions = new Dictionary<string, Pair<string, ThreadStart>>();
		 private readonly VmPauseTimeAccumulator _vmPauseTimeAccumulator = new VmPauseTimeAccumulator();
		 private readonly VmPauseMonitor _vmPauseMonitor;
		 private readonly Monitor _monitor;

		 private StageDetails _current;
		 private bool _printDetailsOnDone;

		 public OnDemandDetailsExecutionMonitor( PrintStream @out, Stream @in, Monitor monitor, IJobScheduler jobScheduler )
		 {
			  this.@out = @out;
			  this.@in = @in;
			  this._monitor = monitor;
			  this._actions["i"] = Pair.of( "Print more detailed information", this.printDetails );
			  this._actions["c"] = Pair.of( "Print more detailed information about current stage", this.printDetailsForCurrentStage );
			  this._vmPauseMonitor = new VmPauseMonitor( Duration.ofMillis( 100 ), Duration.ofMillis( 100 ), NullLog.Instance, jobScheduler, _vmPauseTimeAccumulator );
		 }

		 public override void Initialize( DependencyResolver dependencyResolver )
		 {
			  @out.println( "InteractiveReporterInteractions command list (end with ENTER):" );
			  _actions.forEach( ( key, action ) => @out.println( "  " + key + ": " + action.first() ) );
			  @out.println();
			  _vmPauseMonitor.start();
		 }

		 public override void Start( StageExecution execution )
		 {
			  _details.Add( _current = new StageDetails( execution, _vmPauseTimeAccumulator ) );
		 }

		 public override void End( StageExecution execution, long totalTimeMillis )
		 {
			  _current.collect();
		 }

		 public override void Done( bool successful, long totalTimeMillis, string additionalInformation )
		 {
			  if ( _printDetailsOnDone )
			  {
					PrintDetails();
			  }
			  _vmPauseMonitor.stop();
		 }

		 public override long NextCheckTime()
		 {
			  return currentTimeMillis() + 500;
		 }

		 public override void Check( StageExecution execution )
		 {
			  _current.collect();
			  ReactToUserInput();
		 }

		 private void PrintDetails()
		 {
			  PrintDetailsHeadline();
			  long totalTime = 0;
			  foreach ( StageDetails stageDetails in _details )
			  {
					stageDetails.Print( @out );
					totalTime += stageDetails.TotalTimeMillis;
			  }

			  PrintIndented( @out, "Environment information:" );
			  PrintIndented( @out, "  Free physical memory: " + bytes( OsBeanUtil.FreePhysicalMemory ) );
			  PrintIndented( @out, "  Max VM memory: " + bytes( Runtime.Runtime.maxMemory() ) );
			  PrintIndented( @out, "  Free VM memory: " + bytes( Runtime.Runtime.freeMemory() ) );
			  PrintIndented( @out, "  VM stop-the-world time: " + duration( _vmPauseTimeAccumulator.PauseTime ) );
			  PrintIndented( @out, "  Duration: " + duration( totalTime ) );
			  @out.println();
		 }

		 private void PrintDetailsHeadline()
		 {
			  @out.println();
			  @out.println();
			  PrintIndented( @out, "******** DETAILS " + date() + " ********" );
			  @out.println();

			  // Make sure that if user is interested in details then also print the entire details set when import is done
			  _printDetailsOnDone = true;
			  _monitor.detailsPrinted();
		 }

		 private void PrintDetailsForCurrentStage()
		 {
			  PrintDetailsHeadline();
			  if ( _details.Count > 0 )
			  {
					_details[_details.Count - 1].print( @out );
			  }
		 }

		 private static void PrintIndented( PrintStream @out, string @string )
		 {
			  @out.println( "\t" + @string );
		 }

		 private void ReactToUserInput()
		 {
			  try
			  {
					if ( @in.available() > 0 )
					{
						 // don't close this read, since we really don't want to close the underlying System.in
						 StreamReader reader = new StreamReader( System.in );
						 string line = reader.ReadLine();
						 Pair<string, ThreadStart> action = _actions[line];
						 if ( action != null )
						 {
							  action.Other().run();
						 }
					}
			  }
			  catch ( IOException e )
			  {
					e.printStackTrace( @out );
			  }
		 }

		 private class StageDetails
		 {
			  internal readonly StageExecution Execution;
			  internal readonly long StartTime;
			  internal readonly VmPauseTimeAccumulator VmPauseTimeAccumulator;
			  internal readonly long BaseVmPauseTime;

			  internal long MemoryUsage;
			  internal long IoThroughput;
			  internal long TotalTimeMillis;
			  internal long StageVmPauseTime;
			  internal long DoneBatches;

			  internal StageDetails( StageExecution execution, VmPauseTimeAccumulator vmPauseTimeAccumulator )
			  {
					this.Execution = execution;
					this.VmPauseTimeAccumulator = vmPauseTimeAccumulator;
					this.BaseVmPauseTime = vmPauseTimeAccumulator.PauseTime;
					this.StartTime = currentTimeMillis();
			  }

			  internal virtual void Print( PrintStream @out )
			  {
					PrintIndented( @out, Execution.name() );
					StringBuilder builder = new StringBuilder();
					SpectrumExecutionMonitor.PrintSpectrum( builder, Execution, SpectrumExecutionMonitor.DEFAULT_WIDTH, DetailLevel.NO );
					PrintIndented( @out, builder.ToString() );
					PrintValue( @out, MemoryUsage, "Memory usage", Format.bytes );
					PrintValue( @out, IoThroughput, "I/O throughput", value => bytes( value ) + "/s" );
					PrintValue( @out, StageVmPauseTime, "VM stop-the-world time", Format.duration );
					PrintValue( @out, TotalTimeMillis, "Duration", Format.duration );
					PrintValue( @out, DoneBatches, "Done batches", string.ValueOf );

					@out.println();
			  }

			  internal static void PrintValue( PrintStream @out, long value, string description, System.Func<long, string> toStringConverter )
			  {
					if ( value > 0 )
					{
						 PrintIndented( @out, description + ": " + toStringConverter( value ) );
					}
			  }

			  internal virtual void Collect()
			  {
					TotalTimeMillis = currentTimeMillis() - StartTime;
					StageVmPauseTime = VmPauseTimeAccumulator.PauseTime - BaseVmPauseTime;
					long lastDoneBatches = DoneBatches;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (Step<?> step : execution.steps())
					foreach ( Step<object> step in Execution.steps() )
					{
						 StepStats stats = step.Stats();
						 Stat memoryUsageStat = stats.Stat( Keys.memory_usage );
						 if ( memoryUsageStat != null )
						 {
							  MemoryUsage = max( MemoryUsage, memoryUsageStat.AsLong() );
						 }
						 Stat ioStat = stats.Stat( Keys.io_throughput );
						 if ( ioStat != null )
						 {
							  IoThroughput = ioStat.AsLong();
						 }
						 lastDoneBatches = stats.Stat( Keys.done_batches ).asLong();
					}
					DoneBatches = lastDoneBatches;
			  }
		 }

		 private class VmPauseTimeAccumulator : System.Action<VmPauseMonitor.VmPauseInfo>
		 {
			  internal readonly AtomicLong TotalPauseTime = new AtomicLong();

			  public override void Accept( VmPauseMonitor.VmPauseInfo pauseInfo )
			  {
					TotalPauseTime.addAndGet( pauseInfo.PauseTime );
			  }

			  internal virtual long PauseTime
			  {
				  get
				  {
						return TotalPauseTime.get();
				  }
			  }
		 }
	}

}