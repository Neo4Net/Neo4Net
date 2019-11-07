using System;
using System.Collections.Generic;
using System.Threading;

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
namespace Neo4Net.Kernel.impl.coreapi.schema
{

	using ConstraintViolationException = Neo4Net.GraphDb.ConstraintViolationException;
	using Label = Neo4Net.GraphDb.Label;
	using NotFoundException = Neo4Net.GraphDb.NotFoundException;
	using RelationshipType = Neo4Net.GraphDb.RelationshipType;
	using TransactionTerminatedException = Neo4Net.GraphDb.TransactionTerminatedException;
	using IndexPopulationProgress = Neo4Net.GraphDb.Index.IndexPopulationProgress;
	using ConstraintCreator = Neo4Net.GraphDb.Schema.ConstraintCreator;
	using ConstraintDefinition = Neo4Net.GraphDb.Schema.ConstraintDefinition;
	using IndexCreator = Neo4Net.GraphDb.Schema.IndexCreator;
	using IndexDefinition = Neo4Net.GraphDb.Schema.IndexDefinition;
	using Schema = Neo4Net.GraphDb.Schema.Schema;
	using IndexReference = Neo4Net.Kernel.Api.Internal.IndexReference;
	using InternalIndexState = Neo4Net.Kernel.Api.Internal.InternalIndexState;
	using SchemaRead = Neo4Net.Kernel.Api.Internal.SchemaRead;
	using TokenRead = Neo4Net.Kernel.Api.Internal.TokenRead;
	using TokenWrite = Neo4Net.Kernel.Api.Internal.TokenWrite;
	using InvalidTransactionTypeKernelException = Neo4Net.Kernel.Api.Internal.Exceptions.InvalidTransactionTypeKernelException;
	using KernelException = Neo4Net.Kernel.Api.Internal.Exceptions.KernelException;
	using CreateConstraintFailureException = Neo4Net.Kernel.Api.Internal.Exceptions.Schema.CreateConstraintFailureException;
	using IllegalTokenNameException = Neo4Net.Kernel.Api.Internal.Exceptions.Schema.IllegalTokenNameException;
	using IndexNotFoundKernelException = Neo4Net.Kernel.Api.Internal.Exceptions.Schema.IndexNotFoundKernelException;
	using SchemaKernelException = Neo4Net.Kernel.Api.Internal.Exceptions.Schema.SchemaKernelException;
	using TooManyLabelsException = Neo4Net.Kernel.Api.Internal.Exceptions.Schema.TooManyLabelsException;
	using LabelSchemaDescriptor = Neo4Net.Kernel.Api.Internal.Schema.LabelSchemaDescriptor;
	using RelationTypeSchemaDescriptor = Neo4Net.Kernel.Api.Internal.Schema.RelationTypeSchemaDescriptor;
	using SchemaDescriptor = Neo4Net.Kernel.Api.Internal.Schema.SchemaDescriptor;
	using ConstraintDescriptor = Neo4Net.Kernel.Api.Internal.Schema.constraints.ConstraintDescriptor;
	using KernelTransaction = Neo4Net.Kernel.Api.KernelTransaction;
	using SilentTokenNameLookup = Neo4Net.Kernel.Api.SilentTokenNameLookup;
	using Statement = Neo4Net.Kernel.Api.Statement;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using AlreadyConstrainedException = Neo4Net.Kernel.Api.Exceptions.schema.AlreadyConstrainedException;
	using AlreadyIndexedException = Neo4Net.Kernel.Api.Exceptions.schema.AlreadyIndexedException;
	using DropConstraintFailureException = Neo4Net.Kernel.Api.Exceptions.schema.DropConstraintFailureException;
	using DropIndexFailureException = Neo4Net.Kernel.Api.Exceptions.schema.DropIndexFailureException;
	using RepeatedSchemaComponentException = Neo4Net.Kernel.Api.Exceptions.schema.RepeatedSchemaComponentException;
	using SchemaRuleNotFoundException = Neo4Net.Kernel.Api.Exceptions.schema.SchemaRuleNotFoundException;
	using SchemaDescriptorFactory = Neo4Net.Kernel.Api.schema.SchemaDescriptorFactory;
	using ConstraintDescriptorFactory = Neo4Net.Kernel.Api.schema.constraints.ConstraintDescriptorFactory;
	using NodeExistenceConstraintDescriptor = Neo4Net.Kernel.Api.schema.constraints.NodeExistenceConstraintDescriptor;
	using NodeKeyConstraintDescriptor = Neo4Net.Kernel.Api.schema.constraints.NodeKeyConstraintDescriptor;
	using RelExistenceConstraintDescriptor = Neo4Net.Kernel.Api.schema.constraints.RelExistenceConstraintDescriptor;
	using UniquenessConstraintDescriptor = Neo4Net.Kernel.Api.schema.constraints.UniquenessConstraintDescriptor;
	using IndexPopulationFailure = Neo4Net.Kernel.Impl.Api.index.IndexPopulationFailure;
	using EntityType = Neo4Net.Kernel.Api.StorageEngine.EntityType;
	using PopulationProgress = Neo4Net.Kernel.Api.StorageEngine.schema.PopulationProgress;
	using SchemaRule = Neo4Net.Kernel.Api.StorageEngine.schema.SchemaRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.graphdb.Label.label;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.graphdb.RelationshipType.withName;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.GraphDb.Schema.Schema_IndexState.FAILED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.GraphDb.Schema.Schema_IndexState.ONLINE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.GraphDb.Schema.Schema_IndexState.POPULATING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.collection.Iterables.single;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.collection.Iterators.addToCollection;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.collection.Iterators.asCollection;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.collection.Iterators.map;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.api.schema.SchemaDescriptorFactory.forLabel;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.api.schema.SchemaDescriptorFactory.forRelType;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.api.schema.SchemaDescriptorFactory.multiToken;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.coreapi.schema.IndexDefinitionImpl.labelNameList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.coreapi.schema.PropertyNameUtils.getOrCreatePropertyKeyIds;

	public class SchemaImpl : Schema
	{
		 private readonly System.Func<KernelTransaction> _transactionSupplier;
		 private readonly InternalSchemaActions _actions;

		 public SchemaImpl( System.Func<KernelTransaction> transactionSupplier )
		 {
			  this._transactionSupplier = transactionSupplier;
			  this._actions = new GDBSchemaActions( transactionSupplier );
		 }

		 public override IndexCreator IndexFor( Label label )
		 {
			  return new IndexCreatorImpl( _actions, label );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public Iterable<Neo4Net.GraphDb.Schema.IndexDefinition> getIndexes(final Neo4Net.graphdb.Label label)
		 public override IEnumerable<IndexDefinition> GetIndexes( Label label )
		 {
			  KernelTransaction transaction = _transactionSupplier.get();
			  using ( Statement ignore = transaction.AcquireStatement() )
			  {
					TokenRead tokenRead = transaction.TokenRead();
					SchemaRead schemaRead = transaction.SchemaRead();
					IList<IndexDefinition> definitions = new List<IndexDefinition>();
					int labelId = tokenRead.NodeLabel( label.Name() );
					if ( labelId == Neo4Net.Kernel.Api.Internal.TokenRead_Fields.NO_TOKEN )
					{
						 return emptyList();
					}
					IEnumerator<IndexReference> indexes = schemaRead.IndexesGetForLabel( labelId );
					AddDefinitions( definitions, tokenRead, IndexReference.sortByType( indexes ) );
					return definitions;
			  }
		 }

		 public override IEnumerable<IndexDefinition> GetIndexes()
		 {
			  KernelTransaction transaction = _transactionSupplier.get();
			  SchemaRead schemaRead = transaction.SchemaRead();
			  using ( Statement ignore = transaction.AcquireStatement() )
			  {
					IList<IndexDefinition> definitions = new List<IndexDefinition>();

					IEnumerator<IndexReference> indexes = schemaRead.IndexesGetAll();
					AddDefinitions( definitions, transaction.TokenRead(), IndexReference.sortByType(indexes) );
					return definitions;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private Neo4Net.GraphDb.Schema.IndexDefinition descriptorToDefinition(final Neo4Net.Kernel.Api.Internal.TokenRead tokenRead, Neo4Net.Kernel.Api.Internal.IndexReference index)
		 private IndexDefinition DescriptorToDefinition( TokenRead tokenRead, IndexReference index )
		 {
			  try
			  {
					SchemaDescriptor schema = index.Schema();
					int[] IEntityTokenIds = Schema.EntityTokenIds;
					bool constraintIndex = index.Unique;
					string[] propertyNames = PropertyNameUtils.GetPropertyKeys( tokenRead, index.Properties() );
					switch ( Schema.entityType() )
					{
					case NODE:
						 Label[] labels = new Label[entityTokenIds.Length];
						 for ( int i = 0; i < labels.Length; i++ )
						 {
							  labels[i] = label( tokenRead.NodeLabelName( IEntityTokenIds[i] ) );
						 }
						 return new IndexDefinitionImpl( _actions, index, labels, propertyNames, constraintIndex );
					case RELATIONSHIP:
						 RelationshipType[] relTypes = new RelationshipType[entityTokenIds.Length];
						 for ( int i = 0; i < relTypes.Length; i++ )
						 {
							  relTypes[i] = withName( tokenRead.RelationshipTypeName( IEntityTokenIds[i] ) );
						 }
						 return new IndexDefinitionImpl( _actions, index, relTypes, propertyNames, constraintIndex );
					default:
						 throw new System.ArgumentException( "Cannot create IndexDefinition for " + Schema.entityType() + " IEntity-typed schema." );
					}
			  }
			  catch ( KernelException e )
			  {
					throw new Exception( e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private void addDefinitions(java.util.List<Neo4Net.GraphDb.Schema.IndexDefinition> definitions, final Neo4Net.Kernel.Api.Internal.TokenRead tokenRead, java.util.Iterator<Neo4Net.Kernel.Api.Internal.IndexReference> indexes)
		 private void AddDefinitions( IList<IndexDefinition> definitions, TokenRead tokenRead, IEnumerator<IndexReference> indexes )
		 {
			  addToCollection( map( index => DescriptorToDefinition( tokenRead, index ), indexes ), definitions );
		 }

		 public override void AwaitIndexOnline( IndexDefinition index, long duration, TimeUnit unit )
		 {
			  _actions.assertInOpenTransaction();
			  long timeout = DateTimeHelper.CurrentUnixTimeMillis() + unit.toMillis(duration);
			  do
			  {
					Neo4Net.GraphDb.Schema.Schema_IndexState state = GetIndexState( index );
					switch ( state )
					{
					case Neo4Net.GraphDb.Schema.Schema_IndexState.Online:
						 return;
					case Neo4Net.GraphDb.Schema.Schema_IndexState.Failed:
						 string cause = GetIndexFailure( index );
						 string message = IndexPopulationFailure.appendCauseOfFailure( string.Format( "Index {0} entered a {1} state. Please see database logs.", index, state ), cause );
						 throw new System.InvalidOperationException( message );
					default:
						 try
						 {
							  Thread.Sleep( 100 );
						 }
						 catch ( InterruptedException )
						 {
							  // Ignore interrupted exceptions here.
						 }
						 break;
					}
			  } while ( DateTimeHelper.CurrentUnixTimeMillis() < timeout );
			  throw new System.InvalidOperationException( "Expected index to come online within a reasonable time." );
		 }

		 public override void AwaitIndexesOnline( long duration, TimeUnit unit )
		 {
			  _actions.assertInOpenTransaction();
			  long millisLeft = TimeUnit.MILLISECONDS.convert( duration, unit );
			  ICollection<IndexDefinition> onlineIndexes = new List<IndexDefinition>();

			  for ( IEnumerator<IndexDefinition> iter = Indexes.GetEnumerator(); iter.MoveNext(); )
			  {
					if ( millisLeft < 0 )
					{
						 throw new System.InvalidOperationException( "Expected all indexes to come online within a reasonable time." + "Indexes brought online: " + onlineIndexes + ". Indexes not guaranteed to be online: " + asCollection( iter ) );
					}

					IndexDefinition index = iter.Current;

					long millisBefore = DateTimeHelper.CurrentUnixTimeMillis();
					AwaitIndexOnline( index, millisLeft, TimeUnit.MILLISECONDS );
					millisLeft -= DateTimeHelper.CurrentUnixTimeMillis() - millisBefore;

					onlineIndexes.Add( index );
			  }
		 }

		 public override IndexDefinition GetIndexByName( string indexName )
		 {
			  Objects.requireNonNull( indexName );
			  IEnumerator<IndexDefinition> indexes = Indexes.GetEnumerator();
			  IndexDefinition index = null;
			  while ( indexes.MoveNext() )
			  {
					IndexDefinition candidate = indexes.Current;
					if ( candidate.Name.Equals( indexName ) )
					{
						 if ( index != null )
						 {
							  throw new System.InvalidOperationException( "Multiple indexes found by the name '" + indexName + "'. " + "Try iterating Schema#getIndexes() and filter by name instead." );
						 }
						 index = candidate;
					}
			  }
			  if ( index == null )
			  {
					throw new System.ArgumentException( "No index found with the name '" + indexName + "'." );
			  }
			  return index;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public Neo4Net.GraphDb.Schema.Schema_IndexState getIndexState(final Neo4Net.GraphDb.Schema.IndexDefinition index)
		 public override Neo4Net.GraphDb.Schema.Schema_IndexState GetIndexState( IndexDefinition index )
		 {
			  KernelTransaction transaction = SafeAcquireTransaction( _transactionSupplier );
			  try
			  {
					  using ( Statement ignore = transaction.AcquireStatement() )
					  {
      
						SchemaRead schemaRead = transaction.SchemaRead();
						IndexReference reference = GetIndexReference( schemaRead, transaction.TokenRead(), (IndexDefinitionImpl) index );
						InternalIndexState indexState = schemaRead.IndexGetState( reference );
						switch ( indexState )
						{
						case InternalIndexState.POPULATING:
							 return POPULATING;
						case InternalIndexState.ONLINE:
							 return ONLINE;
						case InternalIndexState.FAILED:
							 return FAILED;
						default:
							 throw new System.ArgumentException( string.Format( "Illegal index state {0}", indexState ) );
						}
					  }
			  }
			  catch ( Exception e ) when ( e is SchemaRuleNotFoundException || e is IndexNotFoundKernelException )
			  {
					throw NewIndexNotFoundException( index, e );
			  }
		 }

		 private NotFoundException NewIndexNotFoundException( IndexDefinition index, KernelException e )
		 {
			  return new NotFoundException( "No index was found corresponding to " + index + ".", e );
		 }

		 public override IndexPopulationProgress GetIndexPopulationProgress( IndexDefinition index )
		 {
			  KernelTransaction transaction = SafeAcquireTransaction( _transactionSupplier );
			  try
			  {
					  using ( Statement ignore = transaction.AcquireStatement() )
					  {
						SchemaRead schemaRead = transaction.SchemaRead();
						IndexReference descriptor = GetIndexReference( schemaRead, transaction.TokenRead(), (IndexDefinitionImpl) index );
						PopulationProgress progress = schemaRead.IndexGetPopulationProgress( descriptor );
						return progress.ToIndexPopulationProgress();
					  }
			  }
			  catch ( Exception e ) when ( e is SchemaRuleNotFoundException || e is IndexNotFoundKernelException )
			  {
					throw NewIndexNotFoundException( index, e );
			  }
		 }

		 public override string GetIndexFailure( IndexDefinition index )
		 {
			  KernelTransaction transaction = SafeAcquireTransaction( _transactionSupplier );
			  try
			  {
					  using ( Statement ignore = transaction.AcquireStatement() )
					  {
						SchemaRead schemaRead = transaction.SchemaRead();
						IndexReference descriptor = GetIndexReference( schemaRead, transaction.TokenRead(), (IndexDefinitionImpl) index );
						return schemaRead.IndexGetFailure( descriptor );
					  }
			  }
			  catch ( Exception e ) when ( e is SchemaRuleNotFoundException || e is IndexNotFoundKernelException )
			  {
					throw NewIndexNotFoundException( index, e );
			  }
		 }

		 public override ConstraintCreator ConstraintFor( Label label )
		 {
			  _actions.assertInOpenTransaction();
			  return new BaseNodeConstraintCreator( _actions, label );
		 }

		 public virtual IEnumerable<ConstraintDefinition> Constraints
		 {
			 get
			 {
				  KernelTransaction transaction = SafeAcquireTransaction( _transactionSupplier );
				  using ( Statement ignore = transaction.AcquireStatement() )
				  {
						return AsConstraintDefinitions( transaction.SchemaRead().constraintsGetAll(), transaction.TokenRead() );
				  }
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public Iterable<Neo4Net.GraphDb.Schema.ConstraintDefinition> getConstraints(final Neo4Net.graphdb.Label label)
		 public virtual IEnumerable<ConstraintDefinition> getConstraints( Label label )
		 {
			  KernelTransaction transaction = SafeAcquireTransaction( _transactionSupplier );
			  using ( Statement ignore = transaction.AcquireStatement() )
			  {
					TokenRead tokenRead = transaction.TokenRead();
					SchemaRead schemaRead = transaction.SchemaRead();
					int labelId = tokenRead.NodeLabel( label.Name() );
					if ( labelId == Neo4Net.Kernel.Api.Internal.TokenRead_Fields.NO_TOKEN )
					{
						 return emptyList();
					}
					return AsConstraintDefinitions( schemaRead.ConstraintsGetForLabel( labelId ), tokenRead );
			  }
		 }

		 public virtual IEnumerable<ConstraintDefinition> getConstraints( RelationshipType type )
		 {
			  KernelTransaction transaction = SafeAcquireTransaction( _transactionSupplier );
			  using ( Statement ignore = transaction.AcquireStatement() )
			  {
					TokenRead tokenRead = transaction.TokenRead();
					SchemaRead schemaRead = transaction.SchemaRead();
					int typeId = tokenRead.RelationshipType( type.Name() );
					if ( typeId == Neo4Net.Kernel.Api.Internal.TokenRead_Fields.NO_TOKEN )
					{
						 return emptyList();
					}
					return AsConstraintDefinitions( schemaRead.ConstraintsGetForRelationshipType( typeId ), tokenRead );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static Neo4Net.Kernel.Api.Internal.IndexReference getIndexReference(Neo4Net.Kernel.Api.Internal.SchemaRead schemaRead, Neo4Net.Kernel.Api.Internal.TokenRead tokenRead, IndexDefinitionImpl index) throws Neo4Net.kernel.api.exceptions.schema.SchemaRuleNotFoundException
		 private static IndexReference GetIndexReference( SchemaRead schemaRead, TokenRead tokenRead, IndexDefinitionImpl index )
		 {
			  // Use the precise embedded index reference when available.
			  IndexReference reference = index.IndexReference;
			  if ( reference != null )
			  {
					return reference;
			  }

			  // Otherwise attempt to reverse engineer the schema that will let us look up the real IndexReference.
			  int[] propertyKeyIds = ResolveAndValidatePropertyKeys( tokenRead, index.PropertyKeysArrayShared );
			  SchemaDescriptor schema;

			  if ( index.NodeIndex )
			  {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
					int[] labelIds = ResolveAndValidateTokens( "Label", index.LabelArrayShared, Label::name, tokenRead.nodeLabel );

					if ( index.MultiTokenIndex )
					{
						 schema = multiToken( labelIds, EntityType.NODE, propertyKeyIds );
					}
					else
					{
						 schema = forLabel( labelIds[0], propertyKeyIds );
					}
			  }
			  else if ( index.RelationshipIndex )
			  {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
					int[] relTypes = ResolveAndValidateTokens( "Relationship type", index.RelationshipTypesArrayShared, RelationshipType::name, tokenRead.relationshipType );

					if ( index.MultiTokenIndex )
					{
						 schema = multiToken( relTypes, EntityType.RELATIONSHIP, propertyKeyIds );
					}
					else
					{
						 schema = forRelType( relTypes[0], propertyKeyIds );
					}
			  }
			  else
			  {
					throw new System.ArgumentException( "The given index is neither a node index, nor a relationship index: " + index + "." );
			  }

			  reference = schemaRead.Index( schema );
			  if ( reference == IndexReference.NO_INDEX )
			  {
					throw new SchemaRuleNotFoundException( Neo4Net.Kernel.Api.StorageEngine.schema.SchemaRule_Kind.IndexRule, schema );
			  }

			  return reference;
		 }

		 private static int[] ResolveAndValidatePropertyKeys( TokenRead tokenRead, string[] propertyKeys )
		 {
			  return ResolveAndValidateTokens( "Property key", propertyKeys, s => s, tokenRead.propertyKey );
		 }

		 private static int[] ResolveAndValidateTokens<T>( string tokenTypeName, T[] tokens, System.Func<T, string> getTokenName, System.Func<string, int> getTokenId )
		 {
			  int[] tokenIds = new int[tokens.Length];
			  for ( int i = 0; i < tokenIds.Length; i++ )
			  {
					string tokenName = getTokenName( tokens[i] );
					int tokenId = getTokenId( tokenName );
					if ( tokenId == Neo4Net.Kernel.Api.Internal.TokenRead_Fields.NO_TOKEN )
					{
						 throw new NotFoundException( tokenTypeName + " " + tokenName + " not found." );
					}
					tokenIds[i] = tokenId;
			  }
			  return tokenIds;
		 }

		 private IEnumerable<ConstraintDefinition> AsConstraintDefinitions<T1>( IEnumerator<T1> constraints, TokenRead tokenRead ) where T1 : Neo4Net.Kernel.Api.Internal.Schema.constraints.ConstraintDescriptor
		 {
			  // Intentionally create an eager list so that used statement can be closed
			  IList<ConstraintDefinition> definitions = new List<ConstraintDefinition>();

			  while ( constraints.MoveNext() )
			  {
					ConstraintDescriptor constraint = constraints.Current;
					definitions.Add( AsConstraintDefinition( constraint, tokenRead ) );
			  }

			  return definitions;
		 }

		 private ConstraintDefinition AsConstraintDefinition( ConstraintDescriptor constraint, TokenRead tokenRead )
		 {
			  // This was turned inside out. Previously a low-level constraint object would reference a public enum type
			  // which made it impossible to break out the low-level component from kernel. There could be a lower level
			  // constraint type introduced to mimic the public ConstraintType, but that would be a duplicate of it
			  // essentially. Checking instanceof here is OKish since the objects it checks here are part of the
			  // internal storage engine API.
			  SilentTokenNameLookup lookup = new SilentTokenNameLookup( tokenRead );
			  if ( constraint is NodeExistenceConstraintDescriptor || constraint is NodeKeyConstraintDescriptor || constraint is UniquenessConstraintDescriptor )
			  {
					SchemaDescriptor schemaDescriptor = constraint.Schema();
					int[] IEntityTokenIds = schemaDescriptor.EntityTokenIds;
					Label[] labels = new Label[entityTokenIds.Length];
					for ( int i = 0; i < IEntityTokenIds.Length; i++ )
					{
						 labels[i] = label( lookup.LabelGetName( IEntityTokenIds[i] ) );
					}
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
					string[] propertyKeys = Arrays.stream( schemaDescriptor.PropertyIds ).mapToObj( lookup.propertyKeyGetName ).toArray( string[]::new );
					if ( constraint is NodeExistenceConstraintDescriptor )
					{
						 return new NodePropertyExistenceConstraintDefinition( _actions, labels[0], propertyKeys );
					}
					else if ( constraint is UniquenessConstraintDescriptor )
					{
						 return new UniquenessConstraintDefinition( _actions, new IndexDefinitionImpl( _actions, null, labels, propertyKeys, true ) );
					}
					else
					{
						 return new NodeKeyConstraintDefinition( _actions, new IndexDefinitionImpl( _actions, null, labels, propertyKeys, true ) );
					}
			  }
			  else if ( constraint is RelExistenceConstraintDescriptor )
			  {
					RelationTypeSchemaDescriptor descriptor = ( RelationTypeSchemaDescriptor ) constraint.Schema();
					return new RelationshipPropertyExistenceConstraintDefinition( _actions, withName( lookup.RelationshipTypeGetName( descriptor.RelTypeId ) ), lookup.PropertyKeyGetName( descriptor.PropertyId ) );
			  }
			  throw new System.ArgumentException( "Unknown constraint " + constraint );
		 }

		 private static KernelTransaction SafeAcquireTransaction( System.Func<KernelTransaction> transactionSupplier )
		 {
			  KernelTransaction transaction = transactionSupplier();
			  if ( transaction.Terminated )
			  {
					Status terminationReason = transaction.ReasonIfTerminated.orElse( Neo4Net.Kernel.Api.Exceptions.Status_Transaction.Terminated );
					throw new TransactionTerminatedException( terminationReason );
			  }
			  return transaction;
		 }

		 private class GDBSchemaActions : InternalSchemaActions
		 {
			  internal readonly System.Func<KernelTransaction> TransactionSupplier;

			  internal GDBSchemaActions( System.Func<KernelTransaction> transactionSupplier )
			  {
					this.TransactionSupplier = transactionSupplier;
			  }

			  public override IndexDefinition CreateIndexDefinition( Label label, Optional<string> indexName, params string[] propertyKeys )
			  {
					KernelTransaction transaction = SafeAcquireTransaction( TransactionSupplier );

					using ( Statement ignore = transaction.AcquireStatement() )
					{
						 try
						 {
							  TokenWrite tokenWrite = transaction.TokenWrite();
							  int labelId = tokenWrite.LabelGetOrCreateForName( label.Name() );
							  int[] propertyKeyIds = getOrCreatePropertyKeyIds( tokenWrite, propertyKeys );
							  LabelSchemaDescriptor descriptor = forLabel( labelId, propertyKeyIds );
							  IndexReference indexReference = transaction.SchemaWrite().indexCreate(descriptor, indexName);
							  return new IndexDefinitionImpl( this, indexReference, new Label[]{ label }, propertyKeys, false );
						 }

						 catch ( IllegalTokenNameException e )
						 {
							  throw new System.ArgumentException( e );
						 }
						 catch ( Exception e ) when ( e is InvalidTransactionTypeKernelException || e is SchemaKernelException )
						 {
							  throw new ConstraintViolationException( e.GetUserMessage( new SilentTokenNameLookup( transaction.TokenRead() ) ), e );
						 }
					}
			  }

			  public override void DropIndexDefinitions( IndexDefinition indexDefinition )
			  {
					KernelTransaction transaction = SafeAcquireTransaction( TransactionSupplier );
					using ( Statement ignore = transaction.AcquireStatement() )
					{
						 try
						 {
							  IndexReference reference = GetIndexReference( transaction.SchemaRead(), transaction.TokenRead(), (IndexDefinitionImpl) indexDefinition );
							  transaction.SchemaWrite().indexDrop(reference);
						 }
						 catch ( NotFoundException )
						 {
							  // Silently ignore invalid label and property names
						 }
						 catch ( Exception e ) when ( e is SchemaRuleNotFoundException || e is DropIndexFailureException )
						 {
							  throw new ConstraintViolationException( e.getUserMessage( new SilentTokenNameLookup( transaction.TokenRead() ) ), e );
						 }
						 catch ( Exception e ) when ( e is InvalidTransactionTypeKernelException || e is SchemaKernelException )
						 {
							  throw new ConstraintViolationException( e.Message, e );
						 }
					}
			  }

			  public override ConstraintDefinition CreatePropertyUniquenessConstraint( IndexDefinition indexDefinition )
			  {
					if ( indexDefinition.MultiTokenIndex )
					{
						 throw new ConstraintViolationException( "A property uniqueness constraint does not support multi-token index definitions. " + "That is, only a single label is supported, but the following labels were provided: " + labelNameList( indexDefinition.Labels, "", "." ) );
					}
					KernelTransaction transaction = SafeAcquireTransaction( TransactionSupplier );
					using ( Statement ignore = transaction.AcquireStatement() )
					{
						 try
						 {
							  TokenWrite tokenWrite = transaction.TokenWrite();
							  int labelId = tokenWrite.LabelGetOrCreateForName( single( indexDefinition.Labels ).name() );
							  int[] propertyKeyIds = getOrCreatePropertyKeyIds( tokenWrite, indexDefinition );
							  transaction.SchemaWrite().uniquePropertyConstraintCreate(forLabel(labelId, propertyKeyIds));
							  return new UniquenessConstraintDefinition( this, indexDefinition );
						 }
						 catch ( Exception e ) when ( e is AlreadyConstrainedException || e is CreateConstraintFailureException || e is AlreadyIndexedException || e is RepeatedSchemaComponentException )
						 {
							  throw new ConstraintViolationException( e.getUserMessage( new SilentTokenNameLookup( transaction.TokenRead() ) ), e );
						 }
						 catch ( IllegalTokenNameException e )
						 {
							  throw new System.ArgumentException( e );
						 }
						 catch ( TooManyLabelsException e )
						 {
							  throw new System.InvalidOperationException( e );
						 }
						 catch ( Exception e ) when ( e is InvalidTransactionTypeKernelException || e is SchemaKernelException )
						 {
							  throw new ConstraintViolationException( e.Message, e );
						 }
					}
			  }

			  public override ConstraintDefinition CreateNodeKeyConstraint( IndexDefinition indexDefinition )
			  {
					if ( indexDefinition.MultiTokenIndex )
					{
						 throw new ConstraintViolationException( "A node key constraint does not support multi-token index definitions. " + "That is, only a single label is supported, but the following labels were provided: " + labelNameList( indexDefinition.Labels, "", "." ) );
					}
					KernelTransaction transaction = SafeAcquireTransaction( TransactionSupplier );
					using ( Statement ignore = transaction.AcquireStatement() )
					{
						 try
						 {
							  TokenWrite tokenWrite = transaction.TokenWrite();
							  int labelId = tokenWrite.LabelGetOrCreateForName( single( indexDefinition.Labels ).name() );
							  int[] propertyKeyIds = getOrCreatePropertyKeyIds( tokenWrite, indexDefinition );
							  transaction.SchemaWrite().nodeKeyConstraintCreate(forLabel(labelId, propertyKeyIds));
							  return new NodeKeyConstraintDefinition( this, indexDefinition );
						 }
						 catch ( Exception e ) when ( e is AlreadyConstrainedException || e is CreateConstraintFailureException || e is AlreadyIndexedException || e is RepeatedSchemaComponentException )
						 {
							  throw new ConstraintViolationException( e.getUserMessage( new SilentTokenNameLookup( transaction.TokenRead() ) ), e );
						 }
						 catch ( IllegalTokenNameException e )
						 {
							  throw new System.ArgumentException( e );
						 }
						 catch ( TooManyLabelsException e )
						 {
							  throw new System.InvalidOperationException( e );
						 }
						 catch ( Exception e ) when ( e is InvalidTransactionTypeKernelException || e is SchemaKernelException )
						 {
							  throw new ConstraintViolationException( e.Message, e );
						 }
					}
			  }

			  public override ConstraintDefinition CreatePropertyExistenceConstraint( Label label, params string[] propertyKeys )
			  {
					KernelTransaction transaction = SafeAcquireTransaction( TransactionSupplier );
					using ( Statement ignore = transaction.AcquireStatement() )
					{
						 try
						 {
							  TokenWrite tokenWrite = transaction.TokenWrite();
							  int labelId = tokenWrite.LabelGetOrCreateForName( label.Name() );
							  int[] propertyKeyIds = getOrCreatePropertyKeyIds( tokenWrite, propertyKeys );
							  transaction.SchemaWrite().nodePropertyExistenceConstraintCreate(forLabel(labelId, propertyKeyIds));
							  return new NodePropertyExistenceConstraintDefinition( this, label, propertyKeys );
						 }
						 catch ( Exception e ) when ( e is AlreadyConstrainedException || e is CreateConstraintFailureException || e is RepeatedSchemaComponentException )
						 {
							  throw new ConstraintViolationException( e.getUserMessage( new SilentTokenNameLookup( transaction.TokenRead() ) ), e );
						 }
						 catch ( IllegalTokenNameException e )
						 {
							  throw new System.ArgumentException( e );
						 }
						 catch ( TooManyLabelsException e )
						 {
							  throw new System.InvalidOperationException( e );
						 }
						 catch ( Exception e ) when ( e is InvalidTransactionTypeKernelException || e is SchemaKernelException )
						 {
							  throw new ConstraintViolationException( e.Message, e );
						 }
					}
			  }

			  public override ConstraintDefinition CreatePropertyExistenceConstraint( RelationshipType type, string propertyKey )
			  {
					KernelTransaction transaction = SafeAcquireTransaction( TransactionSupplier );
					using ( Statement ignore = transaction.AcquireStatement() )
					{
						 try
						 {
							  TokenWrite tokenWrite = transaction.TokenWrite();
							  int typeId = tokenWrite.RelationshipTypeGetOrCreateForName( type.Name() );
							  int[] propertyKeyId = getOrCreatePropertyKeyIds( tokenWrite, propertyKey );
							  transaction.SchemaWrite().relationshipPropertyExistenceConstraintCreate(SchemaDescriptorFactory.forRelType(typeId, propertyKeyId));
							  return new RelationshipPropertyExistenceConstraintDefinition( this, type, propertyKey );
						 }
						 catch ( Exception e ) when ( e is AlreadyConstrainedException || e is CreateConstraintFailureException || e is RepeatedSchemaComponentException )
						 {
							  throw new ConstraintViolationException( e.getUserMessage( new SilentTokenNameLookup( transaction.TokenRead() ) ), e );
						 }
						 catch ( IllegalTokenNameException e )
						 {
							  throw new System.ArgumentException( e );
						 }
						 catch ( Exception e ) when ( e is InvalidTransactionTypeKernelException || e is SchemaKernelException )
						 {
							  throw new ConstraintViolationException( e.Message, e );
						 }
					}
			  }

			  public override void DropPropertyUniquenessConstraint( Label label, string[] properties )
			  {
					KernelTransaction transaction = SafeAcquireTransaction( TransactionSupplier );
					using ( Statement ignore = transaction.AcquireStatement() )
					{
						 try
						 {
							  TokenRead tokenRead = transaction.TokenRead();
							  int labelId = tokenRead.NodeLabel( label.Name() );
							  int[] propertyKeyIds = ResolveAndValidatePropertyKeys( tokenRead, properties );
							  transaction.SchemaWrite().constraintDrop(ConstraintDescriptorFactory.uniqueForLabel(labelId, propertyKeyIds));
						 }
						 catch ( DropConstraintFailureException e )
						 {
							  throw new ConstraintViolationException( e.GetUserMessage( new SilentTokenNameLookup( transaction.TokenRead() ) ), e );
						 }
						 catch ( Exception e ) when ( e is InvalidTransactionTypeKernelException || e is SchemaKernelException )
						 {
							  throw new ConstraintViolationException( e.Message, e );
						 }
					}
			  }

			  public override void DropNodeKeyConstraint( Label label, string[] properties )
			  {
					KernelTransaction transaction = SafeAcquireTransaction( TransactionSupplier );
					using ( Statement ignore = transaction.AcquireStatement() )
					{
						 try
						 {
							  TokenRead tokenRead = transaction.TokenRead();
							  int labelId = tokenRead.NodeLabel( label.Name() );
							  int[] propertyKeyIds = ResolveAndValidatePropertyKeys( tokenRead, properties );
							  transaction.SchemaWrite().constraintDrop(ConstraintDescriptorFactory.nodeKeyForLabel(labelId, propertyKeyIds));
						 }
						 catch ( DropConstraintFailureException e )
						 {
							  throw new ConstraintViolationException( e.GetUserMessage( new SilentTokenNameLookup( transaction.TokenRead() ) ), e );
						 }
						 catch ( Exception e ) when ( e is InvalidTransactionTypeKernelException || e is SchemaKernelException )
						 {
							  throw new ConstraintViolationException( e.Message, e );
						 }
					}
			  }

			  public override void DropNodePropertyExistenceConstraint( Label label, string[] properties )
			  {
					KernelTransaction transaction = SafeAcquireTransaction( TransactionSupplier );
					using ( Statement ignore = transaction.AcquireStatement() )
					{
						 try
						 {
							  TokenRead tokenRead = transaction.TokenRead();
							  int labelId = tokenRead.NodeLabel( label.Name() );
							  int[] propertyKeyIds = ResolveAndValidatePropertyKeys( tokenRead, properties );
							  transaction.SchemaWrite().constraintDrop(ConstraintDescriptorFactory.existsForLabel(labelId, propertyKeyIds));
						 }
						 catch ( DropConstraintFailureException e )
						 {
							  throw new ConstraintViolationException( e.GetUserMessage( new SilentTokenNameLookup( transaction.TokenRead() ) ), e );
						 }
						 catch ( Exception e ) when ( e is InvalidTransactionTypeKernelException || e is SchemaKernelException )
						 {
							  throw new ConstraintViolationException( e.Message, e );
						 }
					}
			  }

			  public override void DropRelationshipPropertyExistenceConstraint( RelationshipType type, string propertyKey )
			  {
					KernelTransaction transaction = SafeAcquireTransaction( TransactionSupplier );
					using ( Statement ignore = transaction.AcquireStatement() )
					{
						 try
						 {
							  TokenRead tokenRead = transaction.TokenRead();

							  int typeId = tokenRead.RelationshipType( type.Name() );
							  int propertyKeyId = tokenRead.PropertyKey( propertyKey );
							  transaction.SchemaWrite().constraintDrop(ConstraintDescriptorFactory.existsForRelType(typeId, propertyKeyId));
						 }
						 catch ( DropConstraintFailureException e )
						 {
							  throw new ConstraintViolationException( e.GetUserMessage( new SilentTokenNameLookup( transaction.TokenRead() ) ), e );
						 }
						 catch ( Exception e ) when ( e is InvalidTransactionTypeKernelException || e is SchemaKernelException )
						 {
							  throw new ConstraintViolationException( e.Message, e );
						 }
					}
			  }

			  public override string GetUserMessage( KernelException e )
			  {
					KernelTransaction transaction = SafeAcquireTransaction( TransactionSupplier );
					using ( Statement ignore = transaction.AcquireStatement() )
					{
						 return e.GetUserMessage( new SilentTokenNameLookup( transaction.TokenRead() ) );
					}
			  }

			  public override void AssertInOpenTransaction()
			  {
					KernelTransaction transaction = TransactionSupplier.get();
					if ( transaction.Terminated )
					{
						 Status terminationReason = transaction.ReasonIfTerminated.orElse( Neo4Net.Kernel.Api.Exceptions.Status_Transaction.Terminated );
						 throw new TransactionTerminatedException( terminationReason );
					}
			  }
		 }
	}

}