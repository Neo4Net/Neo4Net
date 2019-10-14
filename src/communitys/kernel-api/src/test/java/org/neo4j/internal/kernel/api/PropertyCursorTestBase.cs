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
namespace Neo4Net.@internal.Kernel.Api
{
	using Description = org.hamcrest.Description;
	using TypeSafeMatcher = org.hamcrest.TypeSafeMatcher;
	using Test = org.junit.Test;


	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Node = Neo4Net.Graphdb.Node;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using CoordinateReferenceSystem = Neo4Net.Values.Storable.CoordinateReferenceSystem;
	using Value = Neo4Net.Values.Storable.Value;
	using ValueGroup = Neo4Net.Values.Storable.ValueGroup;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.arrayContaining;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.hasItem;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public abstract class PropertyCursorTestBase<G> : KernelAPIReadTestBase<G> where G : KernelAPIReadTestSupport
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("SpellCheckingInspection") private static final String LONG_STRING = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Pellentesque "
		 private const string LONG_STRING = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Pellentesque "
					+ "eget nibh cursus, efficitur risus non, ultrices justo. Nulla laoreet eros mi, non molestie magna "
					+ "luctus in. Fusce nibh neque, tristique ultrices laoreet et, aliquet non dolor. Donec ultrices nisi "
					+ "eget urna luctus volutpat. Vivamus hendrerit eget justo vel scelerisque. Morbi interdum volutpat diam,"
					+ " et cursus arcu efficitur consectetur. Cras vitae facilisis ipsum, vitae ullamcorper orci. Nullam "
					+ "tristique ante sed nibh consequat posuere. Curabitur mauris nisl, condimentum ac varius vel, imperdiet"
					+ " a neque. Sed euismod condimentum nisl, vel efficitur turpis tempus id.\n"
					+ "\n"
					+ "Sed in tempor arcu. Suspendisse molestie rutrum risus a dignissim. Donec et orci non diam tincidunt "
					+ "sollicitudin non id nisi. Aliquam vehicula imperdiet viverra. Cras et lacinia eros. Etiam imperdiet ac"
					+ " dolor ut tristique. Phasellus ut lacinia ex. Pellentesque habitant morbi tristique senectus et netus "
					+ "et malesuada fames ac turpis egestas. Integer libero justo, tincidunt ut felis non, interdum "
					+ "consectetur mauris. Cras eu felis ante. Sed dapibus nulla urna, at elementum tortor ultricies pretium."
					+ " Maecenas sed augue non urna consectetur fringilla vitae eu libero. Vivamus interdum bibendum risus, "
					+ "quis luctus eros.\n"
					+ "\n"
					+ "Sed neque augue, fermentum sit amet iaculis ut, porttitor ac odio. Phasellus et sapien non sapien "
					+ "consequat fermentum accumsan non dolor. Integer eget pellentesque lectus, vitae lobortis ante. Nam "
					+ "elementum, dui ut finibus rutrum, purus mauris efficitur purus, efficitur tempus ante metus bibendum "
					+ "velit. Curabitur commodo, risus et eleifend facilisis, eros augue posuere tortor, eu dictum erat "
					+ "tortor consectetur orci. Fusce a velit dignissim, tempus libero nec, faucibus risus. Nullam pharetra "
					+ "mauris sit amet volutpat facilisis. Pellentesque habitant morbi tristique senectus et netus et "
					+ "malesuada fames ac turpis egestas. Praesent lacinia non felis ut lobortis.\n"
					+ "\n"
					+ "Orci varius natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus. Sed eu nisi dui"
					+ ". Suspendisse imperdiet lorem vel eleifend faucibus. Mauris non venenatis metus. Aenean neque magna, "
					+ "rhoncus vel velit in, dictum convallis leo. Phasellus pulvinar eu sapien ac vehicula. Praesent "
					+ "placerat augue quam, egestas vehicula velit porttitor in. Vivamus velit metus, pellentesque quis "
					+ "fermentum et, porta quis velit. Curabitur sed lacus quis nibh convallis tincidunt.\n"
					+ "\n"
					+ "Etiam eu elit eget dolor dignissim lacinia. Vivamus tortor ex, dapibus id elementum non, suscipit ac "
					+ "nisl. Aenean vel tempor libero, eu venenatis elit. Nunc nec velit eu odio interdum pellentesque sed et"
					+ " eros. Nam quis mi in metus tristique aliquam. Nullam facilisis dapibus lacus, nec lacinia velit. "
					+ "Proin massa enim, accumsan ac libero at, iaculis sodales tellus. Vivamus fringilla justo sed luctus "
					+ "tincidunt. Sed placerat fringilla ex, vel placerat sem faucibus eget. Vestibulum semper dui sit amet "
					+ "efficitur blandit. Donec eu tellus velit. Etiam a mi nec massa euismod posuere. Cras eget lacus leo.";

		 private static long _bare, _byteProp, _shortProp, _intProp, _inlineLongProp, _longProp, _floatProp, _doubleProp, _trueProp, _falseProp, _charProp, _emptyStringProp, _shortStringProp, _longStringProp, _utf8Prop, _smallArray, _bigArray, _pointProp, _dateProp, _allProps;

		 private static string _chinese = "造Unicode之";
		 private static Value _pointValue = Values.pointValue( CoordinateReferenceSystem.Cartesian, 10, 20 );
		 private static Value _dateValue = Values.temporalValue( LocalDate.of( 2018, 7, 26 ) );

		 protected internal virtual bool SupportsBigProperties()
		 {
			  return true;
		 }

		 public override void CreateTestGraph( GraphDatabaseService graphDb )
		 {
			  using ( Transaction tx = graphDb.BeginTx() )
			  {
					_bare = graphDb.CreateNode().Id;

					_byteProp = CreateNodeWithProperty( graphDb, "byteProp", ( sbyte ) 13 );
					_shortProp = CreateNodeWithProperty( graphDb, "shortProp", ( short ) 13 );
					_intProp = CreateNodeWithProperty( graphDb, "intProp", 13 );
					_inlineLongProp = CreateNodeWithProperty( graphDb, "inlineLongProp", 13L );
					_longProp = CreateNodeWithProperty( graphDb, "longProp", long.MaxValue );

					_floatProp = CreateNodeWithProperty( graphDb, "floatProp", 13.0f );
					_doubleProp = CreateNodeWithProperty( graphDb, "doubleProp", 13.0 );

					_trueProp = CreateNodeWithProperty( graphDb, "trueProp", true );
					_falseProp = CreateNodeWithProperty( graphDb, "falseProp", false );

					_charProp = CreateNodeWithProperty( graphDb, "charProp", 'x' );
					_emptyStringProp = CreateNodeWithProperty( graphDb, "emptyStringProp", "" );
					_shortStringProp = CreateNodeWithProperty( graphDb, "shortStringProp", "hello" );
					_longStringProp = CreateNodeWithProperty( graphDb, "longStringProp", LONG_STRING );
					_utf8Prop = CreateNodeWithProperty( graphDb, "utf8Prop", _chinese );

					_smallArray = CreateNodeWithProperty( graphDb, "smallArray", new int[]{ 1, 2, 3, 4 } );
					_bigArray = CreateNodeWithProperty( graphDb, "bigArray", new string[]{ LONG_STRING } );

					_pointProp = CreateNodeWithProperty( graphDb, "pointProp", _pointValue );
					_dateProp = CreateNodeWithProperty( graphDb, "dateProp", _dateValue );

					Node all = graphDb.CreateNode();
					// first property record
					all.SetProperty( "byteProp", ( sbyte ) 13 );
					all.SetProperty( "shortProp", ( short ) 13 );
					all.SetProperty( "intProp", 13 );
					all.SetProperty( "inlineLongProp", 13L );
					// second property record
					all.SetProperty( "longProp", long.MaxValue );
					all.SetProperty( "floatProp", 13.0f );
					all.SetProperty( "doubleProp", 13.0 );
					//                  ^^^
					// third property record halfway through double?
					all.SetProperty( "trueProp", true );
					all.SetProperty( "falseProp", false );

					all.SetProperty( "charProp", 'x' );
					all.SetProperty( "emptyStringProp", "" );
					all.SetProperty( "shortStringProp", "hello" );
					if ( SupportsBigProperties() )
					{
						 all.SetProperty( "longStringProp", LONG_STRING );
					}
					all.SetProperty( "utf8Prop", _chinese );

					if ( SupportsBigProperties() )
					{
						 all.SetProperty( "smallArray", new int[]{ 1, 2, 3, 4 } );
						 all.SetProperty( "bigArray", new string[]{ LONG_STRING } );
					}

					all.SetProperty( "pointProp", _pointValue );
					all.SetProperty( "dateProp", _dateProp );

					_allProps = all.Id;

					tx.Success();
			  }
		 }

		 private long CreateNodeWithProperty( GraphDatabaseService graphDb, string propertyKey, object value )
		 {
			  Node p = graphDb.CreateNode();
			  p.SetProperty( propertyKey, value );
			  return p.Id;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAccessNonExistentProperties()
		 public virtual void ShouldNotAccessNonExistentProperties()
		 {
			  // given
			  using ( NodeCursor node = cursors.allocateNodeCursor(), PropertyCursor props = cursors.allocatePropertyCursor() )
			  {
					// when
					read.singleNode( _bare, node );
					assertTrue( "node by reference", node.Next() );
					assertFalse( "no properties", HasProperties( node, props ) );

					node.Properties( props );
					assertFalse( "no properties by direct method", props.Next() );

					read.nodeProperties( node.NodeReference(), node.PropertiesReference(), props );
					assertFalse( "no properties via property ref", props.Next() );

					assertFalse( "only one node", node.Next() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAccessSingleProperty()
		 public virtual void ShouldAccessSingleProperty()
		 {
			  AssertAccessSingleProperty( _byteProp, Values.of( ( sbyte ) 13 ), ValueGroup.NUMBER );
			  AssertAccessSingleProperty( _shortProp, Values.of( ( short ) 13 ), ValueGroup.NUMBER );
			  AssertAccessSingleProperty( _intProp, Values.of( 13 ), ValueGroup.NUMBER );
			  AssertAccessSingleProperty( _inlineLongProp, Values.of( 13L ), ValueGroup.NUMBER );
			  AssertAccessSingleProperty( _longProp, Values.of( long.MaxValue ), ValueGroup.NUMBER );
			  AssertAccessSingleProperty( _floatProp, Values.of( 13.0f ), ValueGroup.NUMBER );
			  AssertAccessSingleProperty( _doubleProp, Values.of( 13.0 ), ValueGroup.NUMBER );
			  AssertAccessSingleProperty( _trueProp, Values.of( true ), ValueGroup.BOOLEAN );
			  AssertAccessSingleProperty( _falseProp, Values.of( false ), ValueGroup.BOOLEAN );
			  AssertAccessSingleProperty( _charProp, Values.of( 'x' ), ValueGroup.TEXT );
			  AssertAccessSingleProperty( _emptyStringProp, Values.of( "" ), ValueGroup.TEXT );
			  AssertAccessSingleProperty( _shortStringProp, Values.of( "hello" ), ValueGroup.TEXT );
			  if ( SupportsBigProperties() )
			  {
					AssertAccessSingleProperty( _longStringProp, Values.of( LONG_STRING ), ValueGroup.TEXT );
			  }
			  AssertAccessSingleProperty( _utf8Prop, Values.of( _chinese ), ValueGroup.TEXT );
			  if ( SupportsBigProperties() )
			  {
					AssertAccessSingleProperty( _smallArray, Values.of( new int[]{ 1, 2, 3, 4 } ), ValueGroup.NUMBER_ARRAY );
					AssertAccessSingleProperty( _bigArray, Values.of( new string[]{ LONG_STRING } ), ValueGroup.TEXT_ARRAY );
			  }
			  AssertAccessSingleProperty( _pointProp, Values.of( _pointValue ), ValueGroup.GEOMETRY );
			  AssertAccessSingleProperty( _dateProp, Values.of( _dateValue ), ValueGroup.DATE );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAccessAllNodeProperties()
		 public virtual void ShouldAccessAllNodeProperties()
		 {
			  // given
			  using ( NodeCursor node = cursors.allocateNodeCursor(), PropertyCursor props = cursors.allocatePropertyCursor() )
			  {
					// when
					read.singleNode( _allProps, node );
					assertTrue( "node by reference", node.Next() );
					assertTrue( "has properties", HasProperties( node, props ) );

					node.Properties( props );
					ISet<object> values = new HashSet<object>();
					while ( props.Next() )
					{
						 values.Add( props.PropertyValue().asObject() );
					}

					assertTrue( "byteProp", values.Contains( ( sbyte ) 13 ) );
					assertTrue( "shortProp", values.Contains( ( short ) 13 ) );
					assertTrue( "intProp", values.Contains( 13 ) );
					assertTrue( "inlineLongProp", values.Contains( 13L ) );
					assertTrue( "longProp", values.Contains( long.MaxValue ) );
					assertTrue( "floatProp", values.Contains( 13.0f ) );
					assertTrue( "doubleProp", values.Contains( 13.0 ) );
					assertTrue( "trueProp", values.Contains( true ) );
					assertTrue( "falseProp", values.Contains( false ) );
					assertTrue( "charProp", values.Contains( 'x' ) );
					assertTrue( "emptyStringProp", values.Contains( "" ) );
					assertTrue( "shortStringProp", values.Contains( "hello" ) );
					assertTrue( "utf8Prop", values.Contains( _chinese ) );
					if ( SupportsBigProperties() )
					{
						 assertTrue( "longStringProp", values.Contains( LONG_STRING ) );
						 assertThat( "smallArray", values, hasItem( IntArray( 1, 2, 3, 4 ) ) );
						 assertThat( "bigArray", values, hasItem( arrayContaining( LONG_STRING ) ) );
					}
					assertTrue( "pointProp", values.Contains( _pointValue ) );

					int expected = SupportsBigProperties() ? 18 : 15;
					assertEquals( "number of values", expected, values.Count );
			  }
		 }

		 private void AssertAccessSingleProperty( long nodeId, object expectedValue, ValueGroup expectedValueType )
		 {
			  // given
			  using ( NodeCursor node = cursors.allocateNodeCursor(), PropertyCursor props = cursors.allocatePropertyCursor() )
			  {
					// when
					read.singleNode( nodeId, node );
					assertTrue( "node by reference", node.Next() );
					assertTrue( "has properties", HasProperties( node, props ) );

					node.Properties( props );
					assertTrue( "has properties by direct method", props.Next() );
					assertEquals( "correct value", expectedValue, props.PropertyValue() );
					assertEquals( "correct value type ", expectedValueType, props.PropertyType() );
					assertFalse( "single property", props.Next() );

					read.nodeProperties( node.NodeReference(), node.PropertiesReference(), props );
					assertTrue( "has properties via property ref", props.Next() );
					assertEquals( "correct value", expectedValue, props.PropertyValue() );
					assertFalse( "single property", props.Next() );
			  }
		 }

		 private bool HasProperties( NodeCursor node, PropertyCursor props )
		 {
			  node.Properties( props );
			  return props.Next();
		 }

		 private static TypeSafeMatcher<int[]> IntArray( params int[] content )
		 {
			  return new TypeSafeMatcherAnonymousInnerClass( content );
		 }

		 private class TypeSafeMatcherAnonymousInnerClass : TypeSafeMatcher<int[]>
		 {
			 private int[] _content;

			 public TypeSafeMatcherAnonymousInnerClass( int[] content )
			 {
				 this._content = content;
			 }

			 protected internal override bool matchesSafely( int[] item )
			 {
				  if ( item.Length != _content.Length )
				  {
						return false;
				  }
				  for ( int i = 0; i < _content.Length; i++ )
				  {
						if ( item[i] != _content[i] )
						{
							 return false;
						}
				  }
				  return true;
			 }

			 public override void describeTo( Description description )
			 {
				  description.appendValue( _content );
			 }
		 }
	}

}