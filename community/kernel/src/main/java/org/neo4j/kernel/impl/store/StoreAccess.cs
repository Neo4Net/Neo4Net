using System;

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
namespace Org.Neo4j.Kernel.impl.store
{
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Org.Neo4j.Io.layout.DatabaseLayout;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using EmptyVersionContextSupplier = Org.Neo4j.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using CountsAccessor = Org.Neo4j.Kernel.Impl.Api.CountsAccessor;
	using DefaultIdGeneratorFactory = Org.Neo4j.Kernel.impl.store.id.DefaultIdGeneratorFactory;
	using AbstractBaseRecord = Org.Neo4j.Kernel.impl.store.record.AbstractBaseRecord;
	using DynamicRecord = Org.Neo4j.Kernel.impl.store.record.DynamicRecord;
	using LabelTokenRecord = Org.Neo4j.Kernel.impl.store.record.LabelTokenRecord;
	using NodeRecord = Org.Neo4j.Kernel.impl.store.record.NodeRecord;
	using PropertyKeyTokenRecord = Org.Neo4j.Kernel.impl.store.record.PropertyKeyTokenRecord;
	using PropertyRecord = Org.Neo4j.Kernel.impl.store.record.PropertyRecord;
	using RelationshipGroupRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipGroupRecord;
	using RelationshipRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipRecord;
	using RelationshipTypeTokenRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipTypeTokenRecord;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;

	/// <summary>
	/// Not thread safe (since DiffRecordStore is not thread safe), intended for
	/// single threaded use.
	/// 
	/// Make sure to call <seealso cref="initialize()"/> after constructor has been run.
	/// </summary>
	public class StoreAccess
	{
		 // Top level stores
		 private RecordStore<DynamicRecord> _schemaStore;
		 private RecordStore<NodeRecord> _nodeStore;
		 private RecordStore<RelationshipRecord> _relStore;
		 private RecordStore<RelationshipTypeTokenRecord> _relationshipTypeTokenStore;
		 private RecordStore<LabelTokenRecord> _labelTokenStore;
		 private RecordStore<DynamicRecord> _nodeDynamicLabelStore;
		 private RecordStore<PropertyRecord> _propStore;
		 // Transitive stores
		 private RecordStore<DynamicRecord> _stringStore;
		 private RecordStore<DynamicRecord> _arrayStore;
		 private RecordStore<PropertyKeyTokenRecord> _propertyKeyTokenStore;
		 private RecordStore<DynamicRecord> _relationshipTypeNameStore;
		 private RecordStore<DynamicRecord> _labelNameStore;
		 private RecordStore<DynamicRecord> _propertyKeyNameStore;
		 private RecordStore<RelationshipGroupRecord> _relGroupStore;
		 private readonly CountsAccessor _counts;
		 // internal state
		 private bool _closeable;
		 private readonly NeoStores _neoStores;

		 public StoreAccess( NeoStores store )
		 {
			  this._neoStores = store;
			  this._counts = store.Counts;
		 }

		 public StoreAccess( FileSystemAbstraction fileSystem, PageCache pageCache, DatabaseLayout directoryStructure, Config config ) : this( ( new StoreFactory( directoryStructure, config, new DefaultIdGeneratorFactory( fileSystem ), pageCache, fileSystem, NullLogProvider.Instance, EmptyVersionContextSupplier.EMPTY ) ).openAllNeoStores() )
		 {
			  this._closeable = true;
		 }

		 /// <summary>
		 /// This method exists since <seealso cref="wrapStore(RecordStore)"/> might depend on the existence of a variable
		 /// that gets set in a subclass' constructor <strong>after</strong> this constructor of <seealso cref="StoreAccess"/>
		 /// has been executed. I.e. a correct creation of a <seealso cref="StoreAccess"/> instance must be the creation of the
		 /// object plus a call to <seealso cref="initialize()"/>.
		 /// </summary>
		 /// <returns> this </returns>
		 public virtual StoreAccess Initialize()
		 {
			  this._schemaStore = WrapStore( _neoStores.SchemaStore );
			  this._nodeStore = WrapStore( _neoStores.NodeStore );
			  this._relStore = WrapStore( _neoStores.RelationshipStore );
			  this._propStore = WrapStore( _neoStores.PropertyStore );
			  this._stringStore = WrapStore( _neoStores.PropertyStore.StringStore );
			  this._arrayStore = WrapStore( _neoStores.PropertyStore.ArrayStore );
			  this._relationshipTypeTokenStore = WrapStore( _neoStores.RelationshipTypeTokenStore );
			  this._labelTokenStore = WrapStore( _neoStores.LabelTokenStore );
			  this._nodeDynamicLabelStore = WrapStore( WrapNodeDynamicLabelStore( _neoStores.NodeStore.DynamicLabelStore ) );
			  this._propertyKeyTokenStore = WrapStore( _neoStores.PropertyStore.PropertyKeyTokenStore );
			  this._relationshipTypeNameStore = WrapStore( _neoStores.RelationshipTypeTokenStore.NameStore );
			  this._labelNameStore = WrapStore( _neoStores.LabelTokenStore.NameStore );
			  this._propertyKeyNameStore = WrapStore( _neoStores.PropertyStore.PropertyKeyTokenStore.NameStore );
			  this._relGroupStore = WrapStore( _neoStores.RelationshipGroupStore );
			  return this;
		 }

		 public virtual NeoStores RawNeoStores
		 {
			 get
			 {
				  return _neoStores;
			 }
		 }

		 public virtual RecordStore<DynamicRecord> SchemaStore
		 {
			 get
			 {
				  return _schemaStore;
			 }
		 }

		 public virtual RecordStore<NodeRecord> NodeStore
		 {
			 get
			 {
				  return _nodeStore;
			 }
		 }

		 public virtual RecordStore<RelationshipRecord> RelationshipStore
		 {
			 get
			 {
				  return _relStore;
			 }
		 }

		 public virtual RecordStore<RelationshipGroupRecord> RelationshipGroupStore
		 {
			 get
			 {
				  return _relGroupStore;
			 }
		 }

		 public virtual RecordStore<PropertyRecord> PropertyStore
		 {
			 get
			 {
				  return _propStore;
			 }
		 }

		 public virtual RecordStore<DynamicRecord> StringStore
		 {
			 get
			 {
				  return _stringStore;
			 }
		 }

		 public virtual RecordStore<DynamicRecord> ArrayStore
		 {
			 get
			 {
				  return _arrayStore;
			 }
		 }

		 public virtual RecordStore<RelationshipTypeTokenRecord> RelationshipTypeTokenStore
		 {
			 get
			 {
				  return _relationshipTypeTokenStore;
			 }
		 }

		 public virtual RecordStore<LabelTokenRecord> LabelTokenStore
		 {
			 get
			 {
				  return _labelTokenStore;
			 }
		 }

		 public virtual RecordStore<DynamicRecord> NodeDynamicLabelStore
		 {
			 get
			 {
				  return _nodeDynamicLabelStore;
			 }
		 }

		 public virtual RecordStore<PropertyKeyTokenRecord> PropertyKeyTokenStore
		 {
			 get
			 {
				  return _propertyKeyTokenStore;
			 }
		 }

		 public virtual RecordStore<DynamicRecord> RelationshipTypeNameStore
		 {
			 get
			 {
				  return _relationshipTypeNameStore;
			 }
		 }

		 public virtual RecordStore<DynamicRecord> LabelNameStore
		 {
			 get
			 {
				  return _labelNameStore;
			 }
		 }

		 public virtual RecordStore<DynamicRecord> PropertyKeyNameStore
		 {
			 get
			 {
				  return _propertyKeyNameStore;
			 }
		 }

		 public virtual CountsAccessor Counts
		 {
			 get
			 {
				  return _counts;
			 }
		 }

		 private static RecordStore<DynamicRecord> WrapNodeDynamicLabelStore( RecordStore<DynamicRecord> store )
		 {
			  return new RecordStore_DelegatorAnonymousInnerClass( store );
		 }

		 private class RecordStore_DelegatorAnonymousInnerClass : RecordStore_Delegator<DynamicRecord>
		 {
			 public RecordStore_DelegatorAnonymousInnerClass( Org.Neo4j.Kernel.impl.store.RecordStore<DynamicRecord> store ) : base( store )
			 {
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public <FAILURE extends Exception> void accept(RecordStore_Processor<FAILURE> processor, org.neo4j.kernel.impl.store.record.DynamicRecord record) throws FAILURE
			 public override void accept<FAILURE>( RecordStore_Processor<FAILURE> processor, DynamicRecord record ) where FAILURE : Exception
			 {
				  processor.ProcessLabelArrayWithOwner( this, record );
			 }
		 }

		 protected internal virtual RecordStore<R> WrapStore<R>( RecordStore<R> store ) where R : Org.Neo4j.Kernel.impl.store.record.AbstractBaseRecord
		 {
			  return store;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") protected <FAILURE extends Exception> void apply(RecordStore_Processor<FAILURE> processor, RecordStore<?> store) throws FAILURE
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 protected internal virtual void Apply<FAILURE, T1>( RecordStore_Processor<FAILURE> processor, RecordStore<T1> store ) where FAILURE : Exception
		 {
			  processor.ApplyFiltered( store );
		 }

		 public virtual void Close()
		 {
			 lock ( this )
			 {
				  if ( _closeable )
				  {
						_closeable = false;
						_neoStores.close();
				  }
			 }
		 }
	}

}