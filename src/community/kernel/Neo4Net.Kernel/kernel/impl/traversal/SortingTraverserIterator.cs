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
namespace Neo4Net.Kernel.impl.traversal
{

	using Path = Neo4Net.GraphDb.Path;
	using Resource = Neo4Net.GraphDb.Resource;
	using Neo4Net.GraphDb.Traversal;
	using Evaluation = Neo4Net.GraphDb.Traversal.Evaluation;
	using TraversalBranch = Neo4Net.GraphDb.Traversal.TraversalBranch;
	using Neo4Net.Helpers.Collections;

	internal class SortingTraverserIterator : PrefetchingResourceIterator<Path>, TraverserIterator
	{
//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: private final java.util.Comparator<? super org.Neo4Net.graphdb.Path> sortingStrategy;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 private readonly IComparer<object> _sortingStrategy;
		 private readonly MonoDirectionalTraverserIterator _source;
		 private readonly Resource _resource;
		 private IEnumerator<Path> _sortedResultIterator;

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: SortingTraverserIterator(org.Neo4Net.graphdb.Resource resource, java.util.Comparator<? super org.Neo4Net.graphdb.Path> sortingStrategy, MonoDirectionalTraverserIterator source)
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