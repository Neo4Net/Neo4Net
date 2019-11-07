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
	using Node = Neo4Net.GraphDb.Node;
	using Path = Neo4Net.GraphDb.Path;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using Neo4Net.GraphDb;
	using Neo4Net.GraphDb;
	using TraversalMetadata = Neo4Net.GraphDb.Traversal.TraversalMetadata;
	using Traverser = Neo4Net.GraphDb.Traversal.Traverser;
	using Neo4Net.Collections.Helpers;

	public class DefaultTraverser : Traverser
	{
		 private readonly IFactory<TraverserIterator> _traverserIteratorFactory;

		 private TraversalMetadata _lastIterator;

		 internal DefaultTraverser( IFactory<TraverserIterator> traverserIteratorFactory )
		 {
			  this._traverserIteratorFactory = traverserIteratorFactory;
		 }

		 public overrideIResourceIterable<Node> Nodes()
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

		 public overrideIResourceIterable<Relationship> Relationships()
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

			 public override IResourceIterator<Relationship> iterator()
			 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.graphdb.ResourceIterator<Neo4Net.graphdb.Path> pathIterator = pathIterator();
				  IResourceIterator<Path> pathIterator = pathIterator();
				  return new PrefetchingResourceIteratorAnonymousInnerClass( this, pathIterator );
			 }

			 private class PrefetchingResourceIteratorAnonymousInnerClass : PrefetchingResourceIterator<Relationship>
			 {
				 private readonly ResourcePathIterableWrapperAnonymousInnerClass2 _outerInstance;

				 private IResourceIterator<Path> _pathIterator;

				 public PrefetchingResourceIteratorAnonymousInnerClass( ResourcePathIterableWrapperAnonymousInnerClass2 outerInstance, IResourceIterator<Path> pathIterator )
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

		 public override IResourceIterator<Path> Iterator()
		 {
			  TraverserIterator traverserIterator = _traverserIteratorFactory.newInstance();
			  _lastIterator = traverserIterator;
			  return traverserIterator;
		 }

		 public override TraversalMetadata Metadata()
		 {
			  return _lastIterator;
		 }

		 private abstract class ResourcePathIterableWrapper<T> :IResourceIterable<T>
		 {
			 public abstract java.util.stream.Stream<T> Stream();
			  internal readonlyIResourceIterable<Path> IterableToWrap;

			  protected internal ResourcePathIterableWrapper(IResourceIterable<Path> iterableToWrap )
			  {
					this.IterableToWrap = iterableToWrap;
			  }

			  protected internal virtual IResourceIterator<Path> PathIterator()
			  {
					return IterableToWrap.GetEnumerator();
			  }

			  public override IResourceIterator<T> Iterator()
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.graphdb.ResourceIterator<Neo4Net.graphdb.Path> iterator = pathIterator();
					ResourceIterator<Path> iterator = PathIterator();
					return new PrefetchingResourceIteratorAnonymousInnerClass2( this, iterator );
			  }

			  private class PrefetchingResourceIteratorAnonymousInnerClass2 : PrefetchingResourceIterator<T>
			  {
				  private readonly ResourcePathIterableWrapper<T> _outerInstance;

				  private IResourceIterator<Path> _iterator;

				  public PrefetchingResourceIteratorAnonymousInnerClass2( ResourcePathIterableWrapper<T> outerInstance, IResourceIterator<Path> iterator )
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