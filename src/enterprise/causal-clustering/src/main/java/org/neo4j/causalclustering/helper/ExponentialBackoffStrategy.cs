using System;
using System.Diagnostics;

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

	/// <summary>
	/// Exponential backoff strategy helper class. Exponent is always 2.
	/// </summary>
	public class ExponentialBackoffStrategy : TimeoutStrategy
	{
		 private readonly long _initialBackoffTimeMillis;
		 private readonly long _upperBoundBackoffTimeMillis;

		 public ExponentialBackoffStrategy( long initialBackoffTime, long upperBoundBackoffTime, TimeUnit timeUnit )
		 {
			  Debug.Assert( initialBackoffTime <= upperBoundBackoffTime );

			  this._initialBackoffTimeMillis = timeUnit.toMillis( initialBackoffTime );
			  this._upperBoundBackoffTimeMillis = timeUnit.toMillis( upperBoundBackoffTime );
		 }

		 public ExponentialBackoffStrategy( Duration initialBackoffTime, Duration upperBoundBackoffTime ) : this( initialBackoffTime.toMillis(), upperBoundBackoffTime.toMillis(), TimeUnit.MILLISECONDS )
		 {
		 }

		 public override TimeoutStrategy_Timeout NewTimeout()
		 {
			  return new TimeoutStrategy_TimeoutAnonymousInnerClass( this );
		 }

		 private class TimeoutStrategy_TimeoutAnonymousInnerClass : TimeoutStrategy_Timeout
		 {
			 private readonly ExponentialBackoffStrategy _outerInstance;

			 public TimeoutStrategy_TimeoutAnonymousInnerClass( ExponentialBackoffStrategy outerInstance )
			 {
				 this.outerInstance = outerInstance;
				 backoffTimeMillis = outerInstance.initialBackoffTimeMillis;
			 }

			 private long backoffTimeMillis;

			 public long Millis
			 {
				 get
				 {
					  return backoffTimeMillis;
				 }
			 }

			 public void increment()
			 {
				  backoffTimeMillis = Math.Min( backoffTimeMillis * 2, _outerInstance.upperBoundBackoffTimeMillis );
			 }
		 }
	}

}