using System;
using System.Collections.Generic;
using System.Text;

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
namespace Org.Neo4j.Index
{
	using BeforeClass = org.junit.BeforeClass;
	using ClassRule = org.junit.ClassRule;
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using Node = Org.Neo4j.Graphdb.Node;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using DatabaseRule = Org.Neo4j.Test.rule.DatabaseRule;
	using ImpermanentDatabaseRule = Org.Neo4j.Test.rule.ImpermanentDatabaseRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.Label.label;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.count;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class IndexTxStateLookupTest
	public class IndexTxStateLookupTest
	{
		 private const string TRIGGER_LAZY = "this is supposed to be a really long property to trigger lazy loading";
		 private static readonly Random _random = new Random();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("RedundantStringConstructorCall") @Parameterized.Parameters(name = "store=<{0}> lookup=<{1}>") public static Iterable<Object[]> parameters()
		 public static IEnumerable<object[]> Parameters()
		 {
			  IList<object[]> parameters = new List<object[]>();
			  ( ( IList<object[]> )parameters ).AddRange( asList( new object[]{ "name", "name" }, new object[]{ 7, 7L }, new object[]{ 9L, 9 }, new object[]{ 2, 2.0 }, new object[]{ 3L, 3.0 }, new object[]{ 4, 4.0f }, new object[]{ 5L, 5.0f }, new object[]{ 12.0, 12 }, new object[]{ 13.0, 13L }, new object[]{ 14.0f, 14 }, new object[]{ 15.0f, 15L }, new object[]{ 2.5f, 2.5 }, new object[]{ 16.25, 16.25f }, new object[]{ StringArray( "a", "b", "c" ), CharArray( 'a', 'b', 'c' ) }, new object[]{ CharArray( 'd', 'e', 'f' ), StringArray( "d", "e", "f" ) }, new object[]{ SplitStrings( TRIGGER_LAZY ), SplitChars( TRIGGER_LAZY ) }, new object[]{ SplitChars( TRIGGER_LAZY ), SplitStrings( TRIGGER_LAZY ) }, new object[]{ StringArray( "foo", "bar" ), StringArray( "foo", "bar" ) } ) );
			  Type[] numberTypes = new Type[] { typeof( sbyte ), typeof( short ), typeof( int ), typeof( long ), typeof( float ), typeof( double ) };
			  foreach ( Type lhs in numberTypes )
			  {
					foreach ( Type rhs in numberTypes )
					{
						 parameters.Add( RandomNumbers( 3, lhs, rhs ) );
						 parameters.Add( RandomNumbers( 200, lhs, rhs ) );
					}
			  }
			  return parameters;
		 }

		 private class NamedObject
		 {
			  internal readonly object Object;
			  internal readonly string Name;

			  internal NamedObject( object @object, string name )
			  {
					this.Object = @object;
					this.Name = name;
			  }

			  public override string ToString()
			  {
					return Name;
			  }
		 }

		 private static NamedObject StringArray( params string[] items )
		 {
			  return new NamedObject( items, ArrayToString( items ) );
		 }

		 private static NamedObject CharArray( params char[] items )
		 {
			  return new NamedObject( items, ArrayToString( items ) );
		 }

		 private static object[] RandomNumbers( int length, Type lhsType, Type rhsType )
		 {
			  object lhs = Array.CreateInstance( lhsType, length );
			  object rhs = Array.CreateInstance( rhsType, length );
			  for ( int i = 0; i < length; i++ )
			  {
					int value = _random.Next( 128 );
					( ( Array )lhs ).SetValue( Convert( value, lhsType ), i );
					( ( Array )rhs ).SetValue( Convert( value, rhsType ), i );
			  }
			  return new object[]
			  {
				  new NamedObject( lhs, ArrayToString( lhs ) ),
				  new NamedObject( rhs, ArrayToString( rhs ) )
			  };
		 }

		 private static string ArrayToString( object arrayObject )
		 {
			  int length = Array.getLength( arrayObject );
			  string type = arrayObject.GetType().GetElementType().SimpleName;
			  StringBuilder builder = new StringBuilder( "(" + type + ") {" );
			  for ( int i = 0; i < length; i++ )
			  {
					builder.Append( i > 0 ? "," : "" ).Append( Array.get( arrayObject, i ) );
			  }
			  return builder.Append( "}" ).ToString();
		 }

		 private static object Convert( int value, Type type )
		 {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  switch ( type.FullName )
			  {
			  case "byte":
					return ( sbyte ) value;
			  case "short":
					return ( short ) value;
			  case "int":
					return value;
			  case "long":
					return ( long ) value;
			  case "float":
					return ( float ) value;
			  case "double":
					return ( double ) value;
			  default:
					return value;
			  }
		 }

		 private static NamedObject SplitStrings( string @string )
		 {
			  char[] chars = InternalSplitChars( @string );
			  string[] result = new string[chars.Length];
			  for ( int i = 0; i < chars.Length; i++ )
			  {
					result[i] = Convert.ToString( chars[i] );
			  }
			  return StringArray( result );
		 }

		 private static char[] InternalSplitChars( string @string )
		 {
			  char[] result = new char[@string.Length];
			  @string.CopyTo( 0, result, 0, result.Length - 0 );
			  return result;
		 }

		 private static NamedObject SplitChars( string @string )
		 {
			  char[] result = InternalSplitChars( @string );
			  return CharArray( result );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ClassRule public static final org.neo4j.test.rule.DatabaseRule db = new org.neo4j.test.rule.ImpermanentDatabaseRule();
		 public static readonly DatabaseRule Db = new ImpermanentDatabaseRule();

		 private readonly object _store;
		 private readonly object _lookup;

		 public IndexTxStateLookupTest( object store, object lookup )
		 {
			  this._store = RealValue( store );
			  this._lookup = RealValue( lookup );
		 }

		 private object RealValue( object @object )
		 {
			  return @object is NamedObject ? ( ( NamedObject )@object ).Object : @object;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeClass public static void given()
		 public static void Given()
		 {
			  // database with an index on `(:Node).prop`
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().indexFor(label("Node")).on("prop").create();
					tx.Success();
			  }
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().awaitIndexesOnline(10, SECONDS);
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void lookupWithinTransaction()
		 public virtual void LookupWithinTransaction()
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					// when
					Db.createNode( label( "Node" ) ).setProperty( "prop", _store );

					// then
					assertEquals( 1, count( Db.findNodes( label( "Node" ), "prop", _lookup ) ) );

					// no need to actually commit this node
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void lookupWithinTransactionWithCacheEviction()
		 public virtual void LookupWithinTransactionWithCacheEviction()
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					// when
					Db.createNode( label( "Node" ) ).setProperty( "prop", _store );

					// then
					assertEquals( 1, count( Db.findNodes( label( "Node" ), "prop", _lookup ) ) );

					// no need to actually commit this node
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void lookupWithoutTransaction()
		 public virtual void LookupWithoutTransaction()
		 {
			  // when
			  Node node;
			  using ( Transaction tx = Db.beginTx() )
			  {
					( node = Db.createNode( label( "Node" ) ) ).setProperty( "prop", _store );
					tx.Success();
			  }
			  // then
			  using ( Transaction tx = Db.beginTx() )
			  {
					assertEquals( 1, count( Db.findNodes( label( "Node" ), "prop", _lookup ) ) );
					tx.Success();
			  }
			  DeleteNode( node );
		 }

		 private void DeleteNode( Node node )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					node.Delete();
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void lookupWithoutTransactionWithCacheEviction()
		 public virtual void LookupWithoutTransactionWithCacheEviction()
		 {
			  // when
			  Node node;
			  using ( Transaction tx = Db.beginTx() )
			  {
					( node = Db.createNode( label( "Node" ) ) ).setProperty( "prop", _store );
					tx.Success();
			  }
			  // then
			  using ( Transaction tx = Db.beginTx() )
			  {
					assertEquals( 1, count( Db.findNodes( label( "Node" ), "prop", _lookup ) ) );
					tx.Success();
			  }
			  DeleteNode( node );
		 }
	}

}