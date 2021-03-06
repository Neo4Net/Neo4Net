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
namespace Org.Neo4j.Kernel.impl.store.record
{
	using SchemaDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.SchemaDescriptor;
	using ConstraintDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.constraints.ConstraintDescriptor;
	using IndexBackedConstraintDescriptor = Org.Neo4j.Kernel.api.schema.constraints.IndexBackedConstraintDescriptor;
	using SchemaRule = Org.Neo4j.Storageengine.Api.schema.SchemaRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.schema.SchemaUtil.idTokenNameLookup;

	public class ConstraintRule : SchemaRule, Org.Neo4j.@internal.Kernel.Api.schema.constraints.ConstraintDescriptor_Supplier
	{
		 private readonly long? _ownedIndex;
		 private readonly string _name;
		 private readonly long _id;
		 private readonly ConstraintDescriptor _descriptor;

//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
		 public static ConstraintRule ConstraintRuleConflict( long id, ConstraintDescriptor descriptor )
		 {
			  return new ConstraintRule( id, descriptor, null );
		 }

//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
		 public static ConstraintRule ConstraintRuleConflict( long id, IndexBackedConstraintDescriptor descriptor, long ownedIndexRule )
		 {
			  return new ConstraintRule( id, descriptor, ownedIndexRule );
		 }

//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
		 public static ConstraintRule ConstraintRuleConflict( long id, ConstraintDescriptor descriptor, string name )
		 {
			  return new ConstraintRule( id, descriptor, null, name );
		 }

//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
		 public static ConstraintRule ConstraintRuleConflict( long id, IndexBackedConstraintDescriptor descriptor, long ownedIndexRule, string name )
		 {
			  return new ConstraintRule( id, descriptor, ownedIndexRule, name );
		 }

		 internal ConstraintRule( long id, ConstraintDescriptor descriptor, long? ownedIndex ) : this( id, descriptor, ownedIndex, null )
		 {
		 }

		 internal ConstraintRule( long id, ConstraintDescriptor descriptor, long? ownedIndex, string name )
		 {
			  this._id = id;
			  this._descriptor = descriptor;
			  this._ownedIndex = ownedIndex;
			  this._name = SchemaRule.nameOrDefault( name, "constraint_" + id );
		 }

		 public override string ToString()
		 {
			  return "ConstraintRule[id=" + _id + ", descriptor=" + _descriptor.userDescription( idTokenNameLookup ) + ", " +
						 "ownedIndex=" + _ownedIndex + "]";
		 }

		 public override SchemaDescriptor Schema()
		 {
			  return _descriptor.schema();
		 }

		 public virtual ConstraintDescriptor ConstraintDescriptor
		 {
			 get
			 {
				  return _descriptor;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("NumberEquality") public long getOwnedIndex()
		 public virtual long OwnedIndex
		 {
			 get
			 {
				  if ( _ownedIndex == null )
				  {
						throw new System.InvalidOperationException( "This constraint does not own an index." );
				  }
				  return _ownedIndex.Value;
			 }
		 }

		 public override bool Equals( object o )
		 {
			  if ( o is ConstraintRule )
			  {
					ConstraintRule that = ( ConstraintRule ) o;
					return this._descriptor.Equals( that._descriptor );
			  }
			  return false;
		 }

		 public override int GetHashCode()
		 {
			  return _descriptor.GetHashCode();
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