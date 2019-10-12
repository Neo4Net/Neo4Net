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
	using MutableLongObjectMap = org.eclipse.collections.api.map.primitive.MutableLongObjectMap;
	using LongObjectHashMap = org.eclipse.collections.impl.map.mutable.primitive.LongObjectHashMap;

	using Iterables = Neo4Net.Helpers.Collection.Iterables;
	using IntCounter = Neo4Net.Kernel.impl.util.statistics.IntCounter;
	using LocalIntCounter = Neo4Net.Kernel.impl.util.statistics.LocalIntCounter;

	/// <summary>
	/// Manages changes to records in a transaction. Before/after state is supported as well as
	/// deciding when to make a record heavy and when to consider it changed for inclusion in the
	/// transaction as a command.
	/// </summary>
	/// @param <RECORD> type of record </param>
	/// @param <ADDITIONAL> additional payload </param>
	public class RecordChanges<RECORD, ADDITIONAL> : RecordAccess<RECORD, ADDITIONAL>
	{
		 private MutableLongObjectMap<RecordAccess_RecordProxy<RECORD, ADDITIONAL>> _recordChanges = new LongObjectHashMap<RecordAccess_RecordProxy<RECORD, ADDITIONAL>>();
		 private readonly RecordAccess_Loader<RECORD, ADDITIONAL> _loader;
		 private readonly IntCounter _changeCounter;

		 public RecordChanges( RecordAccess_Loader<RECORD, ADDITIONAL> loader, IntCounter globalCounter )
		 {
			  this._loader = loader;
			  this._changeCounter = new LocalIntCounter( globalCounter );
		 }

		 public override string ToString()
		 {
			  return "RecordChanges{" +
						"recordChanges=" + _recordChanges +
						'}';
		 }

		 public override RecordAccess_RecordProxy<RECORD, ADDITIONAL> GetIfLoaded( long key )
		 {
			  return _recordChanges.get( key );
		 }

		 public override RecordAccess_RecordProxy<RECORD, ADDITIONAL> GetOrLoad( long key, ADDITIONAL additionalData )
		 {
			  RecordAccess_RecordProxy<RECORD, ADDITIONAL> result = _recordChanges.get( key );
			  if ( result == null )
			  {
					RECORD record = _loader.load( key, additionalData );
					result = new RecordChange<RECORD, ADDITIONAL>( _recordChanges, _changeCounter, key, record, _loader, false, additionalData );
			  }
			  return result;
		 }

		 public override void SetTo( long key, RECORD newRecord, ADDITIONAL additionalData )
		 {
			  SetRecord( key, newRecord, additionalData );
		 }

		 public override RecordAccess_RecordProxy<RECORD, ADDITIONAL> SetRecord( long key, RECORD record, ADDITIONAL additionalData )
		 {
			  RecordChange<RECORD, ADDITIONAL> recordChange = new RecordChange<RECORD, ADDITIONAL>( _recordChanges, _changeCounter, key, record, _loader, false, additionalData );
			  _recordChanges.put( key, recordChange );
			  return recordChange;
		 }

		 public override int ChangeSize()
		 {
			  return _changeCounter.value();
		 }

		 public override void Close()
		 {
			  if ( _recordChanges.size() <= 32 )
			  {
					_recordChanges.clear();
			  }
			  else
			  {
					// Let's not allow the internal maps to grow too big over time.
					_recordChanges = new LongObjectHashMap<RecordAccess_RecordProxy<RECORD, ADDITIONAL>>();
			  }
			  _changeCounter.clear();
		 }

		 public override RecordAccess_RecordProxy<RECORD, ADDITIONAL> Create( long key, ADDITIONAL additionalData )
		 {
			  if ( _recordChanges.containsKey( key ) )
			  {
					throw new System.InvalidOperationException( key + " already exists" );
			  }

			  RECORD record = _loader.newUnused( key, additionalData );
			  RecordChange<RECORD, ADDITIONAL> change = new RecordChange<RECORD, ADDITIONAL>( _recordChanges, _changeCounter, key, record, _loader, true, additionalData );
			  _recordChanges.put( key, change );
			  return change;
		 }

		 public override IEnumerable<RecordAccess_RecordProxy<RECORD, ADDITIONAL>> Changes()
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  return Iterables.filter( RecordAccess_RecordProxy::isChanged, _recordChanges.values() );
		 }

		 public class RecordChange<RECORD, ADDITIONAL> : RecordAccess_RecordProxy<RECORD, ADDITIONAL>
		 {
			  internal readonly MutableLongObjectMap<RecordAccess_RecordProxy<RECORD, ADDITIONAL>> AllChanges;
			  internal readonly IntCounter ChangeCounter;
			  internal readonly RecordAccess_Loader<RECORD, ADDITIONAL> Loader;

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly ADDITIONAL AdditionalDataConflict;
			  internal readonly RECORD Record;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly bool CreatedConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly long KeyConflict;

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal RECORD BeforeConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal bool ChangedConflict;

			  public RecordChange( MutableLongObjectMap<RecordAccess_RecordProxy<RECORD, ADDITIONAL>> allChanges, IntCounter changeCounter, long key, RECORD record, RecordAccess_Loader<RECORD, ADDITIONAL> loader, bool created, ADDITIONAL additionalData )
			  {
					this.AllChanges = allChanges;
					this.ChangeCounter = changeCounter;
					this.KeyConflict = key;
					this.Record = record;
					this.Loader = loader;
					this.CreatedConflict = created;
					this.AdditionalDataConflict = additionalData;
			  }

			  public override string ToString()
			  {
					return "RecordChange{" + "record=" + Record + "key=" + KeyConflict + "created=" + CreatedConflict + '}';
			  }

			  public virtual long Key
			  {
				  get
				  {
						return KeyConflict;
				  }
			  }

			  public override RECORD ForChangingLinkage()
			  {
					return PrepareForChange();
			  }

			  public override RECORD ForChangingData()
			  {
					EnsureHeavy();
					return PrepareForChange();
			  }

			  internal virtual RECORD PrepareForChange()
			  {
					EnsureHasBeforeRecordImage();
					if ( !this.ChangedConflict )
					{
						 RecordAccess_RecordProxy<RECORD, ADDITIONAL> previous = this.AllChanges.put( KeyConflict, this );

						 if ( previous == null || !previous.Changed )
						 {
							  ChangeCounter.increment();
						 }

						 this.ChangedConflict = true;
					}
					return this.Record;
			  }

			  internal virtual void EnsureHeavy()
			  {
					if ( !CreatedConflict )
					{
						 Loader.ensureHeavy( Record );
						 if ( BeforeConflict != default( RECORD ) )
						 {
							  Loader.ensureHeavy( BeforeConflict );
						 }
					}
			  }

			  public override RECORD ForReadingLinkage()
			  {
					return this.Record;
			  }

			  public override RECORD ForReadingData()
			  {
					EnsureHeavy();
					return this.Record;
			  }

			  public virtual bool Changed
			  {
				  get
				  {
						return this.ChangedConflict;
				  }
			  }

			  public virtual RECORD Before
			  {
				  get
				  {
						EnsureHasBeforeRecordImage();
						return BeforeConflict;
				  }
			  }

			  internal virtual void EnsureHasBeforeRecordImage()
			  {
					if ( BeforeConflict == default( RECORD ) )
					{
						 this.BeforeConflict = Loader.clone( Record );
					}
			  }

			  public virtual bool Created
			  {
				  get
				  {
						return CreatedConflict;
				  }
			  }

			  public virtual ADDITIONAL AdditionalData
			  {
				  get
				  {
						return AdditionalDataConflict;
				  }
			  }
		 }
	}

}