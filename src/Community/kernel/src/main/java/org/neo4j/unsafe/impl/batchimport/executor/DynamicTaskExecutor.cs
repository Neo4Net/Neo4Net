using System;
using System.Diagnostics;
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
namespace Neo4Net.@unsafe.Impl.Batchimport.executor
{

	using Suppliers = Neo4Net.Function.Suppliers;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Integer.max;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Integer.min;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.Exceptions.SILENT_UNCAUGHT_EXCEPTION_HANDLER;

	/// <summary>
	/// Implementation of <seealso cref="TaskExecutor"/> with a maximum queue size and where each processor is a dedicated
	/// <seealso cref="System.Threading.Thread"/> pulling queued tasks and executing them.
	/// </summary>
	public class DynamicTaskExecutor<LOCAL> : TaskExecutor<LOCAL>
	{
		 private readonly BlockingQueue<Task<LOCAL>> _queue;
		 private readonly ParkStrategy _parkStrategy;
		 private readonly string _processorThreadNamePrefix;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private volatile Processor[] processors = (Processor[]) Array.newInstance(Processor.class, 0);
		 private volatile Processor[] _processors = ( Processor[] ) Array.CreateInstance( typeof( Processor ), 0 );
		 private volatile bool _shutDown;
		 private readonly AtomicReference<Exception> _panic = new AtomicReference<Exception>();
		 private readonly System.Func<LOCAL> _initialLocalState;
		 private readonly int _maxProcessorCount;

		 public DynamicTaskExecutor( int initialProcessorCount, int maxProcessorCount, int maxQueueSize, ParkStrategy parkStrategy, string processorThreadNamePrefix ) : this( initialProcessorCount, maxProcessorCount, maxQueueSize, parkStrategy, processorThreadNamePrefix, Suppliers.singleton( null ) )
		 {
		 }

		 public DynamicTaskExecutor( int initialProcessorCount, int maxProcessorCount, int maxQueueSize, ParkStrategy parkStrategy, string processorThreadNamePrefix, System.Func<LOCAL> initialLocalState )
		 {
			  this._maxProcessorCount = maxProcessorCount == 0 ? int.MaxValue : maxProcessorCount;

			  Debug.Assert( this._maxProcessorCount >= initialProcessorCount, "Unexpected initial processor count " + initialProcessorCount + " for max " + maxProcessorCount );

			  this._parkStrategy = parkStrategy;
			  this._processorThreadNamePrefix = processorThreadNamePrefix;
			  this._initialLocalState = initialLocalState;
			  this._queue = new ArrayBlockingQueue<Task<LOCAL>>( maxQueueSize );
			  Processors( initialProcessorCount );
		 }

		 public override int Processors( int delta )
		 {
			  if ( _shutDown || delta == 0 )
			  {
					return _processors.Length;
			  }

			  lock ( this )
			  {
					if ( _shutDown )
					{
						 return _processors.Length;
					}

					int requestedNumber = _processors.Length + delta;
					if ( delta > 0 )
					{
						 requestedNumber = min( requestedNumber, _maxProcessorCount );
						 if ( requestedNumber > _processors.Length )
						 {
							  Processor[] newProcessors = Arrays.copyOf( _processors, requestedNumber );
							  for ( int i = _processors.Length; i < requestedNumber; i++ )
							  {
									newProcessors[i] = new Processor( this, _processorThreadNamePrefix + "-" + i );
							  }
							  this._processors = newProcessors;
						 }
					}
					else
					{
						 requestedNumber = max( 1, requestedNumber );
						 if ( requestedNumber < _processors.Length )
						 {
							  Processor[] newProcessors = Arrays.copyOf( _processors, requestedNumber );
							  for ( int i = newProcessors.Length; i < _processors.Length; i++ )
							  {
									_processors[i].processorShutDown = true;
							  }
							  this._processors = newProcessors;
						 }
					}
					return _processors.Length;
			  }
		 }

		 public override void Submit( Task<LOCAL> task )
		 {
			  AssertHealthy();
			  try
			  {
					while ( !_queue.offer( task, 10, MILLISECONDS ) )
					{ // Then just stay here and try
						 AssertHealthy();
					}
			  }
			  catch ( InterruptedException )
			  {
					Thread.CurrentThread.Interrupt();
			  }
		 }

		 public override void AssertHealthy()
		 {
			  Exception panic = this._panic.get();
			  if ( panic != null )
			  {
					throw new TaskExecutionPanicException( "Executor has been shut down in panic", panic );
			  }
		 }

		 public override void ReceivePanic( Exception cause )
		 {
			  _panic.compareAndSet( null, cause );
		 }

		 public override void Close()
		 {
			 lock ( this )
			 {
				  if ( _shutDown )
				  {
						return;
				  }
      
				  while ( !_queue.Empty && _panic.get() == null )
				  {
						ParkAWhile();
				  }
				  this._shutDown = true;
				  while ( AnyAlive() && _panic.get() == null )
				  {
						ParkAWhile();
				  }
			 }
		 }

		 private bool AnyAlive()
		 {
			  foreach ( Processor processor in _processors )
			  {
					if ( processor.IsAlive )
					{
						 return true;
					}
			  }
			  return false;
		 }

		 private void ParkAWhile()
		 {
			  _parkStrategy.park( Thread.CurrentThread );
		 }

		 private class Processor : Thread
		 {
			 private readonly DynamicTaskExecutor<LOCAL> _outerInstance;

			  // In addition to the global shutDown flag in the executor each processor has a local flag
			  // so that an individual processor can be shut down, for example when reducing number of processors
			  internal volatile bool ProcessorShutDown;

			  internal Processor( DynamicTaskExecutor<LOCAL> outerInstance, string name ) : base( name )
			  {
				  this._outerInstance = outerInstance;
					UncaughtExceptionHandler = SILENT_UNCAUGHT_EXCEPTION_HANDLER;
					start();
			  }

			  public override void Run()
			  {
					// Initialized here since it's the thread itself that needs to call it
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final LOCAL threadLocalState = initialLocalState.get();
					LOCAL threadLocalState = outerInstance.initialLocalState.get();
					while ( !outerInstance.shutDown && !ProcessorShutDown )
					{
						 Task<LOCAL> task;
						 try
						 {
							  task = outerInstance.queue.poll( 10, MILLISECONDS );
						 }
						 catch ( InterruptedException )
						 {
							  Thread.interrupted();
							  break;
						 }

						 if ( task != null )
						 {
							  try
							  {
									task( threadLocalState );
							  }
							  catch ( Exception e )
							  {
									outerInstance.ReceivePanic( e );
									outerInstance.Close();
									throw new Exception( e );
							  }
						 }
					}
			  }
		 }
	}

}