﻿/*
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
namespace Org.Neo4j.Storageengine.Api.schema
{

	using IndexOrder = Org.Neo4j.@internal.Kernel.Api.IndexOrder;
	using IndexReference = Org.Neo4j.@internal.Kernel.Api.IndexReference;
	using IndexValueCapability = Org.Neo4j.@internal.Kernel.Api.IndexValueCapability;
	using TokenNameLookup = Org.Neo4j.@internal.Kernel.Api.TokenNameLookup;
	using IndexProviderDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.IndexProviderDescriptor;
	using SchemaDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.SchemaDescriptor;
	using SchemaDescriptorSupplier = Org.Neo4j.@internal.Kernel.Api.schema.SchemaDescriptorSupplier;
	using SchemaUtil = Org.Neo4j.@internal.Kernel.Api.schema.SchemaUtil;
	using ValueCategory = Org.Neo4j.Values.Storable.ValueCategory;

	/// <summary>
	/// Internal representation of a graph index, including the schema unit it targets (eg. label-property combination)
	/// and the type of index. UNIQUE indexes are used to back uniqueness constraints.
	/// 
	/// An IndexDescriptor might represent an index that has not yet been committed, and therefore carries an optional
	/// user-supplied name. On commit the descriptor is upgraded to a <seealso cref="StoreIndexDescriptor"/> using
	/// <seealso cref="IndexDescriptor.withId(long)"/> or <seealso cref="IndexDescriptor.withIds(long, long)"/>.
	/// </summary>
	public class IndexDescriptor : SchemaDescriptorSupplier, IndexReference
	{
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal readonly SchemaDescriptor SchemaConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal readonly IndexDescriptor.Type TypeConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal readonly Optional<string> UserSuppliedNameConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal readonly IndexProviderDescriptor ProviderDescriptorConflict;

		 internal IndexDescriptor( IndexDescriptor indexDescriptor ) : this( indexDescriptor.SchemaConflict, indexDescriptor.TypeConflict, indexDescriptor.UserSuppliedNameConflict, indexDescriptor.ProviderDescriptorConflict )
		 {
		 }

		 public IndexDescriptor( SchemaDescriptor schema, Type type, Optional<string> userSuppliedName, IndexProviderDescriptor providerDescriptor )
		 {
			  this.SchemaConflict = schema;
			  this.TypeConflict = type;
			  this.UserSuppliedNameConflict = userSuppliedName;
			  this.ProviderDescriptorConflict = providerDescriptor;
		 }

		 // METHODS

		 public virtual Type Type()
		 {
			  return TypeConflict;
		 }

		 public override SchemaDescriptor Schema()
		 {
			  return SchemaConflict;
		 }

		 public virtual bool Unique
		 {
			 get
			 {
				  return TypeConflict == Type.Unique;
			 }
		 }

		 public override int[] Properties()
		 {
			  return SchemaConflict.PropertyIds;
		 }

		 public override string ProviderKey()
		 {
			  return ProviderDescriptorConflict.Key;
		 }

		 public override string ProviderVersion()
		 {
			  return ProviderDescriptorConflict.Version;
		 }

		 public override string Name()
		 {
			  return UserSuppliedNameConflict.orElse( Org.Neo4j.@internal.Kernel.Api.IndexReference_Fields.UNNAMED_INDEX );
		 }

		 public virtual IndexProviderDescriptor ProviderDescriptor()
		 {
			  return ProviderDescriptorConflict;
		 }

		 public override IndexOrder[] OrderCapability( params ValueCategory[] valueCategories )
		 {
			  return ORDER_NONE;
		 }

		 public override IndexValueCapability ValueCapability( params ValueCategory[] valueCategories )
		 {
			  return IndexValueCapability.NO;
		 }

		 public virtual bool FulltextIndex
		 {
			 get
			 {
				  return false;
			 }
		 }

		 public virtual bool EventuallyConsistent
		 {
			 get
			 {
				  return false;
			 }
		 }

		 /// <summary>
		 /// Returns a user friendly description of what this index indexes.
		 /// </summary>
		 /// <param name="tokenNameLookup"> used for looking up names for token ids. </param>
		 /// <returns> a user friendly description of what this index indexes. </returns>
		 public override string UserDescription( TokenNameLookup tokenNameLookup )
		 {
			  return format( "Index( %s, %s )", TypeConflict.name(), SchemaConflict.userDescription(tokenNameLookup) );
		 }

		 public override bool Equals( object o )
		 {
			  if ( o is IndexDescriptor )
			  {
					IndexDescriptor that = ( IndexDescriptor )o;
					return this.Type() == that.Type() && this.Schema().Equals(that.Schema());
			  }
			  return false;
		 }

		 public override int GetHashCode()
		 {
			  return TypeConflict.GetHashCode() & SchemaConflict.GetHashCode();
		 }

		 public override string ToString()
		 {
			  return UserDescription( SchemaUtil.idTokenNameLookup );
		 }

		 /// <summary>
		 /// Create a StoreIndexDescriptor, which represent the commit version of this index.
		 /// </summary>
		 /// <param name="id"> the index id of the committed index </param>
		 /// <returns> a StoreIndexDescriptor </returns>
		 public virtual StoreIndexDescriptor WithId( long id )
		 {
			  AssertValidId( id, "id" );
			  return new StoreIndexDescriptor( this, id );
		 }

		 /// <summary>
		 /// Create a StoreIndexDescriptor, which represent the commit version of this index, that is owned
		 /// by a uniqueness constraint.
		 /// </summary>
		 /// <param name="id"> id of the committed index </param>
		 /// <param name="owningConstraintId"> id of the uniqueness constraint owning this index </param>
		 /// <returns> a StoreIndexDescriptor </returns>
		 public virtual StoreIndexDescriptor WithIds( long id, long owningConstraintId )
		 {
			  AssertValidId( id, "id" );
			  AssertValidId( owningConstraintId, "owning constraint id" );
			  return new StoreIndexDescriptor( this, id, owningConstraintId );
		 }

		 internal virtual void AssertValidId( long id, string idName )
		 {
			  if ( id < 0 )
			  {
					throw new System.ArgumentException( "A " + this.GetType().Name + " " + idName + " must be positive, got " + id );
			  }
		 }

		 public virtual Optional<string> UserSuppliedName
		 {
			 get
			 {
				  return UserSuppliedNameConflict;
			 }
		 }

		 public enum Type
		 {
			  General,
			  Unique
		 }
	}

}