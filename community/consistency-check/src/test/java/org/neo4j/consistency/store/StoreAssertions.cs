﻿/*
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
namespace Org.Neo4j.Consistency.store
{
	using ConsistencyCheckIncompleteException = Org.Neo4j.Consistency.checking.full.ConsistencyCheckIncompleteException;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using ProgressMonitorFactory = Org.Neo4j.Helpers.progress.ProgressMonitorFactory;
	using DatabaseLayout = Org.Neo4j.Io.layout.DatabaseLayout;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using AssertableLogProvider = Org.Neo4j.Logging.AssertableLogProvider;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class StoreAssertions
	{
		 private StoreAssertions()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void assertConsistentStore(org.neo4j.io.layout.DatabaseLayout databaseLayout) throws org.neo4j.consistency.checking.full.ConsistencyCheckIncompleteException
		 public static void AssertConsistentStore( DatabaseLayout databaseLayout )
		 {
			  Config configuration = Config.defaults( GraphDatabaseSettings.pagecache_memory, "8m" );
			  AssertableLogProvider logger = new AssertableLogProvider();
			  ConsistencyCheckService.Result result = ( new ConsistencyCheckService() ).runFullConsistencyCheck(databaseLayout, configuration, ProgressMonitorFactory.NONE, NullLogProvider.Instance, false);

			  assertTrue( "Consistency check for " + databaseLayout + " found inconsistencies:\n\n" + logger.Serialize(), result.Successful );
		 }
	}

}