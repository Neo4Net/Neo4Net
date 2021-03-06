﻿using System;

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
namespace Org.Neo4j.Kernel.builtinprocs
{

	using Predicates = Org.Neo4j.Function.Predicates;
	using IndexReference = Org.Neo4j.@internal.Kernel.Api.IndexReference;
	using InternalIndexState = Org.Neo4j.@internal.Kernel.Api.InternalIndexState;
	using SchemaWrite = Org.Neo4j.@internal.Kernel.Api.SchemaWrite;
	using TokenRead = Org.Neo4j.@internal.Kernel.Api.TokenRead;
	using InvalidTransactionTypeKernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.InvalidTransactionTypeKernelException;
	using ProcedureException = Org.Neo4j.@internal.Kernel.Api.exceptions.ProcedureException;
	using IllegalTokenNameException = Org.Neo4j.@internal.Kernel.Api.exceptions.schema.IllegalTokenNameException;
	using IndexNotFoundKernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.schema.IndexNotFoundKernelException;
	using SchemaKernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.schema.SchemaKernelException;
	using TooManyLabelsException = Org.Neo4j.@internal.Kernel.Api.exceptions.schema.TooManyLabelsException;
	using KernelTransaction = Org.Neo4j.Kernel.api.KernelTransaction;
	using Statement = Org.Neo4j.Kernel.api.Statement;
	using Status = Org.Neo4j.Kernel.Api.Exceptions.Status;
	using LabelSchemaDescriptor = Org.Neo4j.Kernel.api.schema.LabelSchemaDescriptor;
	using SchemaDescriptorFactory = Org.Neo4j.Kernel.api.schema.SchemaDescriptorFactory;
	using IndexPopulationFailure = Org.Neo4j.Kernel.Impl.Api.index.IndexPopulationFailure;
	using IndexingService = Org.Neo4j.Kernel.Impl.Api.index.IndexingService;
	using IndexSamplingMode = Org.Neo4j.Kernel.Impl.Api.index.sampling.IndexSamplingMode;

	public class IndexProcedures : AutoCloseable
	{
		 private readonly KernelTransaction _ktx;
		 private readonly Statement _statement;
		 private readonly IndexingService _indexingService;

		 public IndexProcedures( KernelTransaction tx, IndexingService indexingService )
		 {
			  this._ktx = tx;
			  _statement = tx.AcquireStatement();
			  this._indexingService = indexingService;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void awaitIndexByPattern(String indexPattern, long timeout, java.util.concurrent.TimeUnit timeoutUnits) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 public virtual void AwaitIndexByPattern( string indexPattern, long timeout, TimeUnit timeoutUnits )
		 {
			  IndexSpecifier specifier = IndexSpecifier.ByPattern( indexPattern );
			  WaitUntilOnline( GetIndex( specifier ), specifier, timeout, timeoutUnits );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void awaitIndexByName(String indexName, long timeout, java.util.concurrent.TimeUnit timeoutUnits) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 public virtual void AwaitIndexByName( string indexName, long timeout, TimeUnit timeoutUnits )
		 {
			  IndexSpecifier specifier = IndexSpecifier.ByName( indexName );
			  WaitUntilOnline( GetIndex( specifier ), specifier, timeout, timeoutUnits );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void resampleIndex(String indexSpecification) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 public virtual void ResampleIndex( string indexSpecification )
		 {
			  IndexSpecifier specifier = IndexSpecifier.ByPattern( indexSpecification );
			  try
			  {
					TriggerSampling( GetIndex( specifier ) );
			  }
			  catch ( IndexNotFoundKernelException e )
			  {
					throw new ProcedureException( e.Status(), e.Message, e );
			  }
		 }

		 public virtual void ResampleOutdatedIndexes()
		 {
			  _indexingService.triggerIndexSampling( IndexSamplingMode.TRIGGER_REBUILD_UPDATED );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.util.stream.Stream<BuiltInProcedures.SchemaIndexInfo> createIndex(String indexSpecification, String providerName) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 public virtual Stream<BuiltInProcedures.SchemaIndexInfo> CreateIndex( string indexSpecification, string providerName )
		 {
			  return CreateIndex( indexSpecification, providerName, "index created", ( schemaWrite, descriptor, provider ) => schemaWrite.indexCreate( descriptor, provider, null ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.util.stream.Stream<BuiltInProcedures.SchemaIndexInfo> createUniquePropertyConstraint(String indexSpecification, String providerName) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 public virtual Stream<BuiltInProcedures.SchemaIndexInfo> CreateUniquePropertyConstraint( string indexSpecification, string providerName )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  return CreateIndex( indexSpecification, providerName, "uniqueness constraint online", SchemaWrite::uniquePropertyConstraintCreate );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.util.stream.Stream<BuiltInProcedures.SchemaIndexInfo> createNodeKey(String indexSpecification, String providerName) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 public virtual Stream<BuiltInProcedures.SchemaIndexInfo> CreateNodeKey( string indexSpecification, string providerName )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  return CreateIndex( indexSpecification, providerName, "node key constraint online", SchemaWrite::nodeKeyConstraintCreate );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.util.stream.Stream<BuiltInProcedures.SchemaIndexInfo> createIndex(String indexSpecification, String providerName, String statusMessage, IndexCreator indexCreator) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 private Stream<BuiltInProcedures.SchemaIndexInfo> CreateIndex( string indexSpecification, string providerName, string statusMessage, IndexCreator indexCreator )
		 {
			  AssertProviderNameNotNull( providerName );
			  IndexSpecifier index = IndexSpecifier.ByPattern( indexSpecification );
			  int labelId = GetOrCreateLabelId( index.Label() );
			  int[] propertyKeyIds = GetOrCreatePropertyIds( index.Properties() );
			  try
			  {
					SchemaWrite schemaWrite = _ktx.schemaWrite();
					LabelSchemaDescriptor labelSchemaDescriptor = SchemaDescriptorFactory.forLabel( labelId, propertyKeyIds );
					indexCreator( schemaWrite, labelSchemaDescriptor, providerName );
					return Stream.of( new BuiltInProcedures.SchemaIndexInfo( indexSpecification, providerName, statusMessage ) );
			  }
			  catch ( Exception e ) when ( e is InvalidTransactionTypeKernelException || e is SchemaKernelException )
			  {
					throw new ProcedureException( e.status(), e, e.Message );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void assertProviderNameNotNull(String providerName) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 private static void AssertProviderNameNotNull( string providerName )
		 {
			  if ( string.ReferenceEquals( providerName, null ) )
			  {
					throw new ProcedureException( Org.Neo4j.Kernel.Api.Exceptions.Status_Procedure.ProcedureCallFailed, IndexProviderNullMessage() );
			  }
		 }

		 private static string IndexProviderNullMessage()
		 {
			  return "Could not create index with specified index provider being null.";
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private int getLabelId(String labelName) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 private int GetLabelId( string labelName )
		 {
			  int labelId = _ktx.tokenRead().nodeLabel(labelName);
			  if ( labelId == Org.Neo4j.@internal.Kernel.Api.TokenRead_Fields.NO_TOKEN )
			  {
					throw new ProcedureException( Org.Neo4j.Kernel.Api.Exceptions.Status_Schema.LabelAccessFailed, "No such label %s", labelName );
			  }
			  return labelId;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private int[] getPropertyIds(String[] propertyKeyNames) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 private int[] GetPropertyIds( string[] propertyKeyNames )
		 {
			  int[] propertyKeyIds = new int[propertyKeyNames.Length];
			  for ( int i = 0; i < propertyKeyIds.Length; i++ )
			  {

					int propertyKeyId = _ktx.tokenRead().propertyKey(propertyKeyNames[i]);
					if ( propertyKeyId == Org.Neo4j.@internal.Kernel.Api.TokenRead_Fields.NO_TOKEN )
					{
						 throw new ProcedureException( Org.Neo4j.Kernel.Api.Exceptions.Status_Schema.PropertyKeyAccessFailed, "No such property key %s", propertyKeyNames[i] );
					}
					propertyKeyIds[i] = propertyKeyId;
			  }
			  return propertyKeyIds;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private int getOrCreateLabelId(String labelName) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 private int GetOrCreateLabelId( string labelName )
		 {
			  try
			  {
					return _ktx.tokenWrite().labelGetOrCreateForName(labelName);
			  }
			  catch ( Exception e ) when ( e is TooManyLabelsException || e is IllegalTokenNameException )
			  {
					throw new ProcedureException( e.status(), e, e.Message );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private int[] getOrCreatePropertyIds(String[] propertyKeyNames) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 private int[] GetOrCreatePropertyIds( string[] propertyKeyNames )
		 {
			  int[] propertyKeyIds = new int[propertyKeyNames.Length];
			  for ( int i = 0; i < propertyKeyIds.Length; i++ )
			  {
					try
					{
						 propertyKeyIds[i] = _ktx.tokenWrite().propertyKeyGetOrCreateForName(propertyKeyNames[i]);
					}
					catch ( IllegalTokenNameException e )
					{
						 throw new ProcedureException( e.Status(), e, e.Message );
					}
			  }
			  return propertyKeyIds;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.internal.kernel.api.IndexReference getIndex(IndexSpecifier specifier) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 private IndexReference GetIndex( IndexSpecifier specifier )
		 {
			  if ( !string.ReferenceEquals( specifier.Name(), null ) )
			  {
					// Find index by name.
					IndexReference indexReference = _ktx.schemaRead().indexGetForName(specifier.Name());

					if ( indexReference == IndexReference.NO_INDEX )
					{
						 throw new ProcedureException( Org.Neo4j.Kernel.Api.Exceptions.Status_Schema.IndexNotFound, "No such index '%s'", specifier );
					}
					return indexReference;
			  }
			  else
			  {
					// Find index by label and properties.
					int labelId = GetLabelId( specifier.Label() );
					int[] propertyKeyIds = GetPropertyIds( specifier.Properties() );
					IndexReference indexReference = _ktx.schemaRead().index(labelId, propertyKeyIds);

					if ( indexReference == IndexReference.NO_INDEX )
					{
						 throw new ProcedureException( Org.Neo4j.Kernel.Api.Exceptions.Status_Schema.IndexNotFound, "No such index %s", specifier );
					}
					return indexReference;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void waitUntilOnline(org.neo4j.internal.kernel.api.IndexReference index, IndexSpecifier indexDescription, long timeout, java.util.concurrent.TimeUnit timeoutUnits) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 private void WaitUntilOnline( IndexReference index, IndexSpecifier indexDescription, long timeout, TimeUnit timeoutUnits )
		 {
			  try
			  {
					Predicates.awaitEx( () => IsOnline(indexDescription, index), timeout, timeoutUnits );
			  }
			  catch ( TimeoutException )
			  {
					throw new ProcedureException( Org.Neo4j.Kernel.Api.Exceptions.Status_Procedure.ProcedureTimedOut, "Index on %s did not come online within %s %s", indexDescription, timeout, timeoutUnits );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private boolean isOnline(IndexSpecifier specifier, org.neo4j.internal.kernel.api.IndexReference index) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 private bool IsOnline( IndexSpecifier specifier, IndexReference index )
		 {
			  InternalIndexState state = GetState( specifier, index );
			  switch ( state )
			  {
					case InternalIndexState.POPULATING:
						 return false;
					case InternalIndexState.ONLINE:
						 return true;
					case InternalIndexState.FAILED:
						 string cause = GetFailure( specifier, index );
						 throw new ProcedureException( Org.Neo4j.Kernel.Api.Exceptions.Status_Schema.IndexCreationFailed, IndexPopulationFailure.appendCauseOfFailure( "Index %s is in failed state.", cause ), specifier );
					default:
						 throw new System.InvalidOperationException( "Unknown index state " + state );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.internal.kernel.api.InternalIndexState getState(IndexSpecifier specifier, org.neo4j.internal.kernel.api.IndexReference index) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 private InternalIndexState GetState( IndexSpecifier specifier, IndexReference index )
		 {
			  try
			  {
					return _ktx.schemaRead().indexGetState(index);
			  }
			  catch ( IndexNotFoundKernelException e )
			  {
					throw new ProcedureException( Org.Neo4j.Kernel.Api.Exceptions.Status_Schema.IndexNotFound, e, "No such index %s", specifier );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private String getFailure(IndexSpecifier indexDescription, org.neo4j.internal.kernel.api.IndexReference index) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 private string GetFailure( IndexSpecifier indexDescription, IndexReference index )
		 {
			  try
			  {
					return _ktx.schemaRead().indexGetFailure(index);
			  }
			  catch ( IndexNotFoundKernelException e )
			  {
					throw new ProcedureException( Org.Neo4j.Kernel.Api.Exceptions.Status_Schema.IndexNotFound, e, "No such index %s", indexDescription );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void triggerSampling(org.neo4j.internal.kernel.api.IndexReference index) throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException
		 private void TriggerSampling( IndexReference index )
		 {
			  _indexingService.triggerIndexSampling( index.Schema(), IndexSamplingMode.TRIGGER_REBUILD_ALL );
		 }

		 public override void Close()
		 {
			  _statement.close();
		 }

		 private delegate void IndexCreator( SchemaWrite schemaWrite, LabelSchemaDescriptor descriptor, string providerName );
	}

}