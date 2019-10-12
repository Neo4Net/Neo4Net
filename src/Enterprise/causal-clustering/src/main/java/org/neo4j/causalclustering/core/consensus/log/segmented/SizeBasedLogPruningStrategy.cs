using System;

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
namespace Neo4Net.causalclustering.core.consensus.log.segmented
{
	using Neo4Net.Helpers.Collection;

	internal class SizeBasedLogPruningStrategy : CoreLogPruningStrategy, Visitor<SegmentFile, Exception>
	{
		 private readonly long _bytesToKeep;
		 private long _accumulatedSize;
		 private SegmentFile _file;

		 internal SizeBasedLogPruningStrategy( long bytesToKeep )
		 {
			  this._bytesToKeep = bytesToKeep;
		 }

		 public override long GetIndexToKeep( Segments segments )
		 {
			 lock ( this )
			 {
				  _accumulatedSize = 0;
				  _file = null;
      
				  segments.VisitBackwards( this );
      
				  return _file != null ? ( _file.header().prevIndex() + 1 ) : -1;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean visit(SegmentFile segment) throws RuntimeException
		 public override bool Visit( SegmentFile segment )
		 {
			  if ( _accumulatedSize < _bytesToKeep )
			  {
					_file = segment;
					_accumulatedSize += _file.size();
					return false;
			  }

			  return true;
		 }
	}

}