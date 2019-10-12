using System.Collections.Generic;

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
namespace Org.Neo4j.metrics.output
{
	using Counter = com.codahale.metrics.Counter;
	using Gauge = com.codahale.metrics.Gauge;
	using Histogram = com.codahale.metrics.Histogram;
	using Meter = com.codahale.metrics.Meter;
	using Timer = com.codahale.metrics.Timer;

	public interface EventReporter
	{
		 /// <summary>
		 /// To be called whenever an event occurs. Note that this call might be blocking, hence don't call it from
		 /// time sensitive code or hot path code.
		 /// </summary>
		 /// <param name="gauges"> all of the gauges in the registry </param>
		 /// <param name="counters"> all of the counters in the registry </param>
		 /// <param name="histograms"> all of the histograms in the registry </param>
		 /// <param name="meters"> all of the meters in the registry </param>
		 /// <param name="timers"> all of the timers in the registry </param>
		 void Report( SortedDictionary<string, Gauge> gauges, SortedDictionary<string, Counter> counters, SortedDictionary<string, Histogram> histograms, SortedDictionary<string, Meter> meters, SortedDictionary<string, Timer> timers );
	}

}