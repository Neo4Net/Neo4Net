using System.Collections.Generic;

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
namespace Neo4Net.Kernel.impl.index.labelscan
{

	using Neo4Net.Index.Internal.gbptree;
	using Neo4Net.Index.Internal.gbptree;
	using Neo4Net.Index.Internal.gbptree;
	using LabelScanWriter = Neo4Net.Kernel.api.labelscan.LabelScanWriter;
	using NodeLabelUpdate = Neo4Net.Kernel.api.labelscan.NodeLabelUpdate;
	using LabelScanReader = Neo4Net.Storageengine.Api.schema.LabelScanReader;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Long.min;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.toIntExact;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.index.labelscan.LabelScanValue.RANGE_SIZE;

	/// <summary>
	/// <seealso cref="LabelScanWriter"/> for <seealso cref="NativeLabelScanStore"/>, or rather an <seealso cref="Writer"/> for its
	/// internal <seealso cref="GBPTree"/>.
	/// <para>
	/// <seealso cref="write(NodeLabelUpdate) updates"/> are queued up to a maximum batch size and, for performance,
	/// applied in sorted order (by label and node id) when reaches batch size or on <seealso cref="close()"/>.
	/// </para>
	/// <para>
	/// Updates aren't visible to <seealso cref="LabelScanReader readers"/> immediately, rather when queue happens to be applied.
	/// </para>
	/// <para>
	/// Incoming <seealso cref="NodeLabelUpdate updates"/> are actually modified from representing physical before/after
	/// state to represent logical to-add/to-remove state. These changes are done directly inside the provided
	/// <seealso cref="NodeLabelUpdate.getLabelsAfter()"/> and <seealso cref="NodeLabelUpdate.getLabelsBefore()"/> arrays,
	/// relying on the fact that those arrays are returned in its essential form, instead of copies.
	/// This conversion is done like so mostly to reduce garbage.
	/// 
	/// </para>
	/// </summary>
	/// <seealso cref= PhysicalToLogicalLabelChanges </seealso>
	internal class NativeLabelScanWriter : LabelScanWriter
	{
		 /// <summary>
		 /// <seealso cref="System.Collections.IComparer"/> for sorting the node id ranges, used in batches to apply updates in sorted order.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
		 private static readonly IComparer<NodeLabelUpdate> _updateSorter = System.Collections.IComparer.comparingLong( NodeLabelUpdate::getNodeId );

		 /// <summary>
		 /// <seealso cref="ValueMerger"/> used for adding label->node mappings, see <seealso cref="LabelScanValue.add(LabelScanValue)"/>.
		 /// </summary>
		 private readonly ValueMerger<LabelScanKey, LabelScanValue> _addMerger;

		 /// <summary>
		 /// <seealso cref="ValueMerger"/> used for removing label->node mappings, see <seealso cref="LabelScanValue.remove(LabelScanValue)"/>.
		 /// </summary>
		 private readonly ValueMerger<LabelScanKey, LabelScanValue> _removeMerger;

		 private readonly WriteMonitor _monitor;

		 /// <summary>
		 /// <seealso cref="Writer"/> acquired when acquiring this <seealso cref="NativeLabelScanWriter"/>,
		 /// acquired from <seealso cref="GBPTree.writer()"/>.
		 /// </summary>
		 private Writer<LabelScanKey, LabelScanValue> _writer;

		 /// <summary>
		 /// Instance of <seealso cref="LabelScanKey"/> acting as place to read keys into and also to set for each applied update.
		 /// </summary>
		 private readonly LabelScanKey _key = new LabelScanKey();

		 /// <summary>
		 /// Instance of <seealso cref="LabelScanValue"/> acting as place to read values into and also to update
		 /// for each applied update.
		 /// </summary>
		 private readonly LabelScanValue _value = new LabelScanValue();

		 /// <summary>
		 /// Batch currently building up as <seealso cref="write(NodeLabelUpdate) updates"/> come in. Cursor for where
		 /// to place new updates is <seealso cref="pendingUpdatesCursor"/>. Length of this queue is decided in constructor
		 /// and defines the maximum batch size.
		 /// </summary>
		 private readonly NodeLabelUpdate[] _pendingUpdates;

		 /// <summary>
		 /// Cursor into <seealso cref="pendingUpdates"/>, where to place new <seealso cref="write(NodeLabelUpdate) updates"/>.
		 /// When full the batch is applied and this cursor reset to {@code 0}.
		 /// </summary>
		 private int _pendingUpdatesCursor;

		 /// <summary>
		 /// There are two levels of batching, one for <seealso cref="NodeLabelUpdate updates"/> and one when applying.
		 /// This variable helps keeping track of the second level where updates to the actual <seealso cref="GBPTree"/>
		 /// are batched per node id range, i.e. to add several labelId->nodeId mappings falling into the same
		 /// range, all of those updates are made into one <seealso cref="LabelScanValue"/> and then issues as one update
		 /// to the tree. There are additions and removals, this variable keeps track of which.
		 /// </summary>
		 private bool _addition;

		 /// <summary>
		 /// When applying <seealso cref="NodeLabelUpdate updates"/> (when batch full or in <seealso cref="close()"/>), updates are
		 /// applied labelId by labelId. All updates are scanned through multiple times, with one label in mind at a time.
		 /// For each round the current round tries to figure out which is the closest higher labelId to apply
		 /// in the next round. This variable keeps track of that next labelId.
		 /// </summary>
		 private long _lowestLabelId;

		 internal interface WriteMonitor
		 {
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//			  default void range(long range, int labelId)
	//		  {
	//		  }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//			  default void prepareAdd(long txId, int offset)
	//		  {
	//		  }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//			  default void prepareRemove(long txId, int offset)
	//		  {
	//		  }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//			  default void mergeAdd(LabelScanValue existingValue, LabelScanValue newValue)
	//		  {
	//		  }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//			  default void mergeRemove(LabelScanValue existingValue, LabelScanValue newValue)
	//		  {
	//		  }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//			  default void flushPendingUpdates()
	//		  {
	//		  }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//			  default void writeSessionEnded()
	//		  {
	//		  }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//			  default void force()
	//		  {
	//		  }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//			  default void close()
	//		  {
	//		  }
		 }

		 internal static WriteMonitor EMPTY = new WriteMonitorAnonymousInnerClass();

		 private class WriteMonitorAnonymousInnerClass : WriteMonitor
		 {
		 }

		 internal NativeLabelScanWriter( int batchSize, WriteMonitor monitor )
		 {
			  this._pendingUpdates = new NodeLabelUpdate[batchSize];
			  this._addMerger = ( existingKey, newKey, existingValue, newValue ) =>
			  {
				monitor.MergeAdd( existingValue, newValue );
				return existingValue.add( newValue );
			  };
			  this._removeMerger = ( existingKey, newKey, existingValue, newValue ) =>
			  {
				monitor.MergeRemove( existingValue, newValue );
				return existingValue.remove( newValue );
			  };
			  this._monitor = monitor;
		 }

		 internal virtual NativeLabelScanWriter Initialize( Writer<LabelScanKey, LabelScanValue> writer )
		 {
			  this._writer = writer;
			  this._pendingUpdatesCursor = 0;
			  this._addition = false;
			  this._lowestLabelId = long.MaxValue;
			  return this;
		 }

		 /// <summary>
		 /// Queues a <seealso cref="NodeLabelUpdate"/> to this writer for applying when batch gets full,
		 /// or when <seealso cref="close() closing"/>.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void write(org.Neo4Net.kernel.api.labelscan.NodeLabelUpdate update) throws java.io.IOException
		 public override void Write( NodeLabelUpdate update )
		 {
			  if ( _pendingUpdatesCursor == _pendingUpdates.Length )
			  {
					FlushPendingChanges();
			  }

			  _pendingUpdates[_pendingUpdatesCursor++] = update;
			  PhysicalToLogicalLabelChanges.ConvertToAdditionsAndRemovals( update );
			  CheckNextLabelId( update.LabelsBefore );
			  CheckNextLabelId( update.LabelsAfter );
		 }

		 private void CheckNextLabelId( long[] labels )
		 {
			  if ( labels.Length > 0 && labels[0] != -1 )
			  {
					_lowestLabelId = min( _lowestLabelId, labels[0] );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void flushPendingChanges() throws java.io.IOException
		 private void FlushPendingChanges()
		 {
			  Arrays.sort( _pendingUpdates, 0, _pendingUpdatesCursor, _updateSorter );
			  _monitor.flushPendingUpdates();
			  long currentLabelId = _lowestLabelId;
			  _value.clear();
			  _key.clear();
			  while ( currentLabelId != long.MaxValue )
			  {
					long nextLabelId = long.MaxValue;
					for ( int i = 0; i < _pendingUpdatesCursor; i++ )
					{
						 NodeLabelUpdate update = _pendingUpdates[i];
						 long nodeId = update.NodeId;
						 nextLabelId = ExtractChange( update.LabelsAfter, currentLabelId, nodeId, nextLabelId, true, update.TxId );
						 nextLabelId = ExtractChange( update.LabelsBefore, currentLabelId, nodeId, nextLabelId, false, update.TxId );
					}
					currentLabelId = nextLabelId;
			  }
			  FlushPendingRange();
			  _pendingUpdatesCursor = 0;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private long extractChange(long[] labels, long currentLabelId, long nodeId, long nextLabelId, boolean addition, long txId) throws java.io.IOException
		 private long ExtractChange( long[] labels, long currentLabelId, long nodeId, long nextLabelId, bool addition, long txId )
		 {
			  long foundNextLabelId = nextLabelId;
			  for ( int li = 0; li < labels.Length; li++ )
			  {
					long labelId = labels[li];
					if ( labelId == -1 )
					{
						 break;
					}

					// Have this check here so that we can pick up the next labelId in our change set
					if ( labelId == currentLabelId )
					{
						 Change( currentLabelId, nodeId, addition, txId );

						 // We can do a little shorter check for next labelId here straight away,
						 // we just check the next if it's less than what we currently think is next labelId
						 // and then break right after
						 if ( li + 1 < labels.Length && labels[li + 1] != -1 )
						 {
							  long nextLabelCandidate = labels[li + 1];
							  if ( nextLabelCandidate < currentLabelId )
							  {
									throw new System.ArgumentException( "The node label update contained unsorted label ids " + Arrays.ToString( labels ) );
							  }
							  if ( nextLabelCandidate > currentLabelId )
							  {
									foundNextLabelId = min( foundNextLabelId, nextLabelCandidate );
							  }
						 }
						 break;
					}
					else if ( labelId > currentLabelId )
					{
						 foundNextLabelId = min( foundNextLabelId, labelId );
					}
			  }
			  return foundNextLabelId;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void change(long currentLabelId, long nodeId, boolean add, long txId) throws java.io.IOException
		 private void Change( long currentLabelId, long nodeId, bool add, long txId )
		 {
			  int labelId = toIntExact( currentLabelId );
			  long idRange = RangeOf( nodeId );
			  if ( labelId != _key.labelId || idRange != _key.idRange || _addition != add )
			  {
					FlushPendingRange();

					// Set key to current and reset value
					_key.labelId = labelId;
					_key.idRange = idRange;
					_addition = add;
					_monitor.range( idRange, labelId );
			  }

			  int offset = toIntExact( nodeId % RANGE_SIZE );
			  _value.set( offset );
			  if ( _addition )
			  {
					_monitor.prepareAdd( txId, offset );
			  }
			  else
			  {
					_monitor.prepareRemove( txId, offset );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void flushPendingRange() throws java.io.IOException
		 private void FlushPendingRange()
		 {
			  if ( _value.bits != 0 )
			  {
					// There are changes in the current range, flush them
					_writer.merge( _key, _value, _addition ? _addMerger : _removeMerger );
					// TODO: after a remove we could check if the tree value is empty and if so remove it from the index
					// hmm, or perhaps that could be a feature of ValueAmender?
					_value.clear();
			  }
		 }

		 internal static long RangeOf( long nodeId )
		 {
			  return nodeId / RANGE_SIZE;
		 }

		 /// <summary>
		 /// Applies <seealso cref="write(NodeLabelUpdate) queued updates"/> which has not not yet been applied.
		 /// After this call no more <seealso cref="write(NodeLabelUpdate)"/> can be applied.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  try
			  {
					FlushPendingChanges();
					_monitor.writeSessionEnded();
			  }
			  finally
			  {
					_writer.Dispose();
			  }
		 }
	}

}