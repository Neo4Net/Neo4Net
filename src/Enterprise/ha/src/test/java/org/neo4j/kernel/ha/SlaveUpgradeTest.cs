using System;

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
namespace Neo4Net.Kernel.ha
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using ClusterSettings = Neo4Net.cluster.ClusterSettings;
	using TestHighlyAvailableGraphDatabaseFactory = Neo4Net.Graphdb.factory.TestHighlyAvailableGraphDatabaseFactory;
	using Exceptions = Neo4Net.Helpers.Exceptions;
	using MigrationTestUtils = Neo4Net.Kernel.impl.storemigration.MigrationTestUtils;
	using UpgradeNotAllowedByConfigurationException = Neo4Net.Kernel.impl.storemigration.UpgradeNotAllowedByConfigurationException;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	public class SlaveUpgradeTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory testDirectory = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory TestDirectory = TestDirectory.testDirectory();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void haShouldFailToStartWithOldStore()
		 public virtual void HaShouldFailToStartWithOldStore()
		 {
			  try
			  {
					File storeDirectory = TestDirectory.directory( "haShouldFailToStartWithOldStore" );
					File databaseDirectory = TestDirectory.databaseDir( storeDirectory );
					MigrationTestUtils.find23FormatStoreDirectory( databaseDirectory );

					( new TestHighlyAvailableGraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(databaseDirectory).setConfig(ClusterSettings.server_id, "1").setConfig(ClusterSettings.initial_hosts, "localhost:9999").newGraphDatabase();

					fail( "Should exit abnormally" );
			  }
			  catch ( Exception e )
			  {
					Exception rootCause = Exceptions.rootCause( e );
					assertThat( rootCause, instanceOf( typeof( UpgradeNotAllowedByConfigurationException ) ) );
			  }
		 }
	}

}