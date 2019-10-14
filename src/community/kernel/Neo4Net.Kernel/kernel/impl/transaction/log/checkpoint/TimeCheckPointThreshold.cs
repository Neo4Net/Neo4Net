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
namespace Neo4Net.Kernel.impl.transaction.log.checkpoint
{

	using SystemNanoClock = Neo4Net.Time.SystemNanoClock;

	internal class TimeCheckPointThreshold : AbstractCheckPointThreshold
	{
		 private volatile long _lastCheckPointedTransactionId;
		 private volatile long _lastCheckPointTimeNanos;

		 private readonly long _timeMillisThreshold;
		 private readonly SystemNanoClock _clock;

		 internal TimeCheckPointThreshold( long thresholdMillis, SystemNanoClock clock ) : base( "time threshold" )
		 {
			  this._timeMillisThreshold = thresholdMillis;
			  this._clock = clock;
			  // The random start offset means database in a cluster will not all check-point at the same time.
			  long randomStartOffset = thresholdMillis > 0 ? ThreadLocalRandom.current().nextLong(thresholdMillis) : 0;
			  this._lastCheckPointTimeNanos = clock.Nanos() + TimeUnit.MILLISECONDS.toNanos(randomStartOffset);
		 }

		 public override void Initialize( long transactionId )
		 {
			  _lastCheckPointedTransactionId = transactionId;
		 }

		 protected internal override bool ThresholdReached( long lastCommittedTransactionId )
		 {
			  return lastCommittedTransactionId > _lastCheckPointedTransactionId && _clock.nanos() - _lastCheckPointTimeNanos >= TimeUnit.MILLISECONDS.toNanos(_timeMillisThreshold);
		 }

		 public override void CheckPointHappened( long transactionId )
		 {
			  _lastCheckPointTimeNanos = _clock.nanos();
			  _lastCheckPointedTransactionId = transactionId;
		 }

		 public override long CheckFrequencyMillis()
		 {
			  return _timeMillisThreshold;
		 }
	}

}