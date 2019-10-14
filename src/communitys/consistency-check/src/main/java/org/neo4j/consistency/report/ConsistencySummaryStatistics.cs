using System.Collections.Generic;
using System.Text;

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
namespace Neo4Net.Consistency.report
{

	public class ConsistencySummaryStatistics
	{
		 private readonly IDictionary<RecordType, AtomicInteger> _inconsistentRecordCount = new Dictionary<RecordType, AtomicInteger>( typeof( RecordType ) );
		 private readonly AtomicInteger _totalInconsistencyCount = new AtomicInteger();
		 private readonly AtomicLong _errorCount = new AtomicLong();
		 private readonly AtomicLong _warningCount = new AtomicLong();

		 public ConsistencySummaryStatistics()
		 {
			  foreach ( RecordType recordType in Enum.GetValues( typeof( RecordType ) ) )
			  {
					_inconsistentRecordCount[recordType] = new AtomicInteger();
			  }
		 }

		 public override string ToString()
		 {
			  StringBuilder result = ( new StringBuilder( this.GetType().Name ) ).Append('{');
			  result.Append( "\n\tNumber of errors: " ).Append( _errorCount );
			  result.Append( "\n\tNumber of warnings: " ).Append( _warningCount );
			  foreach ( KeyValuePair<RecordType, AtomicInteger> entry in _inconsistentRecordCount.SetOfKeyValuePairs() )
			  {
					if ( entry.Value.get() != 0 )
					{
						 result.Append( "\n\tNumber of inconsistent " ).Append( entry.Key ).Append( " records: " ).Append( entry.Value );
					}
			  }
			  return result.Append( "\n}" ).ToString();
		 }

		 public virtual bool Consistent
		 {
			 get
			 {
				  return _totalInconsistencyCount.get() == 0;
			 }
		 }

		 public virtual int GetInconsistencyCountForRecordType( RecordType recordType )
		 {
			  return _inconsistentRecordCount[recordType].get();
		 }

		 public virtual int TotalInconsistencyCount
		 {
			 get
			 {
				  return _totalInconsistencyCount.get();
			 }
		 }

		 public virtual void Update( RecordType recordType, int errors, int warnings )
		 {
			  if ( errors > 0 )
			  {
					_inconsistentRecordCount[recordType].addAndGet( errors );
					_totalInconsistencyCount.addAndGet( errors );
					_errorCount.addAndGet( errors );
			  }
			  if ( warnings > 0 )
			  {
					_warningCount.addAndGet( warnings );
			  }
		 }
	}

}