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
namespace Neo4Net.Kernel
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using Settings = Neo4Net.Kernel.configuration.Settings;
	using MetaDataStore = Neo4Net.Kernel.impl.store.MetaDataStore;
	using AssertableLogProvider = Neo4Net.Logging.AssertableLogProvider;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using CleanupRule = Neo4Net.Test.rule.CleanupRule;

	public class DiagnosticsLoggingTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public Neo4Net.test.rule.CleanupRule cleanupRule = new Neo4Net.test.rule.CleanupRule();
		 public CleanupRule CleanupRule = new CleanupRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSeeExpectedDiagnostics()
		 public virtual void ShouldSeeExpectedDiagnostics()
		 {
			  AssertableLogProvider logProvider = new AssertableLogProvider();
			  IGraphDatabaseService db = ( new TestGraphDatabaseFactory() ).setInternalLogProvider(logProvider).newImpermanentDatabaseBuilder().setConfig(GraphDatabaseSettings.dump_configuration, Settings.TRUE).setConfig(GraphDatabaseSettings.pagecache_memory, "4M").newGraphDatabase();
			  CleanupRule.add( db );

			  // THEN we should have logged
			  logProvider.RawMessageMatcher().assertContains("Network information");
			  logProvider.RawMessageMatcher().assertContains("Disk space on partition");
			  logProvider.RawMessageMatcher().assertContains("Local timezone");
			  // page cache info
			  logProvider.RawMessageMatcher().assertContains("Page cache: 4M");
			  // neostore records
			  foreach ( MetaDataStore.Position position in MetaDataStore.Position.values() )
			  {
					logProvider.RawMessageMatcher().assertContains(position.name());
			  }
			  // transaction log info
			  logProvider.RawMessageMatcher().assertContains("Transaction log");
			  logProvider.RawMessageMatcher().assertContains("TimeZone version: ");
		 }
	}

}