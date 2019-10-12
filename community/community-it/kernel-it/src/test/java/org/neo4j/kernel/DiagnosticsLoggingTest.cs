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
namespace Org.Neo4j.Kernel
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using Settings = Org.Neo4j.Kernel.configuration.Settings;
	using MetaDataStore = Org.Neo4j.Kernel.impl.store.MetaDataStore;
	using AssertableLogProvider = Org.Neo4j.Logging.AssertableLogProvider;
	using TestGraphDatabaseFactory = Org.Neo4j.Test.TestGraphDatabaseFactory;
	using CleanupRule = Org.Neo4j.Test.rule.CleanupRule;

	public class DiagnosticsLoggingTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.CleanupRule cleanupRule = new org.neo4j.test.rule.CleanupRule();
		 public CleanupRule CleanupRule = new CleanupRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSeeExpectedDiagnostics()
		 public virtual void ShouldSeeExpectedDiagnostics()
		 {
			  AssertableLogProvider logProvider = new AssertableLogProvider();
			  GraphDatabaseService db = ( new TestGraphDatabaseFactory() ).setInternalLogProvider(logProvider).newImpermanentDatabaseBuilder().setConfig(GraphDatabaseSettings.dump_configuration, Settings.TRUE).setConfig(GraphDatabaseSettings.pagecache_memory, "4M").newGraphDatabase();
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