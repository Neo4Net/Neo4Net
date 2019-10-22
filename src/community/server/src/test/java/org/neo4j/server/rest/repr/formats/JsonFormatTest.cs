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
namespace Neo4Net.Server.rest.repr.formats
{
	using Before = org.junit.Before;
	using Test = org.junit.Test;


	using JsonHelper = Neo4Net.Server.rest.domain.JsonHelper;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class JsonFormatTest
	{
		 private OutputFormat _json;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void createOutputFormat() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CreateOutputFormat()
		 {
			  _json = new OutputFormat( new JsonFormat(), new URI("http://localhost/"), null );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canFormatString()
		 public virtual void CanFormatString()
		 {
			  string IEntity = _json.assemble( ValueRepresentation.@string( "expected value" ) );
			  assertEquals( IEntity, "\"expected value\"" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canFormatListOfStrings()
		 public virtual void CanFormatListOfStrings()
		 {
			  string IEntity = _json.assemble( ListRepresentation.strings( "hello", "world" ) );
			  string expectedString = JsonHelper.createJsonFrom( Arrays.asList( "hello", "world" ) );
			  assertEquals( expectedString, IEntity );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canFormatInteger()
		 public virtual void CanFormatInteger()
		 {
			  string IEntity = _json.assemble( ValueRepresentation.number( 10 ) );
			  assertEquals( "10", IEntity );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canFormatEmptyObject()
		 public virtual void CanFormatEmptyObject()
		 {
			  string IEntity = _json.assemble( new MappingRepresentationAnonymousInnerClass( this ) );
			  assertEquals( JsonHelper.createJsonFrom( Collections.emptyMap() ), IEntity );
		 }

		 private class MappingRepresentationAnonymousInnerClass : MappingRepresentation
		 {
			 private readonly JsonFormatTest _outerInstance;

			 public MappingRepresentationAnonymousInnerClass( JsonFormatTest outerInstance ) : base( "empty" )
			 {
				 this.outerInstance = outerInstance;
			 }

			 protected internal override void serialize( MappingSerializer serializer )
			 {
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canFormatObjectWithStringField()
		 public virtual void CanFormatObjectWithStringField()
		 {
			  string IEntity = _json.assemble( new MappingRepresentationAnonymousInnerClass2( this ) );
			  assertEquals( JsonHelper.createJsonFrom( Collections.singletonMap( "key", "expected string" ) ), IEntity );
		 }

		 private class MappingRepresentationAnonymousInnerClass2 : MappingRepresentation
		 {
			 private readonly JsonFormatTest _outerInstance;

			 public MappingRepresentationAnonymousInnerClass2( JsonFormatTest outerInstance ) : base( "string" )
			 {
				 this.outerInstance = outerInstance;
			 }

			 protected internal override void serialize( MappingSerializer serializer )
			 {
				  serializer.PutString( "key", "expected string" );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canFormatObjectWithUriField()
		 public virtual void CanFormatObjectWithUriField()
		 {
			  string IEntity = _json.assemble( new MappingRepresentationAnonymousInnerClass3( this ) );

			  assertEquals( JsonHelper.createJsonFrom( Collections.singletonMap( "URL", "http://localhost/subpath" ) ), IEntity );
		 }

		 private class MappingRepresentationAnonymousInnerClass3 : MappingRepresentation
		 {
			 private readonly JsonFormatTest _outerInstance;

			 public MappingRepresentationAnonymousInnerClass3( JsonFormatTest outerInstance ) : base( "uri" )
			 {
				 this.outerInstance = outerInstance;
			 }

			 protected internal override void serialize( MappingSerializer serializer )
			 {
				  serializer.PutRelativeUri( "URL", "subpath" );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canFormatObjectWithNestedObject()
		 public virtual void CanFormatObjectWithNestedObject()
		 {
			  string IEntity = _json.assemble( new MappingRepresentationAnonymousInnerClass4( this ) );
			  assertEquals( JsonHelper.createJsonFrom( Collections.singletonMap( "nested", Collections.singletonMap( "data", "expected data" ) ) ), IEntity );
		 }

		 private class MappingRepresentationAnonymousInnerClass4 : MappingRepresentation
		 {
			 private readonly JsonFormatTest _outerInstance;

			 public MappingRepresentationAnonymousInnerClass4( JsonFormatTest outerInstance ) : base( "nesting" )
			 {
				 this.outerInstance = outerInstance;
			 }

			 protected internal override void serialize( MappingSerializer serializer )
			 {
				  serializer.PutMapping( "nested", new MappingRepresentationAnonymousInnerClass5( this ) );
			 }

			 private class MappingRepresentationAnonymousInnerClass5 : MappingRepresentation
			 {
				 private readonly MappingRepresentationAnonymousInnerClass4 _outerInstance;

				 public MappingRepresentationAnonymousInnerClass5( MappingRepresentationAnonymousInnerClass4 outerInstance ) : base( "data" )
				 {
					 this.outerInstance = outerInstance;
				 }

				 protected internal override void serialize( MappingSerializer nested )
				 {
					  nested.PutString( "data", "expected data" );
				 }
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canFormatNestedMapsAndLists() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CanFormatNestedMapsAndLists()
		 {
			  string IEntity = _json.assemble( new MappingRepresentationAnonymousInnerClass6( this ) );

			  assertEquals( "bar",( ( System.Collections.IDictionary )( ( System.Collections.IList ) JsonHelper.jsonToMap( IEntity )["foo"] )[0] )["foo"] );
		 }

		 private class MappingRepresentationAnonymousInnerClass6 : MappingRepresentation
		 {
			 private readonly JsonFormatTest _outerInstance;

			 public MappingRepresentationAnonymousInnerClass6( JsonFormatTest outerInstance ) : base( "test" )
			 {
				 this.outerInstance = outerInstance;
			 }

			 protected internal override void serialize( MappingSerializer serializer )
			 {
				  List<Representation> maps = new List<Representation>();
				  maps.Add( new MappingRepresentationAnonymousInnerClass7( this, serializer ) );
				  serializer.PutList( "foo", new ServerListRepresentation( RepresentationType.MAP, maps ) );
			 }

			 private class MappingRepresentationAnonymousInnerClass7 : MappingRepresentation
			 {
				 private readonly MappingRepresentationAnonymousInnerClass6 _outerInstance;

				 private MappingSerializer _serializer;

				 public MappingRepresentationAnonymousInnerClass7( MappingRepresentationAnonymousInnerClass6 outerInstance, MappingSerializer serializer ) : base( "map" )
				 {
					 this.outerInstance = outerInstance;
					 this._serializer = serializer;
				 }


				 protected internal override void serialize( MappingSerializer serializer )
				 {
					  serializer.PutString( "foo", "bar" );

				 }
			 }
		 }
	}

}