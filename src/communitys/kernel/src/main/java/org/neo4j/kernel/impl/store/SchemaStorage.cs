using System;
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
namespace Neo4Net.Kernel.impl.store
{

	using Predicates = Neo4Net.Functions.Predicates;
	using Neo4Net.Helpers.Collections;
	using MalformedSchemaRuleException = Neo4Net.@internal.Kernel.Api.exceptions.schema.MalformedSchemaRuleException;
	using SchemaDescriptor = Neo4Net.@internal.Kernel.Api.schema.SchemaDescriptor;
	using SchemaDescriptorPredicates = Neo4Net.@internal.Kernel.Api.schema.SchemaDescriptorPredicates;
	using ConstraintDescriptor = Neo4Net.@internal.Kernel.Api.schema.constraints.ConstraintDescriptor;
	using DuplicateSchemaRuleException = Neo4Net.Kernel.Api.Exceptions.schema.DuplicateSchemaRuleException;
	using SchemaRuleNotFoundException = Neo4Net.Kernel.Api.Exceptions.schema.SchemaRuleNotFoundException;
	using ConstraintRule = Neo4Net.Kernel.impl.store.record.ConstraintRule;
	using DynamicRecord = Neo4Net.Kernel.impl.store.record.DynamicRecord;
	using RecordLoad = Neo4Net.Kernel.impl.store.record.RecordLoad;
	using IndexDescriptor = Neo4Net.Storageengine.Api.schema.IndexDescriptor;
	using SchemaRule = Neo4Net.Storageengine.Api.schema.SchemaRule;
	using StoreIndexDescriptor = Neo4Net.Storageengine.Api.schema.StoreIndexDescriptor;

	public class SchemaStorage : SchemaRuleAccess
	{
		 private readonly RecordStore<DynamicRecord> _schemaStore;

		 public SchemaStorage( RecordStore<DynamicRecord> schemaStore )
		 {
			  this._schemaStore = schemaStore;
		 }

		 /// <summary>
		 /// Find the IndexRule that matches the given IndexDescriptor. Filters on index type.
		 /// </summary>
		 /// <returns> the matching IndexRule, or null if no matching IndexRule was found </returns>
		 /// <exception cref="IllegalStateException"> if more than one matching rule. </exception>
		 /// <param name="descriptor"> the target IndexDescriptor </param>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public org.neo4j.storageengine.api.schema.StoreIndexDescriptor indexGetForSchema(final org.neo4j.storageengine.api.schema.IndexDescriptor descriptor)
		 public virtual StoreIndexDescriptor IndexGetForSchema( IndexDescriptor descriptor )
		 {
			  return IndexGetForSchema( descriptor, true );
		 }

		 /// <summary>
		 /// Find the IndexRule that matches the given IndexDescriptor.
		 /// </summary>
		 /// <returns> the matching IndexRule, or null if no matching IndexRule was found </returns>
		 /// <exception cref="IllegalStateException"> if more than one matching rule. </exception>
		 /// <param name="descriptor"> the target IndexDescriptor </param>
		 /// <param name="filterOnType"> whether or not to filter on index type. If {@code false} then only <seealso cref="SchemaDescriptor"/> will be compared. </param>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public org.neo4j.storageengine.api.schema.StoreIndexDescriptor indexGetForSchema(final org.neo4j.storageengine.api.schema.IndexDescriptor descriptor, boolean filterOnType)
		 public virtual StoreIndexDescriptor IndexGetForSchema( IndexDescriptor descriptor, bool filterOnType )
		 {
			  System.Predicate<StoreIndexDescriptor> filter = filterOnType ? descriptor.equals : candidate => candidate.schema().Equals(descriptor.Schema());
			  IEnumerator<StoreIndexDescriptor> indexes = LoadAllSchemaRules( filter, typeof( StoreIndexDescriptor ), false );

			  StoreIndexDescriptor foundRule = null;
			  while ( indexes.MoveNext() )
			  {
					StoreIndexDescriptor candidate = indexes.Current;
					if ( foundRule != null )
					{
						 throw new System.InvalidOperationException( string.Format( "Found more than one matching index, {0} and {1}", foundRule, candidate ) );
					}
					foundRule = candidate;
			  }

			  return foundRule;
		 }

		 /// <summary>
		 /// Find the IndexRule that has the given user supplied name.
		 /// </summary>
		 /// <param name="indexName"> the user supplied index name to look for. </param>
		 /// <returns> the matching IndexRule, or null if no matching index rule was found. </returns>
		 public virtual StoreIndexDescriptor IndexGetForName( string indexName )
		 {
			  IEnumerator<StoreIndexDescriptor> itr = IndexesGetAll();
			  while ( itr.MoveNext() )
			  {
					StoreIndexDescriptor sid = itr.Current;
					if ( sid.UserSuppliedName.map( n => n.Equals( indexName ) ).orElse( false ) )
					{
						 return sid;
					}
			  }
			  return null;
		 }

		 public virtual IEnumerator<StoreIndexDescriptor> IndexesGetAll()
		 {
			  return LoadAllSchemaRules( Predicates.alwaysTrue(), typeof(StoreIndexDescriptor), false );
		 }

		 public virtual IEnumerator<ConstraintRule> ConstraintsGetAll()
		 {
			  return LoadAllSchemaRules( Predicates.alwaysTrue(), typeof(ConstraintRule), false );
		 }

		 public virtual IEnumerator<ConstraintRule> ConstraintsGetAllIgnoreMalformed()
		 {
			  return LoadAllSchemaRules( Predicates.alwaysTrue(), typeof(ConstraintRule), true );
		 }

		 public virtual IEnumerator<ConstraintRule> ConstraintsGetForRelType( int relTypeId )
		 {
			  return LoadAllSchemaRules( rule => SchemaDescriptorPredicates.hasRelType( rule, relTypeId ), typeof( ConstraintRule ), false );
		 }

		 public virtual IEnumerator<ConstraintRule> ConstraintsGetForLabel( int labelId )
		 {
			  return LoadAllSchemaRules( rule => SchemaDescriptorPredicates.hasLabel( rule, labelId ), typeof( ConstraintRule ), false );
		 }

		 public virtual IEnumerator<ConstraintRule> ConstraintsGetForSchema( SchemaDescriptor schemaDescriptor )
		 {
			  return LoadAllSchemaRules( SchemaDescriptor.equalTo( schemaDescriptor ), typeof( ConstraintRule ), false );
		 }

		 /// <summary>
		 /// Get the constraint rule that matches the given ConstraintDescriptor </summary>
		 /// <param name="descriptor"> the ConstraintDescriptor to match </param>
		 /// <returns> the matching ConstrainRule </returns>
		 /// <exception cref="SchemaRuleNotFoundException"> if no ConstraintRule matches the given descriptor </exception>
		 /// <exception cref="DuplicateSchemaRuleException"> if two or more ConstraintRules match the given descriptor </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.kernel.impl.store.record.ConstraintRule constraintsGetSingle(final org.neo4j.internal.kernel.api.schema.constraints.ConstraintDescriptor descriptor) throws org.neo4j.kernel.api.exceptions.schema.SchemaRuleNotFoundException, org.neo4j.kernel.api.exceptions.schema.DuplicateSchemaRuleException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 public virtual ConstraintRule ConstraintsGetSingle( ConstraintDescriptor descriptor )
		 {
			  IEnumerator<ConstraintRule> rules = LoadAllSchemaRules( descriptor.isSame, typeof( ConstraintRule ), false );

//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  if ( !rules.hasNext() )
			  {
					throw new SchemaRuleNotFoundException( Neo4Net.Storageengine.Api.schema.SchemaRule_Kind.map( descriptor ), descriptor.Schema() );
			  }

//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  ConstraintRule rule = rules.next();

//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  if ( rules.hasNext() )
			  {
					throw new DuplicateSchemaRuleException( Neo4Net.Storageengine.Api.schema.SchemaRule_Kind.map( descriptor ), descriptor.Schema() );
			  }
			  return rule;
		 }

		 public virtual IEnumerator<SchemaRule> LoadAllSchemaRules()
		 {
			  return LoadAllSchemaRules( Predicates.alwaysTrue(), typeof(SchemaRule), false );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.storageengine.api.schema.SchemaRule loadSingleSchemaRule(long ruleId) throws org.neo4j.internal.kernel.api.exceptions.schema.MalformedSchemaRuleException
		 public override SchemaRule LoadSingleSchemaRule( long ruleId )
		 {
			  ICollection<DynamicRecord> records;
			  try
			  {
					records = _schemaStore.getRecords( ruleId, RecordLoad.NORMAL );
			  }
			  catch ( Exception e )
			  {
					throw new MalformedSchemaRuleException( e.Message, e );
			  }
			  return SchemaStore.ReadSchemaRule( ruleId, records, NewRecordBuffer() );
		 }

		 /// <summary>
		 /// Scans the schema store and loads all <seealso cref="SchemaRule rules"/> in it. This method is written with the assumption
		 /// that there's no id reuse on schema records.
		 /// </summary>
		 /// <param name="predicate"> filter when loading. </param>
		 /// <param name="returnType"> type of <seealso cref="SchemaRule"/> to load. </param>
		 /// <param name="ignoreMalformed"> whether or not to ignore inconsistent records (used in consistency checking). </param>
		 /// <returns> <seealso cref="System.Collections.IEnumerator"/> of the loaded schema rules, lazily loaded when advancing the iterator. </returns>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: <ReturnType extends org.neo4j.storageengine.api.schema.SchemaRule> java.util.Iterator<ReturnType> loadAllSchemaRules(final System.Predicate<ReturnType> predicate, final Class<ReturnType> returnType, final boolean ignoreMalformed)
		 internal virtual IEnumerator<ReturnType> LoadAllSchemaRules<ReturnType>( System.Predicate<ReturnType> predicate, Type returnType, bool ignoreMalformed ) where ReturnType : Neo4Net.Storageengine.Api.schema.SchemaRule
		 {
				 returnType = typeof( ReturnType );
			  return new PrefetchingIteratorAnonymousInnerClass( this, predicate, returnType, ignoreMalformed );
		 }

		 private class PrefetchingIteratorAnonymousInnerClass : PrefetchingIterator<ReturnType>
		 {
			 private readonly SchemaStorage _outerInstance;

			 private System.Predicate<ReturnType> _predicate;
			 private Type _returnType;
			 private bool _ignoreMalformed;

			 public PrefetchingIteratorAnonymousInnerClass( SchemaStorage outerInstance, System.Predicate<ReturnType> predicate, Type returnType, bool ignoreMalformed )
			 {
				 this.outerInstance = outerInstance;
				 this._predicate = predicate;
				 this._returnType = returnType;
				 this._ignoreMalformed = ignoreMalformed;
				 highestId = outerInstance.schemaStore.HighestPossibleIdInUse;
				 currentId = 1;
				 scratchData = outerInstance.newRecordBuffer();
				 record = outerInstance.schemaStore.NewRecord();
			 }

			 private readonly long highestId;
			 private long currentId;
			 private readonly sbyte[] scratchData;
			 private readonly DynamicRecord record;

			 protected internal override ReturnType fetchNextOrNull()
			 {
				  while ( currentId <= highestId )
				  {
						long id = currentId++;
						_outerInstance.schemaStore.getRecord( id, record, RecordLoad.FORCE );
						if ( record.inUse() && record.StartRecord )
						{
							 // It may be that concurrently to our reading there's a transaction dropping the schema rule
							 // that we're reading and that rule may have spanned multiple dynamic records.
							 try
							 {
								  ICollection<DynamicRecord> records;
								  try
								  {
										records = _outerInstance.schemaStore.getRecords( id, RecordLoad.NORMAL );
								  }
								  catch ( InvalidRecordException )
								  {
										// This may have been due to a concurrent drop of this rule.
										continue;
								  }

								  SchemaRule schemaRule = SchemaStore.ReadSchemaRule( id, records, scratchData );
								  if ( _returnType.IsInstanceOfType( schemaRule ) )
								  {
										ReturnType returnRule = _returnType.cast( schemaRule );
										if ( _predicate( returnRule ) )
										{
											 return returnRule;
										}
								  }
							 }
							 catch ( MalformedSchemaRuleException e )
							 {
								  if ( !_ignoreMalformed )
								  {
										throw new Exception( e );
								  }
							 }
						}
				  }
				  return null;
			 }
		 }

		 public virtual long NewRuleId()
		 {
			  return _schemaStore.nextId();
		 }

		 private sbyte[] NewRecordBuffer()
		 {
			  return new sbyte[_schemaStore.RecordSize * 4];
		 }
	}

}