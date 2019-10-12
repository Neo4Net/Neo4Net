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
	using CountsKey = Org.Neo4j.Kernel.impl.store.counts.keys.CountsKey;
	using Org.Neo4j.Kernel.impl.store.kvstore;
	using ValueUpdate = Org.Neo4j.Kernel.impl.store.kvstore.ValueUpdate;
	using WritableBuffer = Org.Neo4j.Kernel.impl.store.kvstore.WritableBuffer;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.counts.keys.CountsKeyFactory.indexSampleKey;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.counts.keys.CountsKeyFactory.indexStatisticsKey;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.counts.keys.CountsKeyFactory.nodeKey;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.counts.keys.CountsKeyFactory.relationshipKey;

	internal sealed class CountsUpdater : Org.Neo4j.Kernel.Impl.Api.CountsAccessor_Updater, Org.Neo4j.Kernel.Impl.Api.CountsAccessor_IndexStatsUpdater, AutoCloseable
	{
		 private readonly EntryUpdater<CountsKey> _updater;

		 internal CountsUpdater( EntryUpdater<CountsKey> updater )
		 {
			  this._updater = updater;
		 }

		 /// <summary>
		 /// Value format:
		 /// <pre>
		 ///  0 1 2 3 4 5 6 7   8 9 A B C D E F
		 /// [0,0,0,0,0,0,0,0 ; c,c,c,c,c,c,c,c]
		 ///  c - number of matching nodes
		 /// </pre>
		 /// For key format, see <seealso cref="KeyFormat.visitNodeCount(int, long)"/>
		 /// </summary>
		 public override void IncrementNodeCount( long labelId, long delta )
		 {
			  try
			  {
					_updater.apply( nodeKey( labelId ), IncrementSecondBy( delta ) );
			  }
			  catch ( IOException e )
			  {
					throw new UnderlyingStorageException( e );
			  }
		 }

		 /// <summary>
		 /// Value format:
		 /// <pre>
		 ///  0 1 2 3 4 5 6 7   8 9 A B C D E F
		 /// [0,0,0,0,0,0,0,0 ; c,c,c,c,c,c,c,c]
		 ///  c - number of matching relationships
		 /// </pre>
		 /// For key format, see <seealso cref="KeyFormat.visitRelationshipCount(int, int, int, long)"/>
		 /// </summary>
		 public override void IncrementRelationshipCount( long startLabelId, int typeId, long endLabelId, long delta )
		 {
			  try
			  {
					_updater.apply( relationshipKey( startLabelId, typeId, endLabelId ), IncrementSecondBy( delta ) );
			  }
			  catch ( IOException e )
			  {
					throw new UnderlyingStorageException( e );
			  }
		 }

		 /// <summary>
		 /// Value format:
		 /// <pre>
		 ///  0 1 2 3 4 5 6 7   8 9 A B C D E F
		 /// [u,u,u,u,u,u,u,u ; s,s,s,s,s,s,s,s]
		 ///  u - number of updates
		 ///  s - size of index
		 /// </pre>
		 /// For key format, see <seealso cref="KeyFormat.visitIndexStatistics(long, long, long)"/>
		 /// </summary>
		 public override void ReplaceIndexUpdateAndSize( long indexId, long updates, long size )
		 {
			  try
			  {
					_updater.apply( indexStatisticsKey( indexId ), new Write( updates, size ) );
			  }
			  catch ( IOException e )
			  {
					throw new UnderlyingStorageException( e );
			  }
		 }

		 /// <summary>
		 /// Value format:
		 /// <pre>
		 ///  0 1 2 3 4 5 6 7   8 9 A B C D E F
		 /// [u,u,u,u,u,u,u,u ; s,s,s,s,s,s,s,s]
		 ///  u - number of unique values
		 ///  s - size of index
		 /// </pre>
		 /// For key format, see <seealso cref="KeyFormat.visitIndexSample(long, long, long)"/>
		 /// </summary>
		 public override void ReplaceIndexSample( long indexId, long unique, long size )
		 {
			  try
			  {
					_updater.apply( indexSampleKey( indexId ), new Write( unique, size ) );
			  }
			  catch ( IOException e )
			  {
					throw new UnderlyingStorageException( e );
			  }
		 }

		 /// <summary>
		 /// For key format, see <seealso cref="KeyFormat.visitIndexStatistics(long, long, long)"/>
		 /// For value format, see <seealso cref="CountsUpdater.replaceIndexUpdateAndSize(long, long, long)"/>
		 /// </summary>
		 public override void IncrementIndexUpdates( long indexId, long delta )
		 {
			  try
			  {
					_updater.apply( indexStatisticsKey( indexId ), IncrementFirstBy( delta ) );
			  }
			  catch ( IOException e )
			  {
					throw new UnderlyingStorageException( e );
			  }
		 }

		 public override void Close()
		 {
			  _updater.close();
		 }

		 private class Write : ValueUpdate
		 {
			  internal readonly long First;
			  internal readonly long Second;

			  internal Write( long first, long second )
			  {
					this.First = first;
					this.Second = second;
			  }

			  public override void Update( WritableBuffer target )
			  {
					target.PutLong( 0, First );
					target.PutLong( 8, Second );
			  }
		 }

		 private static IncrementLong IncrementFirstBy( long delta )
		 {
			  return new IncrementLong( 0, delta );
		 }

		 private static IncrementLong IncrementSecondBy( long delta )
		 {
			  return new IncrementLong( 8, delta );
		 }

		 private class IncrementLong : ValueUpdate
		 {
			  internal readonly int Offset;
			  internal readonly long Delta;

			  internal IncrementLong( int offset, long delta )
			  {
					this.Offset = offset;
					this.Delta = delta;
			  }

			  public override void Update( WritableBuffer target )
			  {
					target.PutLong( Offset, target.GetLong( Offset ) + Delta );
			  }
		 }
	}

}