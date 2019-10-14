using System.Diagnostics;

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
	using Neo4Net.Kernel.impl.store;
	using RelationshipGroupRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipGroupRecord;
	using BatchSender = Neo4Net.@unsafe.Impl.Batchimport.staging.BatchSender;
	using Neo4Net.@unsafe.Impl.Batchimport.staging;
	using StageControl = Neo4Net.@unsafe.Impl.Batchimport.staging.StageControl;

	/// <summary>
	/// Takes cached <seealso cref="RelationshipGroupRecord relationship groups"/> and sets real ids and
	/// <seealso cref="RelationshipGroupRecord.getNext() next pointers"/>, making them ready for writing to store.
	/// </summary>
	public class EncodeGroupsStep : ProcessorStep<RelationshipGroupRecord[]>
	{
		 private long _nextId = -1;
		 private readonly RecordStore<RelationshipGroupRecord> _store;

		 public EncodeGroupsStep( StageControl control, Configuration config, RecordStore<RelationshipGroupRecord> store ) : base( control, "ENCODE", config, 1 )
		 {
			  this._store = store;
		 }

		 protected internal override void Process( RelationshipGroupRecord[] batch, BatchSender sender )
		 {
			  int groupStartIndex = 0;
			  for ( int i = 0; i < batch.Length; i++ )
			  {
					RelationshipGroupRecord group = batch[i];

					// The iterator over the groups will not produce real next pointers, they are instead
					// a count meaning how many groups come after it. This encoder will set the real group ids.
					long count = group.Next;
					bool lastInChain = count == 0;

					group.Id = _nextId == -1 ? _nextId = _store.nextId() : _nextId;
					if ( !lastInChain )
					{
						 group.Next = _nextId = _store.nextId();
					}
					else
					{
						 group.Next = _nextId = -1;

						 // OK so this group is the last in this chain, which means all the groups in this chain
						 // are now fully populated. We can now prepare these groups so that their potential
						 // secondary units ends up very close by.
						 for ( int j = groupStartIndex; j <= i; j++ )
						 {
							  _store.prepareForCommit( batch[j] );
						 }

						 groupStartIndex = i + 1;
					}
			  }
			  Debug.Assert( groupStartIndex == batch.Length );

			  sender.Send( batch );
		 }
	}

}