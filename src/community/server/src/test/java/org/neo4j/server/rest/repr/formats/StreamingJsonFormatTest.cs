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

	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Node = Neo4Net.GraphDb.Node;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using JsonHelper = Neo4Net.Server.rest.domain.JsonHelper;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class StreamingJsonFormatTest
	{
		 private OutputFormat _json;
		 private MemoryStream _stream;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void createOutputFormat() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CreateOutputFormat()
		 {
			  _stream = new MemoryStream();
			  _json = new OutputFormat( ( new StreamingJsonFormat() ).WriteTo(_stream).usePrettyPrinter(), new URI("http://localhost/"), null );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canFormatNode()
		 public virtual void CanFormatNode()
		 {
			  IGraphDatabaseService db = ( new TestGraphDatabaseFactory() ).newImpermanentDatabase();
			  try
			  {
					  using ( Transaction transaction = Db.beginTx() )
					  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.graphdb.Node n = db.createNode();
						Node n = Db.createNode();
						_json.assemble( new NodeRepresentation( n ) );
					  }
			  }
			  finally
			  {
					Db.shutdown();
			  }
			  assertTrue( _stream.ToString().Contains("\"self\" : \"http://localhost/node/0\",") );
		 }
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canFormatString()
		 public virtual void CanFormatString()
		 {
			  _json.assemble( ValueRepresentation.@string( "expected value" ) );
			  assertEquals( _stream.ToString(), "\"expected value\"" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canFormatListOfStrings()
		 public virtual void CanFormatListOfStrings()
		 {
			  _json.assemble( ListRepresentation.strings( "hello", "world" ) );
			  string expectedString = JsonHelper.createJsonFrom( Arrays.asList( "hello", "world" ) );
			  assertEquals( expectedString, _stream.ToString() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canFormatInteger()
		 public virtual void CanFormatInteger()
		 {
			  _json.assemble( ValueRepresentation.number( 10 ) );
			  assertEquals( "10", _stream.ToString() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canFormatEmptyObject()
		 public virtual void CanFormatEmptyObject()
		 {
			  _json.assemble( new MappingRepresentationAnonymousInnerClass( this ) );
			  assertEquals( JsonHelper.createJsonFrom( Collections.emptyMap() ), _stream.ToString() );
		 }

		 private class MappingRepresentationAnonymousInnerClass : MappingRepresentation
		 {
			 private readonly StreamingJsonFormatTest _outerInstance;

			 public MappingRepresentationAnonymousInnerClass( StreamingJsonFormatTest outerInstance ) : base( "empty" )
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
			  _json.assemble( new MappingRepresentationAnonymousInnerClass2( this ) );
			  assertEquals( JsonHelper.createJsonFrom( Collections.singletonMap( "key", "expected string" ) ), _stream.ToString() );
		 }

		 private class MappingRepresentationAnonymousInnerClass2 : MappingRepresentation
		 {
			 private readonly StreamingJsonFormatTest _outerInstance;

			 public MappingRepresentationAnonymousInnerClass2( StreamingJsonFormatTest outerInstance ) : base( "string" )
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
			  _json.assemble( new MappingRepresentationAnonymousInnerClass3( this ) );

			  assertEquals( JsonHelper.createJsonFrom( Collections.singletonMap( "URL", "http://localhost/subpath" ) ), _stream.ToString() );
		 }

		 private class MappingRepresentationAnonymousInnerClass3 : MappingRepresentation
		 {
			 private readonly StreamingJsonFormatTest _outerInstance;

			 public MappingRepresentationAnonymousInnerClass3( StreamingJsonFormatTest outerInstance ) : base( "uri" )
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
			  _json.assemble( new MappingRepresentationAnonymousInnerClass4( this ) );
			  assertEquals( JsonHelper.createJsonFrom( Collections.singletonMap( "nested", Collections.singletonMap( "data", "expected data" ) ) ), _stream.ToString() );
		 }

		 private class MappingRepresentationAnonymousInnerClass4 : MappingRepresentation
		 {
			 private readonly StreamingJsonFormatTest _outerInstance;

			 public MappingRepresentationAnonymousInnerClass4( StreamingJsonFormatTest outerInstance ) : base( "nesting" )
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
	}

}