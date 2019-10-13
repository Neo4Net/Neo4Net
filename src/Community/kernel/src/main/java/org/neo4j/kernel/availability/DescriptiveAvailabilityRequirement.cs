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
namespace Neo4Net.Kernel.availability
{

	public class DescriptiveAvailabilityRequirement : AvailabilityRequirement
	{
		 private readonly string _requirementDescription;

		 public DescriptiveAvailabilityRequirement( string description )
		 {
			  this._requirementDescription = description;
		 }

		 public override string Description()
		 {
			  return _requirementDescription;
		 }

		 public override bool Equals( object o )
		 {
			  if ( this == o )
			  {
					return true;
			  }
			  if ( o == null || this.GetType() != o.GetType() )
			  {
					return false;
			  }

			  AvailabilityRequirement that = ( AvailabilityRequirement ) o;

			  return Objects.Equals( _requirementDescription, that() );
		 }

		 public override int GetHashCode()
		 {
			  return Objects.GetHashCode( _requirementDescription );
		 }
	}

}