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
namespace Org.Neo4j.Server.rest.transactional
{
	using JsonGenerator = org.codehaus.jackson.JsonGenerator;


	using Label = Org.Neo4j.Graphdb.Label;
	using Node = Org.Neo4j.Graphdb.Node;
	using Path = Org.Neo4j.Graphdb.Path;
	using PropertyContainer = Org.Neo4j.Graphdb.PropertyContainer;
	using Relationship = Org.Neo4j.Graphdb.Relationship;
	using Result = Org.Neo4j.Graphdb.Result;
	using Org.Neo4j.Helpers.Collection;

	internal class GraphExtractionWriter : ResultDataContentWriter
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void write(org.codehaus.jackson.JsonGenerator out, Iterable<String> columns, org.neo4j.graphdb.Result_ResultRow row, TransactionStateChecker txStateChecker) throws java.io.IOException
		 public override void Write( JsonGenerator @out, IEnumerable<string> columns, Org.Neo4j.Graphdb.Result_ResultRow row, TransactionStateChecker txStateChecker )
		 {
			  ISet<Node> nodes = new HashSet<Node>();
			  ISet<Relationship> relationships = new HashSet<Relationship>();
			  Extract( nodes, relationships, Map( columns, row ) );

			  @out.writeObjectFieldStart( "graph" );
			  try
			  {
					WriteNodes( @out, nodes, txStateChecker );
					WriteRelationships( @out, relationships, txStateChecker );
			  }
			  finally
			  {
					@out.writeEndObject();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void writeNodes(org.codehaus.jackson.JsonGenerator out, Iterable<org.neo4j.graphdb.Node> nodes, TransactionStateChecker txStateChecker) throws java.io.IOException
		 private void WriteNodes( JsonGenerator @out, IEnumerable<Node> nodes, TransactionStateChecker txStateChecker )
		 {
			  @out.writeArrayFieldStart( "nodes" );
			  try
			  {
					foreach ( Node node in nodes )
					{
						 @out.writeStartObject();
						 try
						 {
							  long nodeId = node.Id;
							  @out.writeStringField( "id", Convert.ToString( nodeId ) );
							  if ( txStateChecker.IsNodeDeletedInCurrentTx( nodeId ) )
							  {
									MarkDeleted( @out );
							  }
							  else
							  {
									@out.writeArrayFieldStart( "labels" );
									try
									{
										 foreach ( Label label in node.Labels )
										 {
											  @out.writeString( label.Name() );
										 }
									}
									finally
									{
										 @out.writeEndArray();
									}
									WriteProperties( @out, node );
							  }
						 }
						 finally
						 {
							  @out.writeEndObject();
						 }
					}
			  }
			  finally
			  {
					@out.writeEndArray();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void markDeleted(org.codehaus.jackson.JsonGenerator out) throws java.io.IOException
		 private void MarkDeleted( JsonGenerator @out )
		 {
			  @out.writeBooleanField( "deleted", true );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void writeRelationships(org.codehaus.jackson.JsonGenerator out, Iterable<org.neo4j.graphdb.Relationship> relationships, TransactionStateChecker txStateChecker) throws java.io.IOException
		 private void WriteRelationships( JsonGenerator @out, IEnumerable<Relationship> relationships, TransactionStateChecker txStateChecker )
		 {
			  @out.writeArrayFieldStart( "relationships" );
			  try
			  {
					foreach ( Relationship relationship in relationships )
					{
						 @out.writeStartObject();
						 try
						 {
							  long relationshipId = relationship.Id;
							  @out.writeStringField( "id", Convert.ToString( relationshipId ) );
							  if ( txStateChecker.IsRelationshipDeletedInCurrentTx( relationshipId ) )
							  {
									MarkDeleted( @out );
							  }
							  else
							  {
									@out.writeStringField( "type", relationship.Type.name() );
									@out.writeStringField( "startNode", Convert.ToString( relationship.StartNode.Id ) );
									@out.writeStringField( "endNode", Convert.ToString( relationship.EndNode.Id ) );
									WriteProperties( @out, relationship );
							  }
						 }
						 finally
						 {
							  @out.writeEndObject();
						 }
					}
			  }
			  finally
			  {
					@out.writeEndArray();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void writeProperties(org.codehaus.jackson.JsonGenerator out, org.neo4j.graphdb.PropertyContainer container) throws java.io.IOException
		 private void WriteProperties( JsonGenerator @out, PropertyContainer container )
		 {
			  @out.writeObjectFieldStart( "properties" );
			  try
			  {
					foreach ( KeyValuePair<string, object> property in container.AllProperties.SetOfKeyValuePairs() )
					{
						 @out.writeObjectField( property.Key, property.Value );

					}
			  }
			  finally
			  {
					@out.writeEndObject();
			  }
		 }

		 private void Extract<T1>( ISet<Node> nodes, ISet<Relationship> relationships, IEnumerable<T1> source )
		 {
			  foreach ( object item in source )
			  {
					if ( item is Node )
					{
						 nodes.Add( ( Node ) item );
					}
					else if ( item is Relationship )
					{
						 Relationship relationship = ( Relationship ) item;
						 relationships.Add( relationship );
						 nodes.Add( relationship.StartNode );
						 nodes.Add( relationship.EndNode );
					}
					if ( item is Path )
					{
						 Path path = ( Path ) item;
						 foreach ( Node node in path.Nodes() )
						 {
							  nodes.Add( node );
						 }
						 foreach ( Relationship relationship in path.Relationships() )
						 {
							  relationships.Add( relationship );
						 }
					}
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: else if (item instanceof java.util.Map<?, ?>)
					else if ( item is IDictionary<object, ?> )
					{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: extract(nodes, relationships, ((java.util.Map<?, ?>) item).values());
						 Extract( nodes, relationships, ( ( IDictionary<object, ?> ) item ).Values );
					}
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: else if (item instanceof Iterable<?>)
					else if ( item is IEnumerable<object> )
					{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: extract(nodes, relationships, (Iterable<?>) item);
						 Extract( nodes, relationships, ( IEnumerable<object> ) item );
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private static Iterable<?> map(Iterable<String> columns, final org.neo4j.graphdb.Result_ResultRow row)
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 private static IEnumerable<object> Map( IEnumerable<string> columns, Org.Neo4j.Graphdb.Result_ResultRow row )
		 {
			  return new IterableWrapperAnonymousInnerClass( columns, row );
		 }

		 private class IterableWrapperAnonymousInnerClass : IterableWrapper<object, string>
		 {
			 private Org.Neo4j.Graphdb.Result_ResultRow _row;

			 public IterableWrapperAnonymousInnerClass( IEnumerable<string> columns, Org.Neo4j.Graphdb.Result_ResultRow row ) : base( columns )
			 {
				 this._row = row;
			 }

			 protected internal override object underlyingObjectToObject( string key )
			 {
				  return _row.get( key );
			 }
		 }
	}

}