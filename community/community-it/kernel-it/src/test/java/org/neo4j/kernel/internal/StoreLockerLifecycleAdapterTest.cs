using System;
using System.Collections.Generic;

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
namespace Org.Neo4j.Kernel.@internal
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using Settings = Org.Neo4j.Kernel.configuration.Settings;
	using TestGraphDatabaseFactory = Org.Neo4j.Test.TestGraphDatabaseFactory;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.IsInstanceOf.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.stringMap;

	public class StoreLockerLifecycleAdapterTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory directory = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory Directory = TestDirectory.testDirectory();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowDatabasesToUseFilesetsSequentially()
		 public virtual void ShouldAllowDatabasesToUseFilesetsSequentially()
		 {
			  NewDb().shutdown();
			  NewDb().shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAllowDatabasesToUseFilesetsConcurrently()
		 public virtual void ShouldNotAllowDatabasesToUseFilesetsConcurrently()
		 {
			  ShouldNotAllowDatabasesToUseFilesetsConcurrently( stringMap() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAllowDatabasesToUseFilesetsConcurrentlyEvenIfTheyAreInReadOnlyMode()
		 public virtual void ShouldNotAllowDatabasesToUseFilesetsConcurrentlyEvenIfTheyAreInReadOnlyMode()
		 {
			  ShouldNotAllowDatabasesToUseFilesetsConcurrently( stringMap( GraphDatabaseSettings.read_only.name(), Settings.TRUE ) );
		 }

		 private void ShouldNotAllowDatabasesToUseFilesetsConcurrently( IDictionary<string, string> config )
		 {
			  GraphDatabaseService db = NewDb();
			  try
			  {
					( new TestGraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(StoreDir()).setConfig(config).newGraphDatabase();

					fail();
			  }
			  catch ( Exception e )
			  {
					assertThat( e.InnerException.InnerException, instanceOf( typeof( StoreLockException ) ) );
			  }
			  finally
			  {
					Db.shutdown();
			  }
		 }

		 private GraphDatabaseService NewDb()
		 {
			  return ( new TestGraphDatabaseFactory() ).newEmbeddedDatabase(StoreDir());
		 }

		 private File StoreDir()
		 {
			  return Directory.absolutePath();
		 }
	}

}