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
namespace Org.Neo4j.Graphdb.traversal
{

	using Org.Neo4j.Graphdb;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.traversal.Paths.singleNodePath;

	/// <summary>
	/// Provides some common traversal sorting, used by
	/// <seealso cref="TraversalDescription.sort(System.Collections.IComparer)"/>.
	/// </summary>
	public abstract class Sorting
	{
		 // No instances
		 private Sorting()
		 {
		 }

		 /// <summary>
		 /// Sorts <seealso cref="Path"/>s by the property value of each path's end node.
		 /// </summary>
		 /// <param name="propertyKey"> the property key of the values to sort on. </param>
		 /// <returns> a <seealso cref="System.Collections.IComparer"/> suitable for sorting traversal results. </returns>
//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public static java.util.Comparator<? super org.neo4j.graphdb.Path> endNodeProperty(final String propertyKey)
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 public static IComparer<object> EndNodeProperty( string propertyKey )
		 {
			  return new EndNodeComparatorAnonymousInnerClass( propertyKey );
		 }

		 private class EndNodeComparatorAnonymousInnerClass : EndNodeComparator
		 {
			 private string _propertyKey;

			 public EndNodeComparatorAnonymousInnerClass( string propertyKey )
			 {
				 this._propertyKey = propertyKey;
			 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings({ "rawtypes", "unchecked" }) @Override protected int compareNodes(org.neo4j.graphdb.Node endNode1, org.neo4j.graphdb.Node endNode2)
			 protected internal override int compareNodes( Node endNode1, Node endNode2 )
			 {
				  IComparable p1 = ( IComparable ) endNode1.GetProperty( _propertyKey );
				  IComparable p2 = ( IComparable ) endNode2.GetProperty( _propertyKey );
				  if ( p1 == p2 )
				  {
						return 0;
				  }
				  else if ( p1 == null )
				  {
						return int.MinValue;
				  }
				  else if ( p2 == null )
				  {
						return int.MaxValue;
				  }
				  else
				  {
						return p1.CompareTo( p2 );
				  }
			 }
		 }

		 /// <summary>
		 /// Sorts <seealso cref="Path"/>s by the relationship count returned for its end node
		 /// by the supplied {@code expander}.
		 /// </summary>
		 /// <param name="expander"> the <seealso cref="PathExpander"/> to use for getting relationships
		 /// off of each <seealso cref="Path"/>'s end node. </param>
		 /// <returns> a <seealso cref="System.Collections.IComparer"/> suitable for sorting traversal results. </returns>
//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public static java.util.Comparator<? super org.neo4j.graphdb.Path> endNodeRelationshipCount(final org.neo4j.graphdb.PathExpander expander)
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 public static IComparer<object> EndNodeRelationshipCount( PathExpander expander )
		 {
			  return new EndNodeComparatorAnonymousInnerClass2( expander );
		 }

		 private class EndNodeComparatorAnonymousInnerClass2 : EndNodeComparator
		 {
			 private PathExpander _expander;

			 public EndNodeComparatorAnonymousInnerClass2( PathExpander expander )
			 {
				 this._expander = expander;
			 }

			 protected internal override int compareNodes( Node endNode1, Node endNode2 )
			 {
				  int? count1 = count( endNode1, _expander );
				  int? count2 = count( endNode2, _expander );
				  return count1.compareTo( count2 );
			 }

			 private int? count( Node node, PathExpander expander )
			 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Iterator<?> expand = expander.expand(singleNodePath(node), BranchState.NO_STATE).iterator();
				  IEnumerator<object> expand = expander.expand( singleNodePath( node ), BranchState.NO_STATE ).GetEnumerator();
				  int count = 0;
				  while ( expand.MoveNext() )
				  {
						count++;
				  }
				  return count;
			 }
		 }

		 /// <summary>
		 /// Comparator for <seealso cref="Path.endNode() end nodes"/> of two <seealso cref="Path paths"/>
		 /// </summary>
		 private abstract class EndNodeComparator : IComparer<Path>
		 {
			  public override int Compare( Path p1, Path p2 )
			  {
					return CompareNodes( p1.EndNode(), p2.EndNode() );
			  }

			  protected internal abstract int CompareNodes( Node endNode1, Node endNode2 );
		 }
	}

}