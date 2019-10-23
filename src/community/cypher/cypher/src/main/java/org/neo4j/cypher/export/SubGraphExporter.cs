using System;
using System.Collections.Generic;
using System.Text;

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
namespace Neo4Net.Cypher.export
{

	using Direction = Neo4Net.GraphDb.Direction;
	using Label = Neo4Net.GraphDb.Label;
	using Node = Neo4Net.GraphDb.Node;
	using IPropertyContainer = Neo4Net.GraphDb.PropertyContainer;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using ConstraintDefinition = Neo4Net.GraphDb.Schema.ConstraintDefinition;
	using ConstraintType = Neo4Net.GraphDb.Schema.ConstraintType;
	using IndexDefinition = Neo4Net.GraphDb.Schema.IndexDefinition;
	using Iterables = Neo4Net.Helpers.Collections.Iterables;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterables.single;

	public class SubGraphExporter
	{
		 private readonly SubGraph _graph;

		 public SubGraphExporter( SubGraph graph )
		 {
			  this._graph = graph;
		 }

		 public virtual void Export( PrintWriter @out )
		 {
			  Export( @out, null, null );
		 }

		 public virtual void Export( PrintWriter @out, string begin, string commit )
		 {
			  Output( @out, begin );
			  AppendIndexes( @out );
			  AppendConstraints( @out );
			  Output( @out, commit, begin );
			  long nodes = AppendNodes( @out );
			  long relationships = AppendRelationships( @out );
			  if ( nodes + relationships > 0 )
			  {
					@out.println( ";" );
			  }
			  Output( @out, commit );
		 }

		 private void Output( PrintWriter @out, params string[] commands )
		 {
			  foreach ( string command in commands )
			  {
					if ( string.ReferenceEquals( command, null ) )
					{
						 continue;
					}
					@out.println( command );
			  }
		 }
		 private ICollection<string> ExportIndexes()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.List<String> result = new java.util.ArrayList<>();
			  IList<string> result = new List<string>();
			  foreach ( IndexDefinition index in _graph.Indexes )
			  {
					if ( !index.ConstraintIndex )
					{
						 IEnumerator<string> propertyKeys = index.PropertyKeys.GetEnumerator();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						 if ( !propertyKeys.hasNext() )
						 {
							  throw new System.InvalidOperationException( "Indexes should have at least one property key" );
						 }
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						 string id = propertyKeys.next();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						 if ( propertyKeys.hasNext() )
						 {
							  throw new Exception( "Exporting compound indexes is not implemented yet" );
						 }

						 if ( !index.MultiTokenIndex )
						 {
							  string key = Quote( id );
							  string label = Quote( single( index.Labels ).name() );
							  result.Add( "create index on :" + label + "(" + key + ")" );
						 }
						 // We don't know how to deal with multi-token indexes here, so we just ignore them.
					}
			  }
			  result.Sort();
			  return result;
		 }

		 private ICollection<string> ExportConstraints()
		 {
			  IEnumerable<ConstraintDefinition> constraints = _graph.Constraints;
			  int count = 0;
			  if ( constraints is System.Collections.ICollection )
			  {
					count = ( ( ICollection<ConstraintDefinition> ) constraints ).Count;
			  }
			  else
			  {
					foreach ( ConstraintDefinition ignored in constraints )
					{
						 count++;
					}
			  }
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.List<String> result = new java.util.ArrayList<>(count);
			  IList<string> result = new List<string>( count );
			  foreach ( ConstraintDefinition constraint in constraints )
			  {
					if ( !constraint.IsConstraintType( ConstraintType.UNIQUENESS ) )
					{
						 throw new Exception( "Exporting constraints other than uniqueness is not implemented yet" );
					}

					IEnumerator<string> propertyKeys = constraint.PropertyKeys.GetEnumerator();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					if ( !propertyKeys.hasNext() )
					{
						 throw new System.InvalidOperationException( "Constraints should have at least one property key" );
					}
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					string id = propertyKeys.next();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					if ( propertyKeys.hasNext() )
					{
						 throw new Exception( "Exporting compound constraints is not implemented yet" );
					}

					string key = Quote( id );
					string label = Quote( constraint.Label.name() );
					result.Add( "create constraint on (n:" + label + ") assert n." + key + " is unique" );
			  }
			  result.Sort();
			  return result;
		 }

		 private static string Quote( string id )
		 {
			  return "`" + id + "`";
		 }

		 private string LabelString( Node node )
		 {
			  IEnumerator<Label> labels = node.Labels.GetEnumerator();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  if ( !labels.hasNext() )
			  {
					return "";
			  }

			  StringBuilder result = new StringBuilder();
			  while ( labels.MoveNext() )
			  {
					Label next = labels.Current;
					result.Append( ':' ).Append( Quote( next.Name() ) );
			  }
			  return result.ToString();
		 }

		 private string Identifier( Node node )
		 {
			  return "_" + node.Id;
		 }

		 private void AppendIndexes( PrintWriter @out )
		 {
			  foreach ( string line in ExportIndexes() )
			  {
					@out.print( line );
					@out.println( ";" );
			  }
		 }

		 private void AppendConstraints( PrintWriter @out )
		 {
			  foreach ( string line in ExportConstraints() )
			  {
					@out.print( line );
					@out.println( ";" );
			  }
		 }

		 private long AppendRelationships( PrintWriter @out )
		 {
			  long relationships = 0;
			  foreach ( Node node in _graph.Nodes )
			  {
					foreach ( Relationship rel in node.GetRelationships( Direction.OUTGOING ) )
					{
						 AppendRelationship( @out, rel );
						 relationships++;
					}
			  }
			  return relationships;
		 }

		 private void AppendRelationship( PrintWriter @out, Relationship rel )
		 {
			  @out.print( "create (" );
			  @out.print( Identifier( rel.StartNode ) );
			  @out.print( ")-[:" );
			  @out.print( Quote( rel.Type.name() ) );
			  FormatProperties( @out, rel );
			  @out.print( "]->(" );
			  @out.print( Identifier( rel.EndNode ) );
			  @out.println( ")" );
		 }

		 private long AppendNodes( PrintWriter @out )
		 {
			  long nodes = 0;
			  foreach ( Node node in _graph.Nodes )
			  {
					AppendNode( @out, node );
					nodes++;
			  }
			  return nodes;
		 }

		 private void AppendNode( PrintWriter @out, Node node )
		 {
			  @out.print( "create (" );
			  @out.print( Identifier( node ) );
			  string labels = LabelString( node );
			  if ( labels.Length > 0 )
			  {
					@out.print( labels );
			  }
			  FormatProperties( @out, node );
			  @out.println( ")" );
		 }

		 private void FormatProperties( PrintWriter @out, IPropertyContainer pc )
		 {
			  if ( !pc.PropertyKeys.GetEnumerator().hasNext() )
			  {
					return;
			  }
			  @out.print( " " );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String propertyString = formatProperties(pc);
			  string propertyString = FormatProperties( pc );
			  @out.print( propertyString );
		 }

		 private string FormatProperties( IPropertyContainer pc )
		 {
			  StringBuilder result = new StringBuilder();
			  IList<string> keys = Iterables.asList( pc.PropertyKeys );
			  keys.Sort();
			  foreach ( string prop in keys )
			  {
					if ( result.Length > 0 )
					{
						 result.Append( ", " );
					}
					result.Append( Quote( prop ) ).Append( ':' );
					object value = pc.GetProperty( prop );
					result.Append( ToString( value ) );
			  }
			  return "{" + result + "}";
		 }

		 private string ToString<T1>( IEnumerator<T1> iterator )
		 {
			  StringBuilder result = new StringBuilder();
			  while ( iterator.MoveNext() )
			  {
					if ( result.Length > 0 )
					{
						 result.Append( ", " );
					}
					object value = iterator.Current;
					result.Append( ToString( value ) );
			  }
			  return "[" + result + "]";
		 }

		 private string ArrayToString( object value )
		 {
			  StringBuilder result = new StringBuilder();
			  int length = Array.getLength( value );
			  for ( int i = 0; i < length; i++ )
			  {
					if ( i > 0 )
					{
						 result.Append( ", " );
					}
					result.Append( ToString( Array.get( value, i ) ) );
			  }
			  return "[" + result + "]";
		 }

		 private static string EscapeString( string value )
		 {
			  return "\"" + value.replaceAll( "\\\\", "\\\\\\\\" ).replaceAll( "\"", "\\\\\"" ) + "\"";
		 }

		 private string ToString( object value )
		 {
			  if ( value == null )
			  {
					return "null";
			  }
			  if ( value is string )
			  {
					return EscapeString( ( string ) value );
			  }
			  if ( value is float? || value is double? )
			  {
					return string.format( Locale.ROOT, "%f", ( ( Number ) value ).doubleValue() );
			  }
			  if ( value is System.Collections.IEnumerator )
			  {
					return ToString( ( System.Collections.IEnumerator ) value );
			  }
			  if ( value is System.Collections.IEnumerable )
			  {
					return ToString( ( ( System.Collections.IEnumerable ) value ).GetEnumerator() );
			  }
			  if ( value.GetType().IsArray )
			  {
					return ArrayToString( value );
			  }
			  return value.ToString();
		 }
	}

}