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
	using After = org.junit.After;
	using AfterClass = org.junit.AfterClass;
	using Before = org.junit.Before;
	using BeforeClass = org.junit.BeforeClass;
	using ClassRule = org.junit.ClassRule;
	using Test = org.junit.Test;

	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Node = Neo4Net.GraphDb.Node;
	using NotFoundException = Neo4Net.GraphDb.NotFoundException;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.configuration.Settings.TRUE;

	public class TestExceptionTypeOnInvalidIds
	{
		 private const long SMALL_POSITIVE_INTEGER = 5;
		 private const long SMALL_NEGATIVE_INTEGER = -5;
		 private static readonly long _bigPositiveInteger = int.MaxValue;
		 private static readonly long _bigNegativeInteger = int.MinValue;
		 private static readonly long _smallPositiveLong = ( ( long ) int.MaxValue ) + 1;
		 private static readonly long _smallNegativeLong = -( ( long ) int.MinValue ) - 1;
		 private static readonly long _bigPositiveLong = long.MaxValue;
		 private static readonly long _bigNegativeLong = long.MinValue;
		 private static IGraphDatabaseService _graphdb;
		 private static IGraphDatabaseService _graphDbReadOnly;
		 private Transaction _tx;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ClassRule public static final Neo4Net.test.rule.TestDirectory testDirectory = Neo4Net.test.rule.TestDirectory.testDirectory();
		 public static readonly TestDirectory TestDirectory = TestDirectory.testDirectory();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeClass public static void createDatabase()
		 public static void CreateDatabase()
		 {
			  _graphdb = ( new TestGraphDatabaseFactory() ).newEmbeddedDatabase(TestDirectory.storeDir());
			  File databaseDirectory = TestDirectory.databaseLayout( "read_only" ).databaseDirectory();
			  ( new TestGraphDatabaseFactory() ).newEmbeddedDatabase(databaseDirectory).shutdown();
			  _graphDbReadOnly = ( new TestGraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(databaseDirectory).setConfig(GraphDatabaseSettings.read_only, TRUE).newGraphDatabase();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AfterClass public static void destroyDatabase()
		 public static void DestroyDatabase()
		 {
			  _graphDbReadOnly.shutdown();
			  _graphDbReadOnly = null;
			  _graphdb.shutdown();
			  _graphdb = null;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void startTransaction()
		 public virtual void StartTransaction()
		 {
			  _tx = _graphdb.beginTx();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void endTransaction()
		 public virtual void EndTransaction()
		 {
			  _tx.close();
			  _tx = null;
		 }

		 /* behaves as expected */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = Neo4Net.graphdb.NotFoundException.class) public void getNodeBySmallPositiveInteger()
		 public virtual void getNodeBySmallPositiveInteger()
		 {
			  GetNodeById( SMALL_POSITIVE_INTEGER );
			  GetNodeByIdReadOnly( SMALL_POSITIVE_INTEGER );
		 }

		 /* throws IllegalArgumentException instead of NotFoundException */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = Neo4Net.graphdb.NotFoundException.class) public void getNodeBySmallNegativeInteger()
		 public virtual void getNodeBySmallNegativeInteger()
		 {
			  GetNodeById( SMALL_NEGATIVE_INTEGER );
			  GetNodeByIdReadOnly( SMALL_NEGATIVE_INTEGER );
		 }

		 /* behaves as expected */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = Neo4Net.graphdb.NotFoundException.class) public void getNodeByBigPositiveInteger()
		 public virtual void getNodeByBigPositiveInteger()
		 {
			  GetNodeById( _bigPositiveInteger );
			  GetNodeByIdReadOnly( _bigPositiveInteger );
		 }

		 /* throws IllegalArgumentException instead of NotFoundException */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = Neo4Net.graphdb.NotFoundException.class) public void getNodeByBigNegativeInteger()
		 public virtual void getNodeByBigNegativeInteger()
		 {
			  GetNodeById( _bigNegativeInteger );
			  GetNodeByIdReadOnly( _bigNegativeInteger );
		 }

		 /* throws IllegalArgumentException instead of NotFoundException */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = Neo4Net.graphdb.NotFoundException.class) public void getNodeBySmallPositiveLong()
		 public virtual void getNodeBySmallPositiveLong()
		 {
			  GetNodeById( _smallPositiveLong );
			  GetNodeByIdReadOnly( _smallPositiveLong );
		 }

		 /* behaves as expected */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = Neo4Net.graphdb.NotFoundException.class) public void getNodeBySmallNegativeLong()
		 public virtual void getNodeBySmallNegativeLong()
		 {
			  GetNodeById( _smallNegativeLong );
			  GetNodeByIdReadOnly( _smallNegativeLong );
		 }

		 /* throws IllegalArgumentException instead of NotFoundException */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = Neo4Net.graphdb.NotFoundException.class) public void getNodeByBigPositiveLong()
		 public virtual void getNodeByBigPositiveLong()
		 {
			  GetNodeById( _bigPositiveLong );
			  GetNodeByIdReadOnly( _bigPositiveLong );
		 }

		 /* finds the node with id=0, since that what the id truncates to */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = Neo4Net.graphdb.NotFoundException.class) public void getNodeByBigNegativeLong()
		 public virtual void getNodeByBigNegativeLong()
		 {
			  GetNodeById( _bigNegativeLong );
			  GetNodeByIdReadOnly( _bigNegativeLong );
		 }

		 /* behaves as expected */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = Neo4Net.graphdb.NotFoundException.class) public void getRelationshipBySmallPositiveInteger()
		 public virtual void getRelationshipBySmallPositiveInteger()
		 {
			  GetRelationshipById( SMALL_POSITIVE_INTEGER );
			  GetRelationshipByIdReadOnly( SMALL_POSITIVE_INTEGER );
		 }

		 /* throws IllegalArgumentException instead of NotFoundException */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = Neo4Net.graphdb.NotFoundException.class) public void getRelationshipBySmallNegativeInteger()
		 public virtual void getRelationshipBySmallNegativeInteger()
		 {
			  GetRelationshipById( SMALL_NEGATIVE_INTEGER );
			  GetRelationshipByIdReadOnly( SMALL_POSITIVE_INTEGER );
		 }

		 /* behaves as expected */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = Neo4Net.graphdb.NotFoundException.class) public void getRelationshipByBigPositiveInteger()
		 public virtual void getRelationshipByBigPositiveInteger()
		 {
			  GetRelationshipById( _bigPositiveInteger );
			  GetRelationshipByIdReadOnly( _bigPositiveInteger );
		 }

		 /* throws IllegalArgumentException instead of NotFoundException */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = Neo4Net.graphdb.NotFoundException.class) public void getRelationshipByBigNegativeInteger()
		 public virtual void getRelationshipByBigNegativeInteger()
		 {
			  GetRelationshipById( _bigNegativeInteger );
			  GetRelationshipByIdReadOnly( _bigNegativeInteger );
		 }

		 /* throws IllegalArgumentException instead of NotFoundException */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = Neo4Net.graphdb.NotFoundException.class) public void getRelationshipBySmallPositiveLong()
		 public virtual void getRelationshipBySmallPositiveLong()
		 {
			  GetRelationshipById( _smallPositiveLong );
			  GetRelationshipByIdReadOnly( _smallPositiveLong );
		 }

		 /* behaves as expected */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = Neo4Net.graphdb.NotFoundException.class) public void getRelationshipBySmallNegativeLong()
		 public virtual void getRelationshipBySmallNegativeLong()
		 {
			  GetRelationshipById( _smallNegativeLong );
			  GetRelationshipByIdReadOnly( _smallNegativeLong );
		 }

		 /* throws IllegalArgumentException instead of NotFoundException */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = Neo4Net.graphdb.NotFoundException.class) public void getRelationshipByBigPositiveLong()
		 public virtual void getRelationshipByBigPositiveLong()
		 {
			  GetRelationshipById( _bigPositiveLong );
			  GetRelationshipByIdReadOnly( _bigPositiveLong );
		 }

		 /* behaves as expected */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = Neo4Net.graphdb.NotFoundException.class) public void getRelationshipByBigNegativeLong()
		 public virtual void getRelationshipByBigNegativeLong()
		 {
			  GetRelationshipById( _bigNegativeLong );
			  GetRelationshipByIdReadOnly( _bigNegativeLong );
		 }

		 private static void GetNodeById( long index )
		 {
			  Node value = _graphdb.getNodeById( index );
			  fail( string.Format( "Returned Node [0x{0:x}] for index 0x{1:x} (int value: 0x{2:x})", value.Id, index, ( int ) index ) );
		 }

		 private static void GetNodeByIdReadOnly( long index )
		 {
			  Node value = _graphDbReadOnly.getNodeById( index );
			  fail( string.Format( "Returned Node [0x{0:x}] for index 0x{1:x} (int value: 0x{2:x})", value.Id, index, ( int ) index ) );
		 }

		 private static void GetRelationshipById( long index )
		 {
			  Relationship value = _graphdb.getRelationshipById( index );
			  fail( string.Format( "Returned Relationship [0x{0:x}] for index 0x{1:x} (int value: 0x{2:x})", value.Id, index, ( int ) index ) );
		 }

		 private static void GetRelationshipByIdReadOnly( long index )
		 {
			  Relationship value = _graphDbReadOnly.getRelationshipById( index );
			  fail( string.Format( "Returned Relationship [0x{0:x}] for index 0x{1:x} (int value: 0x{2:x})", value.Id, index, ( int ) index ) );
		 }
	}

}