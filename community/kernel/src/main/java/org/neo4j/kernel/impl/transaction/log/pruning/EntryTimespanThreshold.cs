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
namespace Org.Neo4j.Kernel.impl.transaction.log.pruning
{

	public sealed class EntryTimespanThreshold : Threshold
	{
		 private readonly long _timeToKeepInMillis;
		 private readonly Clock _clock;

		 private long _lowerLimit;

		 internal EntryTimespanThreshold( Clock clock, TimeUnit timeUnit, long timeToKeep )
		 {
			  this._clock = clock;
			  this._timeToKeepInMillis = timeUnit.toMillis( timeToKeep );
		 }

		 public override void Init()
		 {
			  _lowerLimit = _clock.millis() - _timeToKeepInMillis;
		 }

		 public override bool Reached( File file, long version, LogFileInformation source )
		 {
			  try
			  {
					long firstStartRecordTimestamp = source.GetFirstStartRecordTimestamp( version );
					return firstStartRecordTimestamp >= 0 && firstStartRecordTimestamp < _lowerLimit;
			  }
			  catch ( IOException e )
			  {
					throw new Exception( e );
			  }
		 }
	}

}