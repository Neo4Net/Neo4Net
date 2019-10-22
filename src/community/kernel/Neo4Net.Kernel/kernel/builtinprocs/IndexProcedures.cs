using System;

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
namespace Neo4Net.Kernel.builtinprocs
{

	using Predicates = Neo4Net.Functions.Predicates;
	using IndexReference = Neo4Net.Internal.Kernel.Api.IndexReference;
	using InternalIndexState = Neo4Net.Internal.Kernel.Api.InternalIndexState;
	using SchemaWrite = Neo4Net.Internal.Kernel.Api.SchemaWrite;
	using TokenRead = Neo4Net.Internal.Kernel.Api.TokenRead;
	using InvalidTransactionTypeKernelException = Neo4Net.Internal.Kernel.Api.exceptions.InvalidTransactionTypeKernelException;
	using ProcedureException = Neo4Net.Internal.Kernel.Api.exceptions.ProcedureException;
	using IllegalTokenNameException = Neo4Net.Internal.Kernel.Api.exceptions.schema.IllegalTokenNameException;
	using IndexNotFoundKernelException = Neo4Net.Internal.Kernel.Api.exceptions.schema.IndexNotFoundKernelException;
	using SchemaKernelException = Neo4Net.Internal.Kernel.Api.exceptions.schema.SchemaKernelException;
	using TooManyLabelsException = Neo4Net.Internal.Kernel.Api.exceptions.schema.TooManyLabelsException;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using Statement = Neo4Net.Kernel.api.Statement;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using LabelSchemaDescriptor = Neo4Net.Kernel.api.schema.LabelSchemaDescriptor;
	using SchemaDescriptorFactory = Neo4Net.Kernel.api.schema.SchemaDescriptorFactory;
	using IndexPopulationFailure = Neo4Net.Kernel.Impl.Api.index.IndexPopulationFailure;
	using IndexingService = Neo4Net.Kernel.Impl.Api.index.IndexingService;
	using IndexSamplingMode = Neo4Net.Kernel.Impl.Api.index.sampling.IndexSamplingMode;

	public class IndexProcedures : IDisposable
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
//ORIGINAL LINE: public void awaitIndexByPattern(String indexPattern, long timeout, java.util.concurrent.TimeUnit timeoutUnits) throws org.Neo4Net.internal.kernel.api.exceptions.ProcedureException
		 public virtual void AwaitIndexByPattern( string indexPattern, long timeout, TimeUnit timeoutUnits )
		 {
			  IndexSpecifier specifier = IndexSpecifier.ByPattern( indexPattern );
			  WaitUntilOnline( GetIndex( specifier ), specifier, timeout, timeoutUnits );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void awaitIndexByName(String indexName, long timeout, java.util.concurrent.TimeUnit timeoutUnits) throws org.Neo4Net.internal.kernel.api.exceptions.ProcedureException
		 public virtual void AwaitIndexByName( string indexName, long timeout, TimeUnit timeoutUnits )
		 {
			  IndexSpecifier specifier = IndexSpecifier.ByName( indexName );
			  WaitUntilOnline( GetIndex( specifier ), specifier, timeout, timeoutUnits );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void resampleIndex(String indexSpecification) throws org.Neo4Net.internal.kernel.api.exceptions.ProcedureException
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
//ORIGINAL LINE: public java.util.stream.Stream<BuiltInProcedures.SchemaIndexInfo> createIndex(String indexSpecification, String providerName) throws org.Neo4Net.internal.kernel.api.exceptions.ProcedureException
		 public virtual Stream<BuiltInProcedures.SchemaIndexInfo> CreateIndex( string indexSpecification, string providerName )
		 {
			  return CreateIndex( indexSpecification, providerName, "index created", ( schemaWrite, descriptor, provider ) => schemaWrite.indexCreate( descriptor, provider, null ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.util.stream.Stream<BuiltInProcedures.SchemaIndexInfo> createUniquePropertyConstraint(String indexSpecification, String providerName) throws org.Neo4Net.internal.kernel.api.exceptions.ProcedureException
		 public virtual Stream<BuiltInProcedures.SchemaIndexInfo> CreateUniquePropertyConstraint( string indexSpecification, string providerName )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  return CreateIndex( indexSpecification, providerName, "uniqueness constraint online", SchemaWrite::uniquePropertyConstraintCreate );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.util.stream.Stream<BuiltInProcedures.SchemaIndexInfo> createNodeKey(String indexSpecification, String providerName) throws org.Neo4Net.internal.kernel.api.exceptions.ProcedureException
		 public virtual Stream<BuiltInProcedures.SchemaIndexInfo> CreateNodeKey( string indexSpecification, string providerName )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  return CreateIndex( indexSpecification, providerName, "node key constraint online", SchemaWrite::nodeKeyConstraintCreate );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.util.stream.Stream<BuiltInProcedures.SchemaIndexInfo> createIndex(String indexSpecification, String providerName, String statusMessage, IndexCreator indexCreator) throws org.Neo4Net.internal.kernel.api.exceptions.ProcedureException
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
//ORIGINAL LINE: private static void assertProviderNameNotNull(String providerName) throws org.Neo4Net.internal.kernel.api.exceptions.ProcedureException
		 private static void AssertProviderNameNotNull( string providerName )
		 {
			  if ( string.ReferenceEquals( providerName, null ) )
			  {
					throw new ProcedureException( Neo4Net.Kernel.Api.Exceptions.Status_Procedure.ProcedureCallFailed, IndexProviderNullMessage() );
			  }
		 }

		 private static string IndexProviderNullMessage()
		 {
			  return "Could not create index with specified index provider being null.";
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private int getLabelId(String labelName) throws org.Neo4Net.internal.kernel.api.exceptions.ProcedureException
		 private int GetLabelId( string labelName )
		 {
			  int labelId = _ktx.tokenRead().nodeLabel(labelName);
			  if ( labelId == Neo4Net.Internal.Kernel.Api.TokenRead_Fields.NO_TOKEN )
			  {
					throw new ProcedureException( Neo4Net.Kernel.Api.Exceptions.Status_Schema.LabelAccessFailed, "No such label %s", labelName );
			  }
			  return labelId;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private int[] getPropertyIds(String[] propertyKeyNames) throws org.Neo4Net.internal.kernel.api.exceptions.ProcedureException
		 private int[] GetPropertyIds( string[] propertyKeyNames )
		 {
			  int[] propertyKeyIds = new int[propertyKeyNames.Length];
			  for ( int i = 0; i < propertyKeyIds.Length; i++ )
			  {

					int propertyKeyId = _ktx.tokenRead().propertyKey(propertyKeyNames[i]);
					if ( propertyKeyId == Neo4Net.Internal.Kernel.Api.TokenRead_Fields.NO_TOKEN )
					{
						 throw new ProcedureException( Neo4Net.Kernel.Api.Exceptions.Status_Schema.PropertyKeyAccessFailed, "No such property key %s", propertyKeyNames[i] );
					}
					propertyKeyIds[i] = propertyKeyId;
			  }
			  return propertyKeyIds;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private int getOrCreateLabelId(String labelName) throws org.Neo4Net.internal.kernel.api.exceptions.ProcedureException
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
//ORIGINAL LINE: private int[] getOrCreatePropertyIds(String[] propertyKeyNames) throws org.Neo4Net.internal.kernel.api.exceptions.ProcedureException
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
//ORIGINAL LINE: private org.Neo4Net.internal.kernel.api.IndexReference getIndex(IndexSpecifier specifier) throws org.Neo4Net.internal.kernel.api.exceptions.ProcedureException
		 private IndexReference GetIndex( IndexSpecifier specifier )
		 {
			  if ( !string.ReferenceEquals( specifier.Name(), null ) )
			  {
					// Find index by name.
					IndexReference indexReference = _ktx.schemaRead().indexGetForName(specifier.Name());

					if ( indexReference == IndexReference.NO_INDEX )
					{
						 throw new ProcedureException( Neo4Net.Kernel.Api.Exceptions.Status_Schema.IndexNotFound, "No such index '%s'", specifier );
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
						 throw new ProcedureException( Neo4Net.Kernel.Api.Exceptions.Status_Schema.IndexNotFound, "No such index %s", specifier );
					}
					return indexReference;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void waitUntilOnline(org.Neo4Net.internal.kernel.api.IndexReference index, IndexSpecifier indexDescription, long timeout, java.util.concurrent.TimeUnit timeoutUnits) throws org.Neo4Net.internal.kernel.api.exceptions.ProcedureException
		 private void WaitUntilOnline( IndexReference index, IndexSpecifier indexDescription, long timeout, TimeUnit timeoutUnits )
		 {
			  try
			  {
					Predicates.awaitEx( () => IsOnline(indexDescription, index), timeout, timeoutUnits );
			  }
			  catch ( TimeoutException )
			  {
					throw new ProcedureException( Neo4Net.Kernel.Api.Exceptions.Status_Procedure.ProcedureTimedOut, "Index on %s did not come online within %s %s", indexDescription, timeout, timeoutUnits );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private boolean isOnline(IndexSpecifier specifier, org.Neo4Net.internal.kernel.api.IndexReference index) throws org.Neo4Net.internal.kernel.api.exceptions.ProcedureException
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
						 throw new ProcedureException( Neo4Net.Kernel.Api.Exceptions.Status_Schema.IndexCreationFailed, IndexPopulationFailure.appendCauseOfFailure( "Index %s is in failed state.", cause ), specifier );
					default:
						 throw new System.InvalidOperationException( "Unknown index state " + state );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.Neo4Net.internal.kernel.api.InternalIndexState getState(IndexSpecifier specifier, org.Neo4Net.internal.kernel.api.IndexReference index) throws org.Neo4Net.internal.kernel.api.exceptions.ProcedureException
		 private InternalIndexState GetState( IndexSpecifier specifier, IndexReference index )
		 {
			  try
			  {
					return _ktx.schemaRead().indexGetState(index);
			  }
			  catch ( IndexNotFoundKernelException e )
			  {
					throw new ProcedureException( Neo4Net.Kernel.Api.Exceptions.Status_Schema.IndexNotFound, e, "No such index %s", specifier );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private String getFailure(IndexSpecifier indexDescription, org.Neo4Net.internal.kernel.api.IndexReference index) throws org.Neo4Net.internal.kernel.api.exceptions.ProcedureException
		 private string GetFailure( IndexSpecifier indexDescription, IndexReference index )
		 {
			  try
			  {
					return _ktx.schemaRead().indexGetFailure(index);
			  }
			  catch ( IndexNotFoundKernelException e )
			  {
					throw new ProcedureException( Neo4Net.Kernel.Api.Exceptions.Status_Schema.IndexNotFound, e, "No such index %s", indexDescription );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void triggerSampling(org.Neo4Net.internal.kernel.api.IndexReference index) throws org.Neo4Net.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException
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