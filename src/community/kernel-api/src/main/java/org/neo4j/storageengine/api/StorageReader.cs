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
namespace Neo4Net.Storageengine.Api
{

	using PrimitiveLongResourceIterator = Neo4Net.Collections.PrimitiveLongResourceIterator;
	using IndexReference = Neo4Net.Internal.Kernel.Api.IndexReference;
	using IEntityNotFoundException = Neo4Net.Internal.Kernel.Api.exceptions.EntityNotFoundException;
	using IndexNotFoundKernelException = Neo4Net.Internal.Kernel.Api.exceptions.schema.IndexNotFoundKernelException;
	using SchemaDescriptor = Neo4Net.Internal.Kernel.Api.schema.SchemaDescriptor;
	using ConstraintDescriptor = Neo4Net.Internal.Kernel.Api.schema.constraints.ConstraintDescriptor;
	using Register_DoubleLongRegister = Neo4Net.Register.Register_DoubleLongRegister;
	using CapableIndexDescriptor = Neo4Net.Storageengine.Api.schema.CapableIndexDescriptor;
	using IndexDescriptor = Neo4Net.Storageengine.Api.schema.IndexDescriptor;
	using IndexReader = Neo4Net.Storageengine.Api.schema.IndexReader;
	using LabelScanReader = Neo4Net.Storageengine.Api.schema.LabelScanReader;

	/// <summary>
	/// Abstraction for accessing data from a <seealso cref="StorageEngine"/>.
	/// <para>
	/// A <seealso cref="StorageReader"/> must be <seealso cref="acquire() acquired"/> before use. After use the statement
	/// should be <seealso cref="release() released"/>. After released the reader can be acquired again.
	/// Creating and closing <seealso cref="StorageReader"/> can be somewhat costly, so there are benefits keeping these readers open
	/// during a longer period of time, with the assumption that it's still one thread at a time using each.
	/// </para>
	/// <para>
	/// </para>
	/// </summary>
	public interface StorageReader : IDisposable, StorageSchemaReader
	{
		 /// <summary>
		 /// Acquires this statement so that it can be used, should later be <seealso cref="release() released"/>.
		 /// Since a <seealso cref="StorageReader"/> can be reused after <seealso cref="release() released"/>, this call should
		 /// do initialization/clearing of state whereas data structures can be kept between uses.
		 /// </summary>
		 void Acquire();

		 /// <summary>
		 /// Releases this statement so that it can later be <seealso cref="acquire() acquired"/> again.
		 /// </summary>
		 void Release();

		 /// <summary>
		 /// Closes this statement so that it can no longer be used nor <seealso cref="acquire() acquired"/>.
		 /// </summary>
		 void Close();

		 /// <returns> <seealso cref="LabelScanReader"/> capable of reading nodes for specific label ids. </returns>
		 LabelScanReader LabelScanReader { get; }

		 /// <summary>
		 /// Returns an <seealso cref="IndexReader"/> for searching IEntity ids given property values. One reader is allocated
		 /// and kept per index throughout the life of a statement, making the returned reader repeatable-read isolation.
		 /// <para>
		 /// <b>NOTE:</b>
		 /// Reader returned from this method should not be closed. All such readers will be closed during <seealso cref="close()"/>
		 /// of the current statement.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="index"> <seealso cref="IndexDescriptor"/> to get reader for. </param>
		 /// <returns> <seealso cref="IndexReader"/> capable of searching IEntity ids given property values. </returns>
		 /// <exception cref="IndexNotFoundKernelException"> if no such index exists. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.Neo4Net.storageengine.api.schema.IndexReader getIndexReader(org.Neo4Net.storageengine.api.schema.IndexDescriptor index) throws org.Neo4Net.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException;
		 IndexReader GetIndexReader( IndexDescriptor index );

		 /// <summary>
		 /// Returns an <seealso cref="IndexReader"/> for searching IEntity ids given property values. A new reader is allocated
		 /// every call to this method, which means that newly committed data since the last call to this method
		 /// will be visible in the returned reader.
		 /// <para>
		 /// <b>NOTE:</b>
		 /// It is caller's responsibility to close the returned reader.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="index"> <seealso cref="IndexDescriptor"/> to get reader for. </param>
		 /// <returns> <seealso cref="IndexReader"/> capable of searching IEntity ids given property values. </returns>
		 /// <exception cref="IndexNotFoundKernelException"> if no such index exists. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.Neo4Net.storageengine.api.schema.IndexReader getFreshIndexReader(org.Neo4Net.storageengine.api.schema.IndexDescriptor index) throws org.Neo4Net.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException;
		 IndexReader GetFreshIndexReader( IndexDescriptor index );

		 /// <summary>
		 /// Reserves a node id for future use to store a node. The reason for it being exposed here is that
		 /// internal ids of nodes and relationships are publicly accessible all the way out to the user.
		 /// This will likely change in the future though.
		 /// </summary>
		 /// <returns> a reserved node id for future use. </returns>
		 long ReserveNode();

		 /// <summary>
		 /// Reserves a relationship id for future use to store a relationship. The reason for it being exposed here is that
		 /// internal ids of nodes and relationships are publicly accessible all the way out to the user.
		 /// This will likely change in the future though.
		 /// </summary>
		 /// <returns> a reserved relationship id for future use. </returns>
		 long ReserveRelationship();

		 long GraphPropertyReference { get; }

		 /// <param name="name"> name of index to find </param>
		 /// <returns> <seealso cref="IndexDescriptor"/> associated with the given {@code name}. </returns>
		 CapableIndexDescriptor IndexGetForName( string name );

		 /// <summary>
		 /// Returns all indexes (including unique) related to a property.
		 /// </summary>
		 IEnumerator<CapableIndexDescriptor> IndexesGetRelatedToProperty( int propertyId );

		 /// <param name="index"> <seealso cref="CapableIndexDescriptor"/> to get related uniqueness constraint for. </param>
		 /// <returns> schema rule id of uniqueness constraint that owns the given {@code index}, or {@code null}
		 /// if the given index isn't related to a uniqueness constraint. </returns>
		 long? IndexGetOwningUniquenessConstraintId( IndexDescriptor index );

		 /// <param name="descriptor"> describing the label and property key (or keys) defining the requested constraint. </param>
		 /// <returns> node property constraints associated with the label and one or more property keys token ids. </returns>
		 IEnumerator<ConstraintDescriptor> ConstraintsGetForSchema( SchemaDescriptor descriptor );

		 bool ConstraintExists( ConstraintDescriptor descriptor );

		 /// 
		 /// <param name="labelId"> The label id of interest. </param>
		 /// <returns> <seealso cref="PrimitiveLongResourceIterator"/> over node ids associated with given label id. </returns>
		 PrimitiveLongResourceIterator NodesGetForLabel( int labelId );

		 /// <summary>
		 /// Return index reference of a stored index.
		 /// </summary>
		 /// <param name="descriptor"> <seealso cref="IndexDescriptor"/> to get provider reference for. </param>
		 /// <returns> <seealso cref="IndexReference"/> for index. </returns>
		 /// <exception cref="IndexNotFoundKernelException"> if index not found. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.Neo4Net.internal.kernel.api.IndexReference indexReference(org.Neo4Net.storageengine.api.schema.IndexDescriptor descriptor) throws org.Neo4Net.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException;
		 IndexReference IndexReference( IndexDescriptor descriptor );

		 /// <summary>
		 /// Visits data about a relationship. The given {@code relationshipVisitor} will be notified.
		 /// </summary>
		 /// <param name="relationshipId"> the id of the relationship to access. </param>
		 /// <param name="relationshipVisitor"> <seealso cref="RelationshipVisitor"/> which will see the relationship data. </param>
		 /// <exception cref="EntityNotFoundException"> if no relationship exists by the given {@code relationshipId}. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: <EXCEPTION extends Exception> void relationshipVisit(long relationshipId, RelationshipVisitor<EXCEPTION> relationshipVisitor) throws org.Neo4Net.internal.kernel.api.exceptions.EntityNotFoundException, EXCEPTION;
		 void relationshipVisit<EXCEPTION>( long relationshipId, RelationshipVisitor<EXCEPTION> relationshipVisitor );

		 /// <summary>
		 /// Releases a previously <seealso cref="reserveNode() reserved"/> node id if it turns out to not actually being used,
		 /// for example in the event of a transaction rolling back.
		 /// </summary>
		 /// <param name="id"> reserved node id to release. </param>
		 void ReleaseNode( long id );

		 /// <summary>
		 /// Releases a previously <seealso cref="reserveRelationship() reserved"/> relationship id if it turns out to not
		 /// actually being used, for example in the event of a transaction rolling back.
		 /// </summary>
		 /// <param name="id"> reserved relationship id to release. </param>
		 void ReleaseRelationship( long id );

		 int ReserveLabelTokenId();

		 int ReservePropertyKeyTokenId();

		 int ReserveRelationshipTypeTokenId();

		 /// <summary>
		 /// Returns number of stored nodes labeled with the label represented by {@code labelId}.
		 /// </summary>
		 /// <param name="labelId"> label id to match. </param>
		 /// <returns> number of stored nodes with this label. </returns>
		 long CountsForNode( int labelId );

		 /// <summary>
		 /// Returns number of stored relationships of a certain {@code typeId} whose start/end nodes are labeled
		 /// with the {@code startLabelId} and {@code endLabelId} respectively.
		 /// </summary>
		 /// <param name="startLabelId"> label id of start nodes to match. </param>
		 /// <param name="typeId"> relationship type id to match. </param>
		 /// <param name="endLabelId"> label id of end nodes to match. </param>
		 /// <returns> number of stored relationships matching these criteria. </returns>
		 long CountsForRelationship( int startLabelId, int typeId, int endLabelId );

		 /// <summary>
		 /// Returns size of index, i.e. number of entities in that index.
		 /// </summary>
		 /// <param name="descriptor"> <seealso cref="SchemaDescriptor"/> to return size for. </param>
		 /// <returns> number of entities in the given index. </returns>
		 /// <exception cref="IndexNotFoundKernelException"> if no such index exists. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: long indexSize(org.Neo4Net.internal.kernel.api.schema.SchemaDescriptor descriptor) throws org.Neo4Net.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException;
		 long IndexSize( SchemaDescriptor descriptor );

		 /// <summary>
		 /// Returns percentage of values in the given {@code index} are unique. A value of {@code 1.0} means that
		 /// all values in the index are unique, e.g. that there are no duplicate values. A value of, say {@code 0.9}
		 /// means that 10% of the values are duplicates.
		 /// </summary>
		 /// <param name="descriptor"> <seealso cref="SchemaDescriptor"/> to get uniqueness percentage for. </param>
		 /// <returns> percentage of values being unique in this index, max {@code 1.0} for all unique. </returns>
		 /// <exception cref="IndexNotFoundKernelException"> if no such index exists. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: double indexUniqueValuesPercentage(org.Neo4Net.internal.kernel.api.schema.SchemaDescriptor descriptor) throws org.Neo4Net.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException;
		 double IndexUniqueValuesPercentage( SchemaDescriptor descriptor );

		 long NodesGetCount();

		 long RelationshipsGetCount();

		 int LabelCount();

		 int PropertyKeyCount();

		 int RelationshipTypeCount();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.Neo4Net.register.Register_DoubleLongRegister indexUpdatesAndSize(org.Neo4Net.internal.kernel.api.schema.SchemaDescriptor descriptor, org.Neo4Net.register.Register_DoubleLongRegister target) throws org.Neo4Net.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException;
		 Register_DoubleLongRegister IndexUpdatesAndSize( SchemaDescriptor descriptor, Register_DoubleLongRegister target );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.Neo4Net.register.Register_DoubleLongRegister indexSample(org.Neo4Net.internal.kernel.api.schema.SchemaDescriptor descriptor, org.Neo4Net.register.Register_DoubleLongRegister target) throws org.Neo4Net.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException;
		 Register_DoubleLongRegister IndexSample( SchemaDescriptor descriptor, Register_DoubleLongRegister target );

		 bool NodeExists( long id );

		 bool RelationshipExists( long id );

		 T getOrCreateSchemaDependantState<T>( Type type, System.Func<StorageReader, T> factory );

		 /// <returns> a new <seealso cref="StorageNodeCursor"/> capable of reading node data from the underlying storage. </returns>
		 StorageNodeCursor AllocateNodeCursor();

		 /// <returns> a new <seealso cref="StoragePropertyCursor"/> capable of reading property data from the underlying storage. </returns>
		 StoragePropertyCursor AllocatePropertyCursor();

		 /// <returns> a new <seealso cref="StorageRelationshipGroupCursor"/> capable of reading relationship group data from the underlying storage. </returns>
		 StorageRelationshipGroupCursor AllocateRelationshipGroupCursor();

		 /// <returns> a new <seealso cref="StorageRelationshipTraversalCursor"/> capable of traversing relationships from the underlying storage. </returns>
		 StorageRelationshipTraversalCursor AllocateRelationshipTraversalCursor();

		 /// <returns> a new <seealso cref="StorageRelationshipScanCursor"/> capable of reading relationship data from the underlying storage. </returns>
		 StorageRelationshipScanCursor AllocateRelationshipScanCursor();

		 /// <summary>
		 /// Get a lock-free snapshot of the current schema, for inspecting the current schema when no mutations are intended.
		 /// <para>
		 /// The index states, such as failure messages and population progress, are not captured in the snapshot, but are instead queried "live".
		 /// This means that if an index in the snapshot is then later deleted, then querying for the state of the index via the snapshot will throw an
		 /// <seealso cref="IndexNotFoundKernelException"/>.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <returns> a snapshot of the current schema. </returns>
		 StorageSchemaReader SchemaSnapshot();
	}

}