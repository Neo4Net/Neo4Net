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
namespace Neo4Net.Kernel.impl.core
{
	using RandomStringUtils = org.apache.commons.lang3.RandomStringUtils;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Node = Neo4Net.Graphdb.Node;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using EphemeralFileSystemRule = Neo4Net.Test.rule.fs.EphemeralFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertArrayEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class LargePropertiesIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.fs.EphemeralFileSystemRule fs = new org.neo4j.test.rule.fs.EphemeralFileSystemRule();
		 public readonly EphemeralFileSystemRule Fs = new EphemeralFileSystemRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void readArrayAndStringPropertiesWithDifferentBlockSizes()
		 public virtual void ReadArrayAndStringPropertiesWithDifferentBlockSizes()
		 {
			  string stringValue = RandomStringUtils.randomAlphanumeric( 10000 );
			  sbyte[] arrayValue = RandomStringUtils.randomAlphanumeric( 10000 ).Bytes;

			  GraphDatabaseService db = ( new TestGraphDatabaseFactory() ).setFileSystem(Fs.get()).newImpermanentDatabaseBuilder().setConfig(GraphDatabaseSettings.string_block_size, "1024").setConfig(GraphDatabaseSettings.array_block_size, "2048").newGraphDatabase();
			  try
			  {
					long nodeId;
					using ( Transaction tx = Db.beginTx() )
					{
						 Node node = Db.createNode();
						 nodeId = node.Id;
						 node.SetProperty( "string", stringValue );
						 node.SetProperty( "array", arrayValue );
						 tx.Success();
					}

					using ( Transaction tx = Db.beginTx() )
					{
						 Node node = Db.getNodeById( nodeId );
						 assertEquals( stringValue, node.GetProperty( "string" ) );
						 assertArrayEquals( arrayValue, ( sbyte[] ) node.GetProperty( "array" ) );
						 tx.Success();
					}
			  }
			  finally
			  {
					Db.shutdown();
			  }
		 }
	}

}