﻿using System.Collections.Generic;

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
namespace Org.Neo4j.causalclustering.upstream
{
	using Test = org.junit.Test;


	using TopologyService = Org.Neo4j.causalclustering.discovery.TopologyService;
	using MemberId = Org.Neo4j.causalclustering.identity.MemberId;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.CausalClusteringSettings.upstream_selection_strategy;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.asSet;

	public class UpstreamDatabaseStrategiesLoaderTest
	{

		 private MemberId _myself = new MemberId( System.Guid.randomUUID() );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnConfiguredClassesOnly()
		 public virtual void ShouldReturnConfiguredClassesOnly()
		 {
			  // given
			  Config config = Config.defaults( upstream_selection_strategy, "dummy" );

			  UpstreamDatabaseStrategiesLoader strategies = new UpstreamDatabaseStrategiesLoader( mock( typeof( TopologyService ) ), config, _myself, NullLogProvider.Instance );

			  // when
			  ISet<UpstreamDatabaseSelectionStrategy> upstreamDatabaseSelectionStrategies = asSet( strategies.GetEnumerator() );

			  // then
			  assertEquals( 1, upstreamDatabaseSelectionStrategies.Count );
			  assertEquals( typeof( UpstreamDatabaseStrategySelectorTest.DummyUpstreamDatabaseSelectionStrategy ), upstreamDatabaseSelectionStrategies.Select( UpstreamDatabaseSelectionStrategy.getClass ).First().get() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnTheFirstStrategyThatWorksFromThoseConfigured()
		 public virtual void ShouldReturnTheFirstStrategyThatWorksFromThoseConfigured()
		 {
			  // given
			  Config config = Config.defaults( upstream_selection_strategy, "yet-another-dummy,dummy,another-dummy" );

			  // when
			  UpstreamDatabaseStrategiesLoader strategies = new UpstreamDatabaseStrategiesLoader( mock( typeof( TopologyService ) ), config, _myself, NullLogProvider.Instance );

			  // then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertEquals( typeof( UpstreamDatabaseStrategySelectorTest.YetAnotherDummyUpstreamDatabaseSelectionStrategy ), strategies.GetEnumerator().next().GetType() );
		 }
	}

}