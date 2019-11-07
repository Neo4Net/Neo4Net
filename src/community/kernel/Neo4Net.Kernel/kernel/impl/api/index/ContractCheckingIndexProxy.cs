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
namespace Neo4Net.Kernel.Impl.Api.index
{

	using IOLimiter = Neo4Net.Io.pagecache.IOLimiter;
	using IndexEntryConflictException = Neo4Net.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using IndexUpdater = Neo4Net.Kernel.Api.Index.IndexUpdater;
	using DelegatingIndexUpdater = Neo4Net.Kernel.Impl.Api.index.updater.DelegatingIndexUpdater;

	/// <summary>
	/// <seealso cref="IndexProxy"/> layer that enforces the dynamic contract of <seealso cref="IndexProxy"/> (cf. Test)
	/// </summary>
	/// <seealso cref= Neo4Net.kernel.impl.api.index.IndexProxy </seealso>
	public class ContractCheckingIndexProxy : DelegatingIndexProxy
	{
		 /// <summary>
		 /// State machine for <seealso cref="IndexProxy proxies"/>
		 /// 
		 /// The logic of <seealso cref="ContractCheckingIndexProxy"/> hinges on the fact that all states
		 /// are always entered and checked in this order (States may be skipped though):
		 /// 
		 /// INIT > STARTING > STARTED > CLOSED
		 /// 
		 /// Valid state transitions are:
		 /// 
		 /// INIT -[:start]-> STARTING -[:implicit]-> STARTED -[:close|:drop]-> CLOSED
		 /// INIT -[:close] -> CLOSED
		 /// 
		 /// Additionally, <seealso cref="ContractCheckingIndexProxy"/> keeps track of the number of open
		 /// calls that started in STARTED state and are still running.  This allows us
		 /// to prevent calls to close() or drop() to go through while there are pending
		 /// commits.
		 /// 
		 /// </summary>
		 private enum State
		 {
			  Init,
			  Starting,
			  Started,
			  Closed
		 }

		 private readonly AtomicReference<State> _state;
		 private readonly AtomicInteger _openCalls;

		 public ContractCheckingIndexProxy( IndexProxy @delegate, bool started ) : base( @delegate )
		 {
			  this._state = new AtomicReference<State>( started ? State.Started : State.Init );
			  this._openCalls = new AtomicInteger( 0 );
		 }

		 public override void Start()
		 {
			  if ( _state.compareAndSet( State.Init, State.Starting ) )
			  {
					try
					{
						 base.Start();
					}
					finally
					{
						 this._state.set( State.Started );
					}
			  }
			  else
			  {
					throw new System.InvalidOperationException( "An IndexProxy can only be started once" );
			  }
		 }

		 public override IndexUpdater NewUpdater( IndexUpdateMode mode )
		 {
			  if ( IndexUpdateMode.Online == mode )
			  {
					OpenCall( "update" );
					return new DelegatingIndexUpdaterAnonymousInnerClass( this, base.NewUpdater( mode ) );
			  }
			  else
			  {
					return base.NewUpdater( mode );
			  }
		 }

		 private class DelegatingIndexUpdaterAnonymousInnerClass : DelegatingIndexUpdater
		 {
			 private readonly ContractCheckingIndexProxy _outerInstance;

			 public DelegatingIndexUpdaterAnonymousInnerClass( ContractCheckingIndexProxy outerInstance, IndexUpdater newUpdater ) : base( newUpdater )
			 {
				 this.outerInstance = outerInstance;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws Neo4Net.kernel.api.exceptions.index.IndexEntryConflictException
			 public override void close()
			 {
				  try
				  {
						@delegate.close();
				  }
				  finally
				  {
						outerInstance.closeCall();
				  }
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void force(Neo4Net.io.pagecache.IOLimiter ioLimiter) throws java.io.IOException
		 public override void Force( IOLimiter ioLimiter )
		 {
			  OpenCall( "force" );
			  try
			  {
					base.Force( ioLimiter );
			  }
			  finally
			  {
					CloseCall();
			  }
		 }

		 public override void Drop()
		 {
			  if ( _state.compareAndSet( State.Init, State.Closed ) )
			  {
					base.Drop();
					return;
			  }

			  if ( State.Starting == _state.get() )
			  {
					throw new System.InvalidOperationException( "Concurrent drop while creating index" );
			  }

			  if ( _state.compareAndSet( State.Started, State.Closed ) )
			  {
					WaitOpenCallsToClose();
					base.Drop();
					return;
			  }

			  throw new System.InvalidOperationException( "IndexProxy already closed" );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  if ( _state.compareAndSet( State.Init, State.Closed ) )
			  {
					base.Close();
					return;
			  }

			  if ( _state.compareAndSet( State.Starting, State.Closed ) )
			  {
					throw new System.InvalidOperationException( "Concurrent close while creating index" );
			  }

			  if ( _state.compareAndSet( State.Started, State.Closed ) )
			  {
					WaitOpenCallsToClose();
					base.Close();
					return;
			  }

			  throw new System.InvalidOperationException( "IndexProxy already closed" );
		 }

		 private void WaitOpenCallsToClose()
		 {
			  while ( _openCalls.intValue() > 0 )
			  {
					LockSupport.parkNanos( TimeUnit.MILLISECONDS.toNanos( 10 ) );
			  }
		 }

		 private void OpenCall( string name )
		 {
			  // do not open call unless we are in STARTED
			  if ( State.Started == _state.get() )
			  {
					// increment openCalls for closers to see
					_openCalls.incrementAndGet();
					// ensure that the previous increment actually gets seen by closers
					if ( State.Closed == _state.get() )
					{
						 throw new System.InvalidOperationException( "Cannot call " + name + "() after index has been closed" );
					}
			  }
			  else
			  {
					throw new System.InvalidOperationException( "Cannot call " + name + "() when index state is " + _state.get() );
			  }
		 }

		 private void CloseCall()
		 {
			  // rollback once the call finished or failed
			  _openCalls.decrementAndGet();
		 }
	}

}