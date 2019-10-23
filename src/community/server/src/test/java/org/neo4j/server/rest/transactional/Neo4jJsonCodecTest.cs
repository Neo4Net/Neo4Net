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
namespace Neo4Net.Server.rest.transactional
{
	using JsonGenerator = org.codehaus.jackson.JsonGenerator;
	using Before = org.junit.Before;
	using Test = org.junit.Test;
	using InOrder = org.mockito.InOrder;
	using Mockito = org.mockito.Mockito;


	using Node = Neo4Net.GraphDb.Node;
	using Path = Neo4Net.GraphDb.Path;
	using IPropertyContainer = Neo4Net.GraphDb.PropertyContainer;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using CRS = Neo4Net.GraphDb.Spatial.CRS;
	using Coordinate = Neo4Net.GraphDb.Spatial.Coordinate;
	using Geometry = Neo4Net.GraphDb.Spatial.Geometry;
	using Point = Neo4Net.GraphDb.Spatial.Point;
	using SpatialMocks = Neo4Net.Test.mockito.mock.SpatialMocks;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyInt;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.startsWith;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doThrow;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.never;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.mockito.mock.SpatialMocks.mockCartesian;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.mockito.mock.SpatialMocks.mockCartesian_3D;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.mockito.mock.SpatialMocks.mockWGS84;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.mockito.mock.SpatialMocks.mockWGS84_3D;

	public class Neo4NetJsonCodecTest : TxStateCheckerTestSupport
	{

		 private Neo4NetJsonCodec _jsonCodec;
		 private JsonGenerator _jsonGenerator;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void init()
		 public virtual void Init()
		 {
			  _jsonCodec = new Neo4NetJsonCodec( Tptpmc );
			  _jsonGenerator = mock( typeof( JsonGenerator ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testPropertyContainerWriting() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestPropertyContainerWriting()
		 {
			  //Given
			  IPropertyContainer IPropertyContainer = mock( typeof( IPropertyContainer ) );
			  when( IPropertyContainer.AllProperties ).thenThrow( typeof( Exception ) );

			  bool exceptionThrown = false;
			  //When
			  try
			  {
					_jsonCodec.writeValue( _jsonGenerator, IPropertyContainer );
			  }
			  catch ( System.ArgumentException )
			  {
					//Then
					verify( _jsonGenerator, never() ).writeEndObject();
					exceptionThrown = true;
			  }

			  assertTrue( exceptionThrown );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testNodeWriting() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestNodeWriting()
		 {
			  //Given
			  IPropertyContainer node = mock( typeof( Node ) );
			  when( node.AllProperties ).thenThrow( typeof( Exception ) );

			  //When
			  try
			  {
					_jsonCodec.writeValue( _jsonGenerator, node );
			  }
			  catch ( Exception )
			  {
					// do nothing
			  }

			  //Then
			  verify( _jsonGenerator, times( 1 ) ).writeEndObject();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testRelationshipWriting() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestRelationshipWriting()
		 {
			  //Given
			  IPropertyContainer relationship = mock( typeof( Relationship ) );
			  when( relationship.AllProperties ).thenThrow( typeof( Exception ) );

			  //When
			  try
			  {
					_jsonCodec.writeValue( _jsonGenerator, relationship );
			  }
			  catch ( Exception )
			  {
					// do nothing
			  }

			  //Then
			  verify( _jsonGenerator, times( 1 ) ).writeEndObject();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testPathWriting() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestPathWriting()
		 {
			  //Given
			  Path path = mock( typeof( Path ) );
			  IPropertyContainer IPropertyContainer = mock( typeof( IPropertyContainer ) );
			  when( IPropertyContainer.AllProperties ).thenThrow( typeof( Exception ) );
//JAVA TO C# CONVERTER WARNING: Unlike Java's ListIterator, enumerators in .NET do not allow altering the collection:
			  when( path.GetEnumerator() ).thenReturn(Arrays.asList(propertyContainer).GetEnumerator());

			  //When
			  try
			  {
					_jsonCodec.writeValue( _jsonGenerator, path );
			  }
			  catch ( Exception )
			  {

			  }

			  //Then
			  verify( _jsonGenerator, times( 1 ) ).writeEndArray();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIteratorWriting() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestIteratorWriting()
		 {
			  //Given
			  IPropertyContainer IPropertyContainer = mock( typeof( IPropertyContainer ) );
			  when( IPropertyContainer.AllProperties ).thenThrow( typeof( Exception ) );

			  //When
			  try
			  {
					_jsonCodec.writeValue( _jsonGenerator, Arrays.asList( IPropertyContainer ) );
			  }
			  catch ( Exception )
			  {

			  }

			  //Then
			  verify( _jsonGenerator, times( 1 ) ).writeEndArray();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testByteArrayWriting() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestByteArrayWriting()
		 {
			  //Given
			  doThrow( new Exception() ).when(_jsonGenerator).writeNumber(anyInt());
			  sbyte[] byteArray = new sbyte[]{ 1, 2, 3 };

			  //When
			  try
			  {
					_jsonCodec.writeValue( _jsonGenerator, byteArray );
			  }
			  catch ( Exception )
			  {

			  }

			  //Then
			  verify( _jsonGenerator, times( 1 ) ).writeEndArray();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testMapWriting() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestMapWriting()
		 {
			  //Given
			  doThrow( new Exception() ).when(_jsonGenerator).writeFieldName(anyString());

			  //When
			  try
			  {
					_jsonCodec.writeValue( _jsonGenerator, new Dictionary<string, string>() );
			  }
			  catch ( Exception )
			  {

			  }

			  //Then
			  verify( _jsonGenerator, times( 1 ) ).writeEndObject();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWriteAMapContainingNullAsKeysAndValues() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldWriteAMapContainingNullAsKeysAndValues()
		 {
			  // given
			  IDictionary<string, string> map = new Dictionary<string, string>();
			  map[null] = null;

			  // when
			  _jsonCodec.writeValue( _jsonGenerator, map );

			  // then
			  verify( _jsonGenerator, times( 1 ) ).writeFieldName( "null" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testGeographicPointWriting() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestGeographicPointWriting()
		 {
			  //Given
			  Point value = SpatialMocks.mockPoint( 12.3, 45.6, mockWGS84() );

			  //When
			  _jsonCodec.writeValue( _jsonGenerator, value );

			  //Then
			  verify( _jsonGenerator, times( 3 ) ).writeEndObject();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testGeographic3DPointWriting() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestGeographic3DPointWriting()
		 {
			  //Given
			  Point value = SpatialMocks.mockPoint( 12.3, 45.6, 78.9, mockWGS84_3D() );

			  //When
			  _jsonCodec.writeValue( _jsonGenerator, value );

			  //Then
			  verify( _jsonGenerator, times( 3 ) ).writeEndObject();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testCartesianPointWriting() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestCartesianPointWriting()
		 {
			  //Given
			  Point value = SpatialMocks.mockPoint( 123.0, 456.0, mockCartesian() );

			  //When
			  _jsonCodec.writeValue( _jsonGenerator, value );

			  //Then
			  verify( _jsonGenerator, times( 3 ) ).writeEndObject();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testCartesian3DPointWriting() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestCartesian3DPointWriting()
		 {
			  //Given
			  Point value = SpatialMocks.mockPoint( 123.0, 456.0, 789.0, mockCartesian_3D() );

			  //When
			  _jsonCodec.writeValue( _jsonGenerator, value );

			  //Then
			  verify( _jsonGenerator, times( 3 ) ).writeEndObject();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testGeometryWriting() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestGeometryWriting()
		 {
			  //Given
			  IList<Coordinate> points = new List<Coordinate>();
			  points.Add( new Coordinate( 1, 2 ) );
			  points.Add( new Coordinate( 2, 3 ) );
			  Geometry value = SpatialMocks.mockGeometry( "LineString", points, mockCartesian() );

			  //When
			  _jsonCodec.writeValue( _jsonGenerator, value );

			  //Then
			  verify( _jsonGenerator, times( 3 ) ).writeEndObject();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testGeometryCrsStructureCartesian() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestGeometryCrsStructureCartesian()
		 {
			  VerifyCRSStructure( mockCartesian() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testGeometryCrsStructureCartesian_3D() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestGeometryCrsStructureCartesian_3D()
		 {
			  VerifyCRSStructure( mockCartesian_3D() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testGeometryCrsStructureWGS84() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestGeometryCrsStructureWGS84()
		 {
			  VerifyCRSStructure( mockWGS84() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testGeometryCrsStructureWGS84_3D() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestGeometryCrsStructureWGS84_3D()
		 {
			  VerifyCRSStructure( mockWGS84_3D() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void verifyCRSStructure(org.Neo4Net.GraphDb.Spatial.CRS crs) throws java.io.IOException
		 private void VerifyCRSStructure( CRS crs )
		 {
			  // When
			  _jsonCodec.writeValue( _jsonGenerator, crs );

			  // Then verify in order
			  InOrder inOrder = Mockito.inOrder( _jsonGenerator );

			  // Start CRS object
			  inOrder.verify( _jsonGenerator ).writeStartObject();
			  // Code
			  inOrder.verify( _jsonGenerator ).writeFieldName( "srid" );
			  inOrder.verify( _jsonGenerator ).writeNumber( crs.Code );
			  // Name
			  inOrder.verify( _jsonGenerator ).writeFieldName( "name" );
			  inOrder.verify( _jsonGenerator ).writeString( crs.Type );
			  // Type
			  inOrder.verify( _jsonGenerator ).writeFieldName( "type" );
			  inOrder.verify( _jsonGenerator ).writeString( "link" );
			  // Properties
			  inOrder.verify( _jsonGenerator ).writeFieldName( "properties" );
			  // Properties object
			  inOrder.verify( _jsonGenerator ).writeStartObject();
			  inOrder.verify( _jsonGenerator ).writeFieldName( "href" );
			  inOrder.verify( _jsonGenerator ).writeString( startsWith( crs.Href ) );
			  inOrder.verify( _jsonGenerator ).writeFieldName( "type" );
			  inOrder.verify( _jsonGenerator ).writeString( "ogcwkt" );
			  // Close both properties and CRS objects
			  inOrder.verify( _jsonGenerator, times( 2 ) ).writeEndObject();
		 }
	}

}