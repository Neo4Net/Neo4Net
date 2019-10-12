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
namespace Neo4Net.metrics.source.jvm
{
	using Gauge = com.codahale.metrics.Gauge;
	using MetricRegistry = com.codahale.metrics.MetricRegistry;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.codahale.metrics.MetricRegistry.name;

	public class MemoryPoolMetrics : JvmMetrics
	{
		 public static readonly string MemoryPool = name( JvmMetrics.NAME_PREFIX, "memory.pool" );
		 private readonly MetricRegistry _registry;

		 public MemoryPoolMetrics( MetricRegistry registry )
		 {
			  this._registry = registry;
		 }

		 public override void Start()
		 {
			  foreach ( MemoryPoolMXBean memPool in ManagementFactory.MemoryPoolMXBeans )
			  {
					_registry.register( name( MemoryPool, PrettifyName( memPool.Name ) ), ( Gauge<long> )() => memPool.Usage.Used );
			  }
		 }

		 public override void Stop()
		 {
			  _registry.removeMatching( ( name, metric ) => name.StartsWith( MemoryPool ) );
		 }
	}

}