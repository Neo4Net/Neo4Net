﻿using System.Collections.Generic;

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
namespace Org.Neo4j.Kernel.impl.coreapi.schema
{

	internal abstract class SinglePropertyConstraintDefinition : PropertyConstraintDefinition
	{
		 protected internal readonly string PropertyKey;

		 protected internal SinglePropertyConstraintDefinition( InternalSchemaActions actions, string propertyKey ) : base( actions )
		 {
			  this.PropertyKey = requireNonNull( propertyKey );
		 }

		 public override IEnumerable<string> PropertyKeys
		 {
			 get
			 {
				  AssertInUnterminatedTransaction();
				  return singleton( PropertyKey );
			 }
		 }
	}

}