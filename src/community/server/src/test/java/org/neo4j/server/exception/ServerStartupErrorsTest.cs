using System;

/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
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
namespace Neo4Net.Server.exception
{
	using Test = org.junit.Test;

	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using UpgradeNotAllowedByConfigurationException = Neo4Net.Kernel.impl.storemigration.UpgradeNotAllowedByConfigurationException;
	using LifecycleException = Neo4Net.Kernel.Lifecycle.LifecycleException;
	using AssertableLogProvider = Neo4Net.Logging.AssertableLogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.lifecycle.LifecycleStatus.STARTED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.lifecycle.LifecycleStatus.STARTING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.logging.AssertableLogProvider.inLog;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.server.exception.ServerStartupErrors.translateToServerStartupError;

	public class ServerStartupErrorsTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDescribeUpgradeFailureInAFriendlyWay()
		 public virtual void ShouldDescribeUpgradeFailureInAFriendlyWay()
		 {
			  // given
			  AssertableLogProvider logging = new AssertableLogProvider();
			  LifecycleException error = new LifecycleException( new object(), STARTING, STARTED, new Exception("Error starting Neo4Net.kernel.ha.factory.EnterpriseFacadeFactory", new LifecycleException(new object(), STARTING, STARTED, new LifecycleException(new object(), STARTING, STARTED, new UpgradeNotAllowedByConfigurationException()))) );

			  // when
			  translateToServerStartupError( error ).describeTo( logging.GetLog( "console" ) );

			  // then
			  logging.AssertExactly( inLog( "console" ).error( "Neo4Net cannot be started because the database files require upgrading and upgrades are disabled " + "in the configuration. Please set '" + GraphDatabaseSettings.allow_upgrade.name() + "' to 'true' " + "in your configuration file and try again." ) );
		 }
	}

}