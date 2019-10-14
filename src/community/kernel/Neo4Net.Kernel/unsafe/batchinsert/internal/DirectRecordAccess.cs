using System.Collections.Generic;
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
namespace Neo4Net.@unsafe.Batchinsert.Internal
{

	using Neo4Net.Helpers.Collections;
	using Neo4Net.Kernel.impl.store;
	using AbstractBaseRecord = Neo4Net.Kernel.Impl.Store.Records.AbstractBaseRecord;
	using Neo4Net.Kernel.impl.transaction.state;
	using IntCounter = Neo4Net.Kernel.impl.util.statistics.IntCounter;

	/// <summary>
	/// Provides direct access to records in a store. Changes are batched up and written whenever <seealso cref="commit()"/>
	/// is called, or <seealso cref="close()"/> for that matter.
	/// </summary>
	public class DirectRecordAccess<RECORD, ADDITIONAL> : RecordAccess<RECORD, ADDITIONAL> where RECORD : Neo4Net.Kernel.Impl.Store.Records.AbstractBaseRecord
	{
		 private readonly RecordStore<RECORD> _store;
		 private readonly Neo4Net.Kernel.impl.transaction.state.RecordAccess_Loader<RECORD, ADDITIONAL> _loader;
		 private readonly IDictionary<long, DirectRecordProxy> _batch = new Dictionary<long, DirectRecordProxy>();

		 private readonly IntCounter _changeCounter = new IntCounter();

		 public DirectRecordAccess( RecordStore<RECORD> store, Neo4Net.Kernel.impl.transaction.state.RecordAccess_Loader<RECORD, ADDITIONAL> loader )
		 {
			  this._store = store;
			  this._loader = loader;
		 }

		 public override Neo4Net.Kernel.impl.transaction.state.RecordAccess_RecordProxy<RECORD, ADDITIONAL> GetOrLoad( long key, ADDITIONAL additionalData )
		 {
			  DirectRecordProxy loaded = _batch[key];
			  if ( loaded != null )
			  {
					return loaded;
			  }
			  return Proxy( key, _loader.load( key, additionalData ), additionalData, false );
		 }

		 private Neo4Net.Kernel.impl.transaction.state.RecordAccess_RecordProxy<RECORD, ADDITIONAL> PutInBatch( long key, DirectRecordProxy proxy )
		 {
			  DirectRecordProxy previous = _batch[key] = proxy;
			  Debug.Assert( previous == null );
			  return proxy;
		 }

		 public override Neo4Net.Kernel.impl.transaction.state.RecordAccess_RecordProxy<RECORD, ADDITIONAL> Create( long key, ADDITIONAL additionalData )
		 {
			  return Proxy( key, _loader.newUnused( key, additionalData ), additionalData, true );
		 }

		 public override Neo4Net.Kernel.impl.transaction.state.RecordAccess_RecordProxy<RECORD, ADDITIONAL> GetIfLoaded( long key )
		 {
			  return _batch[key];
		 }

		 public override void SetTo( long key, RECORD newRecord, ADDITIONAL additionalData )
		 {
			  throw new System.NotSupportedException( "Not supported" );
		 }

		 public override Neo4Net.Kernel.impl.transaction.state.RecordAccess_RecordProxy<RECORD, ADDITIONAL> SetRecord( long key, RECORD record, ADDITIONAL additionalData )
		 {
			  throw new System.NotSupportedException( "Not supported" );
		 }

		 public override int ChangeSize()
		 {
			  return _changeCounter.value();
		 }

		 public override IEnumerable<Neo4Net.Kernel.impl.transaction.state.RecordAccess_RecordProxy<RECORD, ADDITIONAL>> Changes()
		 {
			  return new IterableWrapperAnonymousInnerClass( this, _batch.Values );
		 }

		 private class IterableWrapperAnonymousInnerClass : IterableWrapper<Neo4Net.Kernel.impl.transaction.state.RecordAccess_RecordProxy<RECORD, ADDITIONAL>, DirectRecordProxy>
		 {
			 private readonly DirectRecordAccess<RECORD, ADDITIONAL> _outerInstance;

			 public IterableWrapperAnonymousInnerClass( DirectRecordAccess<RECORD, ADDITIONAL> outerInstance, UnknownType values ) : base( values )
			 {
				 this.outerInstance = outerInstance;
			 }

			 protected internal override Neo4Net.Kernel.impl.transaction.state.RecordAccess_RecordProxy<RECORD, ADDITIONAL> underlyingObjectToObject( DirectRecordProxy @object )
			 {
				  return @object;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private DirectRecordProxy proxy(final long key, final RECORD record, final ADDITIONAL additionalData, boolean created)
		 private DirectRecordProxy Proxy( long key, RECORD record, ADDITIONAL additionalData, bool created )
		 {
			  return new DirectRecordProxy( this, key, record, additionalData, created );
		 }

		 private class DirectRecordProxy : Neo4Net.Kernel.impl.transaction.state.RecordAccess_RecordProxy<RECORD, ADDITIONAL>
		 {
			 private readonly DirectRecordAccess<RECORD, ADDITIONAL> _outerInstance;

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly long KeyConflict;
			  internal readonly RECORD Record;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly ADDITIONAL AdditionalDataConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal bool ChangedConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly bool CreatedConflict;

			  internal DirectRecordProxy( DirectRecordAccess<RECORD, ADDITIONAL> outerInstance, long key, RECORD record, ADDITIONAL additionalData, bool created )
			  {
				  this._outerInstance = outerInstance;
					this.KeyConflict = key;
					this.Record = record;
					this.AdditionalDataConflict = additionalData;
					if ( created )
					{
						 PrepareChange();
					}
					this.CreatedConflict = created;
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
					PrepareChange();
					return Record;
			  }

			  internal virtual void PrepareChange()
			  {
					if ( !ChangedConflict )
					{
						 ChangedConflict = true;
						 outerInstance.putInBatch( KeyConflict, this );
						 outerInstance.changeCounter.Increment();
					}
			  }

			  public override RECORD ForChangingData()
			  {
					outerInstance.loader.EnsureHeavy( Record );
					PrepareChange();
					return Record;
			  }

			  public override RECORD ForReadingLinkage()
			  {
					return Record;
			  }

			  public override RECORD ForReadingData()
			  {
					outerInstance.loader.EnsureHeavy( Record );
					return Record;
			  }

			  public virtual ADDITIONAL AdditionalData
			  {
				  get
				  {
						return AdditionalDataConflict;
				  }
			  }

			  public virtual RECORD Before
			  {
				  get
				  {
						return outerInstance.loader.Load( KeyConflict, AdditionalDataConflict );
				  }
			  }

			  public override string ToString()
			  {
					return Record.ToString();
			  }

			  public virtual void Store()
			  {
					if ( ChangedConflict )
					{
						 outerInstance.store.UpdateRecord( Record );
					}
			  }

			  public virtual bool Changed
			  {
				  get
				  {
						return ChangedConflict;
				  }
			  }

			  public virtual bool Created
			  {
				  get
				  {
						return CreatedConflict;
				  }
			  }
		 }

		 public override void Close()
		 {
			  Commit();
		 }

		 public virtual void Commit()
		 {
			  if ( _changeCounter.value() == 0 )
			  {
					return;
			  }

			  IList<DirectRecordProxy> directRecordProxies = new List<DirectRecordProxy>( _batch.Values );
			  directRecordProxies.sort( ( o1, o2 ) => Long.compare( -o1.Key, o2.Key ) );
			  foreach ( DirectRecordProxy proxy in directRecordProxies )
			  {
					proxy.Store();
			  }
			  _changeCounter.clear();
			  _batch.Clear();
		 }
	}

}