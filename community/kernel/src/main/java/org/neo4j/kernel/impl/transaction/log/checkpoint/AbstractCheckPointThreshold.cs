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

	/// <summary>
	/// Abstract class that implement common logic for making the consumer to consume the description of this
	/// threshold if <seealso cref="thresholdReached(long)"/> is true.
	/// </summary>
	public abstract class AbstractCheckPointThreshold : CheckPointThreshold
	{
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public abstract CheckPointThreshold or(final CheckPointThreshold... thresholds);
		public abstract CheckPointThreshold Or( params CheckPointThreshold[] thresholds );
		public abstract CheckPointThreshold CreateThreshold( Org.Neo4j.Kernel.configuration.Config config, Org.Neo4j.Time.SystemNanoClock clock, Org.Neo4j.Kernel.impl.transaction.log.pruning.LogPruning logPruning, Org.Neo4j.Logging.LogProvider logProvider );
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