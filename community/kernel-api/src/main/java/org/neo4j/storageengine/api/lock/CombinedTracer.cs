using System;

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
namespace Org.Neo4j.Storageengine.Api.@lock
{

	/// <summary>
	/// A <seealso cref="LockTracer"/> that combines multiple <seealso cref="LockTracer tracers"/> into one, invoking each of them for
	/// the <seealso cref="LockTracer.waitForLock(bool, ResourceType, long...) wait events"/> received.
	/// <para>
	/// This is used for when there is a stack of queries in a transaction, or when a system-configured tracer combines with
	/// the query specific tracers.
	/// </para>
	/// </summary>
	internal sealed class CombinedTracer : LockTracer
	{
		 private readonly LockTracer[] _tracers;

		 internal CombinedTracer( params LockTracer[] tracers )
		 {
			  this._tracers = tracers;
		 }

		 public override LockWaitEvent WaitForLock( bool exclusive, ResourceType resourceType, params long[] resourceIds )
		 {
			  LockWaitEvent[] events = new LockWaitEvent[_tracers.Length];
			  for ( int i = 0; i < events.Length; i++ )
			  {
					events[i] = _tracers[i].waitForLock( exclusive, resourceType, resourceIds );
			  }
			  return new CombinedEvent( events );
		 }

		 public override LockTracer Combine( LockTracer tracer )
		 {
			  if ( tracer == NONE )
			  {
					return this;
			  }
			  LockTracer[] tracers;
			  if ( tracer is CombinedTracer )
			  {
					LockTracer[] those = ( ( CombinedTracer ) tracer )._tracers;
					tracers = Arrays.copyOf( this._tracers, this._tracers.Length + those.Length );
					Array.Copy( those, 0, tracers, this._tracers.Length, those.Length );
			  }
			  else
			  {
					tracers = Arrays.copyOf( this._tracers, this._tracers.Length + 1 );
					tracers[this._tracers.Length] = tracer;
			  }
			  return new CombinedTracer( tracers );
		 }

		 private class CombinedEvent : LockWaitEvent
		 {
			  internal readonly LockWaitEvent[] Events;

			  internal CombinedEvent( LockWaitEvent[] events )
			  {
					this.Events = events;
			  }

			  public override void Close()
			  {
					foreach ( LockWaitEvent @event in Events )
					{
						 @event.Close();
					}
			  }
		 }
	}

}