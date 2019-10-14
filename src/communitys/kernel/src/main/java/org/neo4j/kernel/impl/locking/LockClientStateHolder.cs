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
namespace Neo4Net.Kernel.impl.locking
{

	/// <summary>
	/// State control class for Locks.Clients.
	/// Client state represent current Locks.Client state: <b>ACTIVE/PREPARE/STOPPED</b> and number of active clients.
	/// <p/>
	/// Client states are:
	/// <ul>
	/// <li><b>ACTIVE</b> state of fully functional locks client without any restriction or operations limitations.</li>
	/// <li><b>PREPARE</b> state prevents transition into STOPPED state, unless forced as part of closing the lock
	/// client.</li>
	/// <li><b>STOPPED</b> all current lock acquisitions will be interrupted/terminated without obtaining corresponding
	/// lock, new acquisitions will not be possible anymore, all locks that client holds are preserved.</li>
	/// </ul>
	/// </summary>
	public sealed class LockClientStateHolder
	{
		 private const int FLAG_BITS = 2;
		 private static readonly int _clientBits = ( sizeof( int ) * 8 ) - FLAG_BITS;
		 private static readonly int _stopped = 1 << _clientBits;
		 private static readonly int _prepare = 1 << _clientBits - 1;
		 private static readonly int _stateBitMask = _stopped | _prepare;
		 private const int INITIAL_STATE = 0;
		 private AtomicInteger _clientState = new AtomicInteger( INITIAL_STATE );

		 /// <summary>
		 /// Check if we still have any active client
		 /// </summary>
		 /// <returns> true if have any open client, false otherwise. </returns>
		 public bool HasActiveClients()
		 {
			  return GetActiveClients( _clientState.get() ) > 0;
		 }

		 /// <summary>
		 /// Move the client to the PREPARE state, unless it is already STOPPED.
		 /// </summary>
		 public void Prepare( Locks_Client client )
		 {
			  int currentValue;
			  int newValue;
			  do
			  {
					currentValue = _clientState.get();
					if ( IsStopped( currentValue ) )
					{
						 throw new LockClientStoppedException( client );
					}
					newValue = StateWithNewStatus( currentValue, _prepare );
			  } while ( !_clientState.compareAndSet( currentValue, newValue ) );
		 }

		 /// <summary>
		 /// Move the client to STOPPED, unless it is already in PREPARE.
		 /// </summary>
		 public bool StopClient()
		 {
			  int currentValue;
			  int newValue;
			  do
			  {
					currentValue = _clientState.get();
					if ( IsPrepare( currentValue ) )
					{
						 return false; // Can't stop clients that are in PREPARE
					}
					newValue = StateWithNewStatus( currentValue, _stopped );
			  } while ( !_clientState.compareAndSet( currentValue, newValue ) );
			  return true;
		 }

		 /// <summary>
		 /// Move the client to STOPPED as part of closing the current client, regardless of what state it is currently in.
		 /// </summary>
		 public void CloseClient()
		 {
			  int currentValue;
			  int newValue;
			  do
			  {
					currentValue = _clientState.get();
					newValue = StateWithNewStatus( currentValue, _stopped );
			  } while ( !_clientState.compareAndSet( currentValue, newValue ) );
		 }

		 /// <summary>
		 /// Increment active number of clients that use current state instance.
		 /// </summary>
		 /// <param name="client"> the locks client associated with this state; used only to create pretty exception
		 /// with <seealso cref="LockClientStoppedException.LockClientStoppedException(Locks.Client)"/>. </param>
		 /// <exception cref="LockClientStoppedException"> when stopped. </exception>
		 public void IncrementActiveClients( Locks_Client client )
		 {
			  int currentState;
			  do
			  {
					currentState = _clientState.get();
					if ( IsStopped( currentState ) )
					{
						 throw new LockClientStoppedException( client );
					}
			  } while ( !_clientState.compareAndSet( currentState, IncrementActiveClients( currentState ) ) );
		 }

		 /// <summary>
		 /// Decrement number of active clients that use current client state object.
		 /// </summary>
		 public void DecrementActiveClients()
		 {
			  int currentState;
			  do
			  {
					currentState = _clientState.get();
			  } while ( !_clientState.compareAndSet( currentState, DecrementActiveClients( currentState ) ) );
		 }

		 /// <summary>
		 /// Check if stopped
		 /// </summary>
		 /// <returns> true if client is stopped, false otherwise </returns>
		 public bool Stopped
		 {
			 get
			 {
				  return IsStopped( _clientState.get() );
			 }
		 }

		 /// <summary>
		 /// Reset state to initial state disregard any current state or number of active clients
		 /// </summary>
		 public void Reset()
		 {
			  _clientState.set( INITIAL_STATE );
		 }

		 private bool IsPrepare( int clientState )
		 {
			  return GetStatus( clientState ) == _prepare;
		 }

		 private bool isStopped( int clientState )
		 {
			  return GetStatus( clientState ) == _stopped;
		 }

		 private int GetStatus( int clientState )
		 {
			  return clientState & _stateBitMask;
		 }

		 private int GetActiveClients( int clientState )
		 {
			  return clientState & ~_stateBitMask;
		 }

		 private int StateWithNewStatus( int clientState, int newStatus )
		 {
			  return newStatus | GetActiveClients( clientState );
		 }

		 private int IncrementActiveClients( int clientState )
		 {
			  return GetStatus( clientState ) | Math.incrementExact( GetActiveClients( clientState ) );
		 }

		 private int DecrementActiveClients( int clientState )
		 {
			  return GetStatus( clientState ) | Math.decrementExact( GetActiveClients( clientState ) );
		 }
	}

}