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
namespace Org.Neo4j.Kernel.impl.enterprise.transaction.log.checkpoint
{
	using AbstractCheckPointThreshold = Org.Neo4j.Kernel.impl.transaction.log.checkpoint.AbstractCheckPointThreshold;
	using LogPruning = Org.Neo4j.Kernel.impl.transaction.log.pruning.LogPruning;

	public class VolumetricCheckPointThreshold : AbstractCheckPointThreshold
	{
		 private readonly LogPruning _logPruning;

		 public VolumetricCheckPointThreshold( LogPruning logPruning ) : base( "tx log pruning" )
		 {
			  this._logPruning = logPruning;
		 }

		 protected internal override bool ThresholdReached( long lastCommittedTransactionId )
		 {
			  return _logPruning.mightHaveLogsToPrune();
		 }

		 public override void Initialize( long transactionId )
		 {
		 }

		 public override void CheckPointHappened( long transactionId )
		 {
		 }

		 public override long CheckFrequencyMillis()
		 {
			  return DEFAULT_CHECKING_FREQUENCY_MILLIS;
		 }
	}

}