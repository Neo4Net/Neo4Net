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
namespace Org.Neo4j.@unsafe.Impl.Batchimport
{
	using AbstractBaseRecord = Org.Neo4j.Kernel.impl.store.record.AbstractBaseRecord;
	using BatchSender = Org.Neo4j.@unsafe.Impl.Batchimport.staging.BatchSender;
	using Org.Neo4j.@unsafe.Impl.Batchimport.staging;
	using StageControl = Org.Neo4j.@unsafe.Impl.Batchimport.staging.StageControl;
	using Org.Neo4j.@unsafe.Impl.Batchimport.staging;
	using StatsProvider = Org.Neo4j.@unsafe.Impl.Batchimport.stats.StatsProvider;

	/// <summary>
	/// <seealso cref="RecordProcessor"/> in <seealso cref="Step Step-form"/>.
	/// </summary>
	public class RecordProcessorStep<T> : ProcessorStep<T[]> where T : Org.Neo4j.Kernel.impl.store.record.AbstractBaseRecord
	{
		 private readonly RecordProcessor<T> _processor;
		 private readonly bool _endOfLine;

		 public RecordProcessorStep( StageControl control, string name, Configuration config, RecordProcessor<T> processor, bool endOfLine, params StatsProvider[] additionalStatsProviders ) : base( control, name, config, 1, additionalStatsProviders )
		 {
			  this._processor = processor;
			  this._endOfLine = endOfLine;
		 }

		 protected internal override void Process( T[] batch, BatchSender sender )
		 {
			  foreach ( T item in batch )
			  {
					if ( item != null && item.inUse() )
					{
						 if ( !_processor.process( item ) )
						 {
							  // No change for this record
							  item.InUse = false;
						 }
					}
			  }

			  // This step can be used in different stage settings, possible as the last step,
			  // where nothing should be emitted
			  if ( !_endOfLine )
			  {
					sender.Send( batch );
			  }
		 }

		 protected internal override void Done()
		 {
			  base.Done();
			  _processor.done();
		 }
	}

}