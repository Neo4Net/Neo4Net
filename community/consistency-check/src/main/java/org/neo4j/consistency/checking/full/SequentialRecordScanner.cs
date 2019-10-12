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
namespace Org.Neo4j.Consistency.checking.full
{
	using Statistics = Org.Neo4j.Consistency.statistics.Statistics;
	using Org.Neo4j.Helpers.Collection;
	using MultiPartBuilder = Org.Neo4j.Helpers.progress.ProgressMonitorFactory.MultiPartBuilder;

	public class SequentialRecordScanner<RECORD> : RecordScanner<RECORD>
	{
		 public SequentialRecordScanner( string name, Statistics statistics, int threads, BoundedIterable<RECORD> store, MultiPartBuilder builder, RecordProcessor<RECORD> processor, params IterableStore[] warmUpStores ) : base( name, statistics, threads, store, builder, processor, warmUpStores )
		 {
		 }

		 protected internal override void Scan()
		 {
			  foreach ( RECORD record in Store )
			  {
					Processor.process( record );
					Progress.add( 1 );
			  }
		 }
	}

}