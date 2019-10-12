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
namespace Neo4Net.com
{
	using Log = Neo4Net.Logging.Log;

	public class LoggingResourcePoolMonitor : ResourcePool.Monitor_Adapter<ChannelContext>
	{
		 private readonly Log _msgLog;
		 private int _lastCurrentPeakSize = -1;
		 private int _lastTargetSize = -1;

		 public LoggingResourcePoolMonitor( Log msgLog )
		 {
			  this._msgLog = msgLog;
		 }

		 public override void UpdatedCurrentPeakSize( int currentPeakSize )
		 {
			  if ( _lastCurrentPeakSize != currentPeakSize )
			  {
					_msgLog.debug( "ResourcePool updated currentPeakSize to " + currentPeakSize );
					_lastCurrentPeakSize = currentPeakSize;
			  }
		 }

		 public override void Created( ChannelContext resource )
		 {
			  _msgLog.debug( "ResourcePool create resource " + resource );
		 }

		 public override void UpdatedTargetSize( int targetSize )
		 {
			  if ( _lastTargetSize != targetSize )
			  {
					_msgLog.debug( "ResourcePool updated targetSize to " + targetSize );
					_lastTargetSize = targetSize;
			  }
		 }
	}

}