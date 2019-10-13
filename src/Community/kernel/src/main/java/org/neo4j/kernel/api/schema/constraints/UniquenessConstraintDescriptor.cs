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
namespace Neo4Net.Kernel.api.schema.constraints
{

	using LabelSchemaDescriptor = Neo4Net.@internal.Kernel.Api.schema.LabelSchemaDescriptor;

	public class UniquenessConstraintDescriptor : IndexBackedConstraintDescriptor
	{
		 internal UniquenessConstraintDescriptor( LabelSchemaDescriptor schema ) : base( org.neo4j.@internal.kernel.api.schema.constraints.ConstraintDescriptor_Type.Unique, schema )
		 {
		 }

		 protected internal override string ConstraintTypeText()
		 {
			  return "UNIQUE";
		 }
	}

}