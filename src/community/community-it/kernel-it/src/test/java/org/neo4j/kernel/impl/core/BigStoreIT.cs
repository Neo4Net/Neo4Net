using System.Collections.Generic;

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
	using SystemUtils = org.apache.commons.lang3.SystemUtils;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using TestName = org.junit.rules.TestName;


	using Direction = Neo4Net.GraphDb.Direction;
	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Label = Neo4Net.GraphDb.Label;
	using Node = Neo4Net.GraphDb.Node;
	using IPropertyContainer = Neo4Net.GraphDb.PropertyContainer;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using RelationshipType = Neo4Net.GraphDb.RelationshipType;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using Iterables = Neo4Net.Helpers.Collections.Iterables;
	using IdGeneratorFactory = Neo4Net.Kernel.impl.store.id.IdGeneratorFactory;
	using IdType = Neo4Net.Kernel.impl.store.id.IdType;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using EmbeddedDatabaseRule = Neo4Net.Test.rule.EmbeddedDatabaseRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.pow;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assume.assumeTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.MapUtil.map;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.AbstractNeo4NetTestCase.deleteFileOrDirectory;

	public class BigStoreIT : RelationshipType
	{
		 private static readonly RelationshipType _otherType = RelationshipType.withName( "OTHER" );

		 private const string PATH = "target/var/big";
		 private IGraphDatabaseService _db;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.TestName testName = new org.junit.rules.TestName()
		 public TestName testName = new TestNameAnonymousInnerClass();

		 private class TestNameAnonymousInnerClass : TestName
		 {
			 public override string MethodName
			 {
				 get
				 {
					  return outerInstance.GetType().Name + "#" + base.MethodName;
				 }
			 }
		 }
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.Neo4Net.test.rule.EmbeddedDatabaseRule dbRule = new org.Neo4Net.test.rule.EmbeddedDatabaseRule();
		 public EmbeddedDatabaseRule DbRule = new EmbeddedDatabaseRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void doBefore()
		 public virtual void DoBefore()
		 {
			  // Delete before just to be sure
			  deleteFileOrDirectory( new File( PATH ) );
			  _db = DbRule.GraphDatabaseAPI;
		 }

		 public override string Name()
		 {
			  return "BIG_TYPE";
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void create4BPlusStuff() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Create4BPlusStuff()
		 {
			  TestHighIds( ( long ) pow( 2, 32 ), 2, 400 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void create8BPlusStuff() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Create8BPlusStuff()
		 {
			  TestHighIds( ( long ) pow( 2, 33 ), 1, 1000 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void createAndVerify32BitGraph() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CreateAndVerify32BitGraph()
		 {
			  CreateAndVerifyGraphStartingWithId( ( long ) pow( 2, 32 ), 400 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void createAndVerify33BitGraph() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CreateAndVerify33BitGraph()
		 {
			  CreateAndVerifyGraphStartingWithId( ( long ) pow( 2, 33 ), 1000 );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void createAndVerifyGraphStartingWithId(long startId, int requiredHeapMb) throws Exception
		 private void CreateAndVerifyGraphStartingWithId( long startId, int requiredHeapMb )
		 {
			  assumeTrue( MachineIsOkToRunThisTest( requiredHeapMb ) );

			  /*
			   * Will create a layout like this:
			   *
			   * (refNode) --> (node) --> (highNode)
			   *           ...
			   *           ...
			   *
			   * Each node/relationship will have a bunch of different properties on them.
			   */
			  Node refNode = CreateReferenceNode( _db );
			  HighIds = startId - 1000;

			  sbyte[] bytes = new sbyte[45];
			  bytes[2] = 5;
			  bytes[10] = 42;
			  IDictionary<string, object> properties = map( "number", 11, "short string", "test", "long string", "This is a long value, long enough", "array", bytes );
			  Transaction tx = _db.beginTx();
			  int count = 10000;
			  for ( int i = 0; i < count; i++ )
			  {
					Node node = _db.createNode();
					SetProperties( node, properties );
					Relationship rel1 = refNode.CreateRelationshipTo( node, this );
					SetProperties( rel1, properties );
					Node highNode = _db.createNode();
					Relationship rel2 = node.CreateRelationshipTo( highNode, _otherType );
					SetProperties( rel2, properties );
					SetProperties( highNode, properties );
					if ( i % 100 == 0 && i > 0 )
					{
						 tx.Success();
						 tx.Close();
						 tx = _db.beginTx();
					}
			  }
			  tx.Success();
			  tx.Close();

			  _db = DbRule.restartDatabase();

			  // Verify the data
			  int verified = 0;

			  using ( Transaction transaction = _db.beginTx() )
			  {
					refNode = _db.getNodeById( refNode.Id );
					foreach ( Relationship rel in refNode.GetRelationships( Direction.OUTGOING ) )
					{
						 Node node = rel.EndNode;
						 AssertProperties( properties, node );
						 AssertProperties( properties, rel );
						 Node highNode = node.GetSingleRelationship( _otherType, Direction.OUTGOING ).EndNode;
						 AssertProperties( properties, highNode );
						 verified++;
					}
					transaction.Success();
			  }
			  assertEquals( count, verified );
		 }

		 private static readonly Label _reference = Label.label( "Reference" );

		 private Node CreateReferenceNode( IGraphDatabaseService db )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Node node = Db.createNode( _reference );
					tx.Success();
					return node;
			  }
		 }

		 public static bool MachineIsOkToRunThisTest( int requiredHeapMb )
		 {
			  if ( SystemUtils.IS_OS_WINDOWS )
			  {
					// This test cannot be run on Windows because it can't handle files of this size in a timely manner
					return false;
			  }
			  if ( SystemUtils.IS_OS_MAC_OSX )
			  {
					// This test cannot be run on Mac OS X because Mac OS X doesn't support sparse files
					return false;
			  }

			  // Not 1024, matches better wanted result with -Xmx
			  long heapMb = Runtime.Runtime.maxMemory() / (1000 * 1000);
			  return heapMb >= requiredHeapMb;
		 }

		 public static void AssertProperties( IDictionary<string, object> properties, IPropertyContainer IEntity )
		 {
			  int count = 0;
			  foreach ( string key in IEntity.PropertyKeys )
			  {
					object expectedValue = properties[key];
					object IEntityValue = IEntity.GetProperty( key );
					if ( expectedValue.GetType().IsArray )
					{
						 assertTrue( Arrays.Equals( ( sbyte[] ) expectedValue, ( sbyte[] ) IEntityValue ) );
					}
					else
					{
						 assertEquals( expectedValue, IEntityValue );
					}
					count++;
			  }
			  assertEquals( properties.Count, count );
		 }

		 private void SetProperties( IPropertyContainer IEntity, IDictionary<string, object> properties )
		 {
			  foreach ( KeyValuePair<string, object> property in properties.SetOfKeyValuePairs() )
			  {
					entity.SetProperty( property.Key, property.Value );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void testHighIds(long highMark, int minus, int requiredHeapMb) throws java.io.IOException
		 private void TestHighIds( long highMark, int minus, int requiredHeapMb )
		 {
			  if ( !MachineIsOkToRunThisTest( requiredHeapMb ) )
			  {
					return;
			  }

			  long idBelow = highMark - minus;
			  HighIds = idBelow;
			  string propertyKey = "name";
			  int intPropertyValue = 123;
			  string stringPropertyValue = "Long string, longer than would fit in shortstring";
			  long[] arrayPropertyValue = new long[]{ 1021L, 321L, 343212L };

			  Transaction tx = _db.beginTx();
			  Node nodeBelowTheLine = _db.createNode();
			  nodeBelowTheLine.SetProperty( propertyKey, intPropertyValue );
			  assertEquals( idBelow, nodeBelowTheLine.Id );
			  Node nodeAboveTheLine = _db.createNode();
			  nodeAboveTheLine.SetProperty( propertyKey, stringPropertyValue );
			  Relationship relBelowTheLine = nodeBelowTheLine.CreateRelationshipTo( nodeAboveTheLine, this );
			  relBelowTheLine.SetProperty( propertyKey, arrayPropertyValue );
			  assertEquals( idBelow, relBelowTheLine.Id );
			  Relationship relAboveTheLine = nodeAboveTheLine.CreateRelationshipTo( nodeBelowTheLine, this );
			  assertEquals( highMark, relAboveTheLine.Id );
			  assertEquals( highMark, nodeAboveTheLine.Id );
			  assertEquals( intPropertyValue, nodeBelowTheLine.GetProperty( propertyKey ) );
			  assertEquals( stringPropertyValue, nodeAboveTheLine.GetProperty( propertyKey ) );
			  assertTrue( Arrays.Equals( arrayPropertyValue, ( long[] ) relBelowTheLine.GetProperty( propertyKey ) ) );
			  tx.Success();
			  tx.Close();

			  for ( int i = 0; i < 2; i++ )
			  {
					using ( Transaction transaction = _db.beginTx() )
					{
						 assertEquals( nodeAboveTheLine, _db.getNodeById( highMark ) );
						 assertEquals( idBelow, nodeBelowTheLine.Id );
						 assertEquals( highMark, nodeAboveTheLine.Id );
						 assertEquals( idBelow, relBelowTheLine.Id );
						 assertEquals( highMark, relAboveTheLine.Id );
						 assertEquals( relBelowTheLine, _db.getNodeById( idBelow ).getSingleRelationship( this, Direction.OUTGOING ) );
						 assertEquals( relAboveTheLine, _db.getNodeById( idBelow ).getSingleRelationship( this, Direction.INCOMING ) );
						 assertEquals( idBelow, relBelowTheLine.Id );
						 assertEquals( highMark, relAboveTheLine.Id );
						 assertEquals( AsSet( asList( relBelowTheLine, relAboveTheLine ) ), AsSet( Iterables.asCollection( _db.getNodeById( idBelow ).Relationships ) ) );
						 transaction.Success();
					}
					if ( i == 0 )
					{
						 _db = DbRule.restartDatabase();
					}
			  }
		 }

		 private long HighIds
		 {
			 set
			 {
				  SetHighId( IdType.NODE, value );
				  SetHighId( IdType.RELATIONSHIP, value );
				  SetHighId( IdType.PROPERTY, value );
				  SetHighId( IdType.ARRAY_BLOCK, value );
				  SetHighId( IdType.STRING_BLOCK, value );
			 }
		 }

		 private static ICollection<T> AsSet<T>( ICollection<T> collection )
		 {
			  return new HashSet<T>( collection );
		 }

		 private void SetHighId( IdType type, long highId )
		 {
			  ( ( GraphDatabaseAPI )_db ).DependencyResolver.resolveDependency( typeof( IdGeneratorFactory ) ).get( type ).HighId = highId;
		 }
	}

}