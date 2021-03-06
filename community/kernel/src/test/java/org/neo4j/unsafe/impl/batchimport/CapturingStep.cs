﻿using System.Collections.Generic;

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

	using BatchSender = Org.Neo4j.@unsafe.Impl.Batchimport.staging.BatchSender;
	using Org.Neo4j.@unsafe.Impl.Batchimport.staging;
	using StageControl = Org.Neo4j.@unsafe.Impl.Batchimport.staging.StageControl;
	using StatsProvider = Org.Neo4j.@unsafe.Impl.Batchimport.stats.StatsProvider;

	public class CapturingStep<T> : ProcessorStep<T>
	{
		 private readonly IList<T> _receivedBatches = Collections.synchronizedList( new List<T>() );

		 public CapturingStep( StageControl control, string name, Configuration config, params StatsProvider[] additionalStatsProvider ) : base( control, name, config, 1, additionalStatsProvider )
		 {
		 }

		 public virtual IList<T> ReceivedBatches()
		 {
			  return _receivedBatches;
		 }

		 protected internal override void Process( T batch, BatchSender sender )
		 {
			  _receivedBatches.Add( batch );
		 }
	}

}