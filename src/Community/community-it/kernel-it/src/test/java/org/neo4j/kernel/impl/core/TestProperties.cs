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
namespace Neo4Net.Kernel.impl.core
{
	using Test = org.junit.Test;

	using Node = Neo4Net.Graphdb.Node;
	using NotFoundException = Neo4Net.Graphdb.NotFoundException;
	using Transaction = Neo4Net.Graphdb.Transaction;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	public class TestProperties : AbstractNeo4jTestCase
	{
		 private const int VALUE_RANGE_SPLIT = 20;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void addAndRemovePropertiesWithinOneTransaction()
		 public virtual void AddAndRemovePropertiesWithinOneTransaction()
		 {
			  Node node = GraphDb.createNode();

			  node.SetProperty( "name", "oscar" );
			  node.SetProperty( "favourite_numbers", new long?[] { 1L, 2L, 3L } );
			  node.SetProperty( "favourite_colors", new string[] { "blue", "red" } );
			  node.RemoveProperty( "favourite_colors" );
			  NewTransaction();

			  assertNotNull( node.GetProperty( "favourite_numbers", null ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void addAndRemovePropertiesWithinOneTransaction2()
		 public virtual void AddAndRemovePropertiesWithinOneTransaction2()
		 {
			  Node node = GraphDb.createNode();
			  node.SetProperty( "foo", "bar" );

			  NewTransaction();
			  node.SetProperty( "foo2", "bar" );
			  node.RemoveProperty( "foo" );

			  NewTransaction();

			  try
			  {
					node.GetProperty( "foo" );
					fail( "property should not exist" );
			  }
			  catch ( NotFoundException )
			  {
					// good
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void removeAndAddSameProperty()
		 public virtual void RemoveAndAddSameProperty()
		 {
			  Node node = GraphDb.createNode();
			  node.SetProperty( "foo", "bar" );
			  NewTransaction();

			  node.RemoveProperty( "foo" );
			  node.SetProperty( "foo", "bar" );
			  NewTransaction();
			  assertEquals( "bar", node.GetProperty( "foo" ) );

			  node.SetProperty( "foo", "bar" );
			  node.RemoveProperty( "foo" );
			  NewTransaction();
			  assertNull( node.GetProperty( "foo", null ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void removeSomeAndSetSome()
		 public virtual void RemoveSomeAndSetSome()
		 {
			  Node node = GraphDb.createNode();
			  node.SetProperty( "remove me", "trash" );
			  NewTransaction();

			  node.RemoveProperty( "remove me" );
			  node.SetProperty( "foo", "bar" );
			  node.SetProperty( "baz", 17 );
			  NewTransaction();

			  assertEquals( "bar", node.GetProperty( "foo" ) );
			  assertEquals( 17, node.GetProperty( "baz" ) );
			  assertNull( node.GetProperty( "remove me", null ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void removeOneOfThree()
		 public virtual void RemoveOneOfThree()
		 {
			  Node node = GraphDb.createNode();
			  node.SetProperty( "1", 1 );
			  node.SetProperty( "2", 2 );
			  node.SetProperty( "3", 3 );
			  NewTransaction();

			  node.RemoveProperty( "2" );
			  NewTransaction();
			  assertNull( node.GetProperty( "2", null ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testLongPropertyValues()
		 public virtual void TestLongPropertyValues()
		 {
			  Node n = GraphDb.createNode();
			  SetPropertyAndAssertIt( n, -134217728L );
			  SetPropertyAndAssertIt( n, -134217729L );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIntPropertyValues()
		 public virtual void TestIntPropertyValues()
		 {
			  Node n = GraphDb.createNode();
			  SetPropertyAndAssertIt( n, -134217728 );
			  SetPropertyAndAssertIt( n, -134217729 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void booleanRange()
		 public virtual void BooleanRange()
		 {
			  Node node = GraphDb.createNode();
			  SetPropertyAndAssertIt( node, false );
			  SetPropertyAndAssertIt( node, true );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void byteRange()
		 public virtual void ByteRange()
		 {
			  Node node = GraphDb.createNode();
			  sbyte stride = sbyte.MaxValue / VALUE_RANGE_SPLIT;
			  for ( sbyte i = sbyte.MinValue; i < sbyte.MaxValue; )
			  {
					SetPropertyAndAssertIt( node, i );
					i = i > 0 && sbyte.MaxValue - i < stride ? sbyte.MaxValue : ( sbyte )( i + stride );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void charRange()
		 public virtual void CharRange()
		 {
			  Node node = GraphDb.createNode();
			  char stride = char.MaxValue / VALUE_RANGE_SPLIT;
			  for ( char i = char.MinValue; i < char.MaxValue; )
			  {
					SetPropertyAndAssertIt( node, i );
					i = i > ( char )0 && char.MaxValue - i < stride ? char.MaxValue : ( char )( i + stride );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shortRange()
		 public virtual void ShortRange()
		 {
			  Node node = GraphDb.createNode();
			  short stride = short.MaxValue / VALUE_RANGE_SPLIT;
			  for ( short i = short.MinValue; i < short.MaxValue; )
			  {
					SetPropertyAndAssertIt( node, i );
					i = i > 0 && short.MaxValue - i < stride ? short.MaxValue : ( short )( i + stride );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void intRange()
		 public virtual void IntRange()
		 {
			  Node node = GraphDb.createNode();
			  int stride = int.MaxValue / VALUE_RANGE_SPLIT;
			  for ( int i = int.MinValue; i < int.MaxValue; )
			  {
					SetPropertyAndAssertIt( node, i );
					i = i > 0 && int.MaxValue - i < stride ? int.MaxValue : i + stride;
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void longRange()
		 public virtual void LongRange()
		 {
			  Node node = GraphDb.createNode();
			  long stride = long.MaxValue / VALUE_RANGE_SPLIT;
			  for ( long i = long.MinValue; i < long.MaxValue; )
			  {
					SetPropertyAndAssertIt( node, i );
					i = i > 0 && long.MaxValue - i < stride ? long.MaxValue : i + stride;
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void floatRange()
		 public virtual void FloatRange()
		 {
			  Node node = GraphDb.createNode();
			  float stride = 16f;
			  for ( float i = float.Epsilon; i < float.MaxValue; )
			  {
					SetPropertyAndAssertIt( node, i );
					SetPropertyAndAssertIt( node, -i );
					i *= stride;
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void doubleRange()
		 public virtual void DoubleRange()
		 {
			  Node node = GraphDb.createNode();
			  double stride = 4194304d; // 2^23
			  for ( double i = double.Epsilon; i < double.MaxValue; )
			  {
					SetPropertyAndAssertIt( node, i );
					SetPropertyAndAssertIt( node, -i );
					i *= stride;
			  }
		 }

		 private void SetPropertyAndAssertIt( Node node, object value )
		 {
			  node.SetProperty( "key", value );
			  assertEquals( value, node.GetProperty( "key" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void loadManyProperties()
		 public virtual void LoadManyProperties()
		 {
			  Node node = GraphDb.createNode();
			  for ( int i = 0; i < 200; i++ )
			  {
					node.SetProperty( "property " + i, "value" );
			  }
			  NewTransaction();
			  assertEquals( "value", node.GetProperty( "property 0" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void name()
		 public virtual void Name()
		 {
			  Node node = GraphDb.createNode();
			  node.SetProperty( "name", "yo" );
			  node.GetProperty( "name" );
			  Commit();

			  using ( Transaction tx = GraphDb.beginTx() )
			  {
					node.GetProperty( "name" );
					tx.Success();
			  }
		 }
	}

}