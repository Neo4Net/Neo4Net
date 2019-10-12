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
namespace Org.Neo4j.Kernel.impl.transaction.log.checkpoint
{
	internal class CountCommittedTransactionThreshold : AbstractCheckPointThreshold
	{
		 private readonly int _notificationThreshold;

		 private volatile long _nextTransactionIdTarget;

		 internal CountCommittedTransactionThreshold( int notificationThreshold ) : base( "tx count threshold" )
		 {
			  this._notificationThreshold = notificationThreshold;
		 }

		 public override void Initialize( long transactionId )
		 {
			  _nextTransactionIdTarget = transactionId + _notificationThreshold;
		 }

		 protected internal override bool ThresholdReached( long lastCommittedTransactionId )
		 {
			  return lastCommittedTransactionId >= _nextTransactionIdTarget;
		 }

		 public override void CheckPointHappened( long transactionId )
		 {
			  _nextTransactionIdTarget = transactionId + _notificationThreshold;
		 }

		 public override long CheckFrequencyMillis()
		 {
			  // Transaction counts can change at any time, so we need to check fairly regularly to see if a checkpoint
			  // should be triggered.
			  return CheckPointThreshold_Fields.DefaultCheckingFrequencyMillis;
		 }
	}

}