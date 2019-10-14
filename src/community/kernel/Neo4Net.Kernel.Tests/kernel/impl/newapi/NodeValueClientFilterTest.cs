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
namespace Neo4Net.Kernel.Impl.Newapi
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using IndexOrder = Neo4Net.Internal.Kernel.Api.IndexOrder;
	using IndexQuery = Neo4Net.Internal.Kernel.Api.IndexQuery;
	using Read = Neo4Net.Internal.Kernel.Api.Read;
	using StubNodeCursor = Neo4Net.Internal.Kernel.Api.helpers.StubNodeCursor;
	using StubPropertyCursor = Neo4Net.Internal.Kernel.Api.helpers.StubPropertyCursor;
	using TestIndexDescriptorFactory = Neo4Net.Kernel.api.schema.index.TestIndexDescriptorFactory;
	using IndexDescriptor = Neo4Net.Storageengine.Api.schema.IndexDescriptor;
	using IndexProgressor = Neo4Net.Storageengine.Api.schema.IndexProgressor;
	using IndexProgressor_NodeValueClient = Neo4Net.Storageengine.Api.schema.IndexProgressor_NodeValueClient;
	using RandomRule = Neo4Net.Test.rule.RandomRule;
	using Value = Neo4Net.Values.Storable.Value;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.genericMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.NO_VALUE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.stringValue;

	public class NodeValueClientFilterTest : IndexProgressor, IndexProgressor_NodeValueClient
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.RandomRule random = new org.neo4j.test.rule.RandomRule();
		 public readonly RandomRule Random = new RandomRule();

		 private readonly Read _read = mock( typeof( Read ) );
		 private readonly IList<Event> _events = new List<Event>();
		 private readonly StubNodeCursor _node = new StubNodeCursor();
		 private readonly StubPropertyCursor _property = new StubPropertyCursor();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAcceptAllNodesOnNoFilters()
		 public virtual void ShouldAcceptAllNodesOnNoFilters()
		 {
			  // given
			  _node.withNode( 17 );
			  NodeValueClientFilter filter = InitializeFilter();

			  // when
			  filter.Next();
			  assertTrue( filter.AcceptNode( 17, null ) );
			  filter.Close();

			  // then
			  AssertEvents( Initialize(), Event.NEXT, new Event.Node(17, null), Event.CLOSE );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRejectNodeNotInUse()
		 public virtual void ShouldRejectNodeNotInUse()
		 {
			  // given
			  NodeValueClientFilter filter = InitializeFilter( IndexQuery.exists( 12 ) );

			  // when
			  filter.Next();
			  assertFalse( filter.AcceptNode( 17, null ) );
			  filter.Close();

			  // then
			  AssertEvents( Initialize(), Event.NEXT, Event.CLOSE );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRejectNodeWithNoProperties()
		 public virtual void ShouldRejectNodeWithNoProperties()
		 {
			  // given
			  _node.withNode( 17 );
			  NodeValueClientFilter filter = InitializeFilter( IndexQuery.exists( 12 ) );

			  // when
			  filter.Next();
			  assertFalse( filter.AcceptNode( 17, null ) );
			  filter.Close();

			  // then
			  AssertEvents( Initialize(), Event.NEXT, Event.CLOSE );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAcceptNodeWithMatchingProperty()
		 public virtual void ShouldAcceptNodeWithMatchingProperty()
		 {
			  // given
			  _node.withNode( 17, new long[0], genericMap( 12, stringValue( "hello" ) ) );
			  NodeValueClientFilter filter = InitializeFilter( IndexQuery.exists( 12 ) );

			  // when
			  filter.Next();
			  assertTrue( filter.AcceptNode( 17, null ) );
			  filter.Close();

			  // then
			  AssertEvents( Initialize(), Event.NEXT, new Event.Node(17, null), Event.CLOSE );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAcceptNodeWithoutMatchingProperty()
		 public virtual void ShouldNotAcceptNodeWithoutMatchingProperty()
		 {
			  // given
			  _node.withNode( 17, new long[0], genericMap( 7, stringValue( "wrong" ) ) );
			  NodeValueClientFilter filter = InitializeFilter( IndexQuery.exists( 12 ) );

			  // when
			  filter.Next();
			  assertFalse( filter.AcceptNode( 17, null ) );
			  filter.Close();

			  // then
			  AssertEvents( Initialize(), Event.NEXT, Event.CLOSE );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldConsultProvidedAcceptingFiltersForMixOfValuesAndNoValues()
		 public virtual void ShouldConsultProvidedAcceptingFiltersForMixOfValuesAndNoValues()
		 {
			  ShouldConsultProvidedFilters( System.Func.identity(), true );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldConsultProvidedAcceptingFiltersForNullValues()
		 public virtual void ShouldConsultProvidedAcceptingFiltersForNullValues()
		 {
			  ShouldConsultProvidedFilters( v => null, true );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldConsultProvidedDenyingFiltersForMixOfValuesAndNoValues()
		 public virtual void ShouldConsultProvidedDenyingFiltersForMixOfValuesAndNoValues()
		 {
			  ShouldConsultProvidedFilters( System.Func.identity(), false );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldConsultProvidedDenyingFiltersForNullValues()
		 public virtual void ShouldConsultProvidedDenyingFiltersForNullValues()
		 {
			  ShouldConsultProvidedFilters( v => null, false );
		 }

		 private void ShouldConsultProvidedFilters( System.Func<Value[], Value[]> filterValues, bool filterAcceptsValue )
		 {
			  // given
			  long nodeReference = 123;
			  int labelId = 10;
			  int slots = Random.Next( 3, 8 );
			  IndexQuery[] filters = new IndexQuery[slots];
			  Value[] actualValues = new Value[slots];
			  Value[] values = new Value[slots];
			  IDictionary<int, Value> properties = new Dictionary<int, Value>();
			  int[] propertyKeyIds = new int[slots];
			  int filterCount = 0;
			  for ( int i = 0; i < slots; i++ )
			  {
					actualValues[i] = Random.nextValue();
					int propertyKeyId = i;
					propertyKeyIds[i] = propertyKeyId;
					//    we want at least one filter         ,  randomly add filter or not
					if ( ( filterCount == 0 && i == slots - 1 ) || Random.nextBoolean() )
					{
						 object filterValue = ( filterAcceptsValue ? actualValues[i] : AnyOtherValueThan( actualValues[i] ) ).asObjectCopy();
						 filters[i] = IndexQuery.exact( propertyKeyId, filterValue );
						 filterCount++;
					}
					values[i] = Random.nextBoolean() ? NO_VALUE : actualValues[i];
					properties[propertyKeyId] = actualValues[i];
			  }
			  _node.withNode( nodeReference, new long[]{ labelId }, properties );

			  // when
			  NodeValueClientFilter filter = new NodeValueClientFilter( this, _node, _property, _read, filters );
			  filter.Initialize( TestIndexDescriptorFactory.forLabel( labelId, propertyKeyIds ), this, null, IndexOrder.NONE, true );
			  bool accepted = filter.AcceptNode( nodeReference, filterValues( values ) );

			  // then
			  assertEquals( filterAcceptsValue, accepted );
		 }

		 private Value AnyOtherValueThan( Value valueToNotReturn )
		 {
			  Value candidate;
			  do
			  {
					candidate = Random.nextValue();
			  } while ( candidate.Eq( valueToNotReturn ) );
			  return candidate;
		 }

		 private NodeValueClientFilter InitializeFilter( params IndexQuery[] filters )
		 {
			  NodeValueClientFilter filter = new NodeValueClientFilter( this, _node, _property, _read, filters );
			  filter.Initialize( TestIndexDescriptorFactory.forLabel( 11 ), this, null, IndexOrder.NONE, true );
			  return filter;
		 }

		 private void AssertEvents( params Event[] expected )
		 {
			  assertEquals( Arrays.asList( expected ), _events );
		 }

		 private Event.Initialize Initialize( params int[] keys )
		 {
			  return new Event.Initialize( this, keys );
		 }

		 public override void Initialize( IndexDescriptor descriptor, IndexProgressor progressor, IndexQuery[] queries, IndexOrder indexOrder, bool needsValues )
		 {
			  _events.Add( new Event.Initialize( progressor, descriptor.Schema().PropertyIds ) );
		 }

		 public override bool AcceptNode( long reference, Value[] values )
		 {
			  _events.Add( new Event.Node( reference, values ) );
			  return true;
		 }

		 public override bool NeedsValues()
		 {
			  return true;
		 }

		 public override bool Next()
		 {
			  _events.Add( Event.NEXT );
			  return true;
		 }

		 public override void Close()
		 {
			  _events.Add( Event.CLOSE );
		 }

		 private abstract class Event
		 {
			  internal class Initialize : Event
			  {
					[NonSerialized]
					internal readonly IndexProgressor Progressor;
					internal readonly int[] Keys;

					internal Initialize( IndexProgressor progressor, int[] keys )
					{
						 this.Progressor = progressor;
						 this.Keys = keys;
					}

					public override string ToString()
					{
						 return "INITIALIZE(" + Arrays.ToString( Keys ) + ")";
					}
			  }

			  internal static readonly Event CLOSE = new EventAnonymousInnerClass();

			  private class EventAnonymousInnerClass : Event
			  {
				  public override string ToString()
				  {
						return "CLOSE";
				  }
			  }

			  internal static readonly Event NEXT = new EventAnonymousInnerClass2();

			  private class EventAnonymousInnerClass2 : Event
			  {
				  public override string ToString()
				  {
						return "NEXT";
				  }
			  }

			  internal class Node : Event
			  {
					internal readonly long Reference;
					internal readonly Value[] Values;

					internal Node( long reference, Value[] values )
					{
						 this.Reference = reference;
						 this.Values = values;
					}

					public override string ToString()
					{
						 return "Node(" + Reference + "," + Arrays.ToString( Values ) + ")";
					}
			  }

			  public override sealed bool Equals( object other )
			  {
					return ToString().Equals(other.ToString());
			  }

			  public override sealed int GetHashCode()
			  {
					return ToString().GetHashCode();
			  }
		 }
	}

}