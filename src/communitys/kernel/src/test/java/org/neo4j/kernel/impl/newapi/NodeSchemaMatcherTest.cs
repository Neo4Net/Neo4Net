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
namespace Neo4Net.Kernel.Impl.Newapi
{
	using Before = org.junit.Before;
	using Test = org.junit.Test;


	using StubNodeCursor = Neo4Net.@internal.Kernel.Api.helpers.StubNodeCursor;
	using IndexDescriptor = Neo4Net.Storageengine.Api.schema.IndexDescriptor;
	using Value = Neo4Net.Values.Storable.Value;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.contains;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.empty;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.iterator;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.schema.index.TestIndexDescriptorFactory.forLabel;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.stringValue;

	public class NodeSchemaMatcherTest
	{
		 private const int LABEL_ID1 = 10;
		 private const int LABEL_ID2 = 11;
		 private const int NON_EXISTENT_LABEL_ID = 12;
		 private const int PROP_ID1 = 20;
		 private const int PROP_ID2 = 21;
		 private const int UN_INDEXED_PROP_ID = 22;
		 private const int NON_EXISTENT_PROP_ID = 23;
		 private const int SPECIAL_PROP_ID = 24;
		 private static readonly int[] _props = new int[]{ PROP_ID1, PROP_ID2, UN_INDEXED_PROP_ID };

		 internal IndexDescriptor Index1 = forLabel( LABEL_ID1, PROP_ID1 );
		 internal IndexDescriptor Index1_2 = forLabel( LABEL_ID1, PROP_ID1, PROP_ID2 );
		 internal IndexDescriptor IndexWithMissingProperty = forLabel( LABEL_ID1, PROP_ID1, NON_EXISTENT_PROP_ID );
		 internal IndexDescriptor IndexWithMissingLabel = forLabel( NON_EXISTENT_LABEL_ID, PROP_ID1, PROP_ID2 );
		 internal IndexDescriptor IndexOnSpecialProperty = forLabel( LABEL_ID1, PROP_ID1, SPECIAL_PROP_ID );
		 private StubNodeCursor _node;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  Dictionary<int, Value> map = new Dictionary<int, Value>();
			  map[PROP_ID1] = stringValue( "hello" );
			  map[PROP_ID2] = stringValue( "world" );
			  map[UN_INDEXED_PROP_ID] = stringValue( "!" );
			  _node = new StubNodeCursor( false );
			  _node.withNode( 42, new long[]{ LABEL_ID1 }, map );
			  _node.next();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMatchOnSingleProperty()
		 public virtual void ShouldMatchOnSingleProperty()
		 {
			  // when
			  IList<IndexDescriptor> matched = new List<IndexDescriptor>();
			  NodeSchemaMatcher.OnMatchingSchema( iterator( Index1 ), UN_INDEXED_PROP_ID, _props, matched.add );

			  // then
			  assertThat( matched, contains( Index1 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMatchOnTwoProperties()
		 public virtual void ShouldMatchOnTwoProperties()
		 {
			  // when
			  IList<IndexDescriptor> matched = new List<IndexDescriptor>();
			  NodeSchemaMatcher.OnMatchingSchema( iterator( Index1_2 ), UN_INDEXED_PROP_ID, _props, matched.add );

			  // then
			  assertThat( matched, contains( Index1_2 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotMatchIfNodeIsMissingProperty()
		 public virtual void ShouldNotMatchIfNodeIsMissingProperty()
		 {
			  // when
			  IList<IndexDescriptor> matched = new List<IndexDescriptor>();
			  NodeSchemaMatcher.OnMatchingSchema( iterator( IndexWithMissingProperty ), UN_INDEXED_PROP_ID, _props, matched.add );

			  // then
			  assertThat( matched, empty() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotMatchIfNodeIsMissingLabel()
		 public virtual void ShouldNotMatchIfNodeIsMissingLabel()
		 {
			  // when
			  IList<IndexDescriptor> matched = new List<IndexDescriptor>();
			  NodeSchemaMatcher.OnMatchingSchema( iterator( IndexWithMissingLabel ), _node.labels().all(), UN_INDEXED_PROP_ID, _props, matched.add );

			  // then
			  assertThat( matched, empty() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMatchOnSpecialProperty()
		 public virtual void ShouldMatchOnSpecialProperty()
		 {
			  // when
			  IList<IndexDescriptor> matched = new List<IndexDescriptor>();
			  NodeSchemaMatcher.OnMatchingSchema( iterator( IndexOnSpecialProperty ), SPECIAL_PROP_ID, _props, matched.add );

			  // then
			  assertThat( matched, contains( IndexOnSpecialProperty ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMatchSeveralTimes()
		 public virtual void ShouldMatchSeveralTimes()
		 {
			  // given
			  IList<IndexDescriptor> indexes = Arrays.asList( Index1, Index1, Index1_2, Index1_2 );

			  // when
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.List<org.neo4j.storageengine.api.schema.IndexDescriptor> matched = new java.util.ArrayList<>();
			  IList<IndexDescriptor> matched = new List<IndexDescriptor>();
			  NodeSchemaMatcher.OnMatchingSchema( indexes.GetEnumerator(), UN_INDEXED_PROP_ID, _props, matched.add );

			  // then
			  assertThat( matched, equalTo( indexes ) );
		 }
	}

}