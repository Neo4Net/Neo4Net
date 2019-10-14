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
namespace Neo4Net.Kernel.Impl.Index.Schema
{

	using Neo4Net.Cursors;
	using Neo4Net.Index.Internal.gbptree;
	using Neo4Net.Index.Internal.gbptree;
	using NonUniqueIndexSampler = Neo4Net.Kernel.Impl.Api.index.sampling.NonUniqueIndexSampler;
	using IndexSample = Neo4Net.Storageengine.Api.schema.IndexSample;

	/// <summary>
	/// <seealso cref="NonUniqueIndexSampler"/> which performs a full scans of a <seealso cref="GBPTree"/> in <seealso cref="result()"/>.
	/// </summary>
	/// @param <KEY> type of keys in tree. </param>
	/// @param <VALUE> type of values in tree. </param>
	internal class FullScanNonUniqueIndexSampler<KEY, VALUE> : Neo4Net.Kernel.Impl.Api.index.sampling.NonUniqueIndexSampler_Adapter where KEY : NativeIndexKey<KEY> where VALUE : NativeIndexValue
	{
		 private readonly GBPTree<KEY, VALUE> _gbpTree;
		 private readonly IndexLayout<KEY, VALUE> _layout;

		 internal FullScanNonUniqueIndexSampler( GBPTree<KEY, VALUE> gbpTree, IndexLayout<KEY, VALUE> layout )
		 {
			  this._gbpTree = gbpTree;
			  this._layout = layout;
		 }

		 public override IndexSample Result()
		 {
			  KEY lowest = _layout.newKey();
			  lowest.initialize( long.MinValue );
			  lowest.initValuesAsLowest();
			  KEY highest = _layout.newKey();
			  highest.initialize( long.MaxValue );
			  highest.initValuesAsHighest();
			  KEY prev = _layout.newKey();
			  try
			  {
					  using ( RawCursor<Hit<KEY, VALUE>, IOException> seek = _gbpTree.seek( lowest, highest ) )
					  {
						long sampledValues = 0;
						long uniqueValues = 0;
      
						// Get the first one so that prev gets initialized
						if ( seek.Next() )
						{
							 prev = _layout.copyKey( seek.get().key(), prev );
							 sampledValues++;
							 uniqueValues++;
      
							 // Then do the rest
							 while ( seek.Next() )
							 {
								  Hit<KEY, VALUE> hit = seek.get();
								  if ( _layout.compareValue( prev, hit.Key() ) != 0 )
								  {
										uniqueValues++;
										_layout.copyKey( hit.Key(), prev );
								  }
								  // else this is a duplicate of the previous one
								  sampledValues++;
							 }
						}
						return new IndexSample( sampledValues, uniqueValues, sampledValues );
					  }
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }

		 public override IndexSample Result( int numDocs )
		 {
			  throw new System.NotSupportedException();
		 }
	}

}