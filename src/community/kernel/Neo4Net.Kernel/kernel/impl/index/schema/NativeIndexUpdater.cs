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
	using Neo4Net.Index.Internal.gbptree;
	using IOUtils = Neo4Net.Io.IOUtils;
	using IndexEntryConflictException = Neo4Net.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using Neo4Net.Kernel.Api.Index;
	using IndexUpdater = Neo4Net.Kernel.Api.Index.IndexUpdater;
	using Value = Neo4Net.Values.Storable.Value;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.NativeIndexKey.Inclusion.NEUTRAL;

	internal class NativeIndexUpdater<KEY, VALUE> : IndexUpdater where KEY : NativeIndexKey<KEY> where VALUE : NativeIndexValue
	{
		 private readonly KEY _treeKey;
		 private readonly VALUE _treeValue;
		 private readonly ConflictDetectingValueMerger<KEY, VALUE, Value[]> _conflictDetectingValueMerger = new ThrowingConflictDetector<KEY, VALUE, Value[]>( true );
		 private Writer<KEY, VALUE> _writer;

		 private bool _closed = true;

		 internal NativeIndexUpdater( KEY treeKey, VALUE treeValue )
		 {
			  this._treeKey = treeKey;
			  this._treeValue = treeValue;
		 }

		 internal virtual NativeIndexUpdater<KEY, VALUE> Initialize( Writer<KEY, VALUE> writer )
		 {
			  if ( !_closed )
			  {
					throw new System.InvalidOperationException( "Updater still open" );
			  }

			  this._writer = writer;
			  _closed = false;
			  return this;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void process(org.neo4j.kernel.api.index.IndexEntryUpdate<?> update) throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
		 public override void Process<T1>( IndexEntryUpdate<T1> update )
		 {
			  AssertOpen();
			  ProcessUpdate( _treeKey, _treeValue, update, _writer, _conflictDetectingValueMerger );
		 }

		 public override void Close()
		 {
			  _closed = true;
			  IOUtils.closeAllUnchecked( _writer );
		 }

		 private void AssertOpen()
		 {
			  if ( _closed )
			  {
					throw new System.InvalidOperationException( "Updater has been closed" );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: static <KEY extends NativeIndexKey<KEY>, VALUE extends NativeIndexValue> void processUpdate(KEY treeKey, VALUE treeValue, org.neo4j.kernel.api.index.IndexEntryUpdate<?> update, org.neo4j.index.internal.gbptree.Writer<KEY,VALUE> writer, ConflictDetectingValueMerger<KEY,VALUE,org.neo4j.values.storable.Value[]> conflictDetectingValueMerger) throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
		 internal static void ProcessUpdate<KEY, VALUE, T1>( KEY treeKey, VALUE treeValue, IndexEntryUpdate<T1> update, Writer<KEY, VALUE> writer, ConflictDetectingValueMerger<KEY, VALUE, Value[]> conflictDetectingValueMerger ) where KEY : NativeIndexKey<KEY> where VALUE : NativeIndexValue
		 {
			  switch ( update.UpdateMode() )
			  {
			  case ADDED:
					ProcessAdd( treeKey, treeValue, update, writer, conflictDetectingValueMerger );
					break;
			  case CHANGED:
					ProcessChange( treeKey, treeValue, update, writer, conflictDetectingValueMerger );
					break;
			  case REMOVED:
					ProcessRemove( treeKey, update, writer );
					break;
			  default:
					throw new System.ArgumentException();
			  }
		 }

		 private static void ProcessRemove<KEY, VALUE, T1>( KEY treeKey, IndexEntryUpdate<T1> update, Writer<KEY, VALUE> writer ) where KEY : NativeIndexKey<KEY> where VALUE : NativeIndexValue
		 {
			  // todo Do we need to verify that we actually removed something at all?
			  // todo Difference between online and recovery?
			  InitializeKeyFromUpdate( treeKey, update.EntityId, update.Values() );
			  writer.Remove( treeKey );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static <KEY extends NativeIndexKey<KEY>, VALUE extends NativeIndexValue> void processChange(KEY treeKey, VALUE treeValue, org.neo4j.kernel.api.index.IndexEntryUpdate<?> update, org.neo4j.index.internal.gbptree.Writer<KEY,VALUE> writer, ConflictDetectingValueMerger<KEY,VALUE,org.neo4j.values.storable.Value[]> conflictDetectingValueMerger) throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
		 private static void ProcessChange<KEY, VALUE, T1>( KEY treeKey, VALUE treeValue, IndexEntryUpdate<T1> update, Writer<KEY, VALUE> writer, ConflictDetectingValueMerger<KEY, VALUE, Value[]> conflictDetectingValueMerger ) where KEY : NativeIndexKey<KEY> where VALUE : NativeIndexValue
		 {
			  // Remove old entry
			  InitializeKeyFromUpdate( treeKey, update.EntityId, update.BeforeValues() );
			  writer.Remove( treeKey );
			  // Insert new entry
			  InitializeKeyFromUpdate( treeKey, update.EntityId, update.Values() );
			  treeValue.From( update.Values() );
			  conflictDetectingValueMerger.ControlConflictDetection( treeKey );
			  writer.Merge( treeKey, treeValue, conflictDetectingValueMerger );
			  conflictDetectingValueMerger.CheckConflict( update.Values() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static <KEY extends NativeIndexKey<KEY>, VALUE extends NativeIndexValue> void processAdd(KEY treeKey, VALUE treeValue, org.neo4j.kernel.api.index.IndexEntryUpdate<?> update, org.neo4j.index.internal.gbptree.Writer<KEY,VALUE> writer, ConflictDetectingValueMerger<KEY,VALUE,org.neo4j.values.storable.Value[]> conflictDetectingValueMerger) throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
		 private static void ProcessAdd<KEY, VALUE, T1>( KEY treeKey, VALUE treeValue, IndexEntryUpdate<T1> update, Writer<KEY, VALUE> writer, ConflictDetectingValueMerger<KEY, VALUE, Value[]> conflictDetectingValueMerger ) where KEY : NativeIndexKey<KEY> where VALUE : NativeIndexValue
		 {
			  InitializeKeyAndValueFromUpdate( treeKey, treeValue, update.EntityId, update.Values() );
			  conflictDetectingValueMerger.ControlConflictDetection( treeKey );
			  writer.Merge( treeKey, treeValue, conflictDetectingValueMerger );
			  conflictDetectingValueMerger.CheckConflict( update.Values() );
		 }

		 internal static void InitializeKeyAndValueFromUpdate<KEY, VALUE>( KEY treeKey, VALUE treeValue, long entityId, Value[] values ) where KEY : NativeIndexKey<KEY> where VALUE : NativeIndexValue
		 {
			  InitializeKeyFromUpdate( treeKey, entityId, values );
			  treeValue.From( values );
		 }

		 internal static void InitializeKeyFromUpdate<KEY>( KEY treeKey, long entityId, Value[] values ) where KEY : NativeIndexKey<KEY>
		 {
			  treeKey.initialize( entityId );
			  for ( int i = 0; i < values.Length; i++ )
			  {
					treeKey.initFromValue( i, values[i], NEUTRAL );
			  }
		 }
	}

}