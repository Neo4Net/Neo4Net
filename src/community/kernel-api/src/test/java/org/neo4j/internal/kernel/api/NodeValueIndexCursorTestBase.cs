using System;
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
namespace Neo4Net.Internal.Kernel.Api
{
	using MutableLongList = org.eclipse.collections.api.list.primitive.MutableLongList;
	using MutableLongSet = org.eclipse.collections.api.set.primitive.MutableLongSet;
	using LongLists = org.eclipse.collections.impl.factory.primitive.LongLists;
	using LongHashSet = org.eclipse.collections.impl.set.mutable.primitive.LongHashSet;
	using Test = org.junit.Test;


	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Node = Neo4Net.GraphDb.Node;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using KernelException = Neo4Net.Internal.Kernel.Api.exceptions.KernelException;
	using CoordinateReferenceSystem = Neo4Net.Values.Storable.CoordinateReferenceSystem;
	using DateValue = Neo4Net.Values.Storable.DateValue;
	using PointValue = Neo4Net.Values.Storable.PointValue;
	using Value = Neo4Net.Values.Storable.Value;
	using ValueCategory = Neo4Net.Values.Storable.ValueCategory;
	using ValueGroup = Neo4Net.Values.Storable.ValueGroup;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.toIntExact;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assume.assumeTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.Label.label;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.CoordinateReferenceSystem.Cartesian;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.CoordinateReferenceSystem.Cartesian_3D;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.CoordinateReferenceSystem.WGS84;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.CoordinateReferenceSystem.WGS84_3D;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.Values.stringValue;

	public abstract class NodeValueIndexCursorTestBase<G> : KernelAPIReadTestBase<G> where G : KernelAPIReadTestSupport
	{
		 private const int TOTAL_NODE_COUNT = 37;
		 private static long _strOne, _strTwo1, _strTwo2, _strThree1, _strThree2, _strThree3;
		 private static long _boolTrue, _num5, _num6, _num12a, _num12b;
		 private static long _strOneNoLabel;
		 private static long _joeDalton, _williamDalton, _jackDalton, _averellDalton;
		 private static long _date891, _date892, _date86;
		 private static long[] _nodesOfAllPropertyTypes;
		 private static long _whateverPoint;

		 private static readonly PointValue _point_1 = PointValue.parse( "{latitude: 40.7128, longitude: -74.0060, crs: 'wgs-84'}" );
		 private static readonly PointValue _point_2 = PointValue.parse( "{latitude: 40.7128, longitude: -74.006000001, crs: 'wgs-84'}" );

		 public override void CreateTestGraph( IGraphDatabaseService graphDb )
		 {
			  using ( Transaction tx = graphDb.BeginTx() )
			  {
					graphDb.Schema().indexFor(label("Node")).on("prop").create();
					graphDb.Schema().indexFor(label("Node")).on("prop2").create();
					graphDb.Schema().indexFor(label("Node")).on("prop3").create();
					tx.Success();
			  }
			  using ( Transaction tx = graphDb.BeginTx() )
			  {
					graphDb.Schema().indexFor(label("What")).on("ever").create();
					tx.Success();
			  }
			  try
			  {
					  using ( Transaction tx = graphDb.BeginTx() )
					  {
						CreateCompositeIndex( graphDb, "Person", "firstname", "surname" );
						tx.Success();
					  }
			  }
			  catch ( Exception e )
			  {
					throw new AssertionError( e );
			  }
			  using ( Transaction tx = graphDb.BeginTx() )
			  {
					graphDb.Schema().awaitIndexesOnline(5, MINUTES);
					tx.Success();
			  }
			  using ( Transaction tx = graphDb.BeginTx() )
			  {
					_strOne = NodeWithProp( graphDb, "one" );
					_strTwo1 = NodeWithProp( graphDb, "two" );
					_strTwo2 = NodeWithProp( graphDb, "two" );
					_strThree1 = NodeWithProp( graphDb, "three" );
					_strThree2 = NodeWithProp( graphDb, "three" );
					_strThree3 = NodeWithProp( graphDb, "three" );
					NodeWithProp( graphDb, false );
					_boolTrue = NodeWithProp( graphDb, true );
					NodeWithProp( graphDb, 3 ); // Purposely mix ordering
					NodeWithProp( graphDb, 3 );
					NodeWithProp( graphDb, 3 );
					NodeWithProp( graphDb, 2 );
					NodeWithProp( graphDb, 2 );
					NodeWithProp( graphDb, 1 );
					NodeWithProp( graphDb, 4 );
					_num5 = NodeWithProp( graphDb, 5 );
					_num6 = NodeWithProp( graphDb, 6 );
					_num12a = NodeWithProp( graphDb, 12.0 );
					_num12b = NodeWithProp( graphDb, 12.0 );
					NodeWithProp( graphDb, 18 );
					NodeWithProp( graphDb, 24 );
					NodeWithProp( graphDb, 30 );
					NodeWithProp( graphDb, 36 );
					NodeWithProp( graphDb, 42 );
					_strOneNoLabel = NodeWithNoLabel( graphDb, "one" );
					_joeDalton = Person( graphDb, "Joe", "Dalton" );
					_williamDalton = Person( graphDb, "William", "Dalton" );
					_jackDalton = Person( graphDb, "Jack", "Dalton" );
					_averellDalton = Person( graphDb, "Averell", "Dalton" );
					NodeWithProp( graphDb, Values.pointValue( Cartesian, 1, 0 ) ); // Purposely mix order
					NodeWithProp( graphDb, Values.pointValue( Cartesian, 0, 0 ) );
					NodeWithProp( graphDb, Values.pointValue( Cartesian, 0, 0 ) );
					NodeWithProp( graphDb, Values.pointValue( Cartesian, 0, 0 ) );
					NodeWithProp( graphDb, Values.pointValue( Cartesian, 0, 1 ) );
					NodeWithProp( graphDb, Values.pointValue( Cartesian_3D, 0, 0, 0 ) );
					NodeWithProp( graphDb, Values.pointValue( WGS84, 0, 0 ) );
					NodeWithProp( graphDb, Values.pointValue( WGS84_3D, 0, 0, 0 ) );
					_date891 = NodeWithProp( graphDb, DateValue.date( 1989, 3, 24 ) ); // Purposely mix order
					_date86 = NodeWithProp( graphDb, DateValue.date( 1986, 11, 18 ) );
					_date892 = NodeWithProp( graphDb, DateValue.date( 1989, 3, 24 ) );
					NodeWithProp( graphDb, new string[]{ "first", "second", "third" } );
					NodeWithProp( graphDb, new string[]{ "fourth", "fifth", "sixth", "seventh" } );

					MutableLongList listOfIds = LongLists.mutable.empty();
					listOfIds.add( NodeWithWhatever( graphDb, "string" ) );
					listOfIds.add( NodeWithWhatever( graphDb, false ) );
					listOfIds.add( NodeWithWhatever( graphDb, 3 ) );
					listOfIds.add( NodeWithWhatever( graphDb, 13.0 ) );
					_whateverPoint = NodeWithWhatever( graphDb, Values.pointValue( Cartesian, 1, 0 ) );
					listOfIds.add( _whateverPoint );
					listOfIds.add( NodeWithWhatever( graphDb, DateValue.date( 1989, 3, 24 ) ) );
					listOfIds.add( NodeWithWhatever( graphDb, new string[]{ "first", "second", "third" } ) );

					_nodesOfAllPropertyTypes = listOfIds.toArray();

					AssertSameDerivedValue( _point_1, _point_2 );
					NodeWithProp( graphDb, "prop3", _point_1.asObjectCopy() );
					NodeWithProp( graphDb, "prop3", _point_2.asObjectCopy() );
					NodeWithProp( graphDb, "prop3", _point_2.asObjectCopy() );

					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected abstract void createCompositeIndex(org.Neo4Net.graphdb.GraphDatabaseService graphDb, String label, String... properties) throws Exception;
		 protected internal abstract void CreateCompositeIndex( IGraphDatabaseService graphDb, string label, params string[] properties );
		 protected internal abstract string ProviderKey();
		 protected internal abstract string ProviderVersion();

		 protected internal virtual bool IndexProvidesStringValues()
		 {
			  return false;
		 }

		 protected internal virtual bool IndexProvidesNumericValues()
		 {
			  return false;
		 }

		 protected internal virtual bool IndexProvidesArrayValues()
		 {
			  return false;
		 }

		 protected internal virtual bool IndexProvidesBooleanValues()
		 {
			  return false;
		 }

		 protected internal virtual bool IndexProvidesTemporalValues()
		 {
			  return true;
		 }
		 protected internal abstract void AssertSameDerivedValue( PointValue p1, PointValue p2 );

		 protected internal virtual bool IndexProvidesSpatialValues()
		 {
			  return false;
		 }

		 protected internal virtual bool IndexProvidesAllValues()
		 {
			  return false;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPerformExactLookup() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPerformExactLookup()
		 {
			  // given
			  bool needsValues = false;
			  int label = token.nodeLabel( "Node" );
			  int prop = token.propertyKey( "prop" );
			  IndexReference index = schemaRead.index( label, prop );
			  using ( NodeValueIndexCursor node = cursors.allocateNodeValueIndexCursor() )
			  {
					MutableLongSet uniqueIds = new LongHashSet();

					// when
					read.nodeIndexSeek( index, node, IndexOrder.None, needsValues, IndexQuery.Exact( prop, "zero" ) );

					// then
					AssertFoundNodesAndNoValue( node, uniqueIds );

					// when
					read.nodeIndexSeek( index, node, IndexOrder.None, needsValues, IndexQuery.Exact( prop, "one" ) );

					// then
					assertFoundNodesAndNoValue( node, uniqueIds, _strOne );

					// when
					read.nodeIndexSeek( index, node, IndexOrder.None, needsValues, IndexQuery.Exact( prop, "two" ) );

					// then
					assertFoundNodesAndNoValue( node, uniqueIds, _strTwo1, _strTwo2 );

					// when
					read.nodeIndexSeek( index, node, IndexOrder.None, needsValues, IndexQuery.Exact( prop, "three" ) );

					// then
					assertFoundNodesAndNoValue( node, uniqueIds, _strThree1, _strThree2, _strThree3 );

					// when
					read.nodeIndexSeek( index, node, IndexOrder.None, needsValues, IndexQuery.Exact( prop, 1 ) );

					// then
					AssertFoundNodesAndNoValue( node, 1, uniqueIds );

					//when
					read.nodeIndexSeek( index, node, IndexOrder.None, needsValues, IndexQuery.Exact( prop, 2 ) );

					// then
					AssertFoundNodesAndNoValue( node, 2, uniqueIds );

					//when
					read.nodeIndexSeek( index, node, IndexOrder.None, needsValues, IndexQuery.Exact( prop, 3 ) );

					// then
					AssertFoundNodesAndNoValue( node, 3, uniqueIds );

					//when
					read.nodeIndexSeek( index, node, IndexOrder.None, needsValues, IndexQuery.Exact( prop, 6 ) );

					// then
					assertFoundNodesAndNoValue( node, uniqueIds, _num6 );

					// when
					read.nodeIndexSeek( index, node, IndexOrder.None, needsValues, IndexQuery.Exact( prop, 12.0 ) );

					// then
					assertFoundNodesAndNoValue( node, uniqueIds, _num12a, _num12b );

					// when
					read.nodeIndexSeek( index, node, IndexOrder.None, needsValues, IndexQuery.Exact( prop, true ) );

					// then
					assertFoundNodesAndNoValue( node, uniqueIds, _boolTrue );

					// when
					read.nodeIndexSeek( index, node, IndexOrder.None, needsValues, IndexQuery.Exact( prop, Values.pointValue( Cartesian, 0, 0 ) ) );

					// then
					AssertFoundNodesAndNoValue( node, 3, uniqueIds );

					// when
					read.nodeIndexSeek( index, node, IndexOrder.None, needsValues, IndexQuery.Exact( prop, Values.pointValue( Cartesian_3D, 0, 0, 0 ) ) );

					// then
					AssertFoundNodesAndNoValue( node, 1, uniqueIds );

					// when
					read.nodeIndexSeek( index, node, IndexOrder.None, needsValues, IndexQuery.Exact( prop, Values.pointValue( WGS84, 0, 0 ) ) );

					// then
					AssertFoundNodesAndNoValue( node, 1, uniqueIds );

					// when
					read.nodeIndexSeek( index, node, IndexOrder.None, needsValues, IndexQuery.Exact( prop, Values.pointValue( WGS84_3D, 0, 0, 0 ) ) );

					// then
					AssertFoundNodesAndNoValue( node, 1, uniqueIds );

					// when
					read.nodeIndexSeek( index, node, IndexOrder.None, needsValues, IndexQuery.Exact( prop, DateValue.date( 1989, 3, 24 ) ) );

					// then
					AssertFoundNodesAndNoValue( node, 2, uniqueIds );

					// when
					read.nodeIndexSeek( index, node, IndexOrder.None, needsValues, IndexQuery.Exact( prop, DateValue.date( 1986, 11, 18 ) ) );

					// then
					AssertFoundNodesAndNoValue( node, 1, uniqueIds );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPerformExactLookupInCompositeIndex() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPerformExactLookupInCompositeIndex()
		 {
			  // given
			  bool needsValues = false;
			  int label = token.nodeLabel( "Person" );
			  int firstName = token.propertyKey( "firstname" );
			  int surname = token.propertyKey( "surname" );
			  IndexReference index = schemaRead.index( label, firstName, surname );
			  using ( NodeValueIndexCursor node = cursors.allocateNodeValueIndexCursor() )
			  {
					MutableLongSet uniqueIds = new LongHashSet();

					// when
					IndexValueCapability valueCapability = index.ValueCapability( ValueCategory.TEXT, ValueCategory.TEXT );
					read.nodeIndexSeek( index, node, IndexOrder.None, needsValues, IndexQuery.Exact( firstName, "Joe" ), IndexQuery.Exact( surname, "Dalton" ) );

					// then
					assertThat( node.NumberOfProperties(), equalTo(2) );
					AssertFoundNodesAndNoValue( node, 1, uniqueIds );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPerformStringPrefixSearch() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPerformStringPrefixSearch()
		 {
			  // given
			  bool needsValues = IndexProvidesStringValues();
			  int label = token.nodeLabel( "Node" );
			  int prop = token.propertyKey( "prop" );
			  IndexReference index = schemaRead.index( label, prop );
			  IndexValueCapability stringCapability = index.ValueCapability( ValueCategory.TEXT );
			  using ( NodeValueIndexCursor node = cursors.allocateNodeValueIndexCursor() )
			  {
					MutableLongSet uniqueIds = new LongHashSet();

					// when
					read.nodeIndexSeek( index, node, IndexOrder.None, needsValues, IndexQuery.StringPrefix( prop, stringValue( "t" ) ) );

					// then
					assertThat( node.NumberOfProperties(), equalTo(1) );
					assertFoundNodesAndValue( node, uniqueIds, stringCapability, needsValues, _strTwo1, _strTwo2, _strThree1, _strThree2, _strThree3 );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPerformStringSuffixSearch() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPerformStringSuffixSearch()
		 {
			  // given
			  bool needsValues = IndexProvidesStringValues();
			  int label = token.nodeLabel( "Node" );
			  int prop = token.propertyKey( "prop" );
			  IndexReference index = schemaRead.index( label, prop );
			  IndexValueCapability stringCapability = index.ValueCapability( ValueCategory.TEXT );
			  using ( NodeValueIndexCursor node = cursors.allocateNodeValueIndexCursor() )
			  {
					MutableLongSet uniqueIds = new LongHashSet();

					// when
					read.nodeIndexSeek( index, node, IndexOrder.None, needsValues, IndexQuery.StringSuffix( prop, stringValue( "e" ) ) );

					// then
					assertThat( node.NumberOfProperties(), equalTo(1) );
					assertFoundNodesAndValue( node, uniqueIds, stringCapability, needsValues, _strOne, _strThree1, _strThree2, _strThree3 );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPerformStringContainmentSearch() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPerformStringContainmentSearch()
		 {
			  // given
			  bool needsValues = IndexProvidesStringValues();
			  int label = token.nodeLabel( "Node" );
			  int prop = token.propertyKey( "prop" );
			  IndexReference index = schemaRead.index( label, prop );
			  IndexValueCapability stringCapability = index.ValueCapability( ValueCategory.TEXT );
			  using ( NodeValueIndexCursor node = cursors.allocateNodeValueIndexCursor() )
			  {
					MutableLongSet uniqueIds = new LongHashSet();

					// when
					read.nodeIndexSeek( index, node, IndexOrder.None, needsValues, IndexQuery.StringContains( prop, stringValue( "o" ) ) );

					// then
					assertThat( node.NumberOfProperties(), equalTo(1) );
					assertFoundNodesAndValue( node, uniqueIds, stringCapability, needsValues, _strOne, _strTwo1, _strTwo2 );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPerformStringRangeSearch() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPerformStringRangeSearch()
		 {
			  // given
			  bool needsValues = IndexProvidesStringValues();
			  int label = token.nodeLabel( "Node" );
			  int prop = token.propertyKey( "prop" );
			  IndexReference index = schemaRead.index( label, prop );
			  IndexValueCapability stringCapability = index.ValueCapability( ValueCategory.TEXT );
			  using ( NodeValueIndexCursor node = cursors.allocateNodeValueIndexCursor() )
			  {
					MutableLongSet uniqueIds = new LongHashSet();

					// when
					read.nodeIndexSeek( index, node, IndexOrder.None, needsValues, IndexQuery.Range( prop, "one", true, "three", true ) );

					// then

					assertFoundNodesAndValue( node, uniqueIds, stringCapability, needsValues, _strOne, _strThree1, _strThree2, _strThree3 );

					// when
					read.nodeIndexSeek( index, node, IndexOrder.None, needsValues, IndexQuery.Range( prop, "one", true, "three", false ) );

					// then
					assertFoundNodesAndValue( node, uniqueIds, stringCapability, needsValues, _strOne );

					// when
					read.nodeIndexSeek( index, node, IndexOrder.None, needsValues, IndexQuery.Range( prop, "one", false, "three", true ) );

					// then
					assertFoundNodesAndValue( node, uniqueIds, stringCapability, needsValues, _strThree1, _strThree2, _strThree3 );

					// when
					read.nodeIndexSeek( index, node, IndexOrder.None, needsValues, IndexQuery.Range( prop, "one", false, "two", false ) );

					// then
					assertFoundNodesAndValue( node, uniqueIds, stringCapability, needsValues, _strThree1, _strThree2, _strThree3 );

					// when
					read.nodeIndexSeek( index, node, IndexOrder.None, needsValues, IndexQuery.Range( prop, "one", true, "two", true ) );

					// then
					assertFoundNodesAndValue( node, uniqueIds, stringCapability, needsValues, _strOne, _strThree1, _strThree2, _strThree3, _strTwo1, _strTwo2 );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPerformNumericRangeSearch() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPerformNumericRangeSearch()
		 {
			  // given
			  bool needsValues = IndexProvidesNumericValues();
			  int label = token.nodeLabel( "Node" );
			  int prop = token.propertyKey( "prop" );
			  IndexReference index = schemaRead.index( label, prop );
			  IndexValueCapability numberCapability = index.ValueCapability( ValueCategory.NUMBER );
			  using ( NodeValueIndexCursor node = cursors.allocateNodeValueIndexCursor() )
			  {
					MutableLongSet uniqueIds = new LongHashSet();

					// when
					read.nodeIndexSeek( index, node, IndexOrder.None, needsValues, IndexQuery.Range( prop, 5, true, 12, true ) );

					// then
					assertFoundNodesAndValue( node, uniqueIds, numberCapability, needsValues, _num5, _num6, _num12a, _num12b );

					// when
					read.nodeIndexSeek( index, node, IndexOrder.None, needsValues, IndexQuery.Range( prop, 5, true, 12, false ) );

					// then
					assertFoundNodesAndValue( node, uniqueIds, numberCapability, needsValues, _num5, _num6 );

					// when
					read.nodeIndexSeek( index, node, IndexOrder.None, needsValues, IndexQuery.Range( prop, 5, false, 12, true ) );

					// then
					assertFoundNodesAndValue( node, uniqueIds, numberCapability, needsValues, _num6, _num12a, _num12b );

					// when
					read.nodeIndexSeek( index, node, IndexOrder.None, needsValues, IndexQuery.Range( prop, 5, false, 12, false ) );

					// then
					assertFoundNodesAndValue( node, uniqueIds, numberCapability, needsValues, _num6 );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPerformTemporalRangeSearch() throws org.Neo4Net.internal.kernel.api.exceptions.KernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPerformTemporalRangeSearch()
		 {
			  // given
			  bool needsValues = IndexProvidesTemporalValues();
			  int label = token.nodeLabel( "Node" );
			  int prop = token.propertyKey( "prop" );
			  IndexReference index = schemaRead.index( label, prop );
			  IndexValueCapability temporalCapability = index.ValueCapability( ValueCategory.TEMPORAL );
			  using ( NodeValueIndexCursor node = cursors.allocateNodeValueIndexCursor() )
			  {
					MutableLongSet uniqueIds = new LongHashSet();

					// when
					read.nodeIndexSeek( index, node, IndexOrder.None, needsValues, IndexQuery.Range( prop, DateValue.date( 1986, 11, 18 ), true, DateValue.date( 1989, 3, 24 ), true ) );

					// then
					assertFoundNodesAndValue( node, uniqueIds, temporalCapability, needsValues, _date86, _date891, _date892 );

					// when
					read.nodeIndexSeek( index, node, IndexOrder.None, needsValues, IndexQuery.Range( prop, DateValue.date( 1986, 11, 18 ), true, DateValue.date( 1989, 3, 24 ), false ) );

					// then
					assertFoundNodesAndValue( node, uniqueIds, temporalCapability, needsValues, _date86 );

					// when
					read.nodeIndexSeek( index, node, IndexOrder.None, needsValues, IndexQuery.Range( prop, DateValue.date( 1986, 11, 18 ), false, DateValue.date( 1989, 3, 24 ), true ) );

					// then
					assertFoundNodesAndValue( node, uniqueIds, temporalCapability, needsValues, _date891, _date892 );

					// when
					read.nodeIndexSeek( index, node, IndexOrder.None, needsValues, IndexQuery.Range( prop, DateValue.date( 1986, 11, 18 ), false, DateValue.date( 1989, 3, 24 ), false ) );

					// then
					AssertFoundNodesAndValue( node, uniqueIds, temporalCapability, needsValues );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPerformSpatialRangeSearch() throws org.Neo4Net.internal.kernel.api.exceptions.KernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPerformSpatialRangeSearch()
		 {
			  // given
			  bool needsValues = IndexProvidesSpatialValues();
			  int label = token.nodeLabel( "Node" );
			  int prop = token.propertyKey( "prop" );
			  IndexReference index = schemaRead.index( label, prop );
			  IndexValueCapability spatialCapability = index.ValueCapability( ValueCategory.GEOMETRY );
			  using ( NodeValueIndexCursor node = cursors.allocateNodeValueIndexCursor() )
			  {
					MutableLongSet uniqueIds = new LongHashSet();

					// when
					read.nodeIndexSeek( index, node, IndexOrder.None, needsValues, IndexQuery.Range( prop, Cartesian ) );

					// then
					AssertFoundNodesAndValue( node, 5, uniqueIds, spatialCapability, needsValues );

					// when
					read.nodeIndexSeek( index, node, IndexOrder.None, needsValues, IndexQuery.Range( prop, Cartesian_3D ) );

					// then
					AssertFoundNodesAndValue( node, 1, uniqueIds, spatialCapability, needsValues );

					// when
					read.nodeIndexSeek( index, node, IndexOrder.None, needsValues, IndexQuery.Range( prop, WGS84 ) );

					// then
					AssertFoundNodesAndValue( node, 1, uniqueIds, spatialCapability, needsValues );

					// when
					read.nodeIndexSeek( index, node, IndexOrder.None, needsValues, IndexQuery.Range( prop, WGS84_3D ) );

					// then
					AssertFoundNodesAndValue( node, 1, uniqueIds, spatialCapability, needsValues );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPerformBooleanSearch() throws org.Neo4Net.internal.kernel.api.exceptions.KernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPerformBooleanSearch()
		 {
			  // given
			  bool needsValues = IndexProvidesBooleanValues();
			  int label = token.nodeLabel( "Node" );
			  int prop = token.propertyKey( "prop" );
			  IndexReference index = schemaRead.index( label, prop );
			  IndexValueCapability capability = index.ValueCapability( ValueGroup.BOOLEAN.category() );
			  using ( NodeValueIndexCursor node = cursors.allocateNodeValueIndexCursor() )
			  {
					MutableLongSet uniqueIds = new LongHashSet();

					// when
					read.nodeIndexSeek( index, node, IndexOrder.None, needsValues, IndexQuery.Exact( prop, false ) );

					// then
					AssertFoundNodesAndValue( node, 1, uniqueIds, capability, needsValues );

					// when
					read.nodeIndexSeek( index, node, IndexOrder.None, needsValues, IndexQuery.Exact( prop, true ) );

					// then
					AssertFoundNodesAndValue( node, 1, uniqueIds, capability, needsValues );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPerformTextArraySearch() throws org.Neo4Net.internal.kernel.api.exceptions.KernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPerformTextArraySearch()
		 {
			  // given
			  bool needsValues = IndexProvidesArrayValues();
			  int label = token.nodeLabel( "Node" );
			  int prop = token.propertyKey( "prop" );
			  IndexReference index = schemaRead.index( label, prop );
			  IndexValueCapability capability = index.ValueCapability( ValueGroup.TEXT_ARRAY.category() );
			  using ( NodeValueIndexCursor node = cursors.allocateNodeValueIndexCursor() )
			  {
					MutableLongSet uniqueIds = new LongHashSet();

					// when
					read.nodeIndexSeek( index, node, IndexOrder.None, needsValues, IndexQuery.Exact( prop, new string[]{ "first", "second", "third" } ) );

					// then
					AssertFoundNodesAndValue( node, 1, uniqueIds, capability, needsValues );

					// when
					read.nodeIndexSeek( index, node, IndexOrder.None, needsValues, IndexQuery.Exact( prop, new string[]{ "fourth", "fifth", "sixth", "seventh" } ) );

					// then
					AssertFoundNodesAndValue( node, 1, uniqueIds, capability, needsValues );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPerformIndexScan() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPerformIndexScan()
		 {
			  // given
			  int label = token.nodeLabel( "Node" );
			  int prop = token.propertyKey( "prop" );
			  IndexReference index = schemaRead.index( label, prop );
			  IndexValueCapability wildcardCapability = index.ValueCapability( ValueCategory.UNKNOWN );
			  using ( NodeValueIndexCursor node = cursors.allocateNodeValueIndexCursor() )
			  {
					MutableLongSet uniqueIds = new LongHashSet();

					// when
					read.nodeIndexScan( index, node, IndexOrder.None, IndexProvidesAllValues() );

					// then
					assertThat( node.NumberOfProperties(), equalTo(1) );
					AssertFoundNodesAndValue( node, TOTAL_NODE_COUNT, uniqueIds, wildcardCapability, IndexProvidesAllValues() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespectOrderCapabilitiesForNumbers() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRespectOrderCapabilitiesForNumbers()
		 {
			  // given
			  bool needsValues = IndexProvidesNumericValues();
			  int label = token.nodeLabel( "Node" );
			  int prop = token.propertyKey( "prop" );
			  IndexReference index = schemaRead.index( label, prop );
			  IndexOrder[] orderCapabilities = index.OrderCapability( ValueCategory.NUMBER );
			  using ( NodeValueIndexCursor node = cursors.allocateNodeValueIndexCursor() )
			  {
					foreach ( IndexOrder orderCapability in orderCapabilities )
					{
						 // when
						 read.nodeIndexSeek( index, node, orderCapability, needsValues, IndexQuery.Range( prop, 1, true, 42, true ) );

						 // then
						 AssertFoundNodesInOrder( node, orderCapability );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespectOrderCapabilitiesForStrings() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRespectOrderCapabilitiesForStrings()
		 {
			  // given
			  bool needsValues = IndexProvidesStringValues();
			  int label = token.nodeLabel( "Node" );
			  int prop = token.propertyKey( "prop" );
			  IndexReference index = schemaRead.index( label, prop );
			  IndexOrder[] orderCapabilities = index.OrderCapability( ValueCategory.TEXT );
			  using ( NodeValueIndexCursor node = cursors.allocateNodeValueIndexCursor() )
			  {
					foreach ( IndexOrder orderCapability in orderCapabilities )
					{
						 // when
						 read.nodeIndexSeek( index, node, orderCapability, needsValues, IndexQuery.Range( prop, "one", true, "two", true ) );

						 // then
						 AssertFoundNodesInOrder( node, orderCapability );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespectOrderCapabilitiesForTemporal() throws org.Neo4Net.internal.kernel.api.exceptions.KernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRespectOrderCapabilitiesForTemporal()
		 {
			  // given
			  bool needsValues = IndexProvidesTemporalValues();
			  int label = token.nodeLabel( "Node" );
			  int prop = token.propertyKey( "prop" );
			  IndexReference index = schemaRead.index( label, prop );
			  IndexOrder[] orderCapabilities = index.OrderCapability( ValueCategory.TEMPORAL );
			  using ( NodeValueIndexCursor node = cursors.allocateNodeValueIndexCursor() )
			  {
					foreach ( IndexOrder orderCapability in orderCapabilities )
					{
						 // when
						 read.nodeIndexSeek( index, node, orderCapability, needsValues, IndexQuery.Range( prop, DateValue.date( 1986, 11, 18 ), true, DateValue.date( 1989, 3, 24 ), true ) );

						 // then
						 AssertFoundNodesInOrder( node, orderCapability );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespectOrderCapabilitiesForSpatial() throws org.Neo4Net.internal.kernel.api.exceptions.KernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRespectOrderCapabilitiesForSpatial()
		 {
			  // given
			  bool needsValues = IndexProvidesSpatialValues();
			  int label = token.nodeLabel( "Node" );
			  int prop = token.propertyKey( "prop" );
			  IndexReference index = schemaRead.index( label, prop );
			  IndexOrder[] orderCapabilities = index.OrderCapability( ValueCategory.GEOMETRY );
			  using ( NodeValueIndexCursor node = cursors.allocateNodeValueIndexCursor() )
			  {
					foreach ( IndexOrder orderCapability in orderCapabilities )
					{
						 // when
						 read.nodeIndexSeek( index, node, orderCapability, needsValues, IndexQuery.Range( prop, CoordinateReferenceSystem.Cartesian ) );

						 // then
						 AssertFoundNodesInOrder( node, orderCapability );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespectOrderCapabilitiesForStringArray() throws org.Neo4Net.internal.kernel.api.exceptions.KernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRespectOrderCapabilitiesForStringArray()
		 {
			  // given
			  bool needsValues = IndexProvidesSpatialValues();
			  int label = token.nodeLabel( "Node" );
			  int prop = token.propertyKey( "prop" );
			  IndexReference index = schemaRead.index( label, prop );
			  IndexOrder[] orderCapabilities = index.OrderCapability( ValueCategory.TEXT_ARRAY );
			  using ( NodeValueIndexCursor node = cursors.allocateNodeValueIndexCursor() )
			  {
					foreach ( IndexOrder orderCapability in orderCapabilities )
					{
						 // when
						 read.nodeIndexSeek( index, node, orderCapability, needsValues, IndexQuery.Range( prop, Values.of( new string[]{ "first", "second", "third" } ), true, Values.of( new string[]{ "fourth", "fifth", "sixth", "seventh" } ), true ) );

						 // then
						 AssertFoundNodesInOrder( node, orderCapability );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespectOrderCapabilitiesForWildcard() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRespectOrderCapabilitiesForWildcard()
		 {
			  // given
			  bool needsValues = false;
			  int label = token.nodeLabel( "Node" );
			  int prop = token.propertyKey( "prop" );
			  IndexReference index = schemaRead.index( label, prop );
			  IndexOrder[] orderCapabilities = index.OrderCapability( ValueCategory.UNKNOWN );
			  using ( NodeValueIndexCursor node = cursors.allocateNodeValueIndexCursor() )
			  {
					foreach ( IndexOrder orderCapability in orderCapabilities )
					{
						 // when
						 read.nodeIndexSeek( index, node, orderCapability, needsValues, IndexQuery.Exists( prop ) );

						 // then
						 AssertFoundNodesInOrder( node, orderCapability );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldProvideValuesForPoints() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldProvideValuesForPoints()
		 {
			  // given
			  assumeTrue( IndexProvidesAllValues() );

			  int label = token.nodeLabel( "What" );
			  int prop = token.propertyKey( "ever" );
			  IndexReference index = schemaRead.index( label, prop );
			  assertEquals( IndexValueCapability.Yes, index.ValueCapability( ValueCategory.GEOMETRY ) );

			  using ( NodeValueIndexCursor node = cursors.allocateNodeValueIndexCursor() )
			  {
					MutableLongSet uniqueIds = new LongHashSet();

					// when
					read.nodeIndexSeek( index, node, IndexOrder.None, true, IndexQuery.Range( prop, Cartesian ) );

					// then
					assertFoundNodesAndValue( node, uniqueIds, index.ValueCapability( ValueCategory.GEOMETRY ), true, _whateverPoint );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldProvideValuesForAllTypes() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldProvideValuesForAllTypes()
		 {
			  // given
			  assumeTrue( IndexProvidesAllValues() );

			  int label = token.nodeLabel( "What" );
			  int prop = token.propertyKey( "ever" );
			  IndexReference index = schemaRead.index( label, prop );
			  assertEquals( IndexValueCapability.Yes, index.ValueCapability( ValueCategory.UNKNOWN ) );

			  using ( NodeValueIndexCursor node = cursors.allocateNodeValueIndexCursor() )
			  {
					MutableLongSet uniqueIds = new LongHashSet();

					// when
					read.nodeIndexSeek( index, node, IndexOrder.None, true, IndexQuery.Exists( prop ) );

					// then
					assertFoundNodesAndValue( node, uniqueIds, index.ValueCapability( ValueCategory.UNKNOWN ), true, _nodesOfAllPropertyTypes );
			  }
		 }

		 private void AssertFoundNodesInOrder( NodeValueIndexCursor node, IndexOrder indexOrder )
		 {
			  Value currentValue = null;
			  while ( node.Next() )
			  {
					long nodeReference = node.NodeReference();
					Value storedValue = GetPropertyValueFromStore( nodeReference );
					if ( currentValue != null )
					{
						 switch ( indexOrder )
						 {
						 case Neo4Net.Internal.Kernel.Api.IndexOrder.Ascending:
							  assertTrue( "Requested ordering " + indexOrder + " was not respected.", Values.COMPARATOR.Compare( currentValue, storedValue ) <= 0 );
							  break;
						 case Neo4Net.Internal.Kernel.Api.IndexOrder.Descending:
							  assertTrue( "Requested ordering " + indexOrder + " was not respected.", Values.COMPARATOR.Compare( currentValue, storedValue ) >= 0 );
							  break;
						 case Neo4Net.Internal.Kernel.Api.IndexOrder.None:
							  // Don't verify
							  break;
						 default:
							  throw new System.NotSupportedException( "Can not verify ordering for " + indexOrder );
						 }
					}
					currentValue = storedValue;
			  }
		 }

		 private void AssertFoundNodesAndValue( NodeValueIndexCursor node, int nodes, MutableLongSet uniqueIds, IndexValueCapability expectValue, bool indexProvidesValues )
		 {
			  uniqueIds.clear();
			  for ( int i = 0; i < nodes; i++ )
			  {
					assertTrue( "at least " + nodes + " nodes, was " + uniqueIds.size(), node.Next() );
					long nodeReference = node.NodeReference();
					assertTrue( "all nodes are unique", uniqueIds.add( nodeReference ) );

					// Assert has value capability
					if ( IndexValueCapability.Yes.Equals( expectValue ) )
					{
						 assertTrue( "Value capability said index would have value for " + expectValue + ", but didn't", node.HasValue() );
					}

					// Assert has correct value
					if ( indexProvidesValues )
					{
						 assertTrue( "Index did not provide values", node.HasValue() );
						 Value storedValue = GetPropertyValueFromStore( nodeReference );
						 assertThat( "has correct value", node.PropertyValue( 0 ), @is( storedValue ) );
					}
			  }

			  assertFalse( "no more than " + nodes + " nodes", node.Next() );
		 }

		 private void AssertFoundNodesAndNoValue( NodeValueIndexCursor node, int nodes, MutableLongSet uniqueIds )
		 {
			  uniqueIds.clear();
			  for ( int i = 0; i < nodes; i++ )
			  {
					assertTrue( "at least " + nodes + " nodes, was " + uniqueIds.size(), node.Next() );
					long nodeReference = node.NodeReference();
					assertTrue( "all nodes are unique", uniqueIds.add( nodeReference ) );

					// We can't quite assert !node.hasValue() because even tho pure SpatialIndexReader is guaranteed to not return any values,
					// where null could be used, the generic native index, especially when having composite keys including spatial values it's
					// more of a gray area and some keys may be spatial, some not and therefore a proper Value[] will be extracted
					// potentially containing some NO_VALUE values.
			  }

			  assertFalse( "no more than " + nodes + " nodes", node.Next() );
		 }

		 private void AssertFoundNodesAndValue( NodeValueIndexCursor node, MutableLongSet uniqueIds, IndexValueCapability expectValue, bool indexProvidesValues, params long[] expected )
		 {
			  AssertFoundNodesAndValue( node, expected.Length, uniqueIds, expectValue, indexProvidesValues );

			  foreach ( long expectedNode in expected )
			  {
					assertTrue( "expected node " + expectedNode, uniqueIds.contains( expectedNode ) );
			  }
		 }

		 private void AssertFoundNodesAndNoValue( NodeValueIndexCursor node, MutableLongSet uniqueIds, params long[] expected )
		 {
			  AssertFoundNodesAndNoValue( node, expected.Length, uniqueIds );

			  foreach ( long expectedNode in expected )
			  {
					assertTrue( "expected node " + expectedNode, uniqueIds.contains( expectedNode ) );
			  }
		 }

		 private Value GetPropertyValueFromStore( long nodeReference )
		 {
			  using ( NodeCursor storeCursor = cursors.allocateNodeCursor(), PropertyCursor propertyCursor = cursors.allocatePropertyCursor() )
			  {
					read.singleNode( nodeReference, storeCursor );
					storeCursor.Next();
					storeCursor.Properties( propertyCursor );
					propertyCursor.Next();
					return propertyCursor.PropertyValue();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetNoIndexForMissingTokens()
		 public virtual void ShouldGetNoIndexForMissingTokens()
		 {
			  int label = token.nodeLabel( "Node" );
			  int prop = token.propertyKey( "prop" );
			  int badLabel = token.nodeLabel( "BAD_LABEL" );
			  int badProp = token.propertyKey( "badProp" );

			  assertEquals( "bad label", IndexReference.NO_INDEX, schemaRead.index( badLabel, prop ) );
			  assertEquals( "bad prop", IndexReference.NO_INDEX, schemaRead.index( label, badProp ) );
			  assertEquals( "just bad", IndexReference.NO_INDEX, schemaRead.index( badLabel, badProp ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetNoIndexForUnknownTokens()
		 public virtual void ShouldGetNoIndexForUnknownTokens()
		 {
			  int label = token.nodeLabel( "Node" );
			  int prop = token.propertyKey( "prop" );
			  int badLabel = int.MaxValue;
			  int badProp = int.MaxValue;

			  assertEquals( "bad label", IndexReference.NO_INDEX, schemaRead.index( badLabel, prop ) );
			  assertEquals( "bad prop", IndexReference.NO_INDEX, schemaRead.index( label, badProp ) );
			  assertEquals( "just bad", IndexReference.NO_INDEX, schemaRead.index( badLabel, badProp ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetVersionAndKeyFromIndexReference()
		 public virtual void ShouldGetVersionAndKeyFromIndexReference()
		 {
			  // Given
			  int label = token.nodeLabel( "Node" );
			  int prop = token.propertyKey( "prop" );
			  IndexReference index = schemaRead.index( label, prop );

			  assertEquals( ProviderKey(), index.ProviderKey() );
			  assertEquals( ProviderVersion(), index.ProviderVersion() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotFindDeletedNodeInIndexScan() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotFindDeletedNodeInIndexScan()
		 {
			  // Given
			  bool needsValues = IndexProvidesAllValues();
			  int label = token.nodeLabel( "Node" );
			  int prop = token.propertyKey( "prop" );
			  IndexReference index = schemaRead.index( label, prop );
			  IndexValueCapability wildcardCapability = index.ValueCapability( ValueCategory.UNKNOWN );
			  using ( Neo4Net.Internal.Kernel.Api.Transaction tx = beginTransaction(), NodeValueIndexCursor node = cursors.allocateNodeValueIndexCursor() )
			  {
					MutableLongSet uniqueIds = new LongHashSet();

					// when
					tx.DataRead().nodeIndexScan(index, node, IndexOrder.None, needsValues);
					assertThat( node.NumberOfProperties(), equalTo(1) );
					AssertFoundNodesAndValue( node, TOTAL_NODE_COUNT, uniqueIds, wildcardCapability, needsValues );

					// then
					tx.DataWrite().nodeDelete(_strOne);
					tx.DataRead().nodeIndexScan(index, node, IndexOrder.None, needsValues);
					AssertFoundNodesAndValue( node, TOTAL_NODE_COUNT - 1, uniqueIds, wildcardCapability, needsValues );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotFindDeletedNodeInIndexSeek() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotFindDeletedNodeInIndexSeek()
		 {
			  // Given
			  bool needsValues = false;
			  int label = token.nodeLabel( "Node" );
			  int prop = token.propertyKey( "prop" );
			  IndexReference index = schemaRead.index( label, prop );
			  using ( Neo4Net.Internal.Kernel.Api.Transaction tx = beginTransaction(), NodeValueIndexCursor node = cursors.allocateNodeValueIndexCursor() )
			  {
					// when
					tx.DataWrite().nodeDelete(_strOne);
					tx.DataRead().nodeIndexSeek(index, node, IndexOrder.None, needsValues, IndexQuery.Exact(prop, "one"));

					// then
					assertFalse( node.Next() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotFindDNodeWithRemovedLabelInIndexSeek() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotFindDNodeWithRemovedLabelInIndexSeek()
		 {
			  // Given
			  bool needsValues = false;
			  int label = token.nodeLabel( "Node" );
			  int prop = token.propertyKey( "prop" );
			  IndexReference index = schemaRead.index( label, prop );
			  using ( Neo4Net.Internal.Kernel.Api.Transaction tx = beginTransaction(), NodeValueIndexCursor node = cursors.allocateNodeValueIndexCursor() )
			  {
					// when
					tx.DataWrite().nodeRemoveLabel(_strOne, label);
					tx.DataRead().nodeIndexSeek(index, node, IndexOrder.None, needsValues, IndexQuery.Exact(prop, "one"));

					// then
					assertFalse( node.Next() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotFindUpdatedNodeInIndexSeek() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotFindUpdatedNodeInIndexSeek()
		 {
			  // Given
			  bool needsValues = false;
			  int label = token.nodeLabel( "Node" );
			  int prop = token.propertyKey( "prop" );
			  IndexReference index = schemaRead.index( label, prop );
			  using ( Neo4Net.Internal.Kernel.Api.Transaction tx = beginTransaction(), NodeValueIndexCursor node = cursors.allocateNodeValueIndexCursor() )
			  {
					// when
					tx.DataWrite().nodeSetProperty(_strOne, prop, stringValue("ett"));
					tx.DataRead().nodeIndexSeek(index, node, IndexOrder.None, needsValues, IndexQuery.Exact(prop, "one"));

					// then
					assertFalse( node.Next() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFindUpdatedNodeInIndexSeek() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFindUpdatedNodeInIndexSeek()
		 {
			  // Given
			  bool needsValues = false;
			  int label = token.nodeLabel( "Node" );
			  int prop = token.propertyKey( "prop" );
			  IndexReference index = schemaRead.index( label, prop );
			  using ( Neo4Net.Internal.Kernel.Api.Transaction tx = beginTransaction(), NodeValueIndexCursor node = cursors.allocateNodeValueIndexCursor() )
			  {
					// when
					tx.DataWrite().nodeSetProperty(_strOne, prop, stringValue("ett"));
					tx.DataRead().nodeIndexSeek(index, node, IndexOrder.None, needsValues, IndexQuery.Exact(prop, "ett"));

					// then
					assertTrue( node.Next() );
					assertEquals( _strOne, node.NodeReference() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFindSwappedNodeInIndexSeek() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFindSwappedNodeInIndexSeek()
		 {
			  // Given
			  bool needsValues = false;
			  int label = token.nodeLabel( "Node" );
			  int prop = token.propertyKey( "prop" );
			  IndexReference index = schemaRead.index( label, prop );
			  using ( Neo4Net.Internal.Kernel.Api.Transaction tx = beginTransaction(), NodeValueIndexCursor node = cursors.allocateNodeValueIndexCursor() )
			  {
					// when
					tx.DataWrite().nodeRemoveLabel(_strOne, label);
					tx.DataWrite().nodeAddLabel(_strOneNoLabel, label);
					tx.DataRead().nodeIndexSeek(index, node, IndexOrder.None, needsValues, IndexQuery.Exact(prop, "one"));

					// then
					assertTrue( node.Next() );
					assertEquals( _strOneNoLabel, node.NodeReference() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotFindDeletedNodeInRangeSearch() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotFindDeletedNodeInRangeSearch()
		 {
			  // Given
			  bool needsValues = IndexProvidesStringValues();
			  int label = token.nodeLabel( "Node" );
			  int prop = token.propertyKey( "prop" );
			  IndexReference index = schemaRead.index( label, prop );
			  using ( Neo4Net.Internal.Kernel.Api.Transaction tx = beginTransaction(), NodeValueIndexCursor node = cursors.allocateNodeValueIndexCursor() )
			  {
					// when
					tx.DataWrite().nodeDelete(_strOne);
					tx.DataWrite().nodeDelete(_strThree1);
					tx.DataWrite().nodeDelete(_strThree2);
					tx.DataWrite().nodeDelete(_strThree3);
					tx.DataRead().nodeIndexSeek(index, node, IndexOrder.None, needsValues, IndexQuery.Range(prop, "one", true, "three", true));

					// then
					assertFalse( node.Next() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotFindNodeWithRemovedLabelInRangeSearch() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotFindNodeWithRemovedLabelInRangeSearch()
		 {
			  // Given
			  bool needsValues = IndexProvidesStringValues();
			  int label = token.nodeLabel( "Node" );
			  int prop = token.propertyKey( "prop" );
			  IndexReference index = schemaRead.index( label, prop );
			  using ( Neo4Net.Internal.Kernel.Api.Transaction tx = beginTransaction(), NodeValueIndexCursor node = cursors.allocateNodeValueIndexCursor() )
			  {
					// when
					tx.DataWrite().nodeRemoveLabel(_strOne, label);
					tx.DataWrite().nodeRemoveLabel(_strThree1, label);
					tx.DataWrite().nodeRemoveLabel(_strThree2, label);
					tx.DataWrite().nodeRemoveLabel(_strThree3, label);
					tx.DataRead().nodeIndexSeek(index, node, IndexOrder.None, needsValues, IndexQuery.Range(prop, "one", true, "three", true));

					// then
					assertFalse( node.Next() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotFindUpdatedNodeInRangeSearch() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotFindUpdatedNodeInRangeSearch()
		 {
			  // Given
			  bool needsValues = IndexProvidesStringValues();
			  int label = token.nodeLabel( "Node" );
			  int prop = token.propertyKey( "prop" );
			  IndexReference index = schemaRead.index( label, prop );
			  using ( Neo4Net.Internal.Kernel.Api.Transaction tx = beginTransaction(), NodeValueIndexCursor node = cursors.allocateNodeValueIndexCursor() )
			  {
					// when
					tx.DataWrite().nodeSetProperty(_strOne, prop, stringValue("ett"));
					tx.DataWrite().nodeSetProperty(_strThree1, prop, stringValue("tre"));
					tx.DataWrite().nodeSetProperty(_strThree2, prop, stringValue("tre"));
					tx.DataWrite().nodeSetProperty(_strThree3, prop, stringValue("tre"));
					tx.DataRead().nodeIndexSeek(index, node, IndexOrder.None, needsValues, IndexQuery.Range(prop, "one", true, "three", true));

					// then
					assertFalse( node.Next() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFindUpdatedNodeInRangeSearch() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFindUpdatedNodeInRangeSearch()
		 {
			  // Given
			  bool needsValues = IndexProvidesStringValues();
			  int label = token.nodeLabel( "Node" );
			  int prop = token.propertyKey( "prop" );
			  IndexReference index = schemaRead.index( label, prop );
			  using ( Neo4Net.Internal.Kernel.Api.Transaction tx = beginTransaction(), NodeValueIndexCursor node = cursors.allocateNodeValueIndexCursor() )
			  {
					// when
					tx.DataWrite().nodeSetProperty(_strOne, prop, stringValue("ett"));
					tx.DataRead().nodeIndexSeek(index, node, IndexOrder.None, needsValues, IndexQuery.Range(prop, "ett", true, "tre", true));

					// then
					assertTrue( node.Next() );
					assertEquals( _strOne, node.NodeReference() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFindSwappedNodeInRangeSearch() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFindSwappedNodeInRangeSearch()
		 {
			  // Given
			  bool needsValues = IndexProvidesStringValues();
			  int label = token.nodeLabel( "Node" );
			  int prop = token.propertyKey( "prop" );
			  IndexReference index = schemaRead.index( label, prop );
			  using ( Neo4Net.Internal.Kernel.Api.Transaction tx = beginTransaction(), NodeValueIndexCursor node = cursors.allocateNodeValueIndexCursor() )
			  {
					// when
					tx.DataWrite().nodeRemoveLabel(_strOne, label);
					tx.DataWrite().nodeAddLabel(_strOneNoLabel, label);
					tx.DataRead().nodeIndexSeek(index, node, IndexOrder.None, needsValues, IndexQuery.Range(prop, "one", true, "ones", true));

					// then
					assertTrue( node.Next() );
					assertEquals( _strOneNoLabel, node.NodeReference() );
					assertFalse( node.Next() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotFindDeletedNodeInPrefixSearch() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotFindDeletedNodeInPrefixSearch()
		 {
			  // Given
			  bool needsValues = IndexProvidesStringValues();
			  int label = token.nodeLabel( "Node" );
			  int prop = token.propertyKey( "prop" );
			  IndexReference index = schemaRead.index( label, prop );
			  using ( Neo4Net.Internal.Kernel.Api.Transaction tx = beginTransaction(), NodeValueIndexCursor node = cursors.allocateNodeValueIndexCursor() )
			  {
					// when
					tx.DataWrite().nodeDelete(_strOne);
					tx.DataRead().nodeIndexSeek(index, node, IndexOrder.None, needsValues, IndexQuery.StringPrefix(prop, stringValue("on")));

					// then
					assertFalse( node.Next() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotFindNodeWithRemovedLabelInPrefixSearch() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotFindNodeWithRemovedLabelInPrefixSearch()
		 {
			  // Given
			  bool needsValues = IndexProvidesStringValues();
			  int label = token.nodeLabel( "Node" );
			  int prop = token.propertyKey( "prop" );
			  IndexReference index = schemaRead.index( label, prop );
			  using ( Neo4Net.Internal.Kernel.Api.Transaction tx = beginTransaction(), NodeValueIndexCursor node = cursors.allocateNodeValueIndexCursor() )
			  {
					// when
					tx.DataWrite().nodeRemoveLabel(_strOne, label);
					tx.DataRead().nodeIndexSeek(index, node, IndexOrder.None, needsValues, IndexQuery.StringPrefix(prop, stringValue("on")));

					// then
					assertFalse( node.Next() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotFindUpdatedNodeInPrefixSearch() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotFindUpdatedNodeInPrefixSearch()
		 {
			  // Given
			  bool needsValues = IndexProvidesStringValues();
			  int label = token.nodeLabel( "Node" );
			  int prop = token.propertyKey( "prop" );
			  IndexReference index = schemaRead.index( label, prop );
			  using ( Neo4Net.Internal.Kernel.Api.Transaction tx = beginTransaction(), NodeValueIndexCursor node = cursors.allocateNodeValueIndexCursor() )
			  {
					// when
					tx.DataWrite().nodeSetProperty(_strOne, prop, stringValue("ett"));
					tx.DataRead().nodeIndexSeek(index, node, IndexOrder.None, needsValues, IndexQuery.StringPrefix(prop, stringValue("on")));

					// then
					assertFalse( node.Next() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFindUpdatedNodeInPrefixSearch() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFindUpdatedNodeInPrefixSearch()
		 {
			  // Given
			  bool needsValues = IndexProvidesStringValues();
			  int label = token.nodeLabel( "Node" );
			  int prop = token.propertyKey( "prop" );
			  IndexReference index = schemaRead.index( label, prop );
			  using ( Neo4Net.Internal.Kernel.Api.Transaction tx = beginTransaction(), NodeValueIndexCursor node = cursors.allocateNodeValueIndexCursor() )
			  {
					// when
					tx.DataWrite().nodeSetProperty(_strOne, prop, stringValue("ett"));
					tx.DataRead().nodeIndexSeek(index, node, IndexOrder.None, needsValues, IndexQuery.StringPrefix(prop, stringValue("et")));

					// then
					assertTrue( node.Next() );
					assertEquals( _strOne, node.NodeReference() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFindSwappedNodeInPrefixSearch() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFindSwappedNodeInPrefixSearch()
		 {
			  // Given
			  bool needsValues = IndexProvidesStringValues();
			  int label = token.nodeLabel( "Node" );
			  int prop = token.propertyKey( "prop" );
			  IndexReference index = schemaRead.index( label, prop );
			  using ( Neo4Net.Internal.Kernel.Api.Transaction tx = beginTransaction(), NodeValueIndexCursor node = cursors.allocateNodeValueIndexCursor() )
			  {
					// when
					tx.DataWrite().nodeRemoveLabel(_strOne, label);
					tx.DataWrite().nodeAddLabel(_strOneNoLabel, label);
					tx.DataRead().nodeIndexSeek(index, node, IndexOrder.None, needsValues, IndexQuery.StringPrefix(prop, stringValue("on")));

					// then
					assertTrue( node.Next() );
					assertEquals( _strOneNoLabel, node.NodeReference() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotFindDeletedNodeInCompositeIndex() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotFindDeletedNodeInCompositeIndex()
		 {
			  // Given
			  bool needsValues = false;
			  int label = token.nodeLabel( "Person" );
			  int firstName = token.propertyKey( "firstname" );
			  int surname = token.propertyKey( "surname" );
			  IndexReference index = schemaRead.index( label, firstName, surname );
			  using ( Neo4Net.Internal.Kernel.Api.Transaction tx = beginTransaction(), NodeValueIndexCursor node = cursors.allocateNodeValueIndexCursor() )
			  {
					// when
					tx.DataWrite().nodeDelete(_jackDalton);
					tx.DataRead().nodeIndexSeek(index, node, IndexOrder.None, needsValues, IndexQuery.Exact(firstName, "Jack"), IndexQuery.Exact(surname, "Dalton"));

					// then
					assertFalse( node.Next() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotFindNodeWithRemovedLabelInCompositeIndex() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotFindNodeWithRemovedLabelInCompositeIndex()
		 {
			  // Given
			  bool needsValues = false;
			  int label = token.nodeLabel( "Person" );
			  int firstName = token.propertyKey( "firstname" );
			  int surname = token.propertyKey( "surname" );
			  IndexReference index = schemaRead.index( label, firstName, surname );
			  using ( Neo4Net.Internal.Kernel.Api.Transaction tx = beginTransaction(), NodeValueIndexCursor node = cursors.allocateNodeValueIndexCursor() )
			  {
					// when
					tx.DataWrite().nodeRemoveLabel(_joeDalton, label);
					tx.DataRead().nodeIndexSeek(index, node, IndexOrder.None, needsValues, IndexQuery.Exact(firstName, "Joe"), IndexQuery.Exact(surname, "Dalton"));
					// then
					assertFalse( node.Next() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotFindUpdatedNodeInCompositeIndex() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotFindUpdatedNodeInCompositeIndex()
		 {
			  // Given
			  bool needsValues = false;
			  int label = token.nodeLabel( "Person" );
			  int firstName = token.propertyKey( "firstname" );
			  int surname = token.propertyKey( "surname" );
			  IndexReference index = schemaRead.index( label, firstName, surname );
			  using ( Neo4Net.Internal.Kernel.Api.Transaction tx = beginTransaction(), NodeValueIndexCursor node = cursors.allocateNodeValueIndexCursor() )
			  {
					// when
					tx.DataWrite().nodeSetProperty(_jackDalton, firstName, stringValue("Jesse"));
					tx.DataWrite().nodeSetProperty(_jackDalton, surname, stringValue("James"));
					tx.DataRead().nodeIndexSeek(index, node, IndexOrder.None, needsValues, IndexQuery.Exact(firstName, "Jack"), IndexQuery.Exact(surname, "Dalton"));

					// then
					assertFalse( node.Next() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFindUpdatedNodeInCompositeIndex() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFindUpdatedNodeInCompositeIndex()
		 {
			  // Given
			  bool needsValues = false;
			  int label = token.nodeLabel( "Person" );
			  int firstName = token.propertyKey( "firstname" );
			  int surname = token.propertyKey( "surname" );
			  IndexReference index = schemaRead.index( label, firstName, surname );
			  using ( Neo4Net.Internal.Kernel.Api.Transaction tx = beginTransaction(), NodeValueIndexCursor node = cursors.allocateNodeValueIndexCursor() )
			  {
					// when
					tx.DataWrite().nodeSetProperty(_jackDalton, firstName, stringValue("Jesse"));
					tx.DataWrite().nodeSetProperty(_jackDalton, surname, stringValue("James"));
					tx.DataRead().nodeIndexSeek(index, node, IndexOrder.None, needsValues, IndexQuery.Exact(firstName, "Jesse"), IndexQuery.Exact(surname, "James"));

					// then
					assertTrue( node.Next() );
					assertEquals( _jackDalton, node.NodeReference() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFindSwappedNodeInCompositeIndex() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFindSwappedNodeInCompositeIndex()
		 {
			  // Given
			  bool needsValues = false;
			  int label = token.nodeLabel( "Person" );
			  int firstName = token.propertyKey( "firstname" );
			  int surname = token.propertyKey( "surname" );
			  IndexReference index = schemaRead.index( label, firstName, surname );
			  using ( Neo4Net.Internal.Kernel.Api.Transaction tx = beginTransaction(), NodeValueIndexCursor node = cursors.allocateNodeValueIndexCursor() )
			  {
					// when
					tx.DataWrite().nodeRemoveLabel(_joeDalton, label);
					tx.DataWrite().nodeAddLabel(_strOneNoLabel, label);
					tx.DataWrite().nodeSetProperty(_strOneNoLabel, firstName, stringValue("Jesse"));
					tx.DataWrite().nodeSetProperty(_strOneNoLabel, surname, stringValue("James"));
					tx.DataRead().nodeIndexSeek(index, node, IndexOrder.None, needsValues, IndexQuery.Exact(firstName, "Jesse"), IndexQuery.Exact(surname, "James"));

					// then
					assertTrue( node.Next() );
					assertEquals( _strOneNoLabel, node.NodeReference() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCountDistinctValues() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCountDistinctValues()
		 {
			  // Given
			  int label = token.nodeLabel( "Node" );
			  int key = token.propertyKey( "prop2" );
			  IndexReference index = schemaRead.index( label, key );
			  int expectedCount = 100;
			  IDictionary<Value, ISet<long>> expected = new Dictionary<Value, ISet<long>>();
			  using ( Neo4Net.Internal.Kernel.Api.Transaction tx = beginTransaction() )
			  {
					Write write = tx.DataWrite();
					ThreadLocalRandom random = ThreadLocalRandom.current();
					for ( int i = 0; i < expectedCount; i++ )
					{
						 object value = random.nextBoolean() ? (i % 10).ToString() : (i % 10);
						 long nodeId = write.NodeCreate();
						 write.NodeAddLabel( nodeId, label );
						 write.NodeSetProperty( nodeId, key, Values.of( value ) );
						 expected.computeIfAbsent( Values.of( value ), v => new HashSet<>() ).add(nodeId);
					}
					tx.Success();
			  }

			  // then
			  using ( Neo4Net.Internal.Kernel.Api.Transaction tx = beginTransaction(), NodeValueIndexCursor node = cursors.allocateNodeValueIndexCursor() )
			  {
					tx.DataRead().nodeIndexDistinctValues(index, node, true);
					long totalCount = 0;
					bool hasValues = true;
					while ( node.Next() )
					{
						 long count = node.NodeReference();
						 if ( node.HasValue() && node.PropertyValue(0) != null )
						 {
							  Value value = node.PropertyValue( 0 );
							  ISet<long> expectedNodes = expected.Remove( value );
							  assertNotNull( expectedNodes );
							  assertEquals( count, expectedNodes.Count );
						 }
						 else
						 {
							  // Some providers just can't serve the values for all types, which makes this test unable to do detailed checks for those values
							  // and the total count
							  hasValues = false;
						 }
						 totalCount += count;
					}
					if ( hasValues )
					{
						 assertTrue( expected.ToString(), expected.Count == 0 );
					}
					assertEquals( expectedCount, totalCount );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCountDistinctButSimilarPointValues() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCountDistinctButSimilarPointValues()
		 {
			  // given
			  int label = token.nodeLabel( "Node" );
			  int key = token.propertyKey( "prop3" );
			  IndexReference index = schemaRead.index( label, key );

			  // when
			  IDictionary<Value, int> expected = new Dictionary<Value, int>();
			  expected[_point_1] = 1;
			  expected[_point_2] = 2;
			  using ( Neo4Net.Internal.Kernel.Api.Transaction tx = beginTransaction(), NodeValueIndexCursor node = cursors.allocateNodeValueIndexCursor() )
			  {
					tx.DataRead().nodeIndexDistinctValues(index, node, true);

					// then
					while ( node.Next() )
					{
						 assertTrue( node.HasValue() );
						 assertTrue( expected.ContainsKey( node.PropertyValue( 0 ) ) );
						 assertEquals( expected.Remove( node.PropertyValue( 0 ) ).intValue(), toIntExact(node.NodeReference()) );
					}
					assertTrue( expected.Count == 0 );
			  }
		 }

		 private long NodeWithProp( IGraphDatabaseService graphDb, object value )
		 {
			  return NodeWithProp( graphDb, "prop", value );
		 }

		 private long NodeWithProp( IGraphDatabaseService graphDb, string key, object value )
		 {
			  Node node = graphDb.CreateNode( label( "Node" ) );
			  node.SetProperty( key, value );
			  return node.Id;
		 }

		 private long NodeWithWhatever( IGraphDatabaseService graphDb, object value )
		 {
			  Node node = graphDb.CreateNode( label( "What" ) );
			  node.SetProperty( "ever", value );
			  return node.Id;
		 }

		 private long NodeWithNoLabel( IGraphDatabaseService graphDb, object value )
		 {
			  Node node = graphDb.CreateNode();
			  node.SetProperty( "prop", value );
			  return node.Id;
		 }

		 private long Person( IGraphDatabaseService graphDb, string firstName, string surname )
		 {
			  Node node = graphDb.CreateNode( label( "Person" ) );
			  node.SetProperty( "firstname", firstName );
			  node.SetProperty( "surname", surname );
			  return node.Id;
		 }
	}

}