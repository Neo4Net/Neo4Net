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
	using Neo4Net.Collections.Helpers;

	internal abstract class AbstractTraverserIterator : PrefetchingResourceIterator<Path>, TraverserIterator
	{
		public abstract Evaluation Evaluate( Neo4Net.GraphDb.Traversal.TraversalBranch branch, Neo4Net.GraphDb.Traversal.BranchState<STATE> state );
		public abstract bool IsUnique( Neo4Net.GraphDb.Traversal.TraversalBranch branch );
		public abstract bool IsUniqueFirst( Neo4Net.GraphDb.Traversal.TraversalBranch branch );
		public abstract IResourceIterator<R> Map( System.Func<T, R> map );
		public override abstract java.util.stream.Stream<T> Stream();
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal int NumberOfPathsReturnedConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal int NumberOfRelationshipsTraversedConflict;
		 private Resource _resource;

		 protected internal AbstractTraverserIterator( Resource resource )
		 {
			  this._resource = resource;
		 }

		 public virtual int NumberOfPathsReturned
		 {
			 get
			 {
				  return NumberOfPathsReturnedConflict;
			 }
		 }

		 public virtual int NumberOfRelationshipsTraversed
		 {
			 get
			 {
				  return NumberOfRelationshipsTraversedConflict;
			 }
		 }

		 public override void RelationshipTraversed()
		 {
			  NumberOfRelationshipsTraversedConflict++;
		 }

		 public override void UnnecessaryRelationshipTraversed()
		 {
			  NumberOfRelationshipsTraversedConflict++;
		 }

		 public override void Close()
		 {
			  if ( _resource != null )
			  {
					_resource.close();
					_resource = null;
			  }
		 }
	}

}