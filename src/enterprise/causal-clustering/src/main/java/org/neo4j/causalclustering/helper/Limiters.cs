using System.Collections.Concurrent;
using System.Threading;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.causalclustering.helper
{

	using Clocks = Neo4Net.Time.Clocks;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("WeakerAccess") public class Limiters
	public class Limiters
	{
		 private readonly ConcurrentDictionary<object, System.Action<ThreadStart>> _caps = new ConcurrentDictionary<object, System.Action<ThreadStart>>();
		 private readonly Clock _clock;

		 public Limiters()
		 {
			  this._clock = Clocks.systemClock();
		 }

		 public Limiters( Clock clock )
		 {
			  this._clock = clock;
		 }

		 /// <summary>
		 /// Rate limits calls under the specified handle.
		 /// </summary>
		 /// <param name="handle"> A unique handle. </param>
		 /// <param name="minInterval"> The minimum interval between invocations. </param>
		 /// <returns> A rate limited consumer of <seealso cref="System.Threading.ThreadStart"/>s. </returns>
		 public virtual System.Action<ThreadStart> RateLimiter( object handle, Duration minInterval )
		 {
			  return _caps.computeIfAbsent( handle, ignored => Limiters.RateLimiter( minInterval, _clock ) );
		 }

		 public static System.Action<ThreadStart> RateLimiter( Duration minInterval )
		 {
			  return RateLimiter( minInterval, Clocks.systemClock() );
		 }

		 /// <summary>
		 /// Rate limits calls.
		 /// </summary>
		 /// <param name="minInterval"> The minimum interval between invocations. </param>
		 /// <returns> A rate limited consumer of <seealso cref="System.Threading.ThreadStart"/>s. </returns>
		 public static System.Action<ThreadStart> RateLimiter( Duration minInterval, Clock clock )
		 {
			  return new ActionAnonymousInnerClass( minInterval, clock );
		 }

		 private class ActionAnonymousInnerClass : System.Action<ThreadStart>
		 {
			 private Duration _minInterval;
			 private Clock _clock;

			 public ActionAnonymousInnerClass( Duration minInterval, Clock clock )
			 {
				 this._minInterval = minInterval;
				 this._clock = clock;
			 }

			 internal AtomicReference<Instant> lastRunRef = new AtomicReference<Instant>();

			 public override void accept( ThreadStart operation )
			 {
				  Instant now = _clock.instant();
				  Instant lastRun = lastRunRef.get();

				  if ( lastRun == null )
				  {
						if ( lastRunRef.compareAndSet( null, now ) )
						{
							 operation.run();
						}
						return;
				  }

				  if ( lastRun.plus( _minInterval ).isAfter( now ) )
				  {
						return;
				  }

				  if ( lastRunRef.compareAndSet( lastRun, now ) )
				  {
						operation.run();
				  }
			 }
		 }
	}

}