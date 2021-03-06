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
namespace Org.Neo4j.Server.exception
{
	using Test = org.junit.Test;

	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using UpgradeNotAllowedByConfigurationException = Org.Neo4j.Kernel.impl.storemigration.UpgradeNotAllowedByConfigurationException;
	using LifecycleException = Org.Neo4j.Kernel.Lifecycle.LifecycleException;
	using AssertableLogProvider = Org.Neo4j.Logging.AssertableLogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.lifecycle.LifecycleStatus.STARTED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.lifecycle.LifecycleStatus.STARTING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.logging.AssertableLogProvider.inLog;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.exception.ServerStartupErrors.translateToServerStartupError;

	public class ServerStartupErrorsTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDescribeUpgradeFailureInAFriendlyWay()
		 public virtual void ShouldDescribeUpgradeFailureInAFriendlyWay()
		 {
			  // given
			  AssertableLogProvider logging = new AssertableLogProvider();
			  LifecycleException error = new LifecycleException( new object(), STARTING, STARTED, new Exception("Error starting org.neo4j.kernel.ha.factory.EnterpriseFacadeFactory", new LifecycleException(new object(), STARTING, STARTED, new LifecycleException(new object(), STARTING, STARTED, new UpgradeNotAllowedByConfigurationException()))) );

			  // when
			  translateToServerStartupError( error ).describeTo( logging.GetLog( "console" ) );

			  // then
			  logging.AssertExactly( inLog( "console" ).error( "Neo4j cannot be started because the database files require upgrading and upgrades are disabled " + "in the configuration. Please set '" + GraphDatabaseSettings.allow_upgrade.name() + "' to 'true' " + "in your configuration file and try again." ) );
		 }
	}

}