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
namespace Org.Neo4j.Server.web
{
	using After = org.junit.After;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using ListenSocketAddress = Org.Neo4j.Helpers.ListenSocketAddress;
	using NetworkConnectionTracker = Org.Neo4j.Kernel.api.net.NetworkConnectionTracker;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;
	using ImpermanentDatabaseRule = Org.Neo4j.Test.rule.ImpermanentDatabaseRule;
	using SuppressOutput = Org.Neo4j.Test.rule.SuppressOutput;
	using ExclusiveServerTestBase = Org.Neo4j.Test.server.ExclusiveServerTestBase;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.rule.SuppressOutput.suppressAll;

	public class Jetty9WebServerIT : ExclusiveServerTestBase
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.SuppressOutput suppressOutput = suppressAll();
		 public new SuppressOutput SuppressOutput = suppressAll();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.ImpermanentDatabaseRule dbRule = new org.neo4j.test.rule.ImpermanentDatabaseRule();
		 public ImpermanentDatabaseRule DbRule = new ImpermanentDatabaseRule();

		 private Jetty9WebServer _webServer;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToUsePortZero() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToUsePortZero()
		 {
			  // Given
			  _webServer = new Jetty9WebServer( NullLogProvider.Instance, Config.defaults(), NetworkConnectionTracker.NO_OP );

			  _webServer.HttpAddress = new ListenSocketAddress( "localhost", 0 );

			  // When
			  _webServer.start();

			  // Then no exception
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToRestart() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToRestart()
		 {
			  // given
			  _webServer = new Jetty9WebServer( NullLogProvider.Instance, Config.defaults(), NetworkConnectionTracker.NO_OP );
			  _webServer.HttpAddress = new ListenSocketAddress( "127.0.0.1", 7878 );

			  // when
			  _webServer.start();
			  _webServer.stop();
			  _webServer.start();

			  // then no exception
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldStopCleanlyEvenWhenItHasntBeenStarted()
		 public virtual void ShouldStopCleanlyEvenWhenItHasntBeenStarted()
		 {
			  ( new Jetty9WebServer( NullLogProvider.Instance, Config.defaults(), NetworkConnectionTracker.NO_OP ) ).stop();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void cleanup()
		 public virtual void Cleanup()
		 {
			  if ( _webServer != null )
			  {
					_webServer.stop();
			  }
		 }

	}

}