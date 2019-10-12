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
namespace Neo4Net.Kernel.impl.traversal
{

	using Path = Neo4Net.Graphdb.Path;
	using Resource = Neo4Net.Graphdb.Resource;
	using Neo4Net.Graphdb.traversal;
	using Evaluation = Neo4Net.Graphdb.traversal.Evaluation;
	using TraversalBranch = Neo4Net.Graphdb.traversal.TraversalBranch;
	using Neo4Net.Helpers.Collection;

	internal class SortingTraverserIterator : PrefetchingResourceIterator<Path>, TraverserIterator
	{
//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: private final java.util.Comparator<? super org.neo4j.graphdb.Path> sortingStrategy;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 private readonly IComparer<object> _sortingStrategy;
		 private readonly MonoDirectionalTraverserIterator _source;
		 private readonly Resource _resource;
		 private IEnumerator<Path> _sortedResultIterator;

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: SortingTraverserIterator(org.neo4j.graphdb.Resource resource, java.util.Comparator<? super org.neo4j.graphdb.Path> sortingStrategy, MonoDirectionalTraverserIterator source)
		 internal SortingTraverserIterator<T1>( Resource resource, IComparer<T1> sortingStrategy, MonoDirectionalTraverserIterator source )
		 {
			  this._resource = resource;
			  this._sortingStrategy = sortingStrategy;
			  this._source = source;
		 }

		 public virtual int NumberOfPathsReturned
		 {
			 get
			 {
				  return _source.NumberOfPathsReturned;
			 }
		 }

		 public virtual int NumberOfRelationshipsTraversed
		 {
			 get
			 {
				  return _source.NumberOfRelationshipsTraversed;
			 }
		 }

		 public override void RelationshipTraversed()
		 {
			  _source.relationshipTraversed();
		 }

		 public override void UnnecessaryRelationshipTraversed()
		 {
			  _source.unnecessaryRelationshipTraversed();
		 }

		 public override bool IsUniqueFirst( TraversalBranch branch )
		 {
			  return _source.isUniqueFirst( branch );
		 }

		 public override bool IsUnique( TraversalBranch branch )
		 {
			  return _source.isUnique( branch );
		 }

		 public override Evaluation Evaluate( TraversalBranch branch, BranchState state )
		 {
			  return _source.evaluate( branch, state );
		 }

		 protected internal override Path FetchNextOrNull()
		 {
			  if ( _sortedResultIterator == null )
			  {
					_sortedResultIterator = FetchAndSortResult();
			  }
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  return _sortedResultIterator.hasNext() ? _sortedResultIterator.next() : null;
		 }

		 private IEnumerator<Path> FetchAndSortResult()
		 {
			  IList<Path> result = new List<Path>();
			  while ( _source.MoveNext() )
			  {
					result.Add( _source.Current );
			  }
			  result.sort( _sortingStrategy );
			  return result.GetEnumerator();
		 }

		 public override void Close()
		 {
			  _resource.close();
		 }
	}

}