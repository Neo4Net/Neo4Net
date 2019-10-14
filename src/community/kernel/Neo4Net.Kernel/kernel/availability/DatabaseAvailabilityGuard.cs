using System.Collections.Generic;
using System.Diagnostics;
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
namespace Neo4Net.Kernel.availability
{

	using Format = Neo4Net.Helpers.Format;
	using Neo4Net.Helpers;
	using Iterables = Neo4Net.Helpers.Collections.Iterables;
	using Log = Neo4Net.Logging.Log;

	/// <summary>
	/// Single database availability guard.
	/// </summary>
	/// <seealso cref= AvailabilityGuard </seealso>
	public class DatabaseAvailabilityGuard : AvailabilityGuard
	{
		 private const string DATABASE_AVAILABLE_MSG = "Fulfilling of requirement '%s' makes database %s available.";
		 private const string DATABASE_UNAVAILABLE_MSG = "Requirement `%s` makes database %s unavailable.";

		 private readonly AtomicInteger _requirementCount = new AtomicInteger( 0 );
		 private readonly ISet<AvailabilityRequirement> _blockingRequirements = new CopyOnWriteArraySet<AvailabilityRequirement>();
		 private readonly AtomicBoolean _isShutdown = new AtomicBoolean( false );
		 private readonly Listeners<AvailabilityListener> _listeners = new Listeners<AvailabilityListener>();
		 private readonly string _databaseName;
		 private readonly Clock _clock;
		 private readonly Log _log;

		 public DatabaseAvailabilityGuard( string databaseName, Clock clock, Log log )
		 {
			  this._databaseName = databaseName;
			  this._clock = clock;
			  this._log = log;
			  this._listeners.add( new LoggingAvailabilityListener( log, databaseName ) );
		 }

		 public override void Require( AvailabilityRequirement requirement )
		 {
			  if ( !_blockingRequirements.Add( requirement ) )
			  {
					return;
			  }

			  lock ( _requirementCount )
			  {
					if ( _requirementCount.AndIncrement == 0 && !_isShutdown.get() )
					{
						 _log.info( DATABASE_UNAVAILABLE_MSG, requirement(), _databaseName );
						 _listeners.notify( AvailabilityListener.unavailable );
					}
			  }
		 }

		 public override void Fulfill( AvailabilityRequirement requirement )
		 {
			  if ( !_blockingRequirements.remove( requirement ) )
			  {
					return;
			  }

			  lock ( _requirementCount )
			  {
					if ( _requirementCount.AndDecrement == 1 && !_isShutdown.get() )
					{
						 _log.info( DATABASE_AVAILABLE_MSG, requirement(), _databaseName );
						 _listeners.notify( AvailabilityListener.available );
					}
			  }
		 }

		 /// <summary>
		 /// Shutdown the guard. After this method is invoked, the database will always be considered unavailable.
		 /// </summary>
		 public virtual void Shutdown()
		 {
			  lock ( _requirementCount )
			  {
					if ( _isShutdown.getAndSet( true ) )
					{
						 return;
					}

					if ( _requirementCount.get() == 0 )
					{
						 _listeners.notify( AvailabilityListener.unavailable );
					}
			  }
		 }

		 public virtual bool Available
		 {
			 get
			 {
				  return Availability() == Availability.Available;
			 }
		 }

		 public virtual bool Shutdown
		 {
			 get
			 {
				  return Availability() == Availability.Shutdown;
			 }
		 }

		 public virtual bool isAvailable( long millis )
		 {
			  return Availability( millis ) == Availability.Available;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void checkAvailable() throws UnavailableException
		 public override void CheckAvailable()
		 {
			  Await( 0 );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void await(long millis) throws UnavailableException
		 public override void Await( long millis )
		 {
			  Availability availability = availability( millis );
			  if ( availability == Availability.Available )
			  {
					return;
			  }

			  string description = ( availability == Availability.Unavailable ) ? "Timeout waiting for database to become available and allow new transactions. Waited " + Format.duration( millis ) + ". " + DescribeWhoIsBlocking() : "Database not available because it's shutting down";
			  throw new UnavailableException( description );
		 }

		 private Availability Availability()
		 {
			  if ( _isShutdown.get() )
			  {
					return Availability.Shutdown;
			  }

			  int count = _requirementCount.get();
			  if ( count == 0 )
			  {
					return Availability.Available;
			  }

			  Debug.Assert( count > 0 );

			  return Availability.Unavailable;
		 }

		 private Availability Availability( long millis )
		 {
			  Availability availability = availability();
			  if ( availability != Availability.Unavailable )
			  {
					return availability;
			  }

			  long timeout = _clock.millis() + millis;
			  do
			  {
					try
					{
						 Thread.Sleep( 10 );
					}
					catch ( InterruptedException )
					{
						 Thread.interrupted();
						 break;
					}
					availability = availability();
			  } while ( availability == Availability.Unavailable && _clock.millis() < timeout );

			  return availability;
		 }

		 public override void AddListener( AvailabilityListener listener )
		 {
			  _listeners.add( listener );
		 }

		 public override void RemoveListener( AvailabilityListener listener )
		 {
			  _listeners.remove( listener );
		 }

		 /// <returns> a textual description of what components, if any, are blocking access </returns>
		 public virtual string DescribeWhoIsBlocking()
		 {
			  if ( _blockingRequirements.Count > 0 || _requirementCount.get() > 0 )
			  {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
					string causes = Iterables.join( ", ", Iterables.map( AvailabilityRequirement::description, _blockingRequirements ) );
					return _requirementCount.get() + " reasons for blocking: " + causes + ".";
			  }
			  return "No blocking components";
		 }

		 private enum Availability
		 {
			  Available,
			  Unavailable,
			  Shutdown
		 }

		 private class LoggingAvailabilityListener : AvailabilityListener
		 {
			  internal readonly Log Log;
			  internal readonly string DatabaseName;

			  internal LoggingAvailabilityListener( Log log, string databaseName )
			  {
					this.Log = log;
					this.DatabaseName = databaseName;
			  }

			  public override void Available()
			  {
					Log.info( "Database %s is ready.", DatabaseName );
			  }

			  public override void Unavailable()
			  {
					Log.info( "Database %s is unavailable.", DatabaseName );
			  }
		 }
	}

}