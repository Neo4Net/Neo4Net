﻿/*
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
namespace Org.Neo4j.causalclustering.catchup.storecopy
{

	using Clocks = Org.Neo4j.Time.Clocks;

	public class MaximumTotalTime : TerminationCondition
	{
		 private readonly long _endTime;
		 private readonly Clock _clock;
		 private long _time;
		 private TimeUnit _timeUnit;

		 public MaximumTotalTime( long time, TimeUnit timeUnit ) : this( time, timeUnit, Clocks.systemClock() )
		 {
		 }

		 internal MaximumTotalTime( long time, TimeUnit timeUnit, Clock clock )
		 {
			  this._endTime = clock.millis() + timeUnit.toMillis(time);
			  this._clock = clock;
			  this._time = time;
			  this._timeUnit = timeUnit;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void assertContinue() throws StoreCopyFailedException
		 public override void AssertContinue()
		 {
			  if ( _clock.millis() > _endTime )
			  {
					throw new StoreCopyFailedException( format( "Maximum time passed %d %s. Not allowed to continue", _time, _timeUnit ) );
			  }
		 }
	}

}