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
namespace Org.Neo4j.@unsafe.Impl.Batchimport.staging
{

	using DependencyResolver = Org.Neo4j.Graphdb.DependencyResolver;
	using Clocks = Org.Neo4j.Time.Clocks;

	/// <summary>
	/// Gets notified now and then about <seealso cref="StageExecution"/>, where statistics can be read and displayed,
	/// aggregated or in other ways make sense of the data of <seealso cref="StageExecution"/>.
	/// </summary>
	public interface ExecutionMonitor
	{
		 /// <summary>
		 /// Signals start of import. Called only once and before any other method.
		 /// </summary>
		 /// <param name="dependencyResolver"> <seealso cref="DependencyResolver"/> for getting dependencies from. </param>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default void initialize(org.neo4j.graphdb.DependencyResolver dependencyResolver)
	//	 { // empty by default
	//	 }

		 /// <summary>
		 /// Signals the start of a <seealso cref="StageExecution"/>.
		 /// </summary>
		 void Start( StageExecution execution );

		 /// <summary>
		 /// Signals the end of the execution previously <seealso cref="start(StageExecution) started"/>.
		 /// </summary>
		 void End( StageExecution execution, long totalTimeMillis );

		 /// <summary>
		 /// Called after all <seealso cref="StageExecution stage executions"/> have run.
		 /// </summary>
		 void Done( bool successful, long totalTimeMillis, string additionalInformation );

		 /// <returns> next time stamp when this monitor would like to check that status of current execution. </returns>
		 long NextCheckTime();

		 /// <summary>
		 /// Called periodically while executing a <seealso cref="StageExecution"/>.
		 /// </summary>
		 void Check( StageExecution execution );

		 /// <summary>
		 /// Base implementation with most methods defaulting to not doing anything.
		 /// </summary>
	}

	 public abstract class ExecutionMonitor_Adapter : ExecutionMonitor
	 {
		 public abstract void Check( StageExecution execution );
		 public abstract void Initialize( DependencyResolver dependencyResolver );
		  internal readonly Clock Clock;
		  internal readonly long IntervalMillis;

		  public ExecutionMonitor_Adapter( Clock clock, long time, TimeUnit unit )
		  {
				this.Clock = clock;
				this.IntervalMillis = unit.toMillis( time );
		  }

		  public ExecutionMonitor_Adapter( long time, TimeUnit unit ) : this( Clocks.systemClock(), time, unit )
		  {
		  }

		  public override long NextCheckTime()
		  {
				return Clock.millis() + IntervalMillis;
		  }

		  public override void Start( StageExecution execution )
		  { // Do nothing by default
		  }

		  public override void End( StageExecution execution, long totalTimeMillis )
		  { // Do nothing by default
		  }

		  public override void Done( bool successful, long totalTimeMillis, string additionalInformation )
		  { // Do nothing by default
		  }
	 }

}