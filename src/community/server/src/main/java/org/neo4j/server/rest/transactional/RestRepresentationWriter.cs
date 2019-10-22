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

	using Node = Neo4Net.GraphDb.Node;
	using Path = Neo4Net.GraphDb.Path;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using Result = Neo4Net.GraphDb.Result;
	using NodeRepresentation = Neo4Net.Server.rest.repr.NodeRepresentation;
	using OutputFormat = Neo4Net.Server.rest.repr.OutputFormat;
	using Neo4Net.Server.rest.repr;
	using RelationshipRepresentation = Neo4Net.Server.rest.repr.RelationshipRepresentation;
	using Representation = Neo4Net.Server.rest.repr.Representation;
	using RepresentationFormat = Neo4Net.Server.rest.repr.RepresentationFormat;
	using StreamingJsonFormat = Neo4Net.Server.rest.repr.formats.StreamingJsonFormat;

	internal class RestRepresentationWriter : ResultDataContentWriter
	{
		 private readonly URI _baseUri;

		 internal RestRepresentationWriter( URI baseUri )
		 {
			  this._baseUri = baseUri;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void write(org.codehaus.jackson.JsonGenerator out, Iterable<String> columns, org.Neo4Net.graphdb.Result_ResultRow row, TransactionStateChecker txStateChecker) throws java.io.IOException
		 public override void Write( JsonGenerator @out, IEnumerable<string> columns, Neo4Net.GraphDb.Result_ResultRow row, TransactionStateChecker txStateChecker )
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
//ORIGINAL LINE: private void write(org.codehaus.jackson.JsonGenerator out, org.Neo4Net.server.rest.repr.RepresentationFormat format, Object value, TransactionStateChecker checker) throws java.io.IOException
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