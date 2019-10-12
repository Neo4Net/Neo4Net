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
namespace Org.Neo4j.Server.rest.transactional
{

	using JsonGenerator = org.codehaus.jackson.JsonGenerator;

	using Node = Org.Neo4j.Graphdb.Node;
	using Path = Org.Neo4j.Graphdb.Path;
	using Relationship = Org.Neo4j.Graphdb.Relationship;
	using Result = Org.Neo4j.Graphdb.Result;
	using NodeRepresentation = Org.Neo4j.Server.rest.repr.NodeRepresentation;
	using OutputFormat = Org.Neo4j.Server.rest.repr.OutputFormat;
	using Org.Neo4j.Server.rest.repr;
	using RelationshipRepresentation = Org.Neo4j.Server.rest.repr.RelationshipRepresentation;
	using Representation = Org.Neo4j.Server.rest.repr.Representation;
	using RepresentationFormat = Org.Neo4j.Server.rest.repr.RepresentationFormat;
	using StreamingJsonFormat = Org.Neo4j.Server.rest.repr.formats.StreamingJsonFormat;

	internal class RestRepresentationWriter : ResultDataContentWriter
	{
		 private readonly URI _baseUri;

		 internal RestRepresentationWriter( URI baseUri )
		 {
			  this._baseUri = baseUri;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void write(org.codehaus.jackson.JsonGenerator out, Iterable<String> columns, org.neo4j.graphdb.Result_ResultRow row, TransactionStateChecker txStateChecker) throws java.io.IOException
		 public override void Write( JsonGenerator @out, IEnumerable<string> columns, Org.Neo4j.Graphdb.Result_ResultRow row, TransactionStateChecker txStateChecker )
		 {
			  RepresentationFormat format = new StreamingJsonFormat.StreamingRepresentationFormat( @out, null );
			  @out.writeArrayFieldStart( "rest" );
			  try
			  {
					foreach ( string key in columns )
					{
						 write( @out, format, row.Get( key ), txStateChecker );
					}
			  }
			  finally
			  {
					@out.writeEndArray();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void write(org.codehaus.jackson.JsonGenerator out, org.neo4j.server.rest.repr.RepresentationFormat format, Object value, TransactionStateChecker checker) throws java.io.IOException
		 private void Write( JsonGenerator @out, RepresentationFormat format, object value, TransactionStateChecker checker )
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: if (value instanceof java.util.Map<?, ?>)
			  if ( value is IDictionary<object, ?> )
			  {
					@out.writeStartObject();
					try
					{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (java.util.Map.Entry<String, ?> entry : ((java.util.Map<String, ?>) value).entrySet())
						 foreach ( KeyValuePair<string, ?> entry in ( ( IDictionary<string, ?> ) value ).SetOfKeyValuePairs() )
						 {
							  @out.writeFieldName( entry.Key );
							  write( @out, format, entry.Value, checker );
						 }
					}
					finally
					{
						 @out.writeEndObject();
					}
			  }
			  else if ( value is Path )
			  {
					Write( format, new PathRepresentation<>( ( Path ) value ) );
			  }
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: else if (value instanceof Iterable<?>)
			  else if ( value is IEnumerable<object> )
			  {
					@out.writeStartArray();
					try
					{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (Object item : (Iterable<?>) value)
						 foreach ( object item in ( IEnumerable<object> ) value )
						 {
							  Write( @out, format, item, checker );
						 }
					}
					finally
					{
						 @out.writeEndArray();
					}
			  }
			  else if ( value is Node )
			  {
					NodeRepresentation representation = new NodeRepresentation( ( Node ) value );
					representation.TransactionStateChecker = checker;
					Write( format, representation );
			  }
			  else if ( value is Relationship )
			  {
					RelationshipRepresentation representation = new RelationshipRepresentation( ( Relationship ) value );
					representation.TransactionStateChecker = checker;
					Write( format, representation );
			  }
			  else
			  {
					@out.writeObject( value );
			  }
		 }

		 private void Write( RepresentationFormat format, Representation representation )
		 {
			  OutputFormat.write( representation, format, _baseUri );
		 }
	}

}