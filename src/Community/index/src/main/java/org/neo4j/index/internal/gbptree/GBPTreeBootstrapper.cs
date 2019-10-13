using System;

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
namespace Neo4Net.Index.@internal.gbptree
{
	using Pair = org.apache.commons.lang3.tuple.Pair;

	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using SingleFilePageSwapperFactory = Neo4Net.Io.pagecache.impl.SingleFilePageSwapperFactory;
	using MuninnPageCache = Neo4Net.Io.pagecache.impl.muninn.MuninnPageCache;
	using PageCursorTracerSupplier = Neo4Net.Io.pagecache.tracing.cursor.PageCursorTracerSupplier;
	using EmptyVersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using JobScheduler = Neo4Net.Scheduler.JobScheduler;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.config.Configuration.EMPTY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.@internal.gbptree.GBPTree.NO_HEADER_READER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.@internal.gbptree.GBPTree.NO_HEADER_WRITER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.@internal.gbptree.GBPTree.NO_MONITOR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.@internal.gbptree.RecoveryCleanupWorkCollector.ignore;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.pagecache.tracing.PageCacheTracer.NULL;

	public class GBPTreeBootstrapper
	{
		 private readonly PageCache _pageCache;
		 private readonly LayoutBootstrapper _layoutBootstrapper;
		 private readonly bool _readOnly;

		 public GBPTreeBootstrapper( PageCache pageCache, LayoutBootstrapper layoutBootstrapper, bool readOnly )
		 {
			  this._pageCache = pageCache;
			  this._layoutBootstrapper = layoutBootstrapper;
			  this._readOnly = readOnly;
		 }

		 public virtual Bootstrap BootstrapTree( File file, string targetLayout )
		 {
			  try
			  {
					// Get meta information about the tree
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: MetaVisitor<?,?> metaVisitor = new MetaVisitor();
					MetaVisitor<object, ?> metaVisitor = new MetaVisitor();
					GBPTreeStructure.VisitHeader( _pageCache, file, metaVisitor );
					Meta meta = metaVisitor.MetaConflict;
					Pair<TreeState, TreeState> statePair = metaVisitor.StatePair;
					TreeState state = TreeStatePair.SelectNewestValidState( statePair );

					// Create layout and treeNode from meta
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: Layout<?,?> layout = layoutBootstrapper.create(file, pageCache, meta, targetLayout);
					Layout<object, ?> layout = _layoutBootstrapper.create( file, _pageCache, meta, targetLayout );
					TreeNodeSelector.Factory factory = TreeNodeSelector.SelectByFormat( meta.FormatIdentifier, meta.FormatVersion );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: TreeNode<?,?> treeNode = factory.create(meta.getPageSize(), layout);
					TreeNode<object, ?> treeNode = factory.Create( meta.PageSize, layout );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: GBPTree<?,?> tree = new GBPTree<>(pageCache, file, layout, meta.getPageSize(), NO_MONITOR, NO_HEADER_READER, NO_HEADER_WRITER, ignore(), readOnly);
					GBPTree<object, ?> tree = new GBPTree<object, ?>( _pageCache, file, layout, meta.PageSize, NO_MONITOR, NO_HEADER_READER, NO_HEADER_WRITER, ignore(), _readOnly );
					return new SuccessfulBootstrap( tree, layout, treeNode, state, meta );
			  }
			  catch ( Exception )
			  {
					return new FailedBootstrap();
			  }
		 }

		 internal static PageCache PageCache( JobScheduler jobScheduler )
		 {
			  SingleFilePageSwapperFactory swapper = new SingleFilePageSwapperFactory();
			  DefaultFileSystemAbstraction fs = new DefaultFileSystemAbstraction();
			  swapper.Open( fs, EMPTY );
			  PageCursorTracerSupplier cursorTracerSupplier = Neo4Net.Io.pagecache.tracing.cursor.PageCursorTracerSupplier_Fields.Null;
			  return new MuninnPageCache( swapper, 100, NULL, cursorTracerSupplier, EmptyVersionContextSupplier.EMPTY, jobScheduler );
		 }

		 public interface Bootstrap
		 {
			  bool Tree { get; }
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: GBPTree<?,?> getTree();
			  GBPTree<object, ?> Tree { get; }
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: Layout<?,?> getLayout();
			  Layout<object, ?> Layout { get; }
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: TreeNode<?,?> getTreeNode();
			  TreeNode<object, ?> TreeNode { get; }
			  TreeState State { get; }
			  Meta Meta { get; }
		 }

		 private class FailedBootstrap : Bootstrap
		 {
			  public virtual bool Tree
			  {
				  get
				  {
						return false;
				  }
			  }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public GBPTree<?,?> getTree()
			  public virtual GBPTree<object, ?> Tree
			  {
				  get
				  {
						throw new System.InvalidOperationException( "Bootstrap failed" );
				  }
			  }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public Layout<?,?> getLayout()
			  public virtual Layout<object, ?> Layout
			  {
				  get
				  {
						throw new System.InvalidOperationException( "Bootstrap failed" );
				  }
			  }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public TreeNode<?,?> getTreeNode()
			  public virtual TreeNode<object, ?> TreeNode
			  {
				  get
				  {
						throw new System.InvalidOperationException( "Bootstrap failed" );
				  }
			  }

			  public virtual TreeState State
			  {
				  get
				  {
						throw new System.InvalidOperationException( "Bootstrap failed" );
				  }
			  }

			  public virtual Meta Meta
			  {
				  get
				  {
						throw new System.InvalidOperationException( "Bootstrap failed" );
				  }
			  }
		 }

		 private class SuccessfulBootstrap : Bootstrap
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final GBPTree<?,?> tree;
			  internal readonly GBPTree<object, ?> TreeConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final Layout<?,?> layout;
			  internal readonly Layout<object, ?> LayoutConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final TreeNode<?,?> treeNode;
			  internal readonly TreeNode<object, ?> TreeNodeConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly TreeState StateConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly Meta MetaConflict;

			  internal SuccessfulBootstrap<T1, T2, T3>( GBPTree<T1> tree, Layout<T2> layout, TreeNode<T3> treeNode, TreeState state, Meta meta )
			  {
					this.TreeConflict = tree;
					this.LayoutConflict = layout;
					this.TreeNodeConflict = treeNode;
					this.StateConflict = state;
					this.MetaConflict = meta;
			  }

			  public virtual bool Tree
			  {
				  get
				  {
						return true;
				  }
			  }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public GBPTree<?,?> getTree()
			  public virtual GBPTree<object, ?> Tree
			  {
				  get
				  {
						return TreeConflict;
				  }
			  }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public Layout<?,?> getLayout()
			  public virtual Layout<object, ?> Layout
			  {
				  get
				  {
						return LayoutConflict;
				  }
			  }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public TreeNode<?,?> getTreeNode()
			  public virtual TreeNode<object, ?> TreeNode
			  {
				  get
				  {
						return TreeNodeConflict;
				  }
			  }

			  public virtual TreeState State
			  {
				  get
				  {
						return StateConflict;
				  }
			  }

			  public virtual Meta Meta
			  {
				  get
				  {
						return MetaConflict;
				  }
			  }
		 }

		 private class MetaVisitor<KEY, VALUE> : GBPTreeVisitor_Adaptor<KEY, VALUE>
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal Meta MetaConflict;
			  internal Pair<TreeState, TreeState> StatePair;

			  public override void Meta( Meta meta )
			  {
					this.MetaConflict = meta;
			  }

			  public override void TreeState( Pair<TreeState, TreeState> statePair )
			  {
					this.StatePair = statePair;
			  }
		 }
	}

}