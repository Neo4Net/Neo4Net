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

	using Neo4Net.Helpers.Collections;
	using Neo4Net.Helpers.Collections;
	using Key = Neo4Net.@unsafe.Impl.Batchimport.stats.Key;
	using Stat = Neo4Net.@unsafe.Impl.Batchimport.stats.Stat;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.Exceptions.throwIfUnchecked;

	/// <summary>
	/// Default implementation of <seealso cref="StageControl"/>
	/// </summary>
	public class StageExecution : StageControl, AutoCloseable
	{
		 private readonly string _stageName;
		 private readonly string _part;
		 private readonly Configuration _config;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final java.util.Collection<Step<?>> pipeline;
		 private readonly ICollection<Step<object>> _pipeline;
		 private readonly int _orderingGuarantees;
		 private volatile Exception _panic;
		 private readonly bool _shouldRecycle;
		 private readonly ConcurrentLinkedQueue<object> _recycled;

		 public StageExecution<T1>( string stageName, string part, Configuration config, ICollection<T1> pipeline, int orderingGuarantees )
		 {
			  this._stageName = stageName;
			  this._part = part;
			  this._config = config;
			  this._pipeline = pipeline;
			  this._orderingGuarantees = orderingGuarantees;
			  this._shouldRecycle = ( orderingGuarantees & Step_Fields.RECYCLE_BATCHES ) != 0;
			  this._recycled = _shouldRecycle ? new ConcurrentLinkedQueue<object>() : null;
		 }

		 public virtual bool StillExecuting()
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (Step<?> step : pipeline)
			  foreach ( Step<object> step in _pipeline )
			  {
					if ( !step.Completed )
					{
						 return true;
					}
			  }
			  return false;
		 }

		 public virtual void Start()
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (Step<?> step : pipeline)
			  foreach ( Step<object> step in _pipeline )
			  {
					step.Start( _orderingGuarantees );
			  }
		 }

		 public virtual string StageName
		 {
			 get
			 {
				  return _stageName;
			 }
		 }

		 public virtual string Name()
		 {
			  return _stageName + ( !string.ReferenceEquals( _part, null ) ? _part : "" );
		 }

		 public virtual Configuration Config
		 {
			 get
			 {
				  return _config;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public Iterable<Step<?>> steps()
		 public virtual IEnumerable<Step<object>> Steps()
		 {
			  return _pipeline;
		 }

		 /// <param name="stat"> statistics <seealso cref="Key"/>. </param>
		 /// <param name="trueForAscending"> {@code true} for ordering by ascending, otherwise descending. </param>
		 /// <returns> the steps ordered by the <seealso cref="Stat.asLong() long value representation"/> of the given
		 /// {@code stat} accompanied a factor by how it compares to the next value, where a value close to
		 /// {@code 1.0} signals them being close to equal, and a value of for example {@code 0.5} signals that
		 /// the value of the current step is half that of the next step. </returns>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public Iterable<org.neo4j.helpers.collection.Pair<Step<?>,float>> stepsOrderedBy(final org.neo4j.unsafe.impl.batchimport.stats.Key stat, final boolean trueForAscending)
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 public virtual IEnumerable<Pair<Step<object>, float>> StepsOrderedBy( Key stat, bool trueForAscending )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.List<Step<?>> steps = new java.util.ArrayList<>(pipeline);
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
			  IList<Step<object>> steps = new List<Step<object>>( _pipeline );
			  steps.sort((o1, o2) =>
			  {
			  long? stat1 = o1.stats().stat(stat).asLong();
			  long? stat2 = o2.stats().stat(stat).asLong();
			  return trueForAscending ? stat1.compareTo( stat2 ) : stat2.compareTo( stat1 );
			  });

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: return () -> new org.neo4j.helpers.collection.PrefetchingIterator<org.neo4j.helpers.collection.Pair<Step<?>,float>>()
			  return () => new PrefetchingIteratorAnonymousInnerClass(this, steps);
		 }

		 private class PrefetchingIteratorAnonymousInnerClass : PrefetchingIterator<Pair<Step<JavaToDotNetGenericWildcard>, float>>
		 {
			 private readonly StageExecution _outerInstance;

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private System.Collections.Generic.IList<Step<JavaToDotNetGenericWildcard>> steps;
			 private IList<Step<object>> _steps;

			 public PrefetchingIteratorAnonymousInnerClass<T1>( StageExecution outerInstance, IList<T1> steps )
			 {
				 this.outerInstance = outerInstance;
				 this._steps = steps;
				 source = steps.GetEnumerator();
				 next = source.hasNext() ? source.next() : null;
			 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final java.util.Iterator<Step<?>> source;
			 private readonly IEnumerator<Step<object>> source;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private Step<?> next;
			 private Step<object> next;

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: protected org.neo4j.helpers.collection.Pair<Step<?>,float> fetchNextOrNull()
			 protected internal override Pair<Step<object>, float> fetchNextOrNull()
			 {
				  if ( next == null )
				  {
						return null;
				  }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: Step<?> current = next;
				  Step<object> current = next;
				  next = source.hasNext() ? source.next() : null;
				  float factor = next != null ? ( float ) stat( current, stat ) / ( float ) stat( next, stat ) : 1.0f;
				  return Pair.of( current, factor );
			 }

			 private long stat<T1>( Step<T1> step, Key stat12 )
			 {
				  return step.Stats().stat(stat12).asLong();
			 }
		 }

		 public virtual int Size()
		 {
			  return _pipeline.Count;
		 }

		 public override void Panic( Exception cause )
		 {
			 lock ( this )
			 {
				  if ( _panic == null )
				  {
						_panic = cause;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (Step<?> step : pipeline)
						foreach ( Step<object> step in _pipeline )
						{
							 step.ReceivePanic( cause );
							 step.EndOfUpstream();
						}
				  }
				  else
				  {
						if ( !_panic.Equals( cause ) )
						{
							 _panic.addSuppressed( cause );
						}
				  }
			 }
		 }

		 public override void AssertHealthy()
		 {
			  if ( _panic != null )
			  {
					throwIfUnchecked( _panic );
					throw new Exception( _panic );
			  }
		 }

		 public override string ToString()
		 {
			  return this.GetType().Name + "[" + Name() + "]";
		 }

		 public override void Recycle( object batch )
		 {
			  if ( _shouldRecycle )
			  {
					_recycled.offer( batch );
			  }
		 }

		 public override T Reuse<T>( System.Func<T> fallback )
		 {
			  if ( _shouldRecycle )
			  {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") T result = (T) recycled.poll();
					T result = ( T ) _recycled.poll();
					if ( result != null )
					{
						 return result;
					}
			  }

			  return fallback();
		 }

		 public override void Close()
		 {
			  if ( _shouldRecycle )
			  {
					_recycled.clear();
			  }
		 }
	}

}