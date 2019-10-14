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
namespace Neo4Net.@unsafe.Impl.Batchimport
{
	using LongIterator = org.eclipse.collections.api.iterator.LongIterator;

	using Neo4Net.Kernel.impl.store;
	using AbstractBaseRecord = Neo4Net.Kernel.Impl.Store.Records.AbstractBaseRecord;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Long.max;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Long.min;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.collection.PrimitiveLongCollections.range;

	/// <summary>
	/// Returns ids either backwards or forwards. In both directions ids are returned batch-wise, sequentially forwards
	/// in each batch. This means for example that in a range of ]100-0] (i.e. from 100 (exclusive) to 0 (inclusive)
	/// going backwards with a batch size of 40 then ids are returned like this: 80-99, 40-79, 0-39.
	/// This to get higher mechanical sympathy.
	/// </summary>
	public interface RecordIdIterator
	{
		 /// <returns> next batch of ids as <seealso cref="LongIterator"/>, or {@code null} if there are no more ids to return. </returns>
		 LongIterator NextBatch();

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static RecordIdIterator backwards(long lowIncluded, long highExcluded, Configuration config)
	//	 {
	//		  return new Backwards(lowIncluded, highExcluded, config);
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static RecordIdIterator forwards(long lowIncluded, long highExcluded, Configuration config)
	//	 {
	//		  return new Forwards(lowIncluded, highExcluded, config);
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static RecordIdIterator allIn(org.neo4j.kernel.impl.store.RecordStore<JavaToDotNetGenericWildcard extends org.neo4j.kernel.impl.store.record.AbstractBaseRecord> store, Configuration config)
	//	 {
	//		  return forwards(store.getNumberOfReservedLowIds(), store.getHighId(), config);
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static RecordIdIterator allInReversed(org.neo4j.kernel.impl.store.RecordStore<JavaToDotNetGenericWildcard extends org.neo4j.kernel.impl.store.record.AbstractBaseRecord> store, Configuration config)
	//	 {
	//		  return backwards(store.getNumberOfReservedLowIds(), store.getHighId(), config);
	//	 }
	}

	 public class RecordIdIterator_Forwards : RecordIdIterator
	 {
		  internal readonly long LowIncluded;
		  internal readonly long HighExcluded;
		  internal readonly int BatchSize;
		  internal long StartId;

		  public RecordIdIterator_Forwards( long lowIncluded, long highExcluded, Configuration config )
		  {
				this.LowIncluded = lowIncluded;
				this.HighExcluded = highExcluded;
				this.BatchSize = config.BatchSize();
				this.StartId = lowIncluded;
		  }

		  public override LongIterator NextBatch()
		  {
				if ( StartId >= HighExcluded )
				{
					 return null;
				}

				long endId = min( HighExcluded, FindRoofId( StartId ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.iterator.LongIterator result = range(startId, endId - 1);
				LongIterator result = range( StartId, endId - 1 );
				StartId = endId;
				return result;
		  }

		  internal virtual long FindRoofId( long floorId )
		  {
				int rest = ( int )( floorId % BatchSize );
				return max( rest == 0 ? floorId + BatchSize : floorId + BatchSize - rest, LowIncluded );
		  }

		  public override string ToString()
		  {
				return "[" + LowIncluded + "-" + HighExcluded + "[";
		  }
	 }

	 public class RecordIdIterator_Backwards : RecordIdIterator
	 {
		  internal readonly long LowIncluded;
		  internal readonly long HighExcluded;
		  internal readonly int BatchSize;
		  internal long EndId;

		  public RecordIdIterator_Backwards( long lowIncluded, long highExcluded, Configuration config )
		  {
				this.LowIncluded = lowIncluded;
				this.HighExcluded = highExcluded;
				this.BatchSize = config.BatchSize();
				this.EndId = highExcluded;
		  }

		  public override LongIterator NextBatch()
		  {
				if ( EndId <= LowIncluded )
				{
					 return null;
				}

				long startId = FindFloorId( EndId );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.iterator.LongIterator result = range(startId, endId - 1);
				LongIterator result = range( startId, EndId - 1 );
				EndId = max( LowIncluded, startId );
				return result;
		  }

		  internal virtual long FindFloorId( long roofId )
		  {
				int rest = ( int )( roofId % BatchSize );
				return max( rest == 0 ? roofId - BatchSize : roofId - rest, LowIncluded );
		  }

		  public override string ToString()
		  {
				return "]" + HighExcluded + "-" + LowIncluded + "]";
		  }
	 }

}