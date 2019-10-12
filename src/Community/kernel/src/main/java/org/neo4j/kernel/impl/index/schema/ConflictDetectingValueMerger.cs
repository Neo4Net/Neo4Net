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
namespace Neo4Net.Kernel.Impl.Index.Schema
{
	using Neo4Net.Index.@internal.gbptree;
	using Neo4Net.Index.@internal.gbptree;
	using Neo4Net.Index.@internal.gbptree;
	using IndexEntryConflictException = Neo4Net.Kernel.Api.Exceptions.index.IndexEntryConflictException;

	/// <summary>
	/// <seealso cref="ValueMerger"/> which will merely detect conflict, not change any value if conflict, i.e. if the
	/// key already exists. After this merge has been used in a call to <seealso cref="Writer.merge(object, object, ValueMerger)"/>
	/// <seealso cref="checkConflict(REPORT_TYPE)"/> can be called to check whether or not that call conflicted with
	/// an existing key. A call to <seealso cref="checkConflict(REPORT_TYPE)"/> will also initialize the conflict flag.
	/// </summary>
	/// @param <VALUE> type of values being merged. </param>
	internal abstract class ConflictDetectingValueMerger<KEY, VALUE, REPORT_TYPE> : ValueMerger<KEY, VALUE> where KEY : NativeIndexKey<KEY> where VALUE : NativeIndexValue
	{
		 private readonly bool _compareEntityIds;

		 private bool _conflict;
		 private long _existingNodeId;
		 private long _addedNodeId;

		 internal ConflictDetectingValueMerger( bool compareEntityIds )
		 {
			  this._compareEntityIds = compareEntityIds;
		 }

		 public override VALUE Merge( KEY existingKey, KEY newKey, VALUE existingValue, VALUE newValue )
		 {
			  if ( existingKey.EntityId != newKey.EntityId )
			  {
					_conflict = true;
					_existingNodeId = existingKey.EntityId;
					_addedNodeId = newKey.EntityId;
			  }
			  return default( VALUE );
		 }

		 /// <summary>
		 /// To be called for a populated key that is about to be sent off to a <seealso cref="Writer"/>.
		 /// <seealso cref="GBPTree"/>'s ability to check for conflicts while applying updates is an opportunity,
		 /// but also complicates some scenarios. This is why the strictness can be tweaked like this.
		 /// </summary>
		 /// <param name="key"> key to let know about conflict detection strictness. </param>
		 internal virtual void ControlConflictDetection( KEY key )
		 {
			  key.CompareId = _compareEntityIds;
		 }

		 internal virtual bool WasConflicting()
		 {
			  return _conflict;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void reportConflict(REPORT_TYPE toReport) throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
		 internal virtual void ReportConflict( REPORT_TYPE toReport )
		 {
			  _conflict = false;
			  DoReportConflict( _existingNodeId, _addedNodeId, toReport );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void checkConflict(REPORT_TYPE toReport) throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
		 internal virtual void CheckConflict( REPORT_TYPE toReport )
		 {
			  if ( WasConflicting() )
			  {
					ReportConflict( toReport );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: abstract void doReportConflict(long existingNodeId, long addedNodeId, REPORT_TYPE toReport) throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException;
		 internal abstract void DoReportConflict( long existingNodeId, long addedNodeId, REPORT_TYPE toReport );
	}

}