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
namespace Neo4Net.Kernel.Impl.Newapi
{

	using IndexReference = Neo4Net.@internal.Kernel.Api.IndexReference;
	using InternalIndexState = Neo4Net.@internal.Kernel.Api.InternalIndexState;
	using SchemaReadCore = Neo4Net.@internal.Kernel.Api.SchemaReadCore;
	using IndexNotFoundKernelException = Neo4Net.@internal.Kernel.Api.exceptions.schema.IndexNotFoundKernelException;
	using SchemaDescriptor = Neo4Net.@internal.Kernel.Api.schema.SchemaDescriptor;
	using ConstraintDescriptor = Neo4Net.@internal.Kernel.Api.schema.constraints.ConstraintDescriptor;
	using KernelTransactionImplementation = Neo4Net.Kernel.Impl.Api.KernelTransactionImplementation;
	using StorageSchemaReader = Neo4Net.Storageengine.Api.StorageSchemaReader;
	using IndexDescriptor = Neo4Net.Storageengine.Api.schema.IndexDescriptor;
	using PopulationProgress = Neo4Net.Storageengine.Api.schema.PopulationProgress;

	internal class SchemaReadCoreSnapshot : SchemaReadCore
	{
		 private readonly StorageSchemaReader _snapshot;
		 private readonly KernelTransactionImplementation _ktx;
		 private readonly AllStoreHolder _stores;

		 internal SchemaReadCoreSnapshot( StorageSchemaReader snapshot, KernelTransactionImplementation ktx, AllStoreHolder stores )
		 {
			  this._snapshot = snapshot;
			  this._ktx = ktx;
			  this._stores = stores;
		 }

		 public override IndexReference Index( SchemaDescriptor schema )
		 {
			  _ktx.assertOpen();
			  return _stores.indexGetForSchema( _snapshot, schema );
		 }

		 public override IEnumerator<IndexReference> IndexesGetForLabel( int labelId )
		 {
			  _ktx.assertOpen();
			  return _stores.indexesGetForLabel( _snapshot, labelId );
		 }

		 public override IEnumerator<IndexReference> IndexesGetForRelationshipType( int relationshipType )
		 {
			  _ktx.assertOpen();
			  return _stores.indexesGetForRelationshipType( _snapshot, relationshipType );
		 }

		 public override IEnumerator<IndexReference> IndexesGetAll()
		 {
			  _ktx.assertOpen();
			  //noinspection unchecked
			  return ( System.Collections.IEnumerator ) _stores.indexesGetAll( _snapshot );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.internal.kernel.api.InternalIndexState indexGetState(org.neo4j.internal.kernel.api.IndexReference index) throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException
		 public override InternalIndexState IndexGetState( IndexReference index )
		 {
			  AllStoreHolder.AssertValidIndex( index );
			  _ktx.assertOpen();
			  return _stores.indexGetState( _snapshot, ( IndexDescriptor ) index );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.storageengine.api.schema.PopulationProgress indexGetPopulationProgress(org.neo4j.internal.kernel.api.IndexReference index) throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException
		 public override PopulationProgress IndexGetPopulationProgress( IndexReference index )
		 {
			  AllStoreHolder.AssertValidIndex( index );
			  _ktx.assertOpen();
			  return _stores.indexGetPopulationProgress( _snapshot, index );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public String indexGetFailure(org.neo4j.internal.kernel.api.IndexReference index) throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException
		 public override string IndexGetFailure( IndexReference index )
		 {
			  AllStoreHolder.AssertValidIndex( index );
			  return _snapshot.indexGetFailure( index.Schema() );
		 }

		 public override IEnumerator<ConstraintDescriptor> ConstraintsGetForLabel( int labelId )
		 {
			  _ktx.assertOpen();
			  return _stores.constraintsGetForLabel( _snapshot, labelId );
		 }

		 public override IEnumerator<ConstraintDescriptor> ConstraintsGetForRelationshipType( int typeId )
		 {
			  _ktx.assertOpen();
			  return _stores.constraintsGetForRelationshipType( _snapshot, typeId );
		 }

		 public override IEnumerator<ConstraintDescriptor> ConstraintsGetAll()
		 {
			  _ktx.assertOpen();
			  return _stores.constraintsGetAll( _snapshot );
		 }
	}

}