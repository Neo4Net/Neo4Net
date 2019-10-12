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
namespace Org.Neo4j.Kernel.Impl.Api
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Test = org.junit.Test;

	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Node = Org.Neo4j.Graphdb.Node;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using TestGraphDatabaseFactory = Org.Neo4j.Test.TestGraphDatabaseFactory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertArrayEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class PropertyTransactionStateTest
	{
		 private GraphDatabaseService _db;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  _db = ( new TestGraphDatabaseFactory() ).newImpermanentDatabase();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void shutDown()
		 public virtual void ShutDown()
		 {
			  _db.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testUpdateDoubleArrayProperty()
		 public virtual void TestUpdateDoubleArrayProperty()
		 {
			  Node node;
			  using ( Transaction tx = _db.beginTx() )
			  {
					node = _db.createNode();
					node.SetProperty( "foo", new double[] { 0, 0, 0, 0 } );
					tx.Success();
			  }

			  using ( Transaction ignore = _db.beginTx() )
			  {
					for ( int i = 0; i < 100; i++ )
					{
						 double[] data = ( double[] ) node.GetProperty( "foo" );
						 data[2] = i;
						 data[3] = i;
						 node.SetProperty( "foo", data );
						 assertArrayEquals( new double[] { 0, 0, i, i }, ( double[] ) node.GetProperty( "foo" ), 0.1D );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testStringPropertyUpdate()
		 public virtual void TestStringPropertyUpdate()
		 {
			  string key = "foo";
			  Node node;
			  using ( Transaction tx = _db.beginTx() )
			  {
					node = _db.createNode();
					node.SetProperty( key, "one" );
					tx.Success();
			  }

			  using ( Transaction ignore = _db.beginTx() )
			  {
					node.SetProperty( key, "one" );
					node.SetProperty( key, "two" );
					assertEquals( "two", node.GetProperty( key ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSetDoubleArrayProperty()
		 public virtual void TestSetDoubleArrayProperty()
		 {
			  using ( Transaction ignore = _db.beginTx() )
			  {
					Node node = _db.createNode();
					for ( int i = 0; i < 100; i++ )
					{
						 node.SetProperty( "foo", new double[] { 0, 0, i, i } );
						 assertArrayEquals( new double[] { 0, 0, i, i }, ( double[] ) node.GetProperty( "foo" ), 0.1D );
					}
			  }
		 }
	}

}