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
namespace Neo4Net.@internal.Collector
{

	using InvalidArgumentsException = Neo4Net.Kernel.Api.Exceptions.InvalidArgumentsException;

	/// <summary>
	/// Base class for managing state transitions of data-collector daemons.
	/// </summary>
	internal abstract class CollectorStateMachine<DATA>
	{
		 private enum State
		 {
			  Idle,
			  Collecting
		 }

		 internal sealed class Status
		 {
			  internal readonly string Message;

			  internal Status( string message )
			  {
					this.Message = message;
			  }
		 }

		 internal sealed class Result
		 {
			  internal readonly bool Success;
			  internal readonly string Message;

			  internal Result( bool success, string message )
			  {
					this.Success = success;
					this.Message = message;
			  }
		 }

		 internal static Result Success( string message )
		 {
			  return new Result( true, message );
		 }

		 internal static Result Error( string message )
		 {
			  return new Result( false, message );
		 }

		 private State _state;
		 private long _collectionId;
		 private readonly bool _canGetDataWhileCollecting;

		 internal CollectorStateMachine( bool canGetDataWhileCollecting )
		 {
			  this._canGetDataWhileCollecting = canGetDataWhileCollecting;
			  _state = State.Idle;
		 }

		 public virtual Status Status()
		 {
			 lock ( this )
			 {
				  State state = this._state;
				  switch ( state )
				  {
				  case Neo4Net.@internal.Collector.CollectorStateMachine.State.Idle:
						return new Status( "idle" );
				  case Neo4Net.@internal.Collector.CollectorStateMachine.State.Collecting:
						return new Status( "collecting" );
				  default:
						throw new System.InvalidOperationException( "Unknown state " + state );
				  }
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public synchronized Result collect(java.util.Map<String,Object> config) throws org.neo4j.kernel.api.exceptions.InvalidArgumentsException
		 public virtual Result Collect( IDictionary<string, object> config )
		 {
			 lock ( this )
			 {
				  switch ( _state )
				  {
				  case Neo4Net.@internal.Collector.CollectorStateMachine.State.Idle:
						_state = State.Collecting;
						_collectionId++;
						return DoCollect( config, _collectionId );
				  case Neo4Net.@internal.Collector.CollectorStateMachine.State.Collecting:
						return Success( "Collection is already ongoing." );
				  default:
						throw new System.InvalidOperationException( "Unknown state " + _state );
				  }
			 }
		 }

		 public virtual Result Stop( long collectionIdToStop )
		 {
			 lock ( this )
			 {
				  switch ( _state )
				  {
				  case Neo4Net.@internal.Collector.CollectorStateMachine.State.Idle:
						return Success( "Collector is idle, no collection ongoing." );
				  case Neo4Net.@internal.Collector.CollectorStateMachine.State.Collecting:
						if ( this._collectionId <= collectionIdToStop )
						{
							 _state = State.Idle;
							 return DoStop();
						}
						return Success( string.Format( "Collection event {0:D} has already been stopped, a new collection event is ongoing.", collectionIdToStop ) );
				  default:
						throw new System.InvalidOperationException( "Unknown state " + _state );
				  }
			 }
		 }

		 public virtual Result Clear()
		 {
			 lock ( this )
			 {
				  switch ( _state )
				  {
				  case Neo4Net.@internal.Collector.CollectorStateMachine.State.Idle:
						return DoClear();
				  case Neo4Net.@internal.Collector.CollectorStateMachine.State.Collecting:
						return Error( "Collected data cannot be cleared while collecting." );
				  default:
						throw new System.InvalidOperationException( "Unknown state " + _state );
				  }
			 }
		 }

		 public virtual DATA Data
		 {
			 get
			 {
				 lock ( this )
				 {
					  switch ( _state )
					  {
					  case Neo4Net.@internal.Collector.CollectorStateMachine.State.Idle:
							return DoGetData();
					  case Neo4Net.@internal.Collector.CollectorStateMachine.State.Collecting:
							if ( _canGetDataWhileCollecting )
							{
								 return DoGetData();
							}
							throw new System.InvalidOperationException( "Collector is still collecting." );
					  default:
							throw new System.InvalidOperationException( "Unknown state " + _state );
					  }
				 }
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected abstract Result doCollect(java.util.Map<String,Object> config, long collectionId) throws org.neo4j.kernel.api.exceptions.InvalidArgumentsException;
		 protected internal abstract Result DoCollect( IDictionary<string, object> config, long collectionId );
		 protected internal abstract Result DoStop();
		 protected internal abstract Result DoClear();
		 protected internal abstract DATA DoGetData();
	}

}