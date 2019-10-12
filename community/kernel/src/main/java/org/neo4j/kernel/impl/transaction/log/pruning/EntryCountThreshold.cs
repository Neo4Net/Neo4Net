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

	public sealed class EntryCountThreshold : Threshold
	{
		 private readonly long _maxTransactionCount;

		 internal EntryCountThreshold( long maxTransactionCount )
		 {
			  this._maxTransactionCount = maxTransactionCount;
		 }

		 public override void Init()
		 {
			  // nothing to do here
		 }

		 public override bool Reached( File ignored, long version, LogFileInformation source )
		 {
			  try
			  {
					// try to ask next version log file which is my last tx
					long lastTx = source.GetFirstEntryId( version + 1 );
					if ( lastTx == -1 )
					{
						 throw new System.InvalidOperationException( "The next version should always exist, since this is called after rotation and the " + "PruneStrategy never checks the current active log file" );
					}

					long highest = source.LastEntryId;
					return highest - lastTx >= _maxTransactionCount;
			  }
			  catch ( IOException e )
			  {
					throw new Exception( e );
			  }
		 }
	}

}