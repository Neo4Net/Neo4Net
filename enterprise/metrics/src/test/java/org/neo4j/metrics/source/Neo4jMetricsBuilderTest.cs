﻿using System;

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
namespace Org.Neo4j.metrics.source
{
	using MetricRegistry = com.codahale.metrics.MetricRegistry;
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;

	using Config = Org.Neo4j.Kernel.configuration.Config;
	using HttpConnector = Org.Neo4j.Kernel.configuration.HttpConnector;
	using KernelContext = Org.Neo4j.Kernel.impl.spi.KernelContext;
	using SimpleKernelContext = Org.Neo4j.Kernel.impl.spi.SimpleKernelContext;
	using DependencySatisfier = Org.Neo4j.Kernel.impl.util.DependencySatisfier;
	using LifeSupport = Org.Neo4j.Kernel.Lifecycle.LifeSupport;
	using NullLogService = Org.Neo4j.Logging.@internal.NullLogService;
	using EventReporter = Org.Neo4j.metrics.output.EventReporter;
	using ServerMetrics = Org.Neo4j.metrics.source.server.ServerMetrics;
	using Inject = Org.Neo4j.Test.extension.Inject;
	using TestDirectoryExtension = Org.Neo4j.Test.extension.TestDirectoryExtension;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.hasItem;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.not;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.factory.DatabaseInfo.COMMUNITY;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith(TestDirectoryExtension.class) class Neo4jMetricsBuilderTest
	internal class Neo4jMetricsBuilderTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.neo4j.test.rule.TestDirectory testDir;
		 private TestDirectory _testDir;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldAddServerMetricsWhenServerEnabled()
		 internal virtual void ShouldAddServerMetricsWhenServerEnabled()
		 {
			  TestBuildingWithServerMetrics( true );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotAddServerMetricsWhenServerDisabled()
		 internal virtual void ShouldNotAddServerMetricsWhenServerDisabled()
		 {
			  TestBuildingWithServerMetrics( false );
		 }

		 private void TestBuildingWithServerMetrics( bool serverMetricsEnabled )
		 {
			  Config config = ConfigWithServerMetrics( serverMetricsEnabled );
			  KernelContext kernelContext = new SimpleKernelContext( _testDir.databaseDir(), COMMUNITY, mock(typeof(DependencySatisfier)) );
			  LifeSupport life = new LifeSupport();

			  Neo4jMetricsBuilder builder = new Neo4jMetricsBuilder( new MetricRegistry(), mock(typeof(EventReporter)), config, NullLogService.Instance, kernelContext, mock(typeof(Neo4jMetricsBuilder.Dependencies)), life );

			  assertTrue( builder.Build() );

			  if ( serverMetricsEnabled )
			  {
					assertThat( life.LifecycleInstances, hasItem( instanceOf( typeof( ServerMetrics ) ) ) );
			  }
			  else
			  {
					assertThat( life.LifecycleInstances, not( hasItem( instanceOf( typeof( ServerMetrics ) ) ) ) );
			  }
		 }

		 private static Config ConfigWithServerMetrics( bool enabled )
		 {
			  return Config.builder().withSetting((new HttpConnector("http")).enabled, Convert.ToString(enabled)).withSetting(MetricsSettings.neoServerEnabled, "true").build();
		 }
	}

}