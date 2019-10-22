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
namespace Neo4Net.GraphDb.traversal
{


	/// <summary>
	/// Utilities for <seealso cref="org.Neo4Net.graphdb.Path"/> objects.
	/// </summary>
	public class Paths
	{

		 private Paths()
		 {
		 }

		 /// <summary>
		 /// Provides hooks to help build a string representation of a <seealso cref="org.Neo4Net.graphdb.Path"/>. </summary>
		 /// @param <T> the type of <seealso cref="org.Neo4Net.graphdb.Path"/>. </param>
		 public interface PathDescriptor<T> where T : Neo4Net.GraphDb.IPath
		 {
			  /// <summary>
			  /// Returns a string representation of a <seealso cref="org.Neo4Net.graphdb.Node"/>. </summary>
			  /// <param name="path"> the <seealso cref="IPath"/> we're building a string representation
			  /// from. </param>
			  /// <param name="node"> the <seealso cref="org.Neo4Net.graphdb.Node"/> to return a string representation of. </param>
			  /// <returns> a string representation of a <seealso cref="org.Neo4Net.graphdb.Node"/>. </returns>
			  string NodeRepresentation( T path, INode node );

			  /// <summary>
			  /// Returns a string representation of a <seealso cref="org.Neo4Net.graphdb.Relationship"/>. </summary>
			  /// <param name="path"> the <seealso cref="IPath"/> we're building a string representation
			  /// from. </param>
			  /// <param name="from"> the previous <seealso cref="INode"/> in the path. </param>
			  /// <param name="relationship"> the <seealso cref="org.Neo4Net.graphdb.Relationship"/> to return a string
			  /// representation of. </param>
			  /// <returns> a string representation of a <seealso cref="org.Neo4Net.graphdb.Relationship"/>. </returns>
			  string RelationshipRepresentation( T path, INode from, IRelationship relationship );
		 }

		 /// <summary>
		 /// The default <seealso cref="PathDescriptor"/> used in common toString()
		 /// representations in classes implementing <seealso cref="IPath"/>. </summary>
		 /// @param <T> the type of <seealso cref="IPath"/>. </param>
		 public class DefaultPathDescriptor<T> : PathDescriptor<T> where T : Neo4Net.GraphDb.IPath
		 {
			  public override string NodeRepresentation( IPath path, INode node )
			  {
					return "(" + node.Id + ")";
			  }

			  public override string RelationshipRepresentation( IPath path, INode from, IRelationship relationship )
			  {
					string prefix = "-";
					string suffix = "-";
					if ( from.Equals( relationship.EndNode ) )
					{
						 prefix = "<-";
					}
					else
					{
						 suffix = "->";
					}
					return prefix + "[" + relationship.Type.name() + "," + relationship.Id + "]" + suffix;
			  }
		 }

		 /// <summary>
		 /// Method for building a string representation of a <seealso cref="IPath"/>, using
		 /// the given {@code builder}. </summary>
		 /// @param <T> the type of <seealso cref="IPath"/>. </param>
		 /// <param name="path"> the <seealso cref="IPath"/> to build a string representation of. </param>
		 /// <param name="builder"> the <seealso cref="PathDescriptor"/> to get
		 /// <seealso cref="INode"/> and <seealso cref="IRelationship"/> representations from. </param>
		 /// <returns> a string representation of a <seealso cref="IPath"/>. </returns>
		 public static string PathToString<T>( T path, PathDescriptor<T> builder ) where T : Neo4Net.GraphDb.IPath
		 {
			  INode current = path.StartNode();
			  StringBuilder result = new StringBuilder();
			  foreach ( IRelationship rel in path.Relationships() )
			  {
					result.Append( builder.NodeRepresentation( path, current ) );
					result.Append( builder.RelationshipRepresentation( path, current, rel ) );
					current = rel.GetOtherNode( current );
			  }
			  if ( null != current )
			  {
					result.Append( builder.NodeRepresentation( path, current ) );
			  }
			  return result.ToString();
		 }

		 /// <summary>
		 /// TODO: This method re-binds nodes and relationships. It should not.
		 /// 
		 /// Returns the default string representation of a <seealso cref="IPath"/>. It uses
		 /// the <seealso cref="DefaultPathDescriptor"/> to get representations. </summary>
		 /// <param name="path"> the <seealso cref="IPath"/> to build a string representation of. </param>
		 /// <returns> the default string representation of a <seealso cref="IPath"/>. </returns>
		 public static string DefaultPathToString( IPath path )
		 {
			  return PathToString( path, new DefaultPathDescriptor<>() );
		 }

		 /// <summary>
		 /// Returns a quite simple string representation of a <seealso cref="IPath"/>. It
		 /// doesn't print relationship types or ids, just directions. </summary>
		 /// <param name="path"> the <seealso cref="IPath"/> to build a string representation of. </param>
		 /// <returns> a quite simple representation of a <seealso cref="IPath"/>. </returns>
		 public static string SimplePathToString( IPath path )
		 {
			  return PathToString( path, new DefaultPathDescriptorAnonymousInnerClass( path ) );
		 }

		 private class DefaultPathDescriptorAnonymousInnerClass : DefaultPathDescriptor<IPath>
		 {
			 private IPath _path;

			 public DefaultPathDescriptorAnonymousInnerClass( IPath path )
			 {
				 this._path = path;
			 }

			 public override string relationshipRepresentation( IPath path, INode from, IRelationship relationship )
			 {
				  return relationship.StartNode.Equals( from ) ? "-->" : "<--";
			 }
		 }

		 /// <summary>
		 /// Returns a quite simple string representation of a <seealso cref="IPath"/>. It
		 /// doesn't print relationship types or ids, just directions. it uses the
		 /// {@code nodePropertyKey} to try to display that property value as in the
		 /// node representation instead of the node id. If that property doesn't
		 /// exist, the id is used. </summary>
		 /// <param name="path"> the <seealso cref="IPath"/> to build a string representation of. </param>
		 /// <param name="nodePropertyKey"> the key of the property value to display </param>
		 /// <returns> a quite simple representation of a <seealso cref="IPath"/>. </returns>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static String simplePathToString(org.Neo4Net.graphdb.Path path, final String nodePropertyKey)
		 public static string SimplePathToString( IPath path, string nodePropertyKey )
		 {
			  return PathToString( path, new DefaultPathDescriptorAnonymousInnerClass2( path, nodePropertyKey ) );
		 }

		 private class DefaultPathDescriptorAnonymousInnerClass2 : DefaultPathDescriptor<IPath>
		 {
			 private IPath _path;
			 private string _nodePropertyKey;

			 public DefaultPathDescriptorAnonymousInnerClass2( IPath path, string nodePropertyKey )
			 {
				 this._path = path;
				 this._nodePropertyKey = nodePropertyKey;
			 }

			 public override string nodeRepresentation( IPath path, INode node )
			 {
				  return "(" + node.GetProperty( _nodePropertyKey, node.Id ) + ")";
			 }

			 public override string relationshipRepresentation( IPath path, INode from, IRelationship relationship )
			 {
				  return relationship.StartNode.Equals( from ) ? "-->" : "<--";
			 }
		 }

		 /// <summary>
		 /// Create a new <seealso cref="Paths.PathDescriptor"/> that prints values of listed property keys
		 /// and id of nodes and relationships if configured so. </summary>
		 /// <param name="nodeId">            true if node id should be included. </param>
		 /// <param name="relId">             true if relationship id should be included. </param>
		 /// <param name="propertyKeys">      all property keys that should be included. </param>
		 /// @param <T>               the type of the <seealso cref="IPath"/> </param>
		 /// <returns>                  a new <seealso cref="Paths.PathDescriptor"/> </returns>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static <T extends org.Neo4Net.graphdb.Path> PathDescriptor<T> descriptorForIdAndProperties(final boolean nodeId, final boolean relId, final String... propertyKeys)
		 public static PathDescriptor<T> DescriptorForIdAndProperties<T>( bool nodeId, bool relId, params string[] propertyKeys ) where T : Neo4Net.GraphDb.IPath
		 {
			  return new PathDescriptorAnonymousInnerClass( nodeId, relId, propertyKeys );
		 }

		 private class PathDescriptorAnonymousInnerClass : Paths.PathDescriptor<T>
		 {
			 private bool _nodeId;
			 private bool _relId;
			 private string[] _propertyKeys;

			 public PathDescriptorAnonymousInnerClass( bool nodeId, bool relId, string[] propertyKeys )
			 {
				 this._nodeId = nodeId;
				 this._relId = relId;
				 this._propertyKeys = propertyKeys;
			 }

			 public string nodeRepresentation( T path, INode node )
			 {
				  string representation = representation( node );
				  return "(" + ( _nodeId ? node.Id : "" ) + ( _nodeId && !representation.Equals( "" ) ? "," : "" ) + representation + ")";
			 }

			 private string representation( IPropertyContainer IEntity )
			 {
				  StringBuilder builder = new StringBuilder();
				  foreach ( string key in _propertyKeys )
				  {
						object value = IEntity.GetProperty( key, null );
						if ( value != null )
						{
							 if ( builder.Length > 0 )
							 {
								  builder.Append( "," );
							 }
							 builder.Append( value );
						}
				  }
				  return builder.ToString();
			 }

			 public string relationshipRepresentation( T path, INode from, IRelationship relationship )
			 {
				  Direction direction = relationship.EndNode.Equals( from ) ? Direction.INCOMING : Direction.OUTGOING;
				  StringBuilder builder = new StringBuilder();
				  if ( direction.Equals( Direction.INCOMING ) )
				  {
						builder.Append( "<" );
				  }
				  builder.Append( "-[" + ( _relId ? relationship.Id : "" ) );
				  string representation = representation( relationship );
				  if ( _relId && !representation.Equals( "" ) )
				  {
						builder.Append( "," );
				  }
				  builder.Append( representation );
				  builder.Append( "]-" );

				  if ( direction.Equals( Direction.OUTGOING ) )
				  {
						builder.Append( ">" );
				  }
				  return builder.ToString();
			 }
		 }

		 public static IPath SingleNodePath( INode node )
		 {
			  return new SingleNodePath( node );
		 }

		 private class SingleNodePath : IPath
		 {
			  internal readonly INode Node;

			  internal SingleNodePath( INode node )
			  {
					this.Node = node;
			  }

			  public override INode StartNode()
			  {
					return Node;
			  }

			  public override INode EndNode()
			  {
					return Node;
			  }

			  public override IRelationship LastRelationship()
			  {
					return null;
			  }

			  public override IEnumerable<IRelationship> Relationships()
			  {
					return Collections.emptyList();
			  }

			  public override IEnumerable<IRelationship> ReverseRelationships()
			  {
					return Relationships();
			  }

			  public override IEnumerable<INode> Nodes()
			  {
					return Arrays.asList( Node );
			  }

			  public override IEnumerable<INode> ReverseNodes()
			  {
					return Nodes();
			  }

			  public override int Length()
			  {
					return 0;
			  }

			  public override IEnumerator<PropertyContainer> Iterator()
			  {
					return Arrays.asList<PropertyContainer>( Node ).GetEnumerator();
			  }
		 }

		 public static string DefaultPathToStringWithNotInTransactionFallback( IPath path )
		 {
			  try
			  {
					return Paths.DefaultPathToString( path );
			  }
			  catch ( Exception e ) when ( e is NotInTransactionException || e is DatabaseShutdownException )
			  {
					// We don't keep the rel-name lookup if the database is shut down. Source ID and target ID also requires
					// database access in a transaction. However, failing on toString would be uncomfortably evil, so we fall
					// back to noting the relationship type id.
			  }
			  StringBuilder sb = new StringBuilder();
			  foreach ( IRelationship rel in path.Relationships() )
			  {
					if ( sb.Length == 0 )
					{
						 sb.Append( "(?)" );
					}
					sb.Append( "-[?," ).Append( rel.Id ).Append( "]-(?)" );
			  }
			  return sb.ToString();
		 }
	}

}