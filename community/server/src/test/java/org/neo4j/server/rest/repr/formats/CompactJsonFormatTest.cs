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
namespace Org.Neo4j.Server.rest.repr.formats
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;


	using Before = org.junit.Before;
	using Test = org.junit.Test;
	using JsonHelper = Org.Neo4j.Server.rest.domain.JsonHelper;

	public class CompactJsonFormatTest
	{
		 private OutputFormat _json;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void createOutputFormat() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CreateOutputFormat()
		 {
			  _json = new OutputFormat( new CompactJsonFormat(), new URI("http://localhost/"), null );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canFormatString()
		 public virtual void CanFormatString()
		 {
			  string entity = _json.assemble( ValueRepresentation.@string( "expected value" ) );
			  assertEquals( entity, "\"expected value\"" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canFormatListOfStrings()
		 public virtual void CanFormatListOfStrings()
		 {
			  string entity = _json.assemble( ListRepresentation.strings( "hello", "world" ) );
			  string expectedString = JsonHelper.createJsonFrom( Arrays.asList( "hello", "world" ) );
			  assertEquals( expectedString, entity );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canFormatInteger()
		 public virtual void CanFormatInteger()
		 {
			  string entity = _json.assemble( ValueRepresentation.number( 10 ) );
			  assertEquals( "10", entity );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canFormatObjectWithStringField()
		 public virtual void CanFormatObjectWithStringField()
		 {
			  string entity = _json.assemble( new MappingRepresentationAnonymousInnerClass( this ) );
			  assertEquals( JsonHelper.createJsonFrom( Collections.singletonMap( "key", "expected string" ) ), entity );
		 }

		 private class MappingRepresentationAnonymousInnerClass : MappingRepresentation
		 {
			 private readonly CompactJsonFormatTest _outerInstance;

			 public MappingRepresentationAnonymousInnerClass( CompactJsonFormatTest outerInstance ) : base( "string" )
			 {
				 this.outerInstance = outerInstance;
			 }

			 protected internal override void serialize( MappingSerializer serializer )
			 {
				  serializer.PutString( "key", "expected string" );
			 }
		 }

	}

}