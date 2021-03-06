﻿using System;

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
namespace Org.Neo4j.causalclustering.core.consensus.log.segmented
{
	using Org.Neo4j.Helpers.Collection;
	using Log = Org.Neo4j.Logging.Log;
	using LogProvider = Org.Neo4j.Logging.LogProvider;

	internal class EntryBasedLogPruningStrategy : CoreLogPruningStrategy
	{
		 private readonly long _entriesToKeep;
		 private readonly Log _log;

		 internal EntryBasedLogPruningStrategy( long entriesToKeep, LogProvider logProvider )
		 {
			  this._entriesToKeep = entriesToKeep;
			  this._log = logProvider.getLog( this.GetType() );
		 }

		 public override long GetIndexToKeep( Segments segments )
		 {
			  SegmentVisitor visitor = new SegmentVisitor( this );
			  segments.VisitBackwards( visitor );

			  if ( visitor.VisitedCount == 0 )
			  {
					_log.warn( "No log files found during the prune operation. This state should resolve on its own, but" + " if this warning continues, you may want to look for other errors in the user log." );
			  }

			  return visitor.PrevIndex;
		 }

		 private class SegmentVisitor : Visitor<SegmentFile, Exception>
		 {
			 private readonly EntryBasedLogPruningStrategy _outerInstance;

			 public SegmentVisitor( EntryBasedLogPruningStrategy outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  internal long VisitedCount;
			  internal long Accumulated;
			  internal long PrevIndex = -1;
			  internal long LastPrevIndex = -1;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean visit(SegmentFile segment) throws RuntimeException
			  public override bool Visit( SegmentFile segment )
			  {
					VisitedCount++;

					if ( LastPrevIndex == -1 )
					{
						 LastPrevIndex = segment.Header().prevIndex();
						 return false; // first entry, continue visiting next
					}

					PrevIndex = segment.Header().prevIndex();
					Accumulated += LastPrevIndex - PrevIndex;
					LastPrevIndex = PrevIndex;

					return Accumulated >= outerInstance.entriesToKeep;
			  }
		 }
	}

}