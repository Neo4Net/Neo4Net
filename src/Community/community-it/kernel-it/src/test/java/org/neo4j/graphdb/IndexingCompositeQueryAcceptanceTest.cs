using System.Collections.Generic;
using System.Diagnostics;

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
namespace Neo4Net.Graphdb
{
	using LongIterator = org.eclipse.collections.api.iterator.LongIterator;
	using LongSet = org.eclipse.collections.api.set.primitive.LongSet;
	using MutableLongSet = org.eclipse.collections.api.set.primitive.MutableLongSet;
	using LongHashSet = org.eclipse.collections.impl.set.mutable.primitive.LongHashSet;
	using After = org.junit.After;
	using Before = org.junit.Before;
	using ClassRule = org.junit.ClassRule;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using TestName = org.junit.rules.TestName;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using IndexCreator = Neo4Net.Graphdb.schema.IndexCreator;
	using ImpermanentDatabaseRule = Neo4Net.Test.rule.ImpermanentDatabaseRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.IsEqual.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.array;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class IndexingCompositeQueryAcceptanceTest
	public class IndexingCompositeQueryAcceptanceTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ClassRule public static org.neo4j.test.rule.ImpermanentDatabaseRule dbRule = new org.neo4j.test.rule.ImpermanentDatabaseRule();
		 public static ImpermanentDatabaseRule DbRule = new ImpermanentDatabaseRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.TestName testName = new org.junit.rules.TestName();
		 public readonly TestName TestName = new TestName();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters public static java.util.List<Object[]> data()
		 public static IList<object[]> Data()
		 {
			  return Arrays.asList( TestCase( array( 2, 3 ), _biIndexSeek, true ), TestCase( array( 2, 3 ), _biIndexSeek, false ), TestCase( array( 2, 3, 4 ), _triIndexSeek, true ), TestCase( array( 2, 3, 4 ), _triIndexSeek, false ), TestCase( array( 2, 3, 4, 5, 6 ), _mapIndexSeek, true ), TestCase( array( 2, 3, 4, 5, 6 ), _mapIndexSeek, false ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter(0) public String[] keys;
		 public string[] Keys;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter(1) public Object[] values;
		 public object[] Values;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter(2) public Object[][] nonMatching;
		 public object[][] NonMatching;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter(3) public IndexSeek indexSeek;
		 public IndexSeek IndexSeek;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter(4) public boolean withIndex;
		 public bool WithIndex;

		 private static Label _label = Label.label( "LABEL1" );
		 private GraphDatabaseService _db;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  _db = DbRule.GraphDatabaseAPI;
			  if ( WithIndex )
			  {
					using ( Neo4Net.Graphdb.Transaction tx = _db.beginTx() )
					{
						 _db.schema().indexFor(_label).on(Keys[0]).create();

						 IndexCreator indexCreator = _db.schema().indexFor(_label);
						 foreach ( string key in Keys )
						 {
							  indexCreator = indexCreator.On( key );
						 }
						 indexCreator.Create();
						 tx.Success();
					}

					using ( Neo4Net.Graphdb.Transaction tx = _db.beginTx() )
					{
						 _db.schema().awaitIndexesOnline(5, TimeUnit.MINUTES);
						 tx.Success();
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown()
		 public virtual void TearDown()
		 {
			  DbRule.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSupportIndexSeek()
		 public virtual void ShouldSupportIndexSeek()
		 {
			  // GIVEN
			  CreateNodes( _db, _label, NonMatching );
			  LongSet expected = CreateNodes( _db, _label, Values );

			  // WHEN
			  MutableLongSet found = new LongHashSet();
			  using ( Transaction tx = _db.beginTx() )
			  {
					CollectNodes( found, IndexSeek.findNodes( Keys, Values, _db ) );
			  }

			  // THEN
			  assertThat( found, equalTo( expected ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSupportIndexSeekBackwardsOrder()
		 public virtual void ShouldSupportIndexSeekBackwardsOrder()
		 {
			  // GIVEN
			  CreateNodes( _db, _label, NonMatching );
			  LongSet expected = CreateNodes( _db, _label, Values );

			  // WHEN
			  MutableLongSet found = new LongHashSet();
			  string[] reversedKeys = new string[Keys.Length];
			  object[] reversedValues = new object[Keys.Length];
			  for ( int i = 0; i < Keys.Length; i++ )
			  {
					reversedValues[Keys.Length - 1 - i] = Values[i];
					reversedKeys[Keys.Length - 1 - i] = Keys[i];
			  }
			  using ( Transaction tx = _db.beginTx() )
			  {
					CollectNodes( found, IndexSeek.findNodes( reversedKeys, reversedValues, _db ) );
			  }

			  // THEN
			  assertThat( found, equalTo( expected ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIncludeNodesCreatedInSameTxInIndexSeek()
		 public virtual void ShouldIncludeNodesCreatedInSameTxInIndexSeek()
		 {
			  // GIVEN
			  CreateNodes( _db, _label, NonMatching[0], NonMatching[1] );
			  MutableLongSet expected = CreateNodes( _db, _label, Values );
			  // WHEN
			  MutableLongSet found = new LongHashSet();
			  using ( Transaction tx = _db.beginTx() )
			  {
					expected.add( CreateNode( _db, PropertyMap( Keys, Values ), _label ).Id );
					CreateNode( _db, PropertyMap( Keys, NonMatching[2] ), _label );

					CollectNodes( found, IndexSeek.findNodes( Keys, Values, _db ) );
			  }
			  // THEN
			  assertThat( found, equalTo( expected ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotIncludeNodesDeletedInSameTxInIndexSeek()
		 public virtual void ShouldNotIncludeNodesDeletedInSameTxInIndexSeek()
		 {
			  // GIVEN
			  CreateNodes( _db, _label, NonMatching[0] );
			  LongSet toDelete = CreateNodes( _db, _label, Values, NonMatching[1], NonMatching[2] );
			  MutableLongSet expected = CreateNodes( _db, _label, Values );
			  // WHEN
			  MutableLongSet found = new LongHashSet();
			  using ( Transaction tx = _db.beginTx() )
			  {
					LongIterator deleting = toDelete.longIterator();
					while ( deleting.hasNext() )
					{
						 long id = deleting.next();
						 _db.getNodeById( id ).delete();
						 expected.remove( id );
					}

					CollectNodes( found, IndexSeek.findNodes( Keys, Values, _db ) );
			  }
			  // THEN
			  assertThat( found, equalTo( expected ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldConsiderNodesChangedInSameTxInIndexSeek()
		 public virtual void ShouldConsiderNodesChangedInSameTxInIndexSeek()
		 {
			  // GIVEN
			  CreateNodes( _db, _label, NonMatching[0] );
			  LongSet toChangeToMatch = CreateNodes( _db, _label, NonMatching[1] );
			  LongSet toChangeToNotMatch = CreateNodes( _db, _label, Values );
			  MutableLongSet expected = CreateNodes( _db, _label, Values );
			  // WHEN
			  MutableLongSet found = new LongHashSet();
			  using ( Transaction tx = _db.beginTx() )
			  {
					LongIterator toMatching = toChangeToMatch.longIterator();
					while ( toMatching.hasNext() )
					{
						 long id = toMatching.next();
						 SetProperties( id, Values );
						 expected.add( id );
					}
					LongIterator toNotMatching = toChangeToNotMatch.longIterator();
					while ( toNotMatching.hasNext() )
					{
						 long id = toNotMatching.next();
						 SetProperties( id, NonMatching[2] );
						 expected.remove( id );
					}

					CollectNodes( found, IndexSeek.findNodes( Keys, Values, _db ) );
			  }
			  // THEN
			  assertThat( found, equalTo( expected ) );
		 }

		 public virtual MutableLongSet CreateNodes( GraphDatabaseService db, Label label, params object[][] propertyValueTuples )
		 {
			  MutableLongSet expected = new LongHashSet();
			  using ( Transaction tx = Db.beginTx() )
			  {
					foreach ( object[] valueTuple in propertyValueTuples )
					{
						 expected.add( CreateNode( db, PropertyMap( Keys, valueTuple ), label ).Id );
					}
					tx.Success();
			  }
			  return expected;
		 }

		 public static IDictionary<string, object> PropertyMap( string[] keys, object[] valueTuple )
		 {
			  IDictionary<string, object> propertyValues = new Dictionary<string, object>();
			  for ( int i = 0; i < keys.Length; i++ )
			  {
					propertyValues[keys[i]] = valueTuple[i];
			  }
			  return propertyValues;
		 }

		 public virtual void CollectNodes( MutableLongSet bucket, ResourceIterator<Node> toCollect )
		 {
			  while ( toCollect.MoveNext() )
			  {
					bucket.add( toCollect.Current.Id );
			  }
		 }

		 public virtual Node CreateNode( GraphDatabaseService beansAPI, IDictionary<string, object> properties, params Label[] labels )
		 {
			  using ( Transaction tx = beansAPI.BeginTx() )
			  {
					Node node = beansAPI.CreateNode( labels );
					foreach ( KeyValuePair<string, object> property in properties.SetOfKeyValuePairs() )
					{
						 node.SetProperty( property.Key, property.Value );
					}
					tx.Success();
					return node;
			  }
		 }

		 public static object[] TestCase( int?[] values, IndexSeek indexSeek, bool withIndex )
		 {
			  object[][] nonMatching = new object[][] { Plus( values, 1 ), Plus( values, 2 ), Plus( values, 3 ) };
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  string[] keys = java.util.values.Select( v => "key" + v ).ToArray( string[]::new );
			  return new object[]{ keys, values, nonMatching, indexSeek, withIndex };
		 }

		 public static object[] Plus<T>( int?[] values, int offset )
		 {
			  object[] result = new object[values.Length];
			  for ( int i = 0; i < values.Length; i++ )
			  {
					result[i] = values[i] + offset;
			  }
			  return result;
		 }

		 private void SetProperties( long id, object[] values )
		 {
			  Node node = _db.getNodeById( id );
			  for ( int i = 0; i < Keys.Length; i++ )
			  {
					node.SetProperty( Keys[i], values[i] );
			  }
		 }

		 private interface IndexSeek
		 {
			  ResourceIterator<Node> FindNodes( string[] keys, object[] values, GraphDatabaseService db );
		 }

		 private static IndexSeek _biIndexSeek = ( Keys, Values, _db ) =>
		 {
					 Debug.Assert( Keys.Length == 2 );
					 Debug.Assert( Values.Length == 2 );
					 return _db.findNodes( _label, Keys[0], Values[0], Keys[1], Values[1] );
		 };

		 private static IndexSeek _triIndexSeek = ( Keys, Values, _db ) =>
		 {
					 Debug.Assert( Keys.Length == 3 );
					 Debug.Assert( Values.Length == 3 );
					 return _db.findNodes( _label, Keys[0], Values[0], Keys[1], Values[1], Keys[2], Values[2] );
		 };

		 private static IndexSeek _mapIndexSeek = ( Keys, Values, _db ) =>
		 {
					 return _db.findNodes( _label, PropertyMap( Keys, Values ) );
		 };
	}

}