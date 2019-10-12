using System.Diagnostics;

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
namespace Org.Neo4j.Kernel.impl.store.counts
{
	using CountsAccessor = Org.Neo4j.Kernel.Impl.Api.CountsAccessor;
	using CountsVisitor = Org.Neo4j.Kernel.Impl.Api.CountsVisitor;
	using CountsKey = Org.Neo4j.Kernel.impl.store.counts.keys.CountsKey;
	using CountsKeyFactory = Org.Neo4j.Kernel.impl.store.counts.keys.CountsKeyFactory;
	using ReadableBuffer = Org.Neo4j.Kernel.impl.store.kvstore.ReadableBuffer;
	using UnknownKey = Org.Neo4j.Kernel.impl.store.kvstore.UnknownKey;
	using WritableBuffer = Org.Neo4j.Kernel.impl.store.kvstore.WritableBuffer;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.counts.keys.CountsKeyFactory.indexStatisticsKey;

	internal class KeyFormat : CountsVisitor
	{
		 private const sbyte NODE_COUNT = 1;
		 private const sbyte RELATIONSHIP_COUNT = 2;
		 private const sbyte INDEX = 127;
		 private const sbyte INDEX_STATS = 1;
		 private const sbyte INDEX_SAMPLE = 2;
		 private readonly WritableBuffer _buffer;

		 internal KeyFormat( WritableBuffer key )
		 {
			  Debug.Assert( key.Size() >= 16 );
			  this._buffer = key;
		 }

		 /// <summary>
		 /// Key format:
		 /// <pre>
		 ///  0 1 2 3 4 5 6 7   8 9 A B C D E F
		 /// [t,0,0,0,0,0,0,0 ; 0,0,0,0,l,l,l,l]
		 ///  t - entry type - "{@link #NODE_COUNT}"
		 ///  l - label id
		 /// </pre>
		 /// For value format, see <seealso cref="CountsAccessor.Updater.incrementNodeCount(long, long)"/>.
		 /// </summary>
		 public override void VisitNodeCount( int labelId, long count )
		 {
			  _buffer.putByte( 0, NODE_COUNT ).putInt( 12, labelId );
		 }

		 /// <summary>
		 /// Key format:
		 /// <pre>
		 ///  0 1 2 3 4 5 6 7   8 9 A B C D E F
		 /// [t,0,0,0,s,s,s,s ; r,r,r,r,e,e,e,e]
		 ///  t - entry type - "{@link #RELATIONSHIP_COUNT}"
		 ///  s - start label id
		 ///  r - relationship type id
		 ///  e - end label id
		 /// </pre>
		 /// For value format, see <seealso cref="CountsAccessor.Updater.incrementRelationshipCount(long, int, long, long)"/>
		 /// </summary>
		 public override void VisitRelationshipCount( int startLabelId, int typeId, int endLabelId, long count )
		 {
			  _buffer.putByte( 0, RELATIONSHIP_COUNT ).putInt( 4, startLabelId ).putInt( 8, typeId ).putInt( 12, endLabelId );
		 }

		 /// <summary>
		 /// Key format:
		 /// <pre>
		 ///  0 1 2 3 4 5 6 7   8 9 A B C D E F
		 /// [t,0,0,0,i,i,i,i ; 0,0,0,0,0,0,0,k]
		 ///  t - index entry marker - "{@link #INDEX}"
		 ///  k - entry (sub)type - "{@link #INDEX_STATS}"
		 ///  i - index id
		 /// </pre>
		 /// For value format, see <seealso cref="org.neo4j.kernel.impl.store.counts.CountsUpdater.replaceIndexUpdateAndSize(long, long, long)"/>.
		 /// </summary>
		 public override void VisitIndexStatistics( long indexId, long updates, long size )
		 {
			  IndexKey( INDEX_STATS, indexId );
		 }

		 /// <summary>
		 /// Key format:
		 /// <pre>
		 ///  0 1 2 3 4 5 6 7   8 9 A B C D E F
		 /// [t,0,0,0,i,i,i,i ; 0,0,0,0,0,0,0,k]
		 ///  t - index entry marker - "{@link #INDEX}"
		 ///  k - entry (sub)type - "{@link #INDEX_SAMPLE}"
		 ///  i - index id
		 /// </pre>
		 /// For value format, see <seealso cref="org.neo4j.kernel.impl.store.counts.CountsUpdater.replaceIndexSample(long , long, long)"/>.
		 /// </summary>
		 public override void VisitIndexSample( long indexId, long unique, long size )
		 {
			  IndexKey( INDEX_SAMPLE, indexId );
		 }

		 private void IndexKey( sbyte indexKey, long indexId )
		 {
			  _buffer.putByte( 0, INDEX ).putInt( 4, ( int ) indexId ).putByte( 15, indexKey );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static org.neo4j.kernel.impl.store.counts.keys.CountsKey readKey(org.neo4j.kernel.impl.store.kvstore.ReadableBuffer key) throws org.neo4j.kernel.impl.store.kvstore.UnknownKey
		 public static CountsKey ReadKey( ReadableBuffer key )
		 {
			  switch ( key.GetByte( 0 ) )
			  {
			  case KeyFormat.NODE_COUNT:
					return CountsKeyFactory.nodeKey( key.GetInt( 12 ) );
			  case KeyFormat.RELATIONSHIP_COUNT:
					return CountsKeyFactory.relationshipKey( key.GetInt( 4 ), key.GetInt( 8 ), key.GetInt( 12 ) );
			  case KeyFormat.INDEX:
					sbyte indexKeyByte = key.GetByte( 15 );
					long indexId = key.GetInt( 4 );
					switch ( indexKeyByte )
					{
					case KeyFormat.INDEX_STATS:
						 return indexStatisticsKey( indexId );
					case KeyFormat.INDEX_SAMPLE:
						 return CountsKeyFactory.indexSampleKey( indexId );
					default:
						 throw new System.InvalidOperationException( "Unknown index key: " + indexKeyByte );
					}
			  default:
					throw new UnknownKey( "Unknown key type: " + key );
			  }
		 }
	}

}