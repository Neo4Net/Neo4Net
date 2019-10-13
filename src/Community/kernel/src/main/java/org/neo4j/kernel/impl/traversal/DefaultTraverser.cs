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
	using Neo4Net.Functions;
	using Node = Neo4Net.Graphdb.Node;
	using Path = Neo4Net.Graphdb.Path;
	using Relationship = Neo4Net.Graphdb.Relationship;
	using Neo4Net.Graphdb;
	using Neo4Net.Graphdb;
	using TraversalMetadata = Neo4Net.Graphdb.traversal.TraversalMetadata;
	using Traverser = Neo4Net.Graphdb.traversal.Traverser;
	using Neo4Net.Helpers.Collections;

	public class DefaultTraverser : Traverser
	{
		 private readonly Factory<TraverserIterator> _traverserIteratorFactory;

		 private TraversalMetadata _lastIterator;

		 internal DefaultTraverser( Factory<TraverserIterator> traverserIteratorFactory )
		 {
			  this._traverserIteratorFactory = traverserIteratorFactory;
		 }

		 public override ResourceIterable<Node> Nodes()
		 {
			  return new ResourcePathIterableWrapperAnonymousInnerClass( this );
		 }

		 private class ResourcePathIterableWrapperAnonymousInnerClass : ResourcePathIterableWrapper<Node>
		 {
			 private readonly DefaultTraverser _outerInstance;

			 public ResourcePathIterableWrapperAnonymousInnerClass( DefaultTraverser outerInstance ) : base( outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 protected internal override Node convert( Path path )
			 {
				  return path.EndNode();
			 }
		 }

		 public override ResourceIterable<Relationship> Relationships()
		 {
			  return new ResourcePathIterableWrapperAnonymousInnerClass2( this );
		 }

		 private class ResourcePathIterableWrapperAnonymousInnerClass2 : ResourcePathIterableWrapper<Relationship>
		 {
			 private readonly DefaultTraverser _outerInstance;

			 public ResourcePathIterableWrapperAnonymousInnerClass2( DefaultTraverser outerInstance ) : base( outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override ResourceIterator<Relationship> iterator()
			 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.graphdb.ResourceIterator<org.neo4j.graphdb.Path> pathIterator = pathIterator();
				  ResourceIterator<Path> pathIterator = pathIterator();
				  return new PrefetchingResourceIteratorAnonymousInnerClass( this, pathIterator );
			 }

			 private class PrefetchingResourceIteratorAnonymousInnerClass : PrefetchingResourceIterator<Relationship>
			 {
				 private readonly ResourcePathIterableWrapperAnonymousInnerClass2 _outerInstance;

				 private ResourceIterator<Path> _pathIterator;

				 public PrefetchingResourceIteratorAnonymousInnerClass( ResourcePathIterableWrapperAnonymousInnerClass2 outerInstance, ResourceIterator<Path> pathIterator )
				 {
					 this.outerInstance = outerInstance;
					 this._pathIterator = pathIterator;
				 }

				 public override void close()
				 {
					  _pathIterator.close();
				 }

				 protected internal override Relationship fetchNextOrNull()
				 {
					  while ( _pathIterator.MoveNext() )
					  {
							Path path = _pathIterator.Current;
							if ( path.Length() > 0 )
							{
								 return path.LastRelationship();
							}
					  }
					  return null;
				 }
			 }

			 protected internal override Relationship convert( Path path )
			 {
				  return path.LastRelationship();
			 }
		 }

		 public override ResourceIterator<Path> Iterator()
		 {
			  TraverserIterator traverserIterator = _traverserIteratorFactory.newInstance();
			  _lastIterator = traverserIterator;
			  return traverserIterator;
		 }

		 public override TraversalMetadata Metadata()
		 {
			  return _lastIterator;
		 }

		 private abstract class ResourcePathIterableWrapper<T> : ResourceIterable<T>
		 {
			 public abstract java.util.stream.Stream<T> Stream();
			  internal readonly ResourceIterable<Path> IterableToWrap;

			  protected internal ResourcePathIterableWrapper( ResourceIterable<Path> iterableToWrap )
			  {
					this.IterableToWrap = iterableToWrap;
			  }

			  protected internal virtual ResourceIterator<Path> PathIterator()
			  {
					return IterableToWrap.GetEnumerator();
			  }

			  public override ResourceIterator<T> Iterator()
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.graphdb.ResourceIterator<org.neo4j.graphdb.Path> iterator = pathIterator();
					ResourceIterator<Path> iterator = PathIterator();
					return new PrefetchingResourceIteratorAnonymousInnerClass2( this, iterator );
			  }

			  private class PrefetchingResourceIteratorAnonymousInnerClass2 : PrefetchingResourceIterator<T>
			  {
				  private readonly ResourcePathIterableWrapper<T> _outerInstance;

				  private ResourceIterator<Path> _iterator;

				  public PrefetchingResourceIteratorAnonymousInnerClass2( ResourcePathIterableWrapper<T> outerInstance, ResourceIterator<Path> iterator )
				  {
					  this.outerInstance = outerInstance;
					  this._iterator = iterator;
				  }

				  public override void close()
				  {
						_iterator.close();
				  }

				  protected internal override T fetchNextOrNull()
				  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						return _iterator.hasNext() ? outerInstance.Convert(_iterator.next()) : default(T);
				  }
			  }

			  protected internal abstract T Convert( Path path );
		 }
	}

}