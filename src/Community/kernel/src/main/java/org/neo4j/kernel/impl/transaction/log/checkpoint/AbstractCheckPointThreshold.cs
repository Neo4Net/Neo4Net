﻿/*
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

	/// <summary>
	/// Abstract class that implement common logic for making the consumer to consume the description of this
	/// threshold if <seealso cref="thresholdReached(long)"/> is true.
	/// </summary>
	public abstract class AbstractCheckPointThreshold : CheckPointThreshold
	{
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public abstract CheckPointThreshold or(final CheckPointThreshold... thresholds);
		public abstract CheckPointThreshold Or( params CheckPointThreshold[] thresholds );
		public abstract CheckPointThreshold CreateThreshold( Neo4Net.Kernel.configuration.Config config, Neo4Net.Time.SystemNanoClock clock, Neo4Net.Kernel.impl.transaction.log.pruning.LogPruning logPruning, Neo4Net.Logging.LogProvider logProvider );
		public abstract long CheckFrequencyMillis();
		public abstract void CheckPointHappened( long transactionId );
		public abstract void Initialize( long transactionId );
		 private readonly string _description;

		 public AbstractCheckPointThreshold( string description )
		 {
			  this._description = description;
		 }

		 public override bool IsCheckPointingNeeded( long lastCommittedTransactionId, System.Action<string> consumer )
		 {
			  if ( ThresholdReached( lastCommittedTransactionId ) )
			  {
					consumer( _description );
					return true;
			  }
			  return false;
		 }

		 protected internal abstract bool ThresholdReached( long lastCommittedTransactionId );
	}

}