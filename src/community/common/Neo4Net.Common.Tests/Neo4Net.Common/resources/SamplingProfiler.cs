using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
namespace Neo4Net.Resources
{

	internal class SamplingProfiler : Profiler
	{
		 private static readonly long _defaultSampleIntervalNanos = TimeUnit.MILLISECONDS.toNanos( 1 );

		 private readonly ConcurrentLinkedQueue<Thread> _samplerThreads = new ConcurrentLinkedQueue<Thread>();
		 private readonly AtomicBoolean _stopped = new AtomicBoolean();
		 private readonly ConcurrentDictionary<Thread, Sample> _samples = new ConcurrentDictionary<Thread, Sample>();
		 private readonly AtomicLong _sampleIntervalNanos = new AtomicLong( _defaultSampleIntervalNanos );
		 private readonly AtomicLong _underSampling = new AtomicLong();

		 public override void Reset()
		 {
			  _stopped.set( false );
			  _samples.Clear();
			  _underSampling.set( 0 );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void finish() throws InterruptedException
		 public override void Finish()
		 {
			  _stopped.set( true );
			  Thread thread;
			  while ( ( thread = _samplerThreads.poll() ) != null )
			  {
					thread.Interrupt();
					thread.Join();
			  }
		 }

		 public override void PrintProfile( PrintStream @out, string profileTitle )
		 {
			  @out.println( "### " + profileTitle );
			  if ( _underSampling.get() > 0 )
			  {
					long allSamplesTotal = _samples.reduceToLong( long.MaxValue, ( thread, sample ) => sample.get(), 0, (a, b) => a + b );
					@out.println( "Info: Did not achieve target sampling frequency. " + _underSampling + " of " + allSamplesTotal + " samples were delayed." );
			  }
			  foreach ( KeyValuePair<Thread, Sample> entry in _samples.SetOfKeyValuePairs() )
			  {
					Thread thread = entry.Key;
					Sample rootSample = entry.Value;
					rootSample.IntoOrdered();
					@out.println( "Profile (" + rootSample.get() + " samples) " + thread.Name );
					double total = rootSample.get();
					PrintSampleTree( @out, total, rootSample.OrderedChildren, 2 );
			  }
		 }

		 public virtual long SampleIntervalNanos
		 {
			 set
			 {
				  _sampleIntervalNanos.set( value );
			 }
		 }

		 public override Profiler_ProfiledInterval Profile( Thread threadToProfile, long initialDelayNanos )
		 {
			  long capturedSampleIntervalNanos = _sampleIntervalNanos.get();
			  long baseline = System.nanoTime();
			  Thread samplerThread = new Thread(() =>
			  {
				long nextSleepBaseline = initialDelayNanos > 0 ? Sleep( baseline, initialDelayNanos ) : baseline;
				Sample root = _samples.computeIfAbsent( threadToProfile, k => new Sample( null ) );
				while ( !_stopped.get() && threadToProfile.IsAlive )
				{
					 StackTraceElement[] frames = threadToProfile.StackTrace;
					 if ( Thread.CurrentThread.Interrupted ) // Avoid recording samples that overlap with the end of the profiling interval.
					 {
						  break;
					 }
					 Record( root, frames );
					 nextSleepBaseline = Sleep( nextSleepBaseline, capturedSampleIntervalNanos );
				}
			  });
			  _samplerThreads.add( samplerThread );
			  samplerThread.Name = "Sampler for " + threadToProfile.Name;
			  samplerThread.Priority = Thread.NORM_PRIORITY + 1;
			  samplerThread.Daemon = true;
			  samplerThread.Start();
			  return samplerThread.interrupt;
		 }

		 private long Sleep( long baselineNanos, long delayNanoes )
		 {
			  long nextBaseline = System.nanoTime();
			  long sleepNanos = delayNanoes - ( nextBaseline - baselineNanos );
			  if ( sleepNanos > 0 )
			  {
					LockSupport.parkNanos( this, sleepNanos );
			  }
			  else
			  {
					_underSampling.AndIncrement;
					Thread.yield(); // The sampler thread runs with slightly elevated priority, so we yield to give the profiled thread a chance to run.
			  }
			  return nextBaseline + delayNanoes;
		 }

		 private void Record( Sample root, StackTraceElement[] frames )
		 {
			  root.AndIncrement;
			  IDictionary<StackTraceElement, Sample> level = root.Children;
			  // Iterate sample in reverse, since index 0 is top of the stack (most recent method invocation) and we record bottom-to-top.
			  for ( int i = frames.Length - 1; i >= 0 ; i-- )
			  {
					StackTraceElement frame = frames[i];
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
					Sample sample = level.computeIfAbsent( frame, Sample::new );
					sample.AndIncrement;
					level = sample.Children;
			  }
		 }

		 private void PrintSampleTree( PrintStream @out, double total, PriorityQueue<Sample> children, int indent )
		 {
			  Sample child;
			  while ( ( child = children.poll() ) != null )
			  {
					for ( int i = 0; i < indent; i++ )
					{
						 @out.print( ' ' );
					}
					@out.printf( "%.2f%%: %s%n", child.get() / total * 100.0, child.StackTraceElement );
					PrintSampleTree( @out, total, child.OrderedChildren, indent + 2 );
			  }
		 }

		 private sealed class Sample : AtomicLong, IComparable<Sample>
		 {
			  internal readonly StackTraceElement StackTraceElement;
			  internal IDictionary<StackTraceElement, Sample> Children;
			  internal PriorityQueue<Sample> OrderedChildren;

			  internal Sample( StackTraceElement stackTraceElement )
			  {
					this.StackTraceElement = stackTraceElement;
					Children = new ConcurrentDictionary<StackTraceElement, Sample>();
			  }

			  public override int CompareTo( Sample o )
			  {
					// Higher count orders first.
					return Long.compare( o.get(), this.get() );
			  }

			  internal void IntoOrdered()
			  {
					OrderedChildren = new PriorityQueue<Sample>();
					OrderedChildren.addAll( Children.Values );
					Children.Clear();
					foreach ( Sample child in OrderedChildren )
					{
						 child.IntoOrdered();
					}
			  }
		 }
	}

}