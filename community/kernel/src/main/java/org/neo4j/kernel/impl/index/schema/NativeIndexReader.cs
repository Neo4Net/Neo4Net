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
namespace Org.Neo4j.Kernel.Impl.Index.Schema
{

	using PrimitiveLongResourceIterator = Org.Neo4j.Collection.PrimitiveLongResourceIterator;
	using Org.Neo4j.Cursor;
	using Org.Neo4j.Index.@internal.gbptree;
	using Org.Neo4j.Index.@internal.gbptree;
	using IndexOrder = Org.Neo4j.@internal.Kernel.Api.IndexOrder;
	using IndexQuery = Org.Neo4j.@internal.Kernel.Api.IndexQuery;
	using IOUtils = Org.Neo4j.Io.IOUtils;
	using NodePropertyAccessor = Org.Neo4j.Storageengine.Api.NodePropertyAccessor;
	using IndexDescriptor = Org.Neo4j.Storageengine.Api.schema.IndexDescriptor;
	using IndexProgressor = Org.Neo4j.Storageengine.Api.schema.IndexProgressor;
	using IndexReader = Org.Neo4j.Storageengine.Api.schema.IndexReader;
	using IndexSampler = Org.Neo4j.Storageengine.Api.schema.IndexSampler;
	using Value = Org.Neo4j.Values.Storable.Value;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.NativeIndexKey.Inclusion.NEUTRAL;

	internal abstract class NativeIndexReader<KEY, VALUE> : IndexReader where KEY : NativeIndexKey<KEY> where VALUE : NativeIndexValue
	{
		 protected internal readonly IndexDescriptor Descriptor;
		 internal readonly IndexLayout<KEY, VALUE> Layout;
		 internal readonly ISet<RawCursor<Hit<KEY, VALUE>, IOException>> OpenSeekers;
		 internal readonly GBPTree<KEY, VALUE> Tree;

		 internal NativeIndexReader( GBPTree<KEY, VALUE> tree, IndexLayout<KEY, VALUE> layout, IndexDescriptor descriptor )
		 {
			  this.Tree = tree;
			  this.Layout = layout;
			  this.Descriptor = descriptor;
			  this.OpenSeekers = new HashSet<RawCursor<Hit<KEY, VALUE>, IOException>>();
		 }

		 public override void Close()
		 {
			  EnsureOpenSeekersClosed();
		 }

		 public override IndexSampler CreateSampler()
		 {
			  // For a unique index there's an optimization, knowing that all values in it are unique, to simply count
			  // the number of indexed values and create a sample for that count. The GBPTree doesn't have an O(1)
			  // count mechanism, it will have to manually count the indexed values in it to get it.
			  // For that reason this implementation opts for keeping complexity down by just using the existing
			  // non-unique sampler which scans the index and counts (potentially duplicates, of which there will
			  // be none in a unique index).

			  FullScanNonUniqueIndexSampler<KEY, VALUE> sampler = new FullScanNonUniqueIndexSampler<KEY, VALUE>( Tree, Layout );
			  return sampler.result;
		 }

		 public override long CountIndexedNodes( long nodeId, int[] propertyKeyIds, params Value[] propertyValues )
		 {
			  KEY treeKeyFrom = Layout.newKey();
			  KEY treeKeyTo = Layout.newKey();
			  treeKeyFrom.initialize( nodeId );
			  treeKeyTo.initialize( nodeId );
			  for ( int i = 0; i < propertyValues.Length; i++ )
			  {
					treeKeyFrom.initFromValue( i, propertyValues[i], NEUTRAL );
					treeKeyTo.initFromValue( i, propertyValues[i], NEUTRAL );
			  }
			  try
			  {
					  using ( RawCursor<Hit<KEY, VALUE>, IOException> seeker = Tree.seek( treeKeyFrom, treeKeyTo ) )
					  {
						long count = 0;
						while ( seeker.Next() )
						{
							 if ( seeker.get().key().EntityId == nodeId )
							 {
								  count++;
							 }
						}
						return count;
					  }
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }

		 public override PrimitiveLongResourceIterator Query( params IndexQuery[] predicates )
		 {
			  // This method isn't called from main product code anymore so it can might as well delegate to the "real" method
			  NodeValueIterator nodeValueIterator = new NodeValueIterator();
			  Query( nodeValueIterator, IndexOrder.NONE, nodeValueIterator.NeedsValues(), predicates );
			  return nodeValueIterator;
		 }

		 public override void Query( Org.Neo4j.Storageengine.Api.schema.IndexProgressor_NodeValueClient cursor, IndexOrder indexOrder, bool needsValues, params IndexQuery[] predicates )
		 {
			  ValidateQuery( indexOrder, predicates );

			  KEY treeKeyFrom = Layout.newKey();
			  KEY treeKeyTo = Layout.newKey();
			  InitializeFromToKeys( treeKeyFrom, treeKeyTo );

			  bool needFilter = InitializeRangeForQuery( treeKeyFrom, treeKeyTo, predicates );
			  StartSeekForInitializedRange( cursor, treeKeyFrom, treeKeyTo, predicates, indexOrder, needFilter, needsValues );
		 }

		 internal virtual void InitializeFromToKeys( KEY treeKeyFrom, KEY treeKeyTo )
		 {
			  treeKeyFrom.initialize( long.MinValue );
			  treeKeyTo.initialize( long.MaxValue );
		 }

		 public override abstract bool HasFullValuePrecision( params IndexQuery[] predicates );

		 public override void DistinctValues( Org.Neo4j.Storageengine.Api.schema.IndexProgressor_NodeValueClient client, NodePropertyAccessor propertyAccessor, bool needsValues )
		 {
			  KEY lowest = Layout.newKey();
			  lowest.initialize( long.MinValue );
			  lowest.initValuesAsLowest();
			  KEY highest = Layout.newKey();
			  highest.initialize( long.MaxValue );
			  highest.initValuesAsHighest();
			  try
			  {
					RawCursor<Hit<KEY, VALUE>, IOException> seeker = Tree.seek( lowest, highest );
					client.Initialize( Descriptor, new NativeDistinctValuesProgressor<>( seeker, client, OpenSeekers, Layout, Layout.compareValue ), new IndexQuery[0], IndexOrder.NONE, needsValues );
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }

		 internal abstract void ValidateQuery( IndexOrder indexOrder, IndexQuery[] predicates );

		 /// <returns> true if query results from seek will need to be filtered through the predicates, else false </returns>
		 internal abstract bool InitializeRangeForQuery( KEY treeKeyFrom, KEY treeKeyTo, IndexQuery[] predicates );

		 internal virtual void StartSeekForInitializedRange( Org.Neo4j.Storageengine.Api.schema.IndexProgressor_NodeValueClient client, KEY treeKeyFrom, KEY treeKeyTo, IndexQuery[] query, IndexOrder indexOrder, bool needFilter, bool needsValues )
		 {
			  if ( IsEmptyRange( treeKeyFrom, treeKeyTo ) )
			  {
					client.Initialize( Descriptor, IndexProgressor.EMPTY, query, indexOrder, needsValues );
					return;
			  }
			  try
			  {
					RawCursor<Hit<KEY, VALUE>, IOException> seeker = MakeIndexSeeker( treeKeyFrom, treeKeyTo, indexOrder );
					IndexProgressor hitProgressor = GetIndexProgressor( seeker, client, needFilter, query );
					client.Initialize( Descriptor, hitProgressor, query, indexOrder, needsValues );
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.neo4j.cursor.RawCursor<org.neo4j.index.internal.gbptree.Hit<KEY,VALUE>,java.io.IOException> makeIndexSeeker(KEY treeKeyFrom, KEY treeKeyTo, org.neo4j.internal.kernel.api.IndexOrder indexOrder) throws java.io.IOException
		 internal virtual RawCursor<Hit<KEY, VALUE>, IOException> MakeIndexSeeker( KEY treeKeyFrom, KEY treeKeyTo, IndexOrder indexOrder )
		 {
			  if ( indexOrder == IndexOrder.DESCENDING )
			  {
					KEY tmpKey = treeKeyFrom;
					treeKeyFrom = treeKeyTo;
					treeKeyTo = tmpKey;
			  }
			  RawCursor<Hit<KEY, VALUE>, IOException> seeker = Tree.seek( treeKeyFrom, treeKeyTo );
			  OpenSeekers.Add( seeker );
			  return seeker;
		 }

		 private IndexProgressor GetIndexProgressor( RawCursor<Hit<KEY, VALUE>, IOException> seeker, Org.Neo4j.Storageengine.Api.schema.IndexProgressor_NodeValueClient client, bool needFilter, IndexQuery[] query )
		 {
			  return needFilter ? new FilteringNativeHitIndexProgressor<>( seeker, client, OpenSeekers, query ) : new NativeHitIndexProgressor<>( seeker, client, OpenSeekers );
		 }

		 private bool IsEmptyRange( KEY treeKeyFrom, KEY treeKeyTo )
		 {
			  return Layout.compare( treeKeyFrom, treeKeyTo ) > 0;
		 }

		 private void EnsureOpenSeekersClosed()
		 {
			  try
			  {
					IOUtils.closeAll( OpenSeekers );
					OpenSeekers.Clear();
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }
	}

}