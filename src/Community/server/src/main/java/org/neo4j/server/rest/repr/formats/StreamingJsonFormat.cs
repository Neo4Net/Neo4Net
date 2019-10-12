using System;
using System.Collections.Generic;

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
namespace Neo4Net.Server.rest.repr.formats
{
	using JsonFactory = org.codehaus.jackson.JsonFactory;
	using JsonGenerator = org.codehaus.jackson.JsonGenerator;
	using Utf8Generator = org.codehaus.jackson.impl.Utf8Generator;
	using IOContext = org.codehaus.jackson.io.IOContext;
	using ObjectMapper = org.codehaus.jackson.map.ObjectMapper;
	using SerializationConfig = org.codehaus.jackson.map.SerializationConfig;


	using Service = Neo4Net.Helpers.Service;
	using JsonHelper = Neo4Net.Server.rest.domain.JsonHelper;
	using JsonParseException = Neo4Net.Server.rest.domain.JsonParseException;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.rest.domain.JsonHelper.assertSupportedPropertyValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.rest.domain.JsonHelper.readJson;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Service.Implementation(RepresentationFormat.class) public class StreamingJsonFormat extends org.neo4j.server.rest.repr.RepresentationFormat implements org.neo4j.server.rest.repr.StreamingFormat
	public class StreamingJsonFormat : RepresentationFormat, StreamingFormat
	{

		 private readonly JsonFactory _factory;

		 public StreamingJsonFormat() : base(org.neo4j.server.rest.repr.StreamingFormat_Fields.MediaType)
		 {
			  this._factory = CreateJsonFactory();
		 }

		 private JsonFactory CreateJsonFactory()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.codehaus.jackson.map.ObjectMapper objectMapper = new org.codehaus.jackson.map.ObjectMapper();
			  ObjectMapper objectMapper = new ObjectMapper();
			  objectMapper.SerializationConfig.disable( SerializationConfig.Feature.FLUSH_AFTER_WRITE_VALUE );
			  JsonFactory factory = new JsonFactoryAnonymousInnerClass( this, objectMapper );
			  factory.disable( JsonGenerator.Feature.FLUSH_PASSED_TO_STREAM );
			  return factory;
		 }

		 private class JsonFactoryAnonymousInnerClass : JsonFactory
		 {
			 private readonly StreamingJsonFormat _outerInstance;

			 public JsonFactoryAnonymousInnerClass( StreamingJsonFormat outerInstance, ObjectMapper objectMapper ) : base( objectMapper )
			 {
				 this.outerInstance = outerInstance;
			 }

			 protected internal override JsonGenerator _createUTF8JsonGenerator( Stream @out, IOContext ctxt )
			 {
				  const int bufferSize = 1024 * 8;
				  Utf8Generator gen = new Utf8Generator( ctxt, _generatorFeatures, _objectCodec, @out, new sbyte[bufferSize], 0, true );
				  if ( _characterEscapes != null )
				  {
						gen.CharacterEscapes = _characterEscapes;
				  }
				  return gen;
			 }
		 }

		 public override StreamingRepresentationFormat WriteTo( Stream output )
		 {
			  try
			  {
					JsonGenerator g = _factory.createJsonGenerator( output );
					return new StreamingRepresentationFormat( g, this );
			  }
			  catch ( IOException e )
			  {
					throw new WebApplicationException( e );
			  }
		 }

		 protected internal override ListWriter SerializeList( string type )
		 {
			  throw new System.NotSupportedException();
		 }

		 protected internal override string Complete( ListWriter serializer )
		 {
			  throw new System.NotSupportedException();
		 }

		 protected internal override MappingWriter SerializeMapping( string type )
		 {
			  throw new System.NotSupportedException();
		 }

		 protected internal override string Complete( MappingWriter serializer )
		 {
			  throw new System.NotSupportedException();
		 }

		 protected internal override string SerializeValue( string type, object value )
		 {
			  throw new System.NotSupportedException();
		 }

		 private bool Empty( string input )
		 {
			  return string.ReferenceEquals( input, null ) || "".Equals( input.Trim() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.util.Map<String, Object> readMap(String input, String... requiredKeys) throws org.neo4j.server.rest.repr.BadInputException
		 public override IDictionary<string, object> ReadMap( string input, params string[] requiredKeys )
		 {
			  if ( Empty( input ) )
			  {
					return DefaultFormat.validateKeys( Collections.emptyMap(), requiredKeys );
			  }
			  try
			  {
					return DefaultFormat.validateKeys( JsonHelper.jsonToMap( StripByteOrderMark( input ) ), requiredKeys );
			  }
			  catch ( Exception ex )
			  {
					throw new BadInputException( ex );
			  }
		 }

		 public override IList<object> ReadList( string input )
		 {
			  // TODO tobias: Implement readList() [Dec 10, 2010]
			  throw new System.NotSupportedException( "Not implemented: JsonInput.readList()" );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Object readValue(String input) throws org.neo4j.server.rest.repr.BadInputException
		 public override object ReadValue( string input )
		 {
			  if ( Empty( input ) )
			  {
					return Collections.emptyMap();
			  }
			  try
			  {
					return assertSupportedPropertyValue( readJson( StripByteOrderMark( input ) ) );
			  }
			  catch ( JsonParseException ex )
			  {
					throw new BadInputException( ex );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.net.URI readUri(String input) throws org.neo4j.server.rest.repr.BadInputException
		 public override URI ReadUri( string input )
		 {
			  try
			  {
					return new URI( ReadValue( input ).ToString() );
			  }
			  catch ( URISyntaxException e )
			  {
					throw new BadInputException( e );
			  }
		 }

		 private string StripByteOrderMark( string @string )
		 {
			  if ( !string.ReferenceEquals( @string, null ) && @string.Length > 0 && @string[0] == ( char )0xfeff )
			  {
					return @string.Substring( 1 );
			  }
			  return @string;
		 }

		 private class StreamingMappingWriter : MappingWriter
		 {
			  internal readonly JsonGenerator G;

			  internal StreamingMappingWriter( JsonGenerator g )
			  {
					this.G = g;
					try
					{
						 g.writeStartObject();
					}
					catch ( IOException e )
					{
						 throw new WebApplicationException( e );
					}
			  }

			  internal StreamingMappingWriter( JsonGenerator g, string key )
			  {
					this.G = g;
					try
					{
						 g.writeObjectFieldStart( key );
					}
					catch ( IOException e )
					{
						 throw new WebApplicationException( e );
					}
			  }

			  public override MappingWriter NewMapping( string type, string key )
			  {
					return new StreamingMappingWriter( G, key );
			  }

			  public override ListWriter NewList( string type, string key )
			  {
					return new StreamingListWriter( G, key );
			  }

			  public override void WriteValue( string type, string key, object value )
			  {
					try
					{
						 G.writeObjectField( key, value ); // todo individual fields
					}
					catch ( IOException e )
					{
						 throw new WebApplicationException( e );
					}
			  }

			  public override void Done()
			  {
					try
					{
						 G.writeEndObject();
					}
					catch ( IOException e )
					{
						 throw new WebApplicationException( e );
					}
			  }
		 }

		 private class StreamingListWriter : ListWriter
		 {
			  internal readonly JsonGenerator G;

			  internal StreamingListWriter( JsonGenerator g )
			  {
					this.G = g;
					try
					{
						 g.writeStartArray();
					}
					catch ( IOException e )
					{
						 throw new WebApplicationException( e );
					}
			  }

			  internal StreamingListWriter( JsonGenerator g, string key )
			  {
					this.G = g;
					try
					{
						 g.writeArrayFieldStart( key );
					}
					catch ( IOException e )
					{
						 throw new WebApplicationException( e );
					}
			  }

			  public override MappingWriter NewMapping( string type )
			  {
					return new StreamingMappingWriter( G );
			  }

			  public override ListWriter NewList( string type )
			  {
					return new StreamingListWriter( G );
			  }

			  public override void WriteValue( string type, object value )
			  {
					try
					{
						 G.writeObject( value );
					}
					catch ( IOException e )
					{
						 throw new WebApplicationException( e );
					}
			  }

			  public override void Done()
			  {
					try
					{
						 G.writeEndArray();
					}
					catch ( IOException e )
					{
						 throw new WebApplicationException( e );
					}
			  }

		 }

		 public class StreamingRepresentationFormat : RepresentationFormat
		 {
			  internal readonly JsonGenerator G;
			  internal readonly InputFormat InputFormat;

			  public StreamingRepresentationFormat( JsonGenerator g, InputFormat inputFormat ) : base( org.neo4j.server.rest.repr.StreamingFormat_Fields.MediaType )
			  {
					this.G = g;
					this.InputFormat = inputFormat;
			  }

			  public virtual StreamingRepresentationFormat UsePrettyPrinter()
			  {
					G.useDefaultPrettyPrinter();
					return this;
			  }

			  protected internal override string SerializeValue( string type, object value )
			  {
					try
					{
						 G.writeObject( value );
						 return null;
					}
					catch ( IOException e )
					{
						 throw new WebApplicationException( e );
					}
			  }

			  protected internal override ListWriter SerializeList( string type )
			  {
					return new StreamingListWriter( G );
			  }

			  public override MappingWriter SerializeMapping( string type )
			  {
					return new StreamingMappingWriter( G );
			  }

			  protected internal override string Complete( ListWriter serializer )
			  {
					Flush();
					return null; // already done in done()
			  }

			  protected internal override string Complete( MappingWriter serializer )
			  {
					Flush();
					return null; // already done in done()
			  }

			  internal virtual void Flush()
			  {
					try
					{
						 G.flush();
					}
					catch ( IOException e )
					{
						 throw new WebApplicationException( e );
					}
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Object readValue(String input) throws org.neo4j.server.rest.repr.BadInputException
			  public override object ReadValue( string input )
			  {
					return InputFormat.readValue( input );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.util.Map<String, Object> readMap(String input, String... requiredKeys) throws org.neo4j.server.rest.repr.BadInputException
			  public override IDictionary<string, object> ReadMap( string input, params string[] requiredKeys )
			  {
					return InputFormat.readMap( input, requiredKeys );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.util.List<Object> readList(String input) throws org.neo4j.server.rest.repr.BadInputException
			  public override IList<object> ReadList( string input )
			  {
					return InputFormat.readList( input );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.net.URI readUri(String input) throws org.neo4j.server.rest.repr.BadInputException
			  public override URI ReadUri( string input )
			  {
					return InputFormat.readUri( input );
			  }

			  public override void Complete()
			  {
					try
					{
						 // todo only if needed
						 G.flush();
					}
					catch ( IOException e )
					{
						 throw new WebApplicationException( e );
					}
			  }
		 }
	}

}