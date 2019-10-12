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
namespace Neo4Net.causalclustering.diagnostics
{
	using Matchers = org.hamcrest.Matchers;
	using Test = org.junit.Test;

	using ClusterBinder = Neo4Net.causalclustering.identity.ClusterBinder;
	using ClusterId = Neo4Net.causalclustering.identity.ClusterId;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using AssertableLogProvider = Neo4Net.Logging.AssertableLogProvider;
	using SimpleLogService = Neo4Net.Logging.@internal.SimpleLogService;

	public class CoreMonitorTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotDuplicateToAnyLog()
		 public virtual void ShouldNotDuplicateToAnyLog()
		 {
			  AssertableLogProvider userLogProvider = new AssertableLogProvider();
			  AssertableLogProvider debugLogProvider = new AssertableLogProvider();

			  SimpleLogService logService = new SimpleLogService( userLogProvider, debugLogProvider );

			  Monitors monitors = new Monitors();
			  CoreMonitor.Register( logService.InternalLogProvider, logService.UserLogProvider, monitors );

			  ClusterBinder.Monitor monitor = monitors.NewMonitor( typeof( ClusterBinder.Monitor ) );

			  ClusterId clusterId = new ClusterId( System.Guid.randomUUID() );
			  monitor.BoundToCluster( clusterId );

			  userLogProvider.RawMessageMatcher().assertContainsSingle(Matchers.allOf(Matchers.containsString("Bound to cluster with id " + clusterId.Uuid())));

			  debugLogProvider.RawMessageMatcher().assertContainsSingle(Matchers.allOf(Matchers.containsString("Bound to cluster with id " + clusterId.Uuid())));
		 }
	}

}