﻿using System;

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
	using ProgressListener = Org.Neo4j.Helpers.progress.ProgressListener;
	using ProgressMonitorFactory = Org.Neo4j.Helpers.progress.ProgressMonitorFactory;

	internal abstract class RecordScanner<RECORD> : ConsistencyCheckerTask
	{
		 protected internal readonly ProgressListener Progress;
		 protected internal readonly BoundedIterable<RECORD> Store;
		 protected internal readonly RecordProcessor<RECORD> Processor;
		 private readonly IterableStore[] _warmUpStores;

		 internal RecordScanner( string name, Statistics statistics, int threads, BoundedIterable<RECORD> store, ProgressMonitorFactory.MultiPartBuilder builder, RecordProcessor<RECORD> processor, params IterableStore[] warmUpStores ) : base( name, statistics, threads )
		 {
			  this.Store = store;
			  this.Processor = processor;
			  long maxCount = store.MaxCount();
			  this.Progress = maxCount == -1 ? builder.ProgressForUnknownPart( name ) : builder.ProgressForPart( name, maxCount );
			  this._warmUpStores = warmUpStores;
		 }

		 public override void Run()
		 {
			  Statistics.reset();
			  if ( _warmUpStores != null )
			  {
					foreach ( IterableStore store in _warmUpStores )
					{
						 store.warmUpCache();
					}
			  }
			  try
			  {
					Scan();
			  }
			  finally
			  {
					try
					{
						 Store.close();
					}
					catch ( Exception e )
					{
						 Progress.failed( e );
						 throw new Exception( e );
					}
					finally
					{
						 Processor.close();
						 Progress.done();
					}
			  }
			  Statistics.print( Name );
		 }

		 protected internal abstract void Scan();
	}

}