using System.Threading;

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
namespace Org.Neo4j.metrics.source.jvm
{
	using Gauge = com.codahale.metrics.Gauge;
	using MetricRegistry = com.codahale.metrics.MetricRegistry;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.codahale.metrics.MetricRegistry.name;

	public class ThreadMetrics : JvmMetrics
	{
		 public static readonly string ThreadCount = name( JvmMetrics.NAME_PREFIX, "thread.count" );
		 public static readonly string ThreadTotal = name( JvmMetrics.NAME_PREFIX, "thread.total" );

		 private readonly MetricRegistry _registry;
		 private readonly ThreadMXBean _threadMXBean = ManagementFactory.ThreadMXBean;

		 public ThreadMetrics( MetricRegistry registry )
		 {
			  this._registry = registry;
		 }

		 public override void Start()
		 {
			  _registry.register( ThreadCount, ( Gauge<int> ) Thread.activeCount );
			  _registry.register( ThreadTotal, ( Gauge<int> ) _threadMXBean.getThreadCount );
		 }

		 public override void Stop()
		 {
			  _registry.remove( ThreadCount );
			  _registry.remove( ThreadTotal );
		 }
	}


}