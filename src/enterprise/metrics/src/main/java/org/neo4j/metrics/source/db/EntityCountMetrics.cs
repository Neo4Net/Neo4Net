/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.metrics.source.db
{
	using Gauge = com.codahale.metrics.Gauge;
	using MetricRegistry = com.codahale.metrics.MetricRegistry;

	using Documented = Neo4Net.Kernel.Impl.Annotations.Documented;
	using StoreEntityCounters = Neo4Net.Kernel.impl.store.stats.StoreEntityCounters;
	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.codahale.metrics.MetricRegistry.name;

	[Documented(".Database data metrics")]
	public class IEntityCountMetrics : LifecycleAdapter
	{
		 private const string COUNTS_PREFIX = "Neo4Net.ids_in_use";

		 [Documented("The total number of different relationship types stored in the database")]
		 public static readonly string CountsRelationshipType = name( COUNTS_PREFIX, "relationship_type" );
		 [Documented("The total number of different property names used in the database")]
		 public static readonly string CountsProperty = name( COUNTS_PREFIX, "property" );
		 [Documented("The total number of relationships stored in the database")]
		 public static readonly string CountsRelationship = name( COUNTS_PREFIX, "relationship" );
		 [Documented("The total number of nodes stored in the database")]
		 public static readonly string CountsNode = name( COUNTS_PREFIX, "node" );

		 private readonly MetricRegistry _registry;
		 private readonly System.Func<StoreEntityCounters> _storeEntityCountersSupplier;

		 public IEntityCountMetrics( MetricRegistry registry, System.Func<StoreEntityCounters> storeEntityCountersSupplier )
		 {
			  this._registry = registry;
			  this._storeEntityCountersSupplier = storeEntityCountersSupplier;
		 }

		 public override void Start()
		 {
			  StoreEntityCounters counters = _storeEntityCountersSupplier.get();
			  _registry.register( CountsNode, ( Gauge<long> ) counters.nodes );
			  _registry.register( CountsRelationship, ( Gauge<long> ) counters.relationships );
			  _registry.register( CountsProperty, ( Gauge<long> ) counters.properties );
			  _registry.register( CountsRelationshipType, ( Gauge<long> ) counters.relationshipTypes );
		 }

		 public override void Stop()
		 {
			  _registry.remove( CountsNode );
			  _registry.remove( CountsRelationship );
			  _registry.remove( CountsProperty );
			  _registry.remove( CountsRelationshipType );
		 }
	}

}