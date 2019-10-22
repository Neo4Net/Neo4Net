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

	using MapUtil = Neo4Net.Helpers.Collections.MapUtil;
	using DetailLevel = Neo4Net.@unsafe.Impl.Batchimport.stats.DetailLevel;
	using Key = Neo4Net.@unsafe.Impl.Batchimport.stats.Key;
	using Keys = Neo4Net.@unsafe.Impl.Batchimport.stats.Keys;
	using Stat = Neo4Net.@unsafe.Impl.Batchimport.stats.Stat;
	using StatsProvider = Neo4Net.@unsafe.Impl.Batchimport.stats.StatsProvider;
	using StepStats = Neo4Net.@unsafe.Impl.Batchimport.stats.StepStats;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Integer.max;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Integer.min;

	/// <summary>
	/// A bit like a mocked <seealso cref="Step"/>, but easier to work with.
	/// </summary>
	public class ControlledStep<T> : Step<T>, StatsProvider
	{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public static ControlledStep<?> stepWithAverageOf(String name, int maxProcessors, long avg)
		 public static ControlledStep<object> StepWithAverageOf( string name, int maxProcessors, long avg )
		 {
			  return StepWithStats( name, maxProcessors, Keys.avg_processing_time, avg );
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public static ControlledStep<?> stepWithStats(String name, int maxProcessors, java.util.Map<org.Neo4Net.unsafe.impl.batchimport.stats.Key,long> statistics)
		 public static ControlledStep<object> StepWithStats( string name, int maxProcessors, IDictionary<Key, long> statistics )
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: ControlledStep<?> step = new ControlledStep<>(name, maxProcessors);
			  ControlledStep<object> step = new ControlledStep<object>( name, maxProcessors );
			  foreach ( KeyValuePair<Key, long> statistic in statistics.SetOfKeyValuePairs() )
			  {
					step.SetStat( statistic.Key, statistic.Value.longValue() );
			  }
			  return step;
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public static ControlledStep<?> stepWithStats(String name, int maxProcessors, Object... statisticsAltKeyAndValue)
		 public static ControlledStep<object> StepWithStats( string name, int maxProcessors, params object[] statisticsAltKeyAndValue )
		 {
			  return StepWithStats( name, maxProcessors, MapUtil.genericMap( statisticsAltKeyAndValue ) );
		 }

		 private readonly string _name;
		 private readonly IDictionary<Key, ControlledStat> _stats = new Dictionary<Key, ControlledStat>();
		 private readonly int _maxProcessors;
		 private volatile int _numberOfProcessors = 1;
		 private readonly System.Threading.CountdownEvent _completed = new System.Threading.CountdownEvent( 1 );

		 public ControlledStep( string name, int maxProcessors ) : this( name, maxProcessors, 1 )
		 {
		 }

		 public ControlledStep( string name, int maxProcessors, int initialProcessorCount )
		 {
			  this._maxProcessors = maxProcessors == 0 ? int.MaxValue : maxProcessors;
			  this._name = name;
			  Processors( initialProcessorCount - 1 );
		 }

		 public virtual ControlledStep<T> setProcessors( int numberOfProcessors )
		 {
			  // We don't have to assert max processors here since importer will not count every processor
			  // equally. A step being very idle (due to being very very fast) counts as almost nothing.
			  Processors( numberOfProcessors );
			  return this;
		 }

		 public override int Processors( int delta )
		 {
			  if ( delta > 0 )
			  {
					_numberOfProcessors = min( _numberOfProcessors + delta, _maxProcessors );
			  }
			  else if ( delta < 0 )
			  {
					_numberOfProcessors = max( 1, _numberOfProcessors + delta );
			  }
			  return _numberOfProcessors;
		 }

		 public override string Name()
		 {
			  return _name;
		 }

		 public override long Receive( long ticket, T batch )
		 {
			  throw new System.NotSupportedException( "Cannot participate in actual processing yet" );
		 }

		 public virtual void SetStat( Key key, long value )
		 {
			  _stats[key] = new ControlledStat( value );
		 }

		 public override StepStats Stats()
		 {
			  return new StepStats( _name, !Completed, Arrays.asList( this ) );
		 }

		 public override void EndOfUpstream()
		 {
		 }

		 public virtual bool Completed
		 {
			 get
			 {
				  return _completed.CurrentCount == 0;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void awaitCompleted() throws InterruptedException
		 public override void AwaitCompleted()
		 {
			  _completed.await();
		 }

		 public virtual Step<T1> Downstream<T1>
		 {
			 set
			 {
			 }
		 }

		 public override void ReceivePanic( Exception cause )
		 {
		 }

		 public override void Close()
		 {
		 }

		 public override Stat Stat( Key key )
		 {
			  return _stats[key];
		 }

		 public override void Start( int orderingGuarantees )
		 {
		 }

		 public override Key[] Keys()
		 {
			  return _stats.Keys.toArray( new Key[_stats.Count] );
		 }

		 public virtual void Complete()
		 {
			  _completed.Signal();
		 }

		 private class ControlledStat : Stat
		 {
			  internal readonly long Value;

			  internal ControlledStat( long value )
			  {
					this.Value = value;
			  }

			  public override DetailLevel DetailLevel()
			  {
					return DetailLevel.BASIC;
			  }

			  public override long AsLong()
			  {
					return Value;
			  }

			  public override string ToString()
			  {
					return "" + Value;
			  }
		 }

		 public override string ToString()
		 {
			  return this.GetType().Name + "[" + Name() + ", " + _stats + "]";
		 }
	}

}