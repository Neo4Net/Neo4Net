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
namespace Neo4Net.Kernel.impl.transaction.state
{
	using Neo4Net.Helpers.Collection;
	using Tracker = Neo4Net.Kernel.impl.transaction.state.RelationshipCreatorTest.Tracker;

	public class TrackingRecordAccess<RECORD, ADDITIONAL> : RecordAccess<RECORD, ADDITIONAL>
	{
		 private readonly RecordAccess<RECORD, ADDITIONAL> @delegate;
		 private readonly Tracker _tracker;

		 public TrackingRecordAccess( RecordAccess<RECORD, ADDITIONAL> @delegate, Tracker tracker )
		 {
			  this.@delegate = @delegate;
			  this._tracker = tracker;
		 }

		 public override RecordAccess_RecordProxy<RECORD, ADDITIONAL> GetOrLoad( long key, ADDITIONAL additionalData )
		 {
			  return new TrackingRecordProxy<RECORD, ADDITIONAL>( @delegate.GetOrLoad( key, additionalData ), false, _tracker );
		 }

		 public override RecordAccess_RecordProxy<RECORD, ADDITIONAL> Create( long key, ADDITIONAL additionalData )
		 {
			  return new TrackingRecordProxy<RECORD, ADDITIONAL>( @delegate.Create( key, additionalData ), true, _tracker );
		 }

		 public override void Close()
		 {
			  @delegate.Close();
		 }

		 public override RecordAccess_RecordProxy<RECORD, ADDITIONAL> GetIfLoaded( long key )
		 {
			  RecordAccess_RecordProxy<RECORD, ADDITIONAL> actual = @delegate.GetIfLoaded( key );
			  return actual == null ? null : new TrackingRecordProxy<RECORD, ADDITIONAL>( actual, false, _tracker );
		 }

		 public override void SetTo( long key, RECORD newRecord, ADDITIONAL additionalData )
		 {
			  @delegate.SetTo( key, newRecord, additionalData );
		 }

		 public override RecordAccess_RecordProxy<RECORD, ADDITIONAL> SetRecord( long key, RECORD record, ADDITIONAL additionalData )
		 {
			  return @delegate.SetRecord( key, record, additionalData );
		 }

		 public override int ChangeSize()
		 {
			  return @delegate.ChangeSize();
		 }

		 public override IEnumerable<RecordAccess_RecordProxy<RECORD, ADDITIONAL>> Changes()
		 {
			  return new IterableWrapperAnonymousInnerClass( this, @delegate.Changes() );
		 }

		 private class IterableWrapperAnonymousInnerClass : IterableWrapper<RecordAccess_RecordProxy<RECORD, ADDITIONAL>, RecordAccess_RecordProxy<RECORD, ADDITIONAL>>
		 {
			 private readonly TrackingRecordAccess<RECORD, ADDITIONAL> _outerInstance;

			 public IterableWrapperAnonymousInnerClass( TrackingRecordAccess<RECORD, ADDITIONAL> outerInstance, IEnumerable<RecordAccess_RecordProxy<RECORD, ADDITIONAL>> changes ) : base( changes )
			 {
				 this.outerInstance = outerInstance;
			 }

			 protected internal override RecordAccess_RecordProxy<RECORD, ADDITIONAL> underlyingObjectToObject( RecordAccess_RecordProxy<RECORD, ADDITIONAL> actual )
			 {
				  return new TrackingRecordProxy<RECORD, ADDITIONAL>( actual, false, _outerInstance.tracker );
			 }
		 }
	}

}