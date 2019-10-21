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
namespace Neo4Net.Kernel.impl.util.collection
{

	using Neo4Net.Functions;
	using IJobScheduler = Neo4Net.Scheduler.JobScheduler;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static System.currentTimeMillis;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.Format.duration;

	/// <summary>
	/// A concurrent repository that allows users to manage objects with a specified timeout on idleness.
	/// The repository owns the lifecycle of it's objects, granting clients exclusive access to them via its
	/// acquire/release methods.
	/// 
	/// The <seealso cref="run()"/> method here performs one "sweep", checking for idle entries to reap. You will want to trigger
	/// that run on a recurring basis, for instance using <seealso cref="JobScheduler"/>.
	/// </summary>
	public class TimedRepository<KEY, VALUE> : ThreadStart
	{
		 private readonly ConcurrentMap<KEY, Entry> _repo = new ConcurrentDictionary<KEY, Entry>();
		 private readonly IFactory<VALUE> _factory;
		 private readonly System.Action<VALUE> _reaper;
		 private readonly long _timeout;
		 private readonly Clock _clock;

		 private class Entry
		 {
			 private readonly TimedRepository<KEY, VALUE> _outerInstance;

			  internal const int IDLE = 0;
			  internal const int IN_USE = 1;
			  internal const int MARKED_FOR_END = 2;

			  internal readonly AtomicInteger State = new AtomicInteger( IDLE );
			  internal readonly VALUE Value;
			  internal volatile long LatestActivityTimestamp;

			  internal Entry( TimedRepository<KEY, VALUE> outerInstance, VALUE value )
			  {
				  this._outerInstance = outerInstance;
					this.Value = value;
					this.LatestActivityTimestamp = outerInstance.clock.millis();
			  }

			  public virtual bool Acquire()
			  {
					return State.compareAndSet( IDLE, IN_USE );
			  }

			  /// <summary>
			  /// Calling this is only allowed if you have previously acquired this entry. </summary>
			  /// <returns> true if the release was successful, false if this entry has been marked for removal, and thus is not
			  /// allowed to be released back into the public. </returns>
			  public virtual bool Release()
			  {
					LatestActivityTimestamp = outerInstance.clock.millis();
					return State.compareAndSet( IN_USE, IDLE );
			  }

			  public virtual bool MarkForEndingIfInUse()
			  {
					return State.compareAndSet( IN_USE, MARKED_FOR_END );
			  }

			  public virtual bool MarkedForEnding
			  {
				  get
				  {
						return State.get() == MARKED_FOR_END;
				  }
			  }

			  public override string ToString()
			  {
					return format( "%s[%s last accessed at %d (%s ago)", this.GetType().Name, Value, LatestActivityTimestamp, duration(currentTimeMillis() - LatestActivityTimestamp) );
			  }
		 }

		 public TimedRepository( IFactory<VALUE> provider, System.Action<VALUE> reaper, long timeout, Clock clock )
		 {
			  this._factory = provider;
			  this._reaper = reaper;
			  this._timeout = timeout;
			  this._clock = clock;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void begin(KEY key) throws ConcurrentAccessException
		 public virtual void Begin( KEY key )
		 {
			  VALUE instance = _factory.newInstance();
			  Entry existing;
			  if ( ( existing = _repo.putIfAbsent( key, new Entry( this, instance ) ) ) != null )
			  {
					_reaper.accept( instance ); // Need to clear up our optimistically allocated value
					throw new ConcurrentAccessException( string.Format( "Cannot begin '{0}', because {1} with that key already exists.", key, existing ) );
			  }
		 }

		 /// <summary>
		 /// End the life of a stored entry. If the entry is currently in use, it will be thrown out as soon as the other client
		 /// is done with it.
		 /// </summary>
		 public virtual VALUE End( KEY key )
		 {
			  while ( true )
			  {
					Entry entry = _repo.get( key );
					if ( entry == null )
					{
						 return default( VALUE );
					}

					// Ending the life of an entry is somewhat complicated, because we promise the callee here that if someone
					// else is concurrently using the entry, we will ensure that either we or the other user will end the entry
					// life when the other user is done with it.

					// First, assume the entry is in use and try and mark it to be ended by the other user
					if ( entry.MarkForEndingIfInUse() )
					{
						 // The entry was indeed in use, and we successfully marked it to be ended. That's all we need to do here,
						 // the other user will see the ending flag when releasing the entry.
						 return entry.Value;
					}

					// Marking it for ending failed, likely because the entry is currently idle - lets try and just acquire it
					// and throw it out ourselves
					if ( entry.Acquire() )
					{
						 // Got it, just throw it away
						 End0( key, entry.Value );
						 return entry.Value;
					}

					// We didn't manage to mark this for ending, and we didn't manage to acquire it to end it ourselves, which
					// means either we're racing with another thread using it (and we just need to retry), or it's already
					// marked for ending. In the latter case, we can bail here.
					if ( entry.MarkedForEnding )
					{
						 // Someone did indeed manage to mark it for ending, which means it will be thrown out (or has already).
						 return entry.Value;
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public VALUE acquire(KEY key) throws NoSuchEntryException, ConcurrentAccessException
		 public virtual VALUE Acquire( KEY key )
		 {
			  Entry entry = _repo.get( key );
			  if ( entry == null )
			  {
					throw new NoSuchEntryException( string.Format( "Cannot access '{0}', no such entry exists.", key ) );
			  }
			  if ( entry.Acquire() )
			  {
					return entry.Value;
			  }
			  throw new ConcurrentAccessException( string.Format( "Cannot access '{0}', because another client is currently using it.", key ) );
		 }

		 public virtual void Release( KEY key )
		 {
			  Entry entry = _repo.get( key );
			  if ( entry != null && !entry.Release() )
			  {
					// This happens when another client has asked that this entry be ended while we were using it, leaving us
					// a note to not release the object back to the public, and to end its life when we are done with it.
					End0( key, entry.Value );
			  }
		 }

		 public virtual ISet<KEY> Keys()
		 {
			  return _repo.Keys;
		 }

		 public override void Run()
		 {
			  long maxAllowedAge = _clock.millis() - _timeout;
			  foreach ( KEY key in Keys() )
			  {
					Entry entry = _repo.get( key );
					if ( ( entry != null ) && ( entry.LatestActivityTimestamp < maxAllowedAge ) )
					{
						 if ( ( entry.LatestActivityTimestamp < maxAllowedAge ) && entry.Acquire() )
						 {
							  End0( key, entry.Value );
						 }
					}
			  }
		 }

		 private void End0( KEY key, VALUE value )
		 {
			  _repo.remove( key );
			  _reaper.accept( value );
		 }
	}

}