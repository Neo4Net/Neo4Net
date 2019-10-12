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
namespace Org.Neo4j.Kernel.api.schema.constraints
{
	using TokenNameLookup = Org.Neo4j.@internal.Kernel.Api.TokenNameLookup;

	/// <summary>
	/// Internal representation of a graph constraint, including the schema unit it targets (eg. label-property combination)
	/// and the how that schema unit is constrained (eg. "has to exist", or "must be unique").
	/// </summary>
	public abstract class ConstraintDescriptor : Org.Neo4j.@internal.Kernel.Api.schema.constraints.ConstraintDescriptor
	{
		public abstract string PrettyPrint( TokenNameLookup tokenNameLookup );
		public abstract Org.Neo4j.@internal.Kernel.Api.schema.SchemaDescriptor Schema();

		 private readonly ConstraintDescriptor.Type _type;

		 internal ConstraintDescriptor( Org.Neo4j.@internal.Kernel.Api.schema.constraints.ConstraintDescriptor_Type type )
		 {
			  this._type = type;
		 }

		 // METHODS

		 public override Org.Neo4j.@internal.Kernel.Api.schema.constraints.ConstraintDescriptor_Type Type()
		 {
			  return _type;
		 }

		 public override bool EnforcesUniqueness()
		 {
			  return _type.enforcesUniqueness();
		 }

		 public override bool EnforcesPropertyExistence()
		 {
			  return _type.enforcesPropertyExistence();
		 }

		 /// <param name="tokenNameLookup"> used for looking up names for token ids. </param>
		 /// <returns> a user friendly description of this constraint. </returns>
		 public override string UserDescription( TokenNameLookup tokenNameLookup )
		 {
			  return format( "Constraint( %s, %s )", _type.name(), Schema().userDescription(tokenNameLookup) );
		 }

		 public override bool IsSame( Org.Neo4j.@internal.Kernel.Api.schema.constraints.ConstraintDescriptor_Supplier supplier )
		 {
			  return this.Equals( supplier.ConstraintDescriptor );
		 }

		 public override sealed bool Equals( object o )
		 {
			  if ( o is ConstraintDescriptor )
			  {
					ConstraintDescriptor that = ( ConstraintDescriptor )o;
					return this.Type() == that.Type() && this.Schema().Equals(that.Schema());
			  }
			  return false;
		 }

		 public override sealed int GetHashCode()
		 {
			  return _type.GetHashCode() & Schema().GetHashCode();
		 }

		 internal virtual string EscapeLabelOrRelTyp( string name )
		 {
			  if ( name.Contains( ":" ) )
			  {
					return "`" + name + "`";
			  }
			  else
			  {
					return name;
			  }
		 }
	}

}