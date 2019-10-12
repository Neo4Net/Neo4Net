/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
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
namespace Org.Neo4j.causalclustering.helper
{

	public class ConstantTimeTimeoutStrategy : TimeoutStrategy
	{
		 private readonly TimeoutStrategy_Timeout _constantTimeout;

		 public ConstantTimeTimeoutStrategy( long backoffTime, TimeUnit timeUnit )
		 {
			  long backoffTimeMillis = timeUnit.toMillis( backoffTime );

			  _constantTimeout = new TimeoutStrategy_TimeoutAnonymousInnerClass( this, backoffTimeMillis );
		 }

		 private class TimeoutStrategy_TimeoutAnonymousInnerClass : TimeoutStrategy_Timeout
		 {
			 private readonly ConstantTimeTimeoutStrategy _outerInstance;

			 private long _backoffTimeMillis;

			 public TimeoutStrategy_TimeoutAnonymousInnerClass( ConstantTimeTimeoutStrategy outerInstance, long backoffTimeMillis )
			 {
				 this.outerInstance = outerInstance;
				 this._backoffTimeMillis = backoffTimeMillis;
			 }

			 public long Millis
			 {
				 get
				 {
					  return _backoffTimeMillis;
				 }
			 }

			 public void increment()
			 {
			 }
		 }

		 public ConstantTimeTimeoutStrategy( Duration backoffTime ) : this( backoffTime.toMillis(), TimeUnit.MILLISECONDS )
		 {
		 }

		 public override TimeoutStrategy_Timeout NewTimeout()
		 {
			  return _constantTimeout;
		 }
	}

}