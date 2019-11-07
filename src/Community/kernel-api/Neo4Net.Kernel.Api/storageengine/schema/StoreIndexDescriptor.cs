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
namespace Neo4Net.Kernel.Api.StorageEngine.schema
{
	using IIndexCapability = Neo4Net.Kernel.Api.Internal.IIndexCapability;
	using ITokenNameLookup = Neo4Net.Kernel.Api.Internal.ITokenNameLookup;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.Kernel.Api.Internal.Schema.SchemaUtil.idTokenNameLookup;

	/// <summary>
	/// Describes an index which is committed to the database.
	/// 
	/// Adds an index id, a name, and optionally an owning constraint id to the general IndexDescriptor.
	/// </summary>
	public class StoreIndexDescriptor : IndexDescriptor, SchemaRule
	{
		 private readonly long _id;

		 private class SchemaComputerAnonymousInnerClass : Neo4Net.Kernel.Api.Internal.Schema.SchemaComputer<SchemaRule_Kind>
		 {
			 public SchemaRule_Kind computeSpecific( LabelSchemaDescriptor schema )
			 {
				  return NODE_PROPERTY_EXISTENCE_CONSTRAINT;
			 }

			 public SchemaRule_Kind computeSpecific( RelationTypeSchemaDescriptor schema )
			 {
				  return RELATIONSHIP_PROPERTY_EXISTENCE_CONSTRAINT;
			 }

			 public SchemaRule_Kind computeSpecific( SchemaDescriptor schema )
			 {
				  throw new System.InvalidOperationException( "General schema rules cannot support constraints" );
			 }
		 }
		 private readonly long? _owningConstraintId;
		 private readonly string _name;

		 // ** Copy-constructor used by sub-classes.
		 protected internal StoreIndexDescriptor( StoreIndexDescriptor indexDescriptor ) : base( indexDescriptor )
		 {
			  this._id = indexDescriptor._id;
			  this._owningConstraintId = indexDescriptor._owningConstraintId;
			  this._name = indexDescriptor._name;
		 }

		 // ** General purpose constructors.
		 internal StoreIndexDescriptor( IndexDescriptor descriptor, long id ) : this( descriptor, id, null )
		 {
		 }

		 internal StoreIndexDescriptor( IndexDescriptor descriptor, long id, long? owningConstraintId ) : base( descriptor )
		 {

			  this._id = id;
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  this._name = descriptor.UserSuppliedNameConflict.map( SchemaRule::checkName ).orElse( "index_" + id );

			  if ( descriptor.ProviderDescriptor() == null )
			  {
					throw new System.ArgumentException( "null provider descriptor prohibited" );
			  }

			  if ( owningConstraintId != null )
			  {
					AssertValidId( owningConstraintId.Value, "owning constraint id" );
			  }

			  this._owningConstraintId = owningConstraintId;
		 }

		 // ** Owning constraint

		 /// <summary>
		 /// Return the owning constraints of this index.
		 /// 
		 /// The owning constraint can be null during the construction of a uniqueness constraint. This construction first
		 /// creates the unique index, and then waits for the index to become fully populated and online before creating
		 /// the actual constraint. During unique index population the owning constraint will be null.
		 /// </summary>
		 /// <returns> the id of the owning constraint, or null if this has not been set yet. </returns>
		 /// <exception cref="IllegalStateException"> if this IndexRule cannot support uniqueness constraints (ei. the index is not
		 ///                               unique) </exception>
		 public virtual long? OwningConstraint
		 {
			 get
			 {
				  if ( !CanSupportUniqueConstraint() )
				  {
						throw new System.InvalidOperationException( "Can only get owner from constraint indexes." );
				  }
				  return _owningConstraintId;
			 }
		 }

		 public virtual bool CanSupportUniqueConstraint()
		 {
			  return Type() == IndexDescriptor.Type.Unique;
		 }

		 public virtual bool IndexWithoutOwningConstraint
		 {
			 get
			 {
				  return CanSupportUniqueConstraint() && OwningConstraint == null;
			 }
		 }

		 /// <summary>
		 /// Create a <seealso cref="StoreIndexDescriptor"/> with the given owning constraint id.
		 /// </summary>
		 /// <param name="constraintId"> an id >= 0, or null if no owning constraint exists. </param>
		 /// <returns> a new StoreIndexDescriptor with modified owning constraint. </returns>
		 public virtual StoreIndexDescriptor WithOwningConstraint( long? constraintId )
		 {
			  if ( !CanSupportUniqueConstraint() )
			  {
					throw new System.InvalidOperationException( this + " is not a constraint index" );
			  }
			  return new StoreIndexDescriptor( this, _id, constraintId );
		 }

		 // ** Upgrade to capable

		 /// <summary>
		 /// Create a <seealso cref="CapableIndexDescriptor"/> from this index descriptor, with no listed capabilities.
		 /// </summary>
		 /// <returns> a CapableIndexDescriptor. </returns>
		 public virtual CapableIndexDescriptor WithoutCapabilities()
		 {
			  return new CapableIndexDescriptor( this, IIndexCapability.NO_CAPABILITY );
		 }

		 // ** Misc

		 /// <summary>
		 /// WARNING: This toString is currently used in the inconsistency report, and cannot be changed due to backwards
		 ///          compatibility. If you are also annoyed by this, maybe now is the time to fix the inconsistency checker.
		 /// 
		 /// see InconsistencyReportReader.propagate( String, long )
		 /// </summary>
		 public override string ToString()
		 {
			  return ToString( idTokenNameLookup );
		 }

		 public virtual string ToString( ITokenNameLookup tokenNameLookup )
		 {
			  string ownerString = "";
			  if ( CanSupportUniqueConstraint() )
			  {
					ownerString = ", owner=" + _owningConstraintId;
			  }

			  return "IndexRule[id=" + _id + ", descriptor=" + this.UserDescription( tokenNameLookup ) +
						 ", provider=" + this.ProviderDescriptor() + ownerString + "]";
		 }

		 public virtual long Id
		 {
			 get
			 {
				  return _id;
			 }
		 }

		 public virtual string Name
		 {
			 get
			 {
				  return _name;
			 }
		 }
	}

}