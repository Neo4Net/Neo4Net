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
namespace Neo4Net.Consistency.Store
{
	using ConsistencyCheckIncompleteException = Neo4Net.Consistency.checking.full.ConsistencyCheckIncompleteException;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using ProgressMonitorFactory = Neo4Net.Helpers.progress.ProgressMonitorFactory;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using Config = Neo4Net.Kernel.configuration.Config;
	using AssertableLogProvider = Neo4Net.Logging.AssertableLogProvider;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class StoreAssertions
	{
		 private StoreAssertions()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void assertConsistentStore(org.Neo4Net.io.layout.DatabaseLayout databaseLayout) throws org.Neo4Net.consistency.checking.full.ConsistencyCheckIncompleteException
		 public static void AssertConsistentStore( DatabaseLayout databaseLayout )
		 {
			  Config configuration = Config.defaults( GraphDatabaseSettings.pagecache_memory, "8m" );
			  AssertableLogProvider logger = new AssertableLogProvider();
			  ConsistencyCheckService.Result result = ( new ConsistencyCheckService() ).runFullConsistencyCheck(databaseLayout, configuration, ProgressMonitorFactory.NONE, NullLogProvider.Instance, false);

			  assertTrue( "Consistency check for " + databaseLayout + " found inconsistencies:\n\n" + logger.Serialize(), result.Successful );
		 }
	}

}