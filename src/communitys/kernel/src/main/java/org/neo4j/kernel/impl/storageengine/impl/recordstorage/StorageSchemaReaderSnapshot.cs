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
namespace Neo4Net.Kernel.impl.storageengine.impl.recordstorage
{

	using InternalIndexState = Neo4Net.@internal.Kernel.Api.InternalIndexState;
	using IndexNotFoundKernelException = Neo4Net.@internal.Kernel.Api.exceptions.schema.IndexNotFoundKernelException;
	using SchemaDescriptor = Neo4Net.@internal.Kernel.Api.schema.SchemaDescriptor;
	using ConstraintDescriptor = Neo4Net.@internal.Kernel.Api.schema.constraints.ConstraintDescriptor;
	using SchemaCache = Neo4Net.Kernel.Impl.Api.store.SchemaCache;
	using StorageSchemaReader = Neo4Net.Storageengine.Api.StorageSchemaReader;
	using CapableIndexDescriptor = Neo4Net.Storageengine.Api.schema.CapableIndexDescriptor;
	using IndexDescriptor = Neo4Net.Storageengine.Api.schema.IndexDescriptor;
	using PopulationProgress = Neo4Net.Storageengine.Api.schema.PopulationProgress;

	internal class StorageSchemaReaderSnapshot : StorageSchemaReader
	{
		 private readonly SchemaCache _schema;
		 private readonly RecordStorageReader _reader;

		 internal StorageSchemaReaderSnapshot( SchemaCache schema, RecordStorageReader reader )
		 {
			  this._schema = schema;
			  this._reader = reader;
		 }

		 public override CapableIndexDescriptor IndexGetForSchema( SchemaDescriptor descriptor )
		 {
			  return _schema.indexDescriptor( descriptor );
		 }

		 public override IEnumerator<CapableIndexDescriptor> IndexesGetForLabel( int labelId )
		 {
			  return _schema.indexDescriptorsForLabel( labelId );
		 }

		 public override IEnumerator<CapableIndexDescriptor> IndexesGetForRelationshipType( int relationshipType )
		 {
			  return _schema.indexDescriptorsForRelationshipType( relationshipType );
		 }

		 public override IEnumerator<CapableIndexDescriptor> IndexesGetAll()
		 {
			  return _schema.indexDescriptors().GetEnumerator();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.internal.kernel.api.InternalIndexState indexGetState(org.neo4j.storageengine.api.schema.IndexDescriptor descriptor) throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException
		 public override InternalIndexState IndexGetState( IndexDescriptor descriptor )
		 {
			  return _reader.indexGetState( descriptor );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.storageengine.api.schema.PopulationProgress indexGetPopulationProgress(org.neo4j.internal.kernel.api.schema.SchemaDescriptor descriptor) throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException
		 public override PopulationProgress IndexGetPopulationProgress( SchemaDescriptor descriptor )
		 {
			  return _reader.indexGetPopulationProgress( descriptor );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public String indexGetFailure(org.neo4j.internal.kernel.api.schema.SchemaDescriptor descriptor) throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException
		 public override string IndexGetFailure( SchemaDescriptor descriptor )
		 {
			  return _reader.indexGetFailure( descriptor );
		 }

		 public override IEnumerator<ConstraintDescriptor> ConstraintsGetForLabel( int labelId )
		 {
			  return _schema.constraintsForLabel( labelId );
		 }

		 public override IEnumerator<ConstraintDescriptor> ConstraintsGetForRelationshipType( int typeId )
		 {
			  return _schema.constraintsForRelationshipType( typeId );
		 }

		 public override IEnumerator<ConstraintDescriptor> ConstraintsGetAll()
		 {
			  return _schema.constraints();
		 }
	}

}